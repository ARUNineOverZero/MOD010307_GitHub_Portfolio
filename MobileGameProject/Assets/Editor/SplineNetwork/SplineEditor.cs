using System.Collections.Generic;
using MobileGameProject.SplineNetwork;
using UnityEditor;
using UnityEngine;
using UnityEditor.ShortcutManagement;
using Unity.Mathematics;

namespace MobileGameProjectEditor.SplineNetwork
{
    [CustomEditor(typeof(Spline))]
    public class SplineEditor : Editor
    {
        #region Fields

        private Spline _spline;
        private int _hoverNodeIndex = -1;
        private bool _isEditing;
        private SplineDrawer _drawer;
        private SplineSelector _selector;
        private SplineManipulator _manipulator;
        private SplineActions _actions;
        private SplineSelectionInfo _selectionInfo;
        private SplineMeshDrawer _meshDrawer;
        private bool _createHeld;

        private Vector3 _lastPosition;
        private Quaternion _lastRotation;
        private Vector3 _lastLocalScale;

        #endregion Fields

        #region Unity Callbacks

        private void OnEnable()
        {
            _current = this;
            _spline = (Spline) target;
            _selector = new SplineSelector(_spline);
            _drawer = new SplineDrawer(_spline, _selector);
             _meshDrawer = new SplineMeshDrawer(_spline);
            _manipulator = new SplineManipulator(_spline, _selector);
            _actions = new SplineActions(_spline, _selector, _meshDrawer);
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
            _meshDrawer.DrawSceneGUI();

            if (_isEditing)
                EditSceneGUI(e);
            else
                Tools.hidden = false;

            _drawer.RequestRepaint(e);
        }

        public override void OnInspectorGUI()
        {
            SplineEditorSettings.DrawSplineSettingsGUI();
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
            int prevCount = _selector.SelectedCount;
            var prevHandle = _selector.SelectedHandle;
            var prevNodes = new HashSet<int>(_selector.SelectedNodeIndices);

            _selector.HandleSelectionInput(e);
            _manipulator.RefreshPivot();

            if (_selector.SelectedCount != prevCount
                   || !Equals(prevHandle, _selector.SelectedHandle)
                   || !prevNodes.SetEquals(_selector.SelectedNodeIndices))
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
            int prevCount = _selector.SelectedCount;

            _actions.HandleCreateInput(e);

            if (_selector.SelectedCount != prevCount)
            {
                _manipulator.RefreshPivot();
                Repaint();
            }
        }

        #endregion SceneGUI

        #region Inspector

        private void DrawEditGUI()
        {
            GUI.backgroundColor = _isEditing ? Color.green : Color.white;
            if (GUILayout.Button(_isEditing ? "Exit Edit Mode" : "Enter Edit Mode", GUILayout.Height(30)))
            {
                _isEditing = !_isEditing;
                Tools.hidden = _isEditing;
                _selector.ClearSelection();
                SceneView.RepaintAll();
            }
            
            if (_isEditing)
            {
                _selectionInfo.DrawGUI();
                
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
                    _actions.ResetHandles();
                
                if (GUILayout.Button("Smooth Segmment Handles", GUILayout.Height(30)))
                    _actions.SmoothHandles();

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
            else if (GUILayout.Button("Create Mesh", GUILayout.Height(30)))
                _actions.CreateMeshObject();
           
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
        
        [Shortcut("Spline Network/Select Segments From Selected Nodes", typeof(SplineShortcutContext), KeyCode.A, ShortcutModifiers.Alt)]
        private static void SelectSegmentsFromSelectedNodesShortcut()
        {            
            if (_current == null || !_current._isEditing) return;
            _current._selector.SelectSegmentsFromSelectedNodes();
            _current.Repaint();
        }

        [ClutchShortcut("Spline Network/Create Node", typeof(SplineShortcutContext), KeyCode.C)]
        private static void CreateNodeClutch(ShortcutArguments args)
        {
            if (_current == null) return;
            _current._createHeld = args.stage == ShortcutStage.Begin;
        }
        
        [ClutchShortcut("Spline Network/Create Segment", typeof(SplineShortcutContext), KeyCode.F)]
        private static void CreateSegmentClutch(ShortcutArguments args)
        {
            if (_current == null) return;
            _current._actions.CreateSegmentBetweenSelected();
            _current.Repaint();
        }
        
        [Shortcut("Spline Network/Delete Nodes", typeof(SplineShortcutContext), KeyCode.X)]
        private static void DeleteNodesShortcut()
        {
            if (_current == null || !_current._isEditing) return;
            _current._actions.DeleteSelectedNodes();
            _current.Repaint();
        }

        [Shortcut("Spline Network/Delete Segment", typeof(SplineShortcutContext), KeyCode.X, ShortcutModifiers.Alt)]
        private static void DeleteSegmentShortcut()
        {
            if (_current == null || !_current._isEditing) return;
            _current._actions.DeleteSegmentBetweenSelected();
            _current.Repaint();
        }
        
        [Shortcut("Spline Network/Reset Nodes Handles", typeof(SplineShortcutContext), KeyCode.Z)]
        private static void ResetNodeHandlesShortcut()
        {
            if (_current == null || !_current._isEditing) return;
            _current._actions.ResetHandles();
            _current.Repaint();
        }

        [Shortcut("Spline Network/Reset Segment Handles", typeof(SplineShortcutContext), KeyCode.Z, ShortcutModifiers.Alt)]
        private static void ResetNodeSegmentShortcut()
        {
            if (_current == null || !_current._isEditing) return;
            _current._actions.ResetHandlesOnSelectedSegments();
            _current.Repaint();
        }

        [Shortcut("Spline Network/Smooth Nodes Handles", typeof(SplineShortcutContext), KeyCode.Z, ShortcutModifiers.Shift)]
        private static void SmoothNodeSegmentShortcut()
        {
            if (_current == null || !_current._isEditing) return;
            _current._actions.SmoothHandles();
            _current.Repaint();
        }
        
        #endregion Static
    }
}