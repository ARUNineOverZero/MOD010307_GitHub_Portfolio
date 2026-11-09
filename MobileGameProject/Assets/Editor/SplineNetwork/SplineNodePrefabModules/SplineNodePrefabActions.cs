using MobileGameProject.SplineNetwork;
using UnityEngine;

namespace MobileGameProjectEditor.SplineNetwork
{
    public class SplineNodePrefabActions
    {
        private readonly SplineNodePrefab _splineNodePrefab;
        private readonly SplineNodePrefabSelector _selector;
        public SplineNodePrefabActions(SplineNodePrefab splineNodePrefab, SplineNodePrefabSelector selector)
        {
            _splineNodePrefab = splineNodePrefab;
            _selector = selector;
        }

        private void ResetPivot()
        {
            if (_selector.SelectedCount != 1) return;

            var socket = _splineNodePrefab.sockets[_selector.SelectedSocketIndices.First()];
            if (socket.localPosition == Vector3.zero && socket.localRotation == Quaternion.identity) return;

            var mf = _splineNodePrefab.GetComponent<MeshFilter>();
            if (mf?.sharedMesh == null) return;

            var mesh = mf.sharedMesh;
            var invMatrix = Matrix4x4.TRS(socket.localPosition, socket.localRotation, Vector3.one).inverse;
            var invRot = Quaternion.Inverse(socket.localRotation);

            var vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
                vertices[i] = invMatrix.MultiplyPoint3x4(vertices[i]);
            mesh.vertices = vertices;

            var normals = mesh.normals;
            if (normals is { Length: > 0 })
            {
                for (int i = 0; i < normals.Length; i++)
                    normals[i] = invRot * normals[i];
                mesh.normals = normals;
            }

            var tangents = mesh.tangents;
            if (tangents is { Length: > 0 })
            {
                for (int i = 0; i < tangents.Length; i++)
                {
                    var t = invRot * (Vector3)tangents[i];
                    tangents[i] = new Vector4(t.x, t.y, t.z, tangents[i].w);
                }
                mesh.tangents = tangents;
            }

            mesh.RecalculateBounds();

            var tform = _splineNodePrefab.transform;
            tform.position = tform.TransformPoint(socket.localPosition);
            tform.rotation *= socket.localRotation;

            socket.localPosition = Vector3.zero;
            socket.localRotation = Quaternion.identity;

            var mc = _splineNodePrefab.GetComponent<MeshCollider>();
            if (mc != null)
            {
                mc.sharedMesh = null;
                mc.sharedMesh = mesh;
            }
        }
    }
}