

using MobileGameProject.SplineNetwork;
using UnityEditor;

namespace MobileGameProjectEditor.SplineNetwork
{
    [CustomEditor(typeof(SplineNodePrefab))]
    public class SplineNodePrefabEditor : Editor
    {

        #region Fields

        private SplineNodePrefab _splineNodePrefab;
        private SplineNodePrefabDrawer _drawer;

        #endregion Fields

        #region Unity Callbacks

        public void OnEnable()
        {
            _splineNodePrefab = (SplineNodePrefab)target;
            _drawer = new SplineNodePrefabDrawer(_splineNodePrefab);
        }

        #endregion Unity Callbacks

        #region Static


        #endregion Static
    }
}