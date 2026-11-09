using MobileGameProject.SplineNetwork;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace MobileGameProjectEditor.SplineNetwork
{
    public class SplineNodePrefabDrawer : ADrawer
    {
        private SplineNodePrefab _splineNodePrefab;

        public SplineNodePrefabDrawer(SplineNodePrefab splineNodePrefab)
        {
            _splineNodePrefab = splineNodePrefab;
        }

        public void DrawSceneGUI()
        {
            DrawNodeSockets();
        }

        public void DrawNodeSockets()
        {
            for (int i = 0; i < _splineNodePrefab.sockets.Count; i++)
            {
                Vector3 worldPosition = _splineNodePrefab.GetSocketWorldPosition(i);
                Color color = SplineEditorSettings.normalColor;
                float size = HandleUtility.GetHandleSize(worldPosition) * SplineEditorSettings.nodeBaseSize;

                DrawWithOcclusion(
                    () => DrawNodeSquare(worldPosition, size, color),
                    color,
                    new Color(color.r, color.g, color.b, color.a * 0.4f));
            }
        }
    }
}