using MobileGameProject.SplineNetwork;
using UnityEditor;
using UnityEngine;

namespace MobileGameProjectEditor.SplineNetwork
{
    public class SplineDrawer
    {
        private readonly Spline _spline;
        private readonly SplineEditorSettings _settings;
        private readonly SplineSelector _selector;

        public SplineDrawer(Spline spline, SplineEditorSettings settings, SplineSelector selector)
        {
            _spline = spline;
            _settings = settings;
            _selector = selector;
        }

        public void DrawNodes()
        {
            for (int i = 0; i < _spline.nodes.Count; i++)
            {
                Vector3 worldPos = _spline.GetNodeWorldPosition(i);
                bool isSelected = _selector.IsNodeSelected(i);
                bool isHovered = _selector.HoverNodeIndex == i;

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

        public void DrawHandles()
        {
            for (int i = 0; i < _spline.segments.Count; i++)
            {
                var seg = _spline.segments[i];

                if (_selector.IsNodeSelected(seg.startNodeIndex))
                    DrawHandle(i, true, seg.startNodeIndex, seg.startHandleOffset);

                if (_selector.IsNodeSelected(seg.endNodeIndex))
                    DrawHandle(i, false, seg.endNodeIndex, seg.endHandleOffset);
            }
        }

        public void DrawSegments()
        {
            foreach (var seg in _spline.segments)
                DrawSegmentLine(seg, _settings.normalColor);
        }

        public void RequestRepaint(Event e)
        {
            if (e.type == EventType.MouseMove || e.type == EventType.MouseDrag)
                SceneView.RepaintAll();
        }

        private void DrawHandle(
            int segmentIndex,
            bool isStart,
            int nodeIndex,
            Vector3 localOffset)
        {
            Vector3 worldPos = _spline.GetNodeWorldPosition(nodeIndex);
            Vector3 handleWorld = worldPos + _spline.transform.TransformVector(localOffset);

            bool isHovered = _selector.HoverHandleSegmentIndex == segmentIndex && _selector.HoverHandleIsStart == isStart;
            bool isSelected = _selector.SelectedHandle.HasValue &&
                              _selector.SelectedHandle.Value.segmentIndex == segmentIndex &&
                              _selector.SelectedHandle.Value.isStart == isStart;

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

        private void DrawSegmentLine(SplineSegment segment, Color color)
        {
            if (segment.startNodeIndex < 0 || segment.startNodeIndex >= _spline.nodes.Count ||
                segment.endNodeIndex < 0 || segment.endNodeIndex >= _spline.nodes.Count)
                return;

            if (_settings.segmentLevelOfDetail <= 2)
            {
                Handles.color = color;
                Handles.DrawLine(
                    _spline.GetNodeWorldPosition(segment.startNodeIndex),
                    _spline.GetNodeWorldPosition(segment.endNodeIndex));
                return;
            }

            Vector3 p0 = _spline.GetNodeWorldPosition(segment.startNodeIndex);
            Vector3 p3 = _spline.GetNodeWorldPosition(segment.endNodeIndex);
            Vector3 p1 = p0 + _spline.transform.TransformVector(segment.startHandleOffset);
            Vector3 p2 = p3 + _spline.transform.TransformVector(segment.endHandleOffset);

            Vector3[] points = new Vector3[_settings.segmentLevelOfDetail + 1];
            for (int i = 0; i <= _settings.segmentLevelOfDetail; i++)
                points[i] = EvaluateCubicBezier(p0, p1, p2, p3, i / (float) _settings.segmentLevelOfDetail);

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