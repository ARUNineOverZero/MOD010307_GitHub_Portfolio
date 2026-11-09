using System.Collections.Generic;
using System.Linq;
using MobileGameProject.SplineNetwork;
using UnityEditor;
using UnityEngine;

namespace MobileGameProjectEditor.SplineNetwork
{
    public class SplineActions : AActions
    {
        private readonly Spline _spline;
        private readonly SplineSelector _selector;

        public SplineActions(Spline spline, SplineSelector selector)
        {
            _spline = spline;
            _selector = selector;
        }

        public override void CreateNodeAt(Vector3 worldPos)
        {
            Undo.RecordObject(_spline, "Add Spline Node");

            Vector3 localPos = _spline.transform.InverseTransformPoint(worldPos);
            int newIndex = _spline.AddNode(localPos, _selector.SelectedNodeIndices.ToArray());

            _selector.SelectedNodeIndices.Clear();
            _selector.SelectedNodeIndices.Add(newIndex);

            EditorUtility.SetDirty(_spline);
        }

        public void DeleteSelectedNodes()
        {
            if (_selector.SelectedCount == 0) return;

            Undo.RecordObject(_spline, "Delete Spline Nodes");

            _spline.RemoveAllNodesAt(_selector.SelectedNodeIndices.ToArray());
            _selector.ClearSelection();

            EditorUtility.SetDirty(_spline);
        }

        public void CreateSegmentBetweenSelected()
        {
            if (_selector.SelectedCount != 2) return;

            var segment = _spline.segments.FirstOrDefault(x =>
                _selector.IsNodeSelected(x.startNodeIndex) &&
                _selector.IsNodeSelected(x.endNodeIndex));

            if (segment != null) return;

            Undo.RecordObject(_spline, "Create Spline Segment");

            var indices = _selector.SelectedNodeIndices.ToArray();
            _spline.AddSegment(indices[0], indices[1]);

            EditorUtility.SetDirty(_spline);
        }

        public void DeleteSegmentBetweenSelected()
        {
            if (_selector.SelectedCount != 2) return;

            var segment = _spline.segments.FirstOrDefault(x =>
                _selector.IsNodeSelected(x.startNodeIndex) &&
                _selector.IsNodeSelected(x.endNodeIndex));

            if (segment == null) return;

            Undo.RecordObject(_spline, "Delete Spline Segment");

            _spline.RemoveSegment(segment);

            EditorUtility.SetDirty(_spline);
        }

        public void Clear()
        {
            Undo.RecordObject(_spline, "Clear Spline Data");

            _spline.Clear();
            _selector.ClearSelection();

            EditorUtility.SetDirty(_spline);
        }

        public void ResetHandlesOnSelectedSegments()
        {
            if (_selector.SelectedCount < 2) return;

            Undo.RecordObject(_spline, "Reset Segment Handles");

            bool changed = false;

            foreach (var segment in _selector.GetSelectedSegments())
            {
                Vector3 startPos = _spline.nodes[segment.startNodeIndex].position;
                Vector3 endPos = _spline.nodes[segment.endNodeIndex].position;
                Vector3 delta = endPos - startPos;

                segment.startHandleOffset = delta * Spline.DefaultHandleOffsetFactor;
                segment.endHandleOffset = -delta * Spline.DefaultHandleOffsetFactor;

                changed = true;
            }

            if (changed)
                EditorUtility.SetDirty(_spline);
        }

        public void DropSelectedNodesToSurface()
        {
            if (_selector.SelectedCount == 0) return;

            Undo.RecordObject(_spline, "Drop Nodes to Surface");

            bool changed = false;
            foreach (int index in _selector.SelectedNodeIndices)
            {
                Vector3 worldPos = _spline.GetNodeWorldPosition(index);
                Ray ray = new Ray(worldPos, Vector3.down);

                if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
                {
                    _spline.nodes[index].position = _spline.transform.InverseTransformPoint(hit.point);
                    changed = true;
                }
            }

            if (changed)
                EditorUtility.SetDirty(_spline);
        }

