

using System;
using MobileGameProject.SplineNetwork;
using Unity.VisualScripting;
using UnityEditor;

namespace MobileGameProjectEditor.SplineNetwork
{
    [CustomEditor(typeof(SplineNodePrefab))]
    public class SplineNodePrefabEditor : Editor
    {
        #region Fields

        private SplineNodePrefab _splineNodePrefab;
        private SplineNodePrefabDrawer _drawer;
        private bool _isEditing;

        #endregion Fields

        #region Unity Callbacks

        public void OnEnable()
        {
            _splineNodePrefab = (SplineNodePrefab)target;
            _drawer = new SplineNodePrefabDrawer(_splineNodePrefab);
        }

        private void OnDisable()
        {
            if(_isEditing)
                Tools.hidden = false;
        }

        private void OnSceneGUI()
        {
            
        }

        public override void OnInspectorGUI()
        {
            SplineEditorSettings.DrawSplineNodePrefabSettingsGUI();
            DrawDefaultInspector();
        }

        #endregion Unity Callbacks

        #region Static


        #endregion Static
    }
}