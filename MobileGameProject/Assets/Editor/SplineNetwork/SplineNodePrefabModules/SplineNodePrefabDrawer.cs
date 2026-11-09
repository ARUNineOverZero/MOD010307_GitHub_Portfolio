using MobileGameProject.SplineNetwork;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace MobileGameProjectEditor.SplineNetwork
{
    public class SplineNodePrefabDrawer
    {
        private SplineNodePrefab _splineNodePrefab;

        public SplineNodePrefabDrawer(SplineNodePrefab splineNodePrefab)
        {
            _splineNodePrefab = splineNodePrefab;
        }
    }
}