

using System;
using System.Collections.Generic;
using Codice.Client.Common;
using MobileGameProject.SplineNetwork;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace MobileGameProjectEditor.SplineNetwork
{
    [CustomEditor(typeof(SplineNodePrefab))]
    public class SplineNodePrefabEditor : Editor
    {
        #region Fields

        private SplineNodePrefab _splineNodePrefab;
        private SplineNodePrefabDrawer _drawer;
        private SplineNodePrefabSelector _selector;
        private bool _isEditing;
        private bool _createHeld;

        #endregion Fields

        #region Unity Callbacks

        public void OnEnable()
        {
            _splineNodePrefab = (SplineNodePrefab)target;
            _selector = new SplineNodePrefabSelector(_splineNodePrefab);
            _drawer = new SplineNodePrefabDrawer(_splineNodePrefab, _selector);
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

            if(!_createHeld)
            {
                SelectionSceneGUI(e);
            }
        }

         private void SelectionSceneGUI(Event e)
        {
            int prevCount = _selector.SelectedCount;
            var prevNodes = new HashSet<int>(_selector.SelectedSocketIndices);

            _selector.HandleSelectionInput(e);

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
        }

        #endregion Unity Callbacks

        #region Static


        #endregion Static
    }
}