using System.Linq;
using MobileGameProject.SplineNetwork;
using UnityEditor;
using UnityEngine;

namespace MobileGameProjectEditor.SplineNetwork
{
    public class SplineDrawer : ADrawer
    {
        private readonly Spline _spline;
        private readonly SplineSelector _selector;

        public SplineDrawer(Spline spline, SplineSelector selector)
        {
            _spline = spline;
            _selector = selector;
        }

        public void DrawSceneGUI()
        {
            DrawNodes();
            DrawHandles();
            DrawSegments();
        }

        public void DrawNodes()
        {
            for (int i = 0; i < _spline.nodes.Count; i++)
            {
                Vector3 worldPos = _spline.GetNodeWorldPosition(i);
                bool isSelected = _selector.IsNodeSelected(i);
                bool isHovered = _selector.HoverNodeIndex == i;

                Color color = SplineEditorSettings.normalColor;
                float size = HandleUtility.GetHandleSize(worldPos) * SplineEditorSettings.nodeBaseSize;

                if (isSelected)
                    color = SplineEditorSettings.selectedColor;
                else if (isHovered)
                {
                    color = SplineEditorSettings.hoverColor;
                    size *= SplineEditorSettings.hoverSizeMultiplier;
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
                DrawSegmentLine(seg, SplineEditorSettings.normalColor);
        }

        private void DrawHandle(int segmentIndex, bool isStart, int nodeIndex, Vector3 localOffset)
        {
            var seg = _spline.segments[segmentIndex];
            string socketId = isStart ? seg.startSocket : seg.endSocket;

            Vector3 origin = GetSegmentEndWorldPosition(nodeIndex, socketId);
            Vector3 handleWorld = origin + _spline.transform.TransformVector(localOffset);

            bool isHovered = _selector.HoverHandleSegmentIndex == segmentIndex && _selector.HoverHandleIsStart == isStart;
            bool isSelected = _selector.SelectedHandle.HasValue &&
                              _selector.SelectedHandle.Value.segmentIndex == segmentIndex &&
                              _selector.SelectedHandle.Value.isStart == isStart;

            Handles.color = SplineEditorSettings.hoverColor;
            Handles.DrawLine(origin, handleWorld);

            float size = HandleUtility.GetHandleSize(handleWorld) * SplineEditorSettings.nodeBaseSize;
            Color color = SplineEditorSettings.hoverColor;

            if (isSelected)
                color = SplineEditorSettings.selectedColor;
            else if (isHovered)
                size *= SplineEditorSettings.hoverSizeMultiplier;

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

            Vector3 p0 = GetSegmentEndWorldPosition(segment.startNodeIndex, segment.startSocket);
            Vector3 p3 = GetSegmentEndWorldPosition(segment.endNodeIndex, segment.endSocket);

            Color occluded = new Color(color.r, color.g, color.b, color.a * 0.35f);

            if (SplineEditorSettings.segmentLevelOfDetail <= 2)
            {
                DrawWithOcclusion(() => Handles.DrawLine(p0, p3), color, occluded);
                return;
            }

            Vector3 p1 = p0 + _spline.transform.TransformVector(segment.startHandleOffset);
            Vector3 p2 = p3 + _spline.transform.TransformVector(segment.endHandleOffset);

            Vector3[] points = new Vector3[SplineEditorSettings.segmentLevelOfDetail + 1];
            for (int i = 0; i <= SplineEditorSettings.segmentLevelOfDetail; i++)
                points[i] = EvaluateCubicBezier(p0, p1, p2, p3, i / (float)SplineEditorSettings.segmentLevelOfDetail);

            DrawWithOcclusion(() => Handles.DrawPolyLine(points), color, occluded);
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

        private Vector3 GetSegmentEndWorldPosition(int nodeIndex, string socketId)
        {
            Vector3 nodeWorld = _spline.GetNodeWorldPosition(nodeIndex);
            var node = _spline.nodes[nodeIndex];

            if (node.prefab == null || string.IsNullOrEmpty(socketId))
                return nodeWorld;

            var socket = node.prefab.sockets.FirstOrDefault(s => s.id == socketId);
            if (socket == null)
                return nodeWorld;

            Quaternion nodeWorldRot = _spline.transform.rotation * node.rotation;
            return nodeWorld + nodeWorldRot * socket.localPosition;
        }
    }
}