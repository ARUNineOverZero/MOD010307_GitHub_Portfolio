using UnityEditor;
using UnityEngine;

namespace MobileGameProjectEditor.SplineNetwork
{
    [System.Serializable]
    public static class SplineEditorSettings
    {
        public static float nodeBaseSize = 0.06f;
        public static float hoverSizeMultiplier = 1.35f;
        public static int segmentLevelOfDetail = 10;

        public static Color normalColor = Color.white;
        public static Color hoverColor = Color.yellow;
        public static Color selectedColor = Color.green;

        public static float hoverTestRadiusFactor = 0.6f;
        public static float hoverPixelThreshold = 8f;

        public static void DrawSettingsGUI()
        {
            EditorGUILayout.LabelField("Visual Settings", EditorStyles.boldLabel);

            nodeBaseSize = EditorGUILayout.Slider("Node Size", nodeBaseSize, 0.02f, 0.3f);
            hoverSizeMultiplier = EditorGUILayout.Slider("Hover Multiplier", hoverSizeMultiplier, 1f, 2f);
            segmentLevelOfDetail = EditorGUILayout.IntSlider("Segment LOD", segmentLevelOfDetail, 2, 50);

            normalColor = EditorGUILayout.ColorField("Normal", normalColor);
            hoverColor = EditorGUILayout.ColorField("Hover", hoverColor);
            selectedColor = EditorGUILayout.ColorField("Selected", selectedColor);

            if (GUI.changed)
                SceneView.RepaintAll();
        }
    }
}