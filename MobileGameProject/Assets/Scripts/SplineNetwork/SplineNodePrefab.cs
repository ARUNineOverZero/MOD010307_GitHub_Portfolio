using System.Collections.Generic;
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
    }
}
