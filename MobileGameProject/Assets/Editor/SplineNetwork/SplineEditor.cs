using System.Collections.Generic;
using MobileGameProject.SplineNetwork;
using UnityEditor;
using UnityEngine;
using UnityEditor.ShortcutManagement;

namespace MobileGameProjectEditor.SplineNetwork
{
    [CustomEditor(typeof(Spline))]
    public class SplineEditor : Editor
    {
        #region Fields

        private Spline _spline;
        private int _hoverNodeIndex = -1;
        private bool _isEditing;

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
            _current = this;
            _spline = (Spline) target;
            _settings = new SplineEditorSettings();
            _selector = new SplineSelector(_spline, _settings);
            _drawer = new SplineDrawer(_spline, _settings, _selector);
            _manipulator = new SplineManipulator(_spline, _selector);
            _actions = new SplineActions(_spline, _selector);
            _selectionInfo = new SplineSelectionInfo(_spline, _manipulator, _selector);
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnDisable()
        {
            if (_current == this)
                _current = null;
            
            Tools.hidden = false;
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        private void OnSceneGUI()
        {
            if (_spline == null) return;

            Event e = Event.current;

            _selector.UpdateHoverSelection();
            _drawer.DrawSceneGUI();

            if (_isEditing)
                EditSceneGUI(e);
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

        #endregion Unity Callbacks

        #region SceneGUI

        private void EditSceneGUI(Event e)
        {
            Tools.hidden = true;

            if (!_createHeld)
            {
                ManipulateSceneGUI();
                SelectionSceneGUI(e);
            }
            else
                CreateSceneGUI(e);
        }

        private void SelectionSceneGUI(Event e)
        {
            int prevCount = _selector.SelectededCount;
            var prevHandle = _selector.SelectedHandle;
            var prevNodes = new HashSet<int>(_selector.SelectedNodeIndices);

            _selector.HandleSelectionInput(e);
            _manipulator.RefreshPivot();

            if (SelectionChanged(prevCount, prevHandle, prevNodes))
                Repaint();
        }

        private void ManipulateSceneGUI()
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
        }

        private void CreateSceneGUI(Event e)
        {
            int prevCount = _selector.SelectededCount;

            _actions.HandleCreateInput(e);

            if (_selector.SelectededCount != prevCount)
            {
                _manipulator.RefreshPivot();
                Repaint();
            }
        }

        private bool SelectionChanged(int prevCount, HandleSelection? prevHandle, HashSet<int> prevNodes)
        {
            return _selector.SelectededCount != prevCount
                   || !Equals(prevHandle, _selector.SelectedHandle)
                   || !prevNodes.SetEquals(_selector.SelectedNodeIndices);
        }

        #endregion SceneGUI

        #region InspectorGUI

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

        #endregion InspectorGUI

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

        #region Events

        private void OnUndoRedo()
        {
            _selector.SelectedNodeIndices.RemoveWhere(i => i < 0 || i >= _spline.nodes.Count);
            SceneView.RepaintAll();
        }

        #endregion Events
        
        #region Static
        
        private static SplineEditor _current;
        public static bool IsEditing => _current != null && _current._isEditing;
        
        [Shortcut("Spline Network/Drop Nodes to Surface", typeof(SplineShortcutContext), KeyCode.End)]
        private static void DropNodesShortcut()
        {
            if (_current == null || !_current._isEditing) return;
            _current._actions.DropSelectedNodesToSurface();
            _current.Repaint();
        }

        [Shortcut("Spline Network/Select All Nodes", typeof(SplineShortcutContext), KeyCode.A, ShortcutModifiers.Action)]
        private static void SelectAllShortcut()
        {
            if (_current == null || !_current._isEditing) return;
            _current._selector.SelectAll();
            _current.Repaint();
        }

        [ClutchShortcut("Spline Network/Create Node", typeof(SplineShortcutContext), KeyCode.N)]
        private static void CreateNodeClutch(ShortcutArguments args)
        {
            if (_current == null) return;
            _current._createHeld = args.stage == ShortcutStage.Begin;
        }
        
        #endregion Static
    }
}