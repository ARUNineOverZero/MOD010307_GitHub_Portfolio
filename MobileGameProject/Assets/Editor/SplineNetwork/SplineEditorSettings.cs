using UnityEngine;

namespace MobileGameProjectEditor.SplineNetwork
{
    public class SplineEditorSettings
    {
        public float nodeBaseSize = 0.06f;
        public float hoverSizeMultiplier = 1.35f;
        public int segmentLevelOfDetail = 10;

        public Color normalColor = Color.white;
        public Color hoverColor = Color.yellow;
        public Color selectedColor = Color.green;

        public const float hoverTestRadiusFactor = 0.6f;
        public const float hoverPixelThreshold = 8f;
    }
}