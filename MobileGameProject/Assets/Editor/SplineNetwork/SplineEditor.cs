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
        private SplineSelectionInfo _selectionInfo;
        private bool _createHeld;

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
            _selectionInfo = new SplineSelectionInfo(_spline, _selector);
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
            
            CheckShortcuts(e);

            _selector.UpdateHover();
            _selector.UpdateHandleHover();

            _drawer.DrawNodes();
            _drawer.DrawHandles();
            _drawer.DrawSegments();

            if (_isEditing)
            {
                Tools.hidden = true;

                if (!_createHeld)
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

                    int prevCount = _selector.SelectededCount;
                    var prevHandle = _selector.SelectedHandle;
                    
                    _selector.HandleSelectionInput(e);
                    
                    if (_selector.SelectededCount != prevCount || _selector.SelectedHandle != prevHandle)
                        Repaint();
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
            DrawSettingsGUI();
            DrawDefaultInspector();
            EditorGUILayout.Space(8);
            DrawEditGUI();
            GUI.backgroundColor = Color.white;
        }
        
        private void DrawSettingsGUI()
        {
            EditorGUILayout.LabelField("Visual Settings", EditorStyles.boldLabel);

            _settings.nodeBaseSize = EditorGUILayout.Slider("Node Size", _settings.nodeBaseSize, 0.02f, 0.3f);
            _settings.hoverSizeMultiplier = EditorGUILayout.Slider("Hover Multiplier", _settings.hoverSizeMultiplier, 1f, 2f);
            _settings.segmentLevelOfDetail = EditorGUILayout.IntSlider("Segment LOD", _settings.segmentLevelOfDetail, 2, 50);

            _settings.normalColor = EditorGUILayout.ColorField("Normal", _settings.normalColor);
            _settings.hoverColor = EditorGUILayout.ColorField("Hover", _settings.hoverColor);
            _settings.selectedColor = EditorGUILayout.ColorField("Selected", _settings.selectedColor);

            if (GUI.changed)
                SceneView.RepaintAll();
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
            
            _selectionInfo.DrawGUI();

            if (_isEditing)
            {
                GUI.backgroundColor = Color.white;

                EditorGUILayout.LabelField("Spline Edit", EditorStyles.boldLabel);
                EditorGUILayout.Space(4);

                EditorGUILayout.LabelField("Node Edit", EditorStyles.boldLabel);
                EditorGUILayout.Space(4);

                GUI.backgroundColor = Color.red;
                
                if (GUILayout.Button("Delete Selected", GUILayout.Height(30)))
                    _actions.DeleteSelectedNodes();
                
                GUI.backgroundColor = Color.white;
                
                if (GUILayout.Button("Reset Node Handles", GUILayout.Height(30)))
                    _actions.ResetNodeHandles();

                EditorGUILayout.LabelField("Segment Edit", EditorStyles.boldLabel);
                EditorGUILayout.Space(4);

                if (GUILayout.Button("Create Segmment", GUILayout.Height(30)))
                    _actions.CreateSegmentBetweenSelected();
                
                GUI.backgroundColor = Color.red;
                
                if (GUILayout.Button("Delete Segment", GUILayout.Height(30)))
                    _actions.DeleteSegmentBetweenSelected();
                
                GUI.backgroundColor = Color.white;
                
                if (GUILayout.Button("Reset Segmment Handles", GUILayout.Height(30)))
                    _actions.ResetHandlesOnSelectedSegments();

                if (GUILayout.Button("Clear Spline Data", GUILayout.Height(30)))
                    _actions.Clear();
            }
        }

        #endregion Inspector
        
        #region Shortcuts

        private void CheckShortcuts(Event e)
        {
            DropNodeShortCut(e);
            CheckCreateMode(e);
            SelectAll(e);
        }

        private void DropNodeShortCut(Event e)
        {
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.End)
            {
                _actions.DropSelectedNodesToSurface();
                e.Use();
            }
        }

        private void CheckCreateMode(Event e)
        {
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.N)
            {
                _createHeld = true;
                e.Use();
            }
            else if (e.type == EventType.KeyUp && e.keyCode == KeyCode.N)
            {
                _createHeld = false;
                e.Use();
            }
        }
        
        private void SelectAll(Event e)
        {
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.A && e.control)
            {
                _selector.SelectAll();
                e.Use();
            }
        }
        
        #endregion Shortcuts

        #region Events

        private void OnUndoRedo()
        {
            _selector.SelectedNodeIndices.RemoveWhere(i => i < 0 || i >= _spline.nodes.Count);
            SceneView.RepaintAll();
        }

        #endregion Events
    }
}