        public void ResetHandles()
        {
            if (_selector.SelectedCount == 0 && !_selector.SelectedHandle.HasValue) return;

            Undo.RecordObject(_spline, "Reset Handles");

            bool changed = false;

            if (_selector.SelectedHandle.HasValue)
            {
                var hs = _selector.SelectedHandle.Value;
                var segment = _spline.segments[hs.segmentIndex];

                Vector3 startPos = _spline.nodes[segment.startNodeIndex].position;
                Vector3 endPos = _spline.nodes[segment.endNodeIndex].position;
                Vector3 delta = endPos - startPos;

                if (hs.isStart)
                    segment.startHandleOffset = delta * Spline.DefaultHandleOffsetFactor;
                else
                    segment.endHandleOffset = -delta * Spline.DefaultHandleOffsetFactor;

                changed = true;
            }
            else
            {
                foreach (var segment in _spline.segments)
                {
                    bool startSelected = _selector.IsNodeSelected(segment.startNodeIndex);
                    bool endSelected = _selector.IsNodeSelected(segment.endNodeIndex);

                    if (!startSelected && !endSelected)
                        continue;

                    Vector3 startPos = _spline.nodes[segment.startNodeIndex].position;
                    Vector3 endPos = _spline.nodes[segment.endNodeIndex].position;
                    Vector3 delta = endPos - startPos;

                    if (startSelected)
                        segment.startHandleOffset = delta * Spline.DefaultHandleOffsetFactor;

                    if (endSelected)
                        segment.endHandleOffset = -delta * Spline.DefaultHandleOffsetFactor;

                    changed = true;
                }
            }

            if (changed)
                EditorUtility.SetDirty(_spline);
        }

        public void SmoothHandles()
        {
            if (_selector.SelectedCount == 0) return;
            
            Undo.RecordObject(_spline, "Smooth Handles");

            bool changed = false;

            var nodesToSmooth = new HashSet<int>(_selector.SelectedNodeIndices);

            foreach (int nodeIndex in nodesToSmooth)
            {
                var segmentCount = _spline.segments.Where(x => x.startNodeIndex == nodeIndex ||  x.endNodeIndex == nodeIndex).Count();

                if (segmentCount != 2) continue;
                
                if (TryGetSmoothDirection(nodeIndex, out Vector3 smoothDir))
                {
                    ApplySmoothDirection(nodeIndex, smoothDir);
                    changed = true;
                }
            }

            if (changed)
                EditorUtility.SetDirty(_spline);
        }

        private bool TryGetSmoothDirection(int nodeIndex, out Vector3 direction)
        {
            direction = Vector3.zero;
            Vector3 nodePos = _spline.nodes[nodeIndex].position;

            var neighbors = new List<Vector3>();

            foreach (var segment in _spline.segments)
            {
                if (segment.startNodeIndex == nodeIndex)
                    neighbors.Add(_spline.nodes[segment.endNodeIndex].position);
                else if (segment.endNodeIndex == nodeIndex)
                    neighbors.Add(_spline.nodes[segment.startNodeIndex].position);
            }

            if (neighbors.Count == 0) return false;
            
            if (neighbors.Count == 2)
            {
                direction = neighbors[1] - neighbors[0];
                if (direction.sqrMagnitude < 0.0001f) return false;
                direction.Normalize();
                return true;
            }
            
            foreach (var n in neighbors)
            {
                Vector3 d = n - nodePos;
                if (d.sqrMagnitude > 0.0001f)
                    direction += d.normalized;
            }

            if (direction.sqrMagnitude < 0.0001f) return false;
            direction.Normalize();
            return true;
        }

        private void ApplySmoothDirection(int nodeIndex, Vector3 smoothDir)
        {
            Vector3 nodePos = _spline.nodes[nodeIndex].position;

            foreach (var segment in _spline.segments)
            {
                bool isStart = segment.startNodeIndex == nodeIndex;
                bool isEnd = segment.endNodeIndex == nodeIndex;
                if (!isStart && !isEnd) continue;

                int otherIndex = isStart ? segment.endNodeIndex : segment.startNodeIndex;
                Vector3 toOther = _spline.nodes[otherIndex].position - nodePos;
                
                Vector3 handleDir = Vector3.Dot(toOther, smoothDir) >= 0f ? smoothDir : -smoothDir;

                float length = isStart
                    ? segment.startHandleOffset.magnitude
                    : segment.endHandleOffset.magnitude;

                if (length < 0.0001f)
                    length = toOther.magnitude * Spline.DefaultHandleOffsetFactor;

                if (isStart)
                    segment.startHandleOffset = handleDir * length;
                else
                    segment.endHandleOffset = handleDir * length;
            }
        }
    }
}