using System.Collections.Generic;
using System.Linq;
using MobileGameProject.SplineNetwork;
using UnityEditor;
using UnityEngine;

namespace MobileGameProjectEditor.SplineNetwork
{
    [CustomEditor(typeof(Spline))]
    public class SplineEditor : Editor
    {
        #region Fields

        private Spline _spline;
        private int _hoverNodeIndex = -1;
        private bool _isEditing = false;

        private SplineEditorSettings _settings;
        private SplineDrawer _drawer;
        private SplineSelector _selector;
        private SplineManipulator _manipulator;

        #endregion Fields

        #region Unity Callbacks

        private void OnEnable()
        {
            _spline = (Spline) target;
            _settings = new SplineEditorSettings();
            _selector = new SplineSelector(_settings);
            _drawer = new SplineDrawer(_settings, _selector);
            _manipulator = new SplineManipulator(_spline, _selector);
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnDisable()
        {
            Tools.hidden = false;
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        private void OnSceneGUI()
        {
            if (_spline == null) return;

            Event e = Event.current;

            _selector.UpdateHover(_spline);
            _selector.UpdateHandleHover(_spline);

            _drawer.DrawNodes(_spline);
            _drawer.DrawHandles(_spline);
            _drawer.DrawSegments(_spline);

            if (_isEditing)
            {
                Tools.hidden = true;

                if (e.control)
                {
                    HandleCreateInput(e);
                }
                else
                {
                    switch (Tools.current)
                    {
                        case Tool.Move:
                            _manipulator.HandleMovement();
                            break;
                        case Tool.Rotate:
                            _manipulator.HandleRotation();
                            break;
                    }

                    _selector.HandleSelectionInput(e);
                }
            }
            else
            {
                Tools.hidden = false;
            }

            _drawer.RequestRepaint(e);
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space(8);
            DrawEditGUI();
            GUI.backgroundColor = Color.white;
        }

        #endregion Unity Callbacks

        #region Inspector

        private void DrawEditGUI()
        {
            GUI.backgroundColor = _isEditing ? Color.green : Color.white;
            if (GUILayout.Button(_isEditing ? "Exit Edit Mode" : "Enter Edit Mode", GUILayout.Height(30)))
            {
                _isEditing = !_isEditing;
                Tools.hidden = _isEditing;
                _selector.SelectedNodeIndices.Clear();
                SceneView.RepaintAll();
            }

            if (_isEditing)
            {
                GUI.backgroundColor = Color.white;

                EditorGUILayout.LabelField("Spline Edit", EditorStyles.boldLabel);
                EditorGUILayout.Space(4);

                EditorGUILayout.LabelField("Node Edit", EditorStyles.boldLabel);
                EditorGUILayout.Space(4);

                if (GUILayout.Button("Delete Selected", GUILayout.Height(30)))
                    DeleteSelectedNodes();

                EditorGUILayout.LabelField("Segment Edit", EditorStyles.boldLabel);
                EditorGUILayout.Space(4);

                if (GUILayout.Button("Create Segmment", GUILayout.Height(30)))
                    CreateSegmment();

                if (GUILayout.Button("Delete Segment", GUILayout.Height(30)))
                    DeleteSegmment();

                if (GUILayout.Button("Clear Spline Data", GUILayout.Height(30)))
                    ClearSplineData();
            }
        }

        private void DeleteSelectedNodes()
        {
            if (_selector.SelectededCount == 0) return;

            Undo.RecordObject(_spline, "Delete Spline Nodes");

            _spline.RemoveAllNodesAt(_selector.SelectedNodeIndices.ToArray());
            _selector.SelectedNodeIndices.Clear();
            EditorUtility.SetDirty(_spline);
            SceneView.RepaintAll();
        }

        private void CreateSegmment()
        {
            if (_selector.SelectededCount != 2) return;

            var segment = _spline.segments.FirstOrDefault(x =>
                _selector.IsNodeSelected(x.startNodeIndex) && _selector.IsNodeSelected(x.endNodeIndex));
            if (segment == null)
            {
                Undo.RecordObject(_spline, "Create Spline Segment");

                var indices = _selector.SelectedNodeIndices.ToArray();
                _spline.AddSegment(indices[0], indices[1]);
            }

            EditorUtility.SetDirty(_spline);
            SceneView.RepaintAll();
        }

        private void DeleteSegmment()
        {
            if (_selector.SelectededCount != 2) return;

            var segment = _spline.segments.FirstOrDefault(x =>
                _selector.IsNodeSelected(x.startNodeIndex) && _selector.IsNodeSelected(x.endNodeIndex));
            if (segment == null) return;

            Undo.RecordObject(_spline, "Delete Spline Segment");

            _spline.RemoveSegment(segment);

            EditorUtility.SetDirty(_spline);
            SceneView.RepaintAll();
        }

        private void ClearSplineData()
        {
            Undo.RecordObject(_spline, "Clear Spline Data");

            _spline.Clear();
            _selector.ClearSelection();
            EditorUtility.SetDirty(_spline);
            SceneView.RepaintAll();
        }

        #endregion Inspector

        #region Helpers

        private bool TryGetClickedWorldPosition(Event e, out Vector3 worldPos)
        {
            var cam = SceneView.currentDrawingSceneView.camera;
            var position = e.mousePosition;

            position.y = cam.scaledPixelHeight - position.y;
            var ray = cam.ScreenPointToRay(position);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                worldPos = hit.point;
                return true;
            }

            worldPos = Vector3.zero;
            return false;
        }

        #endregion Helpers

        #region Create

        private void HandleCreateInput(Event e)
        {
            if (e.type != EventType.MouseDown || e.button != 0) return;

            if (_selector.SelectedHandle.HasValue)
                _selector.ClearSelectedHandle();

            if (TryGetClickedWorldPosition(e, out Vector3 worldPos))
            {
                CreateNodeAt(worldPos);
                e.Use();
                Repaint();
            }
        }

        private void CreateNodeAt(Vector3 worldPos)
        {
            Undo.RecordObject(_spline, "Add Spline Node");

            var localpos = _spline.transform.InverseTransformPoint(worldPos);
            int newIndex = _spline.AddNode(localpos, _selector.SelectedNodeIndices.ToArray());

            _selector.SelectedNodeIndices.Clear();
            _selector.SelectedNodeIndices.Add(newIndex);

            EditorUtility.SetDirty(_spline);
        }

        #endregion Create

        #region Events

        private void OnUndoRedo()
        {
            _selector.SelectedNodeIndices.RemoveWhere(i => i < 0 || i >= _spline.nodes.Count);
            SceneView.RepaintAll();
        }

        #endregion Events
    }
}