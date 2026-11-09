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
        public Quaternion rotation;
        public SplineNodePrefab prefab;
    }

    [System.Serializable]
    public class SplineSegment
    {
        public int startNodeIndex;
        public string startSocket;
        public int endNodeIndex;
        public string endSocket;
        public Vector3 startHandleOffset;
        public Vector3 endHandleOffset;
        public SplineNodePrefab prefab;
    }

    public class Spline : MonoBehaviour
    {
        public List<SplineNode> nodes = new List<SplineNode>();
        public List<SplineSegment> segments = new List<SplineSegment>();

        public  const float DefaultHandleOffsetFactor = .33f;

        public void Clear()
        {
            nodes.Clear();
            segments.Clear();
        }

        #region Nodes
        
        public Vector3 GetNodeWorldPosition(int index) => transform.TransformPoint(nodes[index].position);

        public Vector3 GetSegmentEndWorldPosition(int nodeIndex, string socketId)
        {
            Vector3 nodeWorld = GetNodeWorldPosition(nodeIndex);
            var node = nodes[nodeIndex];

            if (node.prefab == null || string.IsNullOrEmpty(socketId))
                return nodeWorld;

            var socket = node.prefab.sockets.FirstOrDefault(s => s.id == socketId);
            if (socket == null)
                return nodeWorld;

            Quaternion nodeWorldRot = transform.rotation * node.rotation;
            return nodeWorld + nodeWorldRot * socket.localPosition;
        }

        public int AddNode(Vector3 localPosition, params int[] connectedNodeIndeices)
        {
            int newIndex = nodes.Count;
            var rotation = Quaternion.identity;
            bool hasConnectedNodes = connectedNodeIndeices != null & connectedNodeIndeices.Length > 0;
            
            if(hasConnectedNodes)
            {
                Vector3 averageDirection = Vector3.zero;
                int valid = 0;

                foreach (var index in connectedNodeIndeices)
                {
                    if (index < 0 || index >= nodes.Count) continue;

                    Vector3 direction = localPosition - nodes[index].position;
                    if (direction.sqrMagnitude > .0001f)
                    {
                        averageDirection += direction.normalized;
                        valid++;
                    }
                }
                
                if (valid > 0 && averageDirection.sqrMagnitude > .0001f)
                    rotation = Quaternion.LookRotation(averageDirection.normalized, Vector3.up);
            }
            
            nodes.Add(new SplineNode() { position = localPosition, rotation = rotation });
            
            if (hasConnectedNodes)
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
