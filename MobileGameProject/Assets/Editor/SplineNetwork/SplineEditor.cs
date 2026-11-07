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
            _drawer = new SplineDrawer(_settings);
            _selector = new SplineSelector(_settings);
            _manipulator = new SplineManipulator();
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

            _drawer.DrawNodes(_spline, _selector.SelectedNodeIndices, _selector.HoverNodeIndex);
            _drawer.DrawHandles(_spline, _selector.SelectedNodeIndices, _selector.HoverHandleSegmentIndex, _selector.HoverHandleIsStart, _selector.SelectedHandle);
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
                            HandleMovement();
                            break;
                        case Tool.Rotate:
                            HandleRotation();
                            break;
                        default:
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
            if (_selector.SelectedNodeIndices.Count == 0) return;

            Undo.RecordObject(_spline, "Delete Spline Nodes");

            _spline.RemoveAllNodesAt(_selector.SelectedNodeIndices.ToArray());
            _selector.SelectedNodeIndices.Clear();
            EditorUtility.SetDirty(_spline);
            SceneView.RepaintAll();
        }

        private void CreateSegmment()
        {
            if (_selector.SelectedNodeIndices.Count != 2) return;

            var segment = _spline.segments.FirstOrDefault(x =>
                _selector.SelectedNodeIndices.Contains(x.startNodeIndex) && _selector.SelectedNodeIndices.Contains(x.endNodeIndex));
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
            if (_selector.SelectedNodeIndices.Count != 2) return;

            var segment = _spline.segments.FirstOrDefault(x =>
                _selector.SelectedNodeIndices.Contains(x.startNodeIndex) && _selector.SelectedNodeIndices.Contains(x.endNodeIndex));
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
            _selector.SelectedNodeIndices.Clear();
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

        private Vector3 GetSplineWorldPosition(int index) => _spline.transform.TransformPoint(_spline.nodes[index].position);

        private Quaternion GetHandleRotation(int nodeIndex = -1)
        {
            if (Tools.pivotRotation == PivotRotation.Global)
                return Quaternion.identity;

            if (nodeIndex == -1)
                return _spline.transform.rotation;

            var node = _spline.nodes[nodeIndex];
            return _spline.transform.rotation * node.rotation;
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

        #region Editing

        private void HandleMovement()
        {
            if (_selector.SelectedNodeIndices.Count == 0) return;

            if (_selector.SelectedHandle.HasValue)
                HandleNodeHandleMovement();
            else
                HandleNodeMovement();
        }

        private void HandleNodeMovement()
        {
            Vector3 worldCenter = Vector3.zero;
            foreach (int index in _selector.SelectedNodeIndices)
                worldCenter += GetSplineWorldPosition(index);

            worldCenter /= _selector.SelectedNodeIndices.Count;

            int pivotIndex = _selector.SelectedNodeIndices.First();
            Quaternion handleRot = GetHandleRotation(pivotIndex);

            Vector3 newWorldCenter = Handles.PositionHandle(worldCenter, handleRot);
            if (newWorldCenter != worldCenter)
            {
                Vector3 delta = newWorldCenter - worldCenter;

                Undo.RecordObject(_spline, "Move Spline Nodes");
                foreach (int index in _selector.SelectedNodeIndices)
                {
                    Vector3 worldPos = GetSplineWorldPosition(index);
                    Vector3 newWorldPos = worldPos + delta;
                    _spline.nodes[index].position = _spline.transform.InverseTransformPoint(newWorldPos);
                }

                EditorUtility.SetDirty(_spline);
            }
        }

        private void HandleNodeHandleMovement()
        {
            var segmentHandle = _selector.SelectedHandle.Value;
            var segment = _spline.segments[segmentHandle.segmentIndex];
            var nodeIndex = segmentHandle.isStart ? segment.startNodeIndex : segment.endNodeIndex;
            Vector3 worldPos = GetSplineWorldPosition(nodeIndex);
            Vector3 handleWorld = worldPos + _spline.transform.TransformVector(segmentHandle.isStart ? segment.startHandleOffset : segment.endHandleOffset);
            Quaternion handleRot = GetHandleRotation();
            Vector3 newHandleWorld = Handles.PositionHandle(handleWorld, handleRot);
            if (newHandleWorld != handleWorld)
            {
                Undo.RecordObject(_spline, "Move Spline Nodes Handle");

                Vector3 worldOffset = newHandleWorld - worldPos;
                Vector3 localOffset = _spline.transform.InverseTransformVector(worldOffset);

                if (segmentHandle.isStart)
                    segment.startHandleOffset = localOffset;
                else
                    segment.endHandleOffset = localOffset;

                EditorUtility.SetDirty(_spline);
            }
        }

        private void HandleRotation()
        {
            if (_selector.SelectedNodeIndices.Count == 0) return;

            if (!_selector.SelectedHandle.HasValue)
                HandleNodeRotation();
        }

        private void HandleNodeRotation()
        {
            if (_selector.SelectedNodeIndices.Count != 1) return;

            int index = _selector.SelectedNodeIndices.First();
            var node = _spline.nodes[index];

            Vector3 worldPos = GetSplineWorldPosition(index);
            Quaternion worldRot = _spline.transform.rotation * node.rotation;

            Quaternion newWorldRot = Handles.RotationHandle(worldRot, worldPos);

            if (newWorldRot != worldRot)
            {
                Undo.RecordObject(_spline, "Rotate Spline Node");
                node.rotation = Quaternion.Inverse(_spline.transform.rotation) * newWorldRot;
                EditorUtility.SetDirty(_spline);
            }
        }

        #endregion Editing

        #region Events

        private void OnUndoRedo()
        {
            _selector.SelectedNodeIndices.RemoveWhere(i => i < 0 || i >= _spline.nodes.Count);
            SceneView.RepaintAll();
        }

        #endregion Events
    }
}