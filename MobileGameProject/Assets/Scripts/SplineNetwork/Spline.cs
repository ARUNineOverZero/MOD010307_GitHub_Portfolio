using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MobileGameProject.SplineNetwork
{
    [System.Serializable]
    public class SplineNode
    {
        public Vector3 position;
    }

    [System.Serializable]
    public class SplineSegment
    {
        public int startNodeIndex;
        public int endNodeIndex;
        public Vector3 startHandleOffset;
        public Vector3 endHandleOffset;
    }

    public class Spline : MonoBehaviour
    {
        public List<SplineNode> nodes = new List<SplineNode>();
        public List<SplineSegment> segments = new List<SplineSegment>();

        private const float DefaultHandleOffsetFactor = .33f;

        public void Clear()
        {
            nodes.Clear();
            segments.Clear();
        }

        #region Nodes

        public int AddNode(Vector3 worldPosition, params int[] connectedNodeIndeices)
        {
            int newIndex = nodes.Count;
            nodes.Add(new SplineNode() { position = worldPosition });

            if(connectedNodeIndeices == null || connectedNodeIndeices.Length == 0) return newIndex;

            foreach(var index in connectedNodeIndeices)
                AddSegment(index, newIndex);

            return newIndex;
        }

        public void RemoveNodeAt(int index)
        {
            if (index < 0 || index >= nodes.Count) return;

            segments.RemoveAll(s => s.startNodeIndex == index || s.endNodeIndex == index);

            nodes.RemoveAt(index);

            foreach (var segment in segments)
            {
                if (segment.startNodeIndex > index)
                    segment.startNodeIndex--;

                if (segment.endNodeIndex > index)
                    segment.endNodeIndex--;
            }
        }

        public bool RemoveNode(SplineNode node)
        {
            int index = nodes.IndexOf(node);
            if (index < 0) return false;

            RemoveNodeAt(index);
            return true;
        }

        public void RemoveAllNodesAt(int[] indices)
        {
            if (indices == null || indices.Length == 0) return;

            foreach(var i in indices.OrderByDescending(x => x))
                RemoveNodeAt(i);
        }

        public int RemoveAllNodes(Predicate<SplineNode> predicate)
        {
            if (predicate == null) return 0;

            var indicesToRemove = new List<int>();
            for (int i = 0; i < nodes.Count; i++)
                if (predicate(nodes[i]))
                    indicesToRemove.Add(i);

            if (indicesToRemove.Count == 0) return 0;

            indicesToRemove.Sort((a, b) => b.CompareTo(a));
            foreach (int index in indicesToRemove)
                RemoveNodeAt(index);

            return indicesToRemove.Count;
        }

        #endregion Nodes

        #region Segments

        public void AddSegment(int startIndex, int endIndex)
        {
            if (startIndex == endIndex
            || startIndex < 0
            || endIndex < 0
            || startIndex >= nodes.Count
            || endIndex >= nodes.Count)
                return;

            var seg = new SplineSegment()
            {
                startNodeIndex = startIndex,
                endNodeIndex = endIndex,
                startHandleOffset = (nodes[endIndex].position - nodes[startIndex].position) * DefaultHandleOffsetFactor,
                endHandleOffset = (nodes[startIndex].position - nodes[endIndex].position) * DefaultHandleOffsetFactor
            };

            segments.Add(seg);
        }

        public void RemoveSegmentAt(int index)
        {
            segments.RemoveAt(index);
        }

        public bool RemoveSegment(SplineSegment segment)
        {
            return segments.Remove(segment);
        }

        public int RemoveAllSegments(Predicate<SplineSegment> predicate)
        {
            return segments.RemoveAll(predicate);
        }

        #endregion Segments
    }
}
