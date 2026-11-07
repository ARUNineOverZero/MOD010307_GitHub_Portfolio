using MobileGameProject.SplineNetwork;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

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

                DrawWithOcclusion(
                    () => DrawNodeSquare(worldPos, size, color),
                    color,
                    new Color(color.r, color.g, color.b, color.a * 0.4f));
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

        private void DrawHandle(int segmentIndex, bool isStart, int nodeIndex, Vector3 localOffset)
        {
            Vector3 worldPos = _spline.GetNodeWorldPosition(nodeIndex);
            Vector3 handleWorld = worldPos + _spline.transform.TransformVector(localOffset);

            bool isHovered = _selector.HoverHandleSegmentIndex == segmentIndex && _selector.HoverHandleIsStart == isStart;
            bool isSelected = _selector.SelectedHandle.HasValue &&
                              _selector.SelectedHandle.Value.segmentIndex == segmentIndex &&
                              _selector.SelectedHandle.Value.isStart == isStart;

            Handles.color = _settings.hoverColor;
            Handles.DrawLine(worldPos, handleWorld);

            float size = HandleUtility.GetHandleSize(handleWorld) * _settings.nodeBaseSize;
            Color color = _settings.hoverColor;

            if (isSelected)
                color = _settings.selectedColor;
            else if (isHovered)
                size *= _settings.hoverSizeMultiplier;

            DrawWithOcclusion(
                () => DrawNodeSquare(handleWorld, size, color),
                color,
                new Color(color.r, color.g, color.b, color.a * 0.4f));
        }

        private void DrawSegmentLine(SplineSegment segment, Color color)
        {
            if (segment.startNodeIndex < 0 || segment.startNodeIndex >= _spline.nodes.Count ||
                segment.endNodeIndex < 0 || segment.endNodeIndex >= _spline.nodes.Count)
                return;

            Color occluded = new Color(color.r, color.g, color.b, color.a * 0.35f);
            if (_settings.segmentLevelOfDetail <= 2)
            {
                DrawWithOcclusion(
                    () => Handles.DrawLine(_spline.GetNodeWorldPosition(segment.startNodeIndex), _spline.GetNodeWorldPosition(segment.endNodeIndex)),
                    color,
                    occluded);
                return;
            }

            Vector3 p0 = _spline.GetNodeWorldPosition(segment.startNodeIndex);
            Vector3 p3 = _spline.GetNodeWorldPosition(segment.endNodeIndex);
            Vector3 p1 = p0 + _spline.transform.TransformVector(segment.startHandleOffset);
            Vector3 p2 = p3 + _spline.transform.TransformVector(segment.endHandleOffset);

            Vector3[] points = new Vector3[_settings.segmentLevelOfDetail + 1];
            for (int i = 0; i <= _settings.segmentLevelOfDetail; i++)
                points[i] = EvaluateCubicBezier(p0, p1, p2, p3, i / (float) _settings.segmentLevelOfDetail);

            DrawWithOcclusion(
                () => Handles.DrawPolyLine(points),
                color,
                occluded);
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

        private void DrawWithOcclusion(System.Action draw, Color normalColor, Color occludedColor)
        {
            var prevZTest = Handles.zTest;
            var prevColor = Handles.color;

            Handles.zTest = CompareFunction.Greater;
            Handles.color = occludedColor;
            draw();

            Handles.zTest = CompareFunction.LessEqual;
            Handles.color = normalColor;
            draw();

            Handles.zTest = prevZTest;
            Handles.color = prevColor;
        }
    }
}