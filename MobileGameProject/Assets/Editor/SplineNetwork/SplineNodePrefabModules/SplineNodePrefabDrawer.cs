using MobileGameProject.SplineNetwork;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace MobileGameProjectEditor.SplineNetwork
{
    public class SplineNodePrefabDrawer : ADrawer
    {
        private readonly SplineNodePrefab _splineNodePrefab;
        private readonly SplineNodePrefabSelector _selector;

        public SplineNodePrefabDrawer(SplineNodePrefab splineNodePrefab, SplineNodePrefabSelector selector)
        {
            _splineNodePrefab = splineNodePrefab;
            _selector = selector;
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
                bool isSelected = _selector.IsNodeSelected(i);
                bool isHovered = _selector.HoverSocketIndex == i;

                Color color = SplineEditorSettings.normalColor;
                float size = HandleUtility.GetHandleSize(worldPosition) * SplineEditorSettings.nodeBaseSize;

                if (isSelected)
                    color = SplineEditorSettings.selectedColor;
                else if (isHovered)
                {
                    color = SplineEditorSettings.hoverColor;
                    size *= SplineEditorSettings.hoverSizeMultiplier;
                }


                DrawWithOcclusion(
                    () => DrawNodeSquare(worldPosition, size, color),
                    color,
                    new Color(color.r, color.g, color.b, color.a * 0.4f));
            }
        }
    }
}