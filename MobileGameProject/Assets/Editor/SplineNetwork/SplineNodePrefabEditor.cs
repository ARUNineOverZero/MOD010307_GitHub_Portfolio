

using System;
using System.Collections.Generic;
using System.Linq;
using Codice.Client.Common;
using MobileGameProject.SplineNetwork;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace MobileGameProjectEditor.SplineNetwork
{
    [CustomEditor(typeof(SplineNodePrefab))]
    public class SplineNodePrefabEditor : Editor
    {
        #region Fields

        private SplineNodePrefab _splineNodePrefab;
        private SplineNodePrefabDrawer _drawer;
        private SplineNodePrefabSelector _selector;
        private SplineNodePrefabManipulator _manipulator;
        private SplineNodePrefabActions _actions;
        private SplineNodePrefabSelectionInfo _selectionInfo;
        private bool _isEditing;
        private bool _createHeld;

        #endregion Fields

        #region Unity Callbacks

        public void OnEnable()
        {
            _current = this;
            _splineNodePrefab = (SplineNodePrefab)target;
            _selector = new SplineNodePrefabSelector(_splineNodePrefab);
            _manipulator = new SplineNodePrefabManipulator(_splineNodePrefab, _selector);
            _drawer = new SplineNodePrefabDrawer(_splineNodePrefab, _selector);
            _actions = new SplineNodePrefabActions(_splineNodePrefab, _selector);
            _selectionInfo = new SplineNodePrefabSelectionInfo(_splineNodePrefab, _manipulator, _selector);
        }

        private void OnDisable()
        {
            if (_isEditing)
                Tools.hidden = false;
        }

        private void OnSceneGUI()
        {
            if (_splineNodePrefab == null) return;

            Event e = Event.current;
            _selector.UpdateSocketHover();
            _drawer.DrawSceneGUI();

            if (_isEditing)
                EditSceneGUI(e);
            else
                Tools.hidden = false;

            _drawer.RequestRepaint(e);
        }

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

        private void SelectionSceneGUI(Event e)
        {
            int prevCount = _selector.SelectedCount;
            var prevNodes = new HashSet<int>(_selector.SelectedSocketIndices);

            _selector.HandleSelectionInput(e);
            _manipulator.RefreshPivot();

            if (_selector.SelectedCount != prevCount || prevNodes.SetEquals(_selector.SelectedSocketIndices))
                Repaint();
        }

        public override void OnInspectorGUI()
        {
            SplineEditorSettings.DrawSplineNodePrefabSettingsGUI();
            DrawDefaultInspector();
            EditorGUILayout.Space(8);
            DrawEditGUI();
        }

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

                if (GUILayout.Button("Set Pivot", GUILayout.Height(30)))
                {
                    _actions.ResetPivot();
                    SceneView.RepaintAll();
                }
            }
        }

        #endregion Unity Callbacks

        #region Static

        private static SplineNodePrefabEditor _current;
        public static bool IsEditing => _current != null && _current._isEditing;

        [Shortcut("Spline Network/Sockets/Select All", typeof(SplineNodePrefabShortcutContext), KeyCode.A, ShortcutModifiers.Action)]
        private static void SelectAllShortcut()
        {
            if (_current == null || !_current._isEditing) return;
            _current._selector.SelectAll();
            _current.Repaint();
        }

        [ClutchShortcut("Spline Network/Sockets/Create", typeof(SplineNodePrefabShortcutContext), KeyCode.C)]
        private static void CreateNodeClutch(ShortcutArguments args)
        {
            if (_current == null) return;
            _current._createHeld = args.stage == ShortcutStage.Begin;
        }

        [Shortcut("Spline Network/Sockets/Delete", typeof(SplineNodePrefabShortcutContext), KeyCode.X)]
        private static void DeleteNodesShortcut()
        {
            if (_current == null || !_current._isEditing) return;
            _current._actions.DeleteSelectedSockets();
            _current.Repaint();
        }

        #endregion Static
    }
}