using System.Collections.Generic;
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

        public void AddNode(Vector3 worldPosition)
        {
            nodes.Add(new SplineNode() { position = worldPosition});
        }

        public void AddSegment(int startIndex, int endIndex)
        {
            if(startIndex == endIndex 
            || startIndex < 0 
            || endIndex < 0 
            || startIndex >= nodes.Count 
            || endIndex >= nodes.Count)
                return;

                var seg = new SplineSegment()
                {
                };
                
                segments.Add(seg);
            }
        }
    }
}

