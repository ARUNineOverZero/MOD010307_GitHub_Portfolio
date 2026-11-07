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
        private SplineActions _actions;

        #endregion Fields

        #region Unity Callbacks

        private void OnEnable()
        {
            _spline = (Spline) target;
            _settings = new SplineEditorSettings();
            _selector = new SplineSelector(_spline, _settings);
            _drawer = new SplineDrawer(_spline, _settings, _selector);
            _manipulator = new SplineManipulator(_spline, _selector);
            _actions = new SplineActions(_spline, _selector);
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

            _selector.UpdateHover();
            _selector.UpdateHandleHover();

            _drawer.DrawNodes();
            _drawer.DrawHandles();
            _drawer.DrawSegments();

            if (_isEditing)
            {
                Tools.hidden = true;

                if (!e.control)
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
                else
                    _actions.HandleCreateInput(e);
            }
            else
                Tools.hidden = false;

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
                    _actions.DeleteSelectedNodes();

                EditorGUILayout.LabelField("Segment Edit", EditorStyles.boldLabel);
                EditorGUILayout.Space(4);

                if (GUILayout.Button("Create Segmment", GUILayout.Height(30)))
                    _actions.CreateSegmentBetweenSelected();

                if (GUILayout.Button("Delete Segment", GUILayout.Height(30)))
                    _actions.DeleteSegmentBetweenSelected();

                if (GUILayout.Button("Clear Spline Data", GUILayout.Height(30)))
                    _actions.Clear();
            }
        }

        #endregion Inspector

        #region Events

        private void OnUndoRedo()
        {
            _selector.SelectedNodeIndices.RemoveWhere(i => i < 0 || i >= _spline.nodes.Count);
            SceneView.RepaintAll();
        }

        #endregion Events
    }
}