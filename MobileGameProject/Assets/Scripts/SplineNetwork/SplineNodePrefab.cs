using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace MobileGameProject.SplineNetwork
{
    [System.Serializable]
    public class NodeSocket
    {
        public string id = "Socket";
        public Vector3 localPosition;
        public Quaternion localRotation;
    }

    public class SplineNodePrefab : MonoBehaviour
    {
        public List<NodeSocket> sockets = new List<NodeSocket>();

        public int AddSocket(Vector3 localPos)
        {
            int newIndex = sockets.Count;
            sockets.Add(new NodeSocket() { localPosition = localPos, localRotation = quaternion.identity});
            return newIndex;
        }

        public Vector3 GetSocketWorldPosition(int index)
        {
            if (index < 0 || index >= sockets.Count)
                return transform.position;

            return transform.TransformPoint(sockets[index].localPosition);
        }

        public Quaternion GetSocketWorldRotation(int index)
        {
            if (index < 0 || index >= sockets.Count)
                return transform.rotation;

            return transform.rotation * sockets[index].localRotation;
        }

        public void RemoveAllSocketsAt(int[] indices)
        {
            if (indices == null || indices.Length == 0) return;

            foreach(var i in indices.OrderByDescending(x => x))
                RemoveSocketAt(i);
        }

        private void RemoveSocketAt(int index)
        {
            if (index < 0 || index >= sockets.Count) return;

            sockets.RemoveAt(index);
        }
    }
}
