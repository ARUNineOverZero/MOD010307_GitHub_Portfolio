using System.CodeDom;
using System.Collections.Generic;
using MobileGameProject.SplineNetwork;
using UnityEditor;
using UnityEngine;
using static MobileGameProjectEditor.SplineNetwork.SplineEditor;

namespace MobileGameProjectEditor.SplineNetwork
{
    public struct HandleSelection
    {
        public int segmentIndex;
        public bool isStart;
    }
    
    public class SplineDrawer
    {
        private SplineEditorSettings _settings;
        public SplineDrawer(SplineEditorSettings settings) => _settings = settings;

        public void DrawNodes(Spline spline, HashSet<int> selectedNodeIndices, int hoverIndex)
        {
            for (int i = 0; i < spline.nodes.Count; i++)
            {
                Vector3 worldPos = spline.GetNodeWorldPosition(i);
                bool isSelected = selectedNodeIndices != null && selectedNodeIndices.Contains(i);
                bool isHovered = hoverIndex == i;

                Color color = _settings.normalColor;
                float size = HandleUtility.GetHandleSize(worldPos) * _settings.nodeBaseSize;

                if (isSelected)
                    color = _settings.selectedColor;
                else if (isHovered)
                {
                    color = _settings.hoverColor;
                    size *= _settings.hoverSizeMultiplier;
                }

                DrawNodeSquare(worldPos, size, color);
            }
        }

        public void DrawHandles(
            Spline spline,
            HashSet<int> selectedNodeIndices,
            int hoverHandleSegment,
            bool hoverHandleIsStart,
            HandleSelection? selectedHandle)
        {
            for (int i = 0; i < spline.segments.Count; i++)
            {
                var seg = spline.segments[i];

                if (selectedNodeIndices.Contains(seg.startNodeIndex))
                    DrawHandle(spline, i, true, seg.startNodeIndex, seg.startHandleOffset,
                               hoverHandleSegment, hoverHandleIsStart, selectedHandle);

                if (selectedNodeIndices.Contains(seg.endNodeIndex))
                    DrawHandle(spline, i, false, seg.endNodeIndex, seg.endHandleOffset,
                               hoverHandleSegment, hoverHandleIsStart, selectedHandle);
            }
        }

        public void DrawSegments(Spline spline)
        {
            foreach (var seg in spline.segments)
                DrawSegmentLine(spline, seg, _settings.normalColor);
        }

        public void RequestRepaint(Event e)
        {
            if (e.type == EventType.MouseMove || e.type == EventType.MouseDrag)
                SceneView.RepaintAll();
        }

        private void DrawHandle(
            Spline spline,
            int segmentIndex,
            bool isStart,
            int nodeIndex,
            Vector3 localOffset,
            int hoverHandleSegment,
            bool hoverHandleIsStart,
            HandleSelection? selectedHandle)
        {
            Vector3 worldPos = spline.GetNodeWorldPosition(nodeIndex);
            Vector3 handleWorld = worldPos + spline.transform.TransformVector(localOffset);

            bool isHovered = hoverHandleSegment == segmentIndex && hoverHandleIsStart == isStart;
            bool isSelected = selectedHandle.HasValue &&
                              selectedHandle.Value.segmentIndex == segmentIndex &&
                              selectedHandle.Value.isStart == isStart;

            Handles.color = _settings.hoverColor;
            Handles.DrawLine(worldPos, handleWorld);

            float size = HandleUtility.GetHandleSize(worldPos) * _settings.nodeBaseSize;
            Color color = _settings.normalColor;

            if (isSelected)
                color = _settings.selectedColor;
            else if (isHovered)
            {
                color = _settings.hoverColor;
                size *= _settings.hoverSizeMultiplier;
            }

            DrawNodeSquare(handleWorld, size, color);
        }

        private void DrawSegmentLine(Spline spline, SplineSegment segment, Color color)
        {
            if (segment.startNodeIndex < 0 || segment.startNodeIndex >= spline.nodes.Count ||
                segment.endNodeIndex < 0 || segment.endNodeIndex >= spline.nodes.Count)
                return;

            if (_settings.segmentLevelOfDetail <= 2)
            {
                Handles.color = color;
                Handles.DrawLine(
                    spline.GetNodeWorldPosition(segment.startNodeIndex),
                    spline.GetNodeWorldPosition(segment.endNodeIndex));
                return;
            }

            Vector3 p0 = spline.GetNodeWorldPosition(segment.startNodeIndex);
            Vector3 p3 = spline.GetNodeWorldPosition(segment.endNodeIndex);
            Vector3 p1 = p0 + spline.transform.TransformVector(segment.startHandleOffset);
            Vector3 p2 = p3 + spline.transform.TransformVector(segment.endHandleOffset);

            Vector3[] points = new Vector3[_settings.segmentLevelOfDetail + 1];
            for (int i = 0; i <= _settings.segmentLevelOfDetail; i++)
                points[i] = EvaluateCubicBezier(p0, p1, p2, p3, i / (float)_settings.segmentLevelOfDetail);

            Handles.color = color;
            Handles.DrawPolyLine(points);
        }

        private void DrawNodeSquare(Vector3 center, float size, Color color)
        {
            var cam = SceneView.currentDrawingSceneView.camera;
            if (cam == null) return;

            Vector3 right = cam.transform.right * size;
            Vector3 up = cam.transform.up * size;

            Vector3[] verts =
            {
                center - right - up,
                center - right + up,
                center + right + up,
                center + right - up
            };

            Handles.DrawSolidRectangleWithOutline(verts, color, color);
        }

        private static Vector3 EvaluateCubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float u = 1f - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            return uuu * p0
                 + 3f * uu * t * p1
                 + 3f * u * tt * p2
                 + ttt * p3;
        }
    }
}