using UnityEditor;
using UnityEngine;

namespace MobileGameProjectEditor.SplineNetwork
{
    [System.Serializable]
    public static class SplineEditorSettings
    {
        public static float nodeBaseSize = DefaultNodeBaseSize;
        public static float hoverSizeMultiplier = DefaultHoverSizeMultiplier;
        public static int segmentLevelOfDetail = DefaultSegmentLevelOfDetail;

        public static Color normalColor = DefaultNormalColor;
        public static Color hoverColor = DefaultHoverColor;
        public static Color selectedColor = DefaultSelectedColor;

        public static float hoverTestRadiusFactor = DefaultHoverTestRadiusFactor;
        public static float hoverPixelThreshold = DefaultHoverPixelThreshold;

        public const float DefaultNodeBaseSize = 0.06f;
        public const float DefaultHoverSizeMultiplier = 1.35f;
        public const int DefaultSegmentLevelOfDetail = 10;

        public static Color DefaultNormalColor => Color.white;
        public static Color DefaultHoverColor => Color.yellow;
        public static Color DefaultSelectedColor => Color.green;

        public const float DefaultHoverTestRadiusFactor = 0.6f;
        public const float DefaultHoverPixelThreshold = 8f;

        public static void ResetToDefault()
        {
            nodeBaseSize = DefaultNodeBaseSize;
            hoverSizeMultiplier = DefaultHoverSizeMultiplier;
            segmentLevelOfDetail = DefaultSegmentLevelOfDetail;

            normalColor = DefaultNormalColor;
            hoverColor = DefaultHoverColor;
            selectedColor = DefaultSelectedColor;

            hoverTestRadiusFactor = DefaultHoverTestRadiusFactor;
            hoverPixelThreshold = DefaultHoverPixelThreshold;
        }

        private static void ResetToDefaultGUI()
        {
            var backgroundColor = GUI.backgroundColor;
            EditorGUILayout.Space(4);
            GUI.backgroundColor = Color.white;

            if (GUILayout.Button("Reset Settings To Default", GUILayout.Height(30)))
                ResetToDefault();

            EditorGUILayout.Space(4);

            GUI.backgroundColor = backgroundColor;
        }

        public static void DrawSplineSettingsGUI()
        {
            EditorGUILayout.LabelField("Visual Settings", EditorStyles.boldLabel);

            nodeBaseSize = EditorGUILayout.Slider("Node Size", nodeBaseSize, 0.02f, 0.3f);
            hoverSizeMultiplier = EditorGUILayout.Slider("Hover Multiplier", hoverSizeMultiplier, 1f, 2f);
            segmentLevelOfDetail = EditorGUILayout.IntSlider("Segment LOD", segmentLevelOfDetail, 2, 50);

            normalColor = EditorGUILayout.ColorField("Normal", normalColor);
            hoverColor = EditorGUILayout.ColorField("Hover", hoverColor);
            selectedColor = EditorGUILayout.ColorField("Selected", selectedColor);
            
            ResetToDefaultGUI();

            if (GUI.changed)
                SceneView.RepaintAll();
        }

        public static void DrawSplineNodePrefabSettingsGUI()
        {
            EditorGUILayout.LabelField("Visual Settings", EditorStyles.boldLabel);

            nodeBaseSize = EditorGUILayout.Slider("Node Size", nodeBaseSize, 0.02f, 0.3f);
            hoverSizeMultiplier = EditorGUILayout.Slider("Hover Multiplier", hoverSizeMultiplier, 1f, 2f);

            normalColor = EditorGUILayout.ColorField("Normal", normalColor);
            hoverColor = EditorGUILayout.ColorField("Hover", hoverColor);
            selectedColor = EditorGUILayout.ColorField("Selected", selectedColor);

            ResetToDefaultGUI();

            if (GUI.changed)
                SceneView.RepaintAll();
        }
    }
}