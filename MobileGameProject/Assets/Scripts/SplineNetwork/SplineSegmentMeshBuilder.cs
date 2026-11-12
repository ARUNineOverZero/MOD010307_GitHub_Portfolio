using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MobileGameProject.SplineNetwork
{
    public static class SplineSegmentMeshBuilder
    {
        public struct PieceInfo
        {
            public Vector3 start;
            public Vector3 forward;
            public float length;
        }

        public struct CurveInfo
        {
            public Vector3 p0, p1, p2, p3;
            public float length;
            public Quaternion startRotation;
            public Quaternion endRotation;
        }

        public static Mesh Build(Spline spline, SplineSegment segment, int lod)
        {
            if (segment.prefab == null) return null;
            if (!TryGetPieceInfo(segment.prefab, out PieceInfo piece)) return null;
            if (!TryGetSourceMesh(segment.prefab, out var source)) return null;
            if (!TryGetCurve(spline, segment, lod, out var curve)) return null;

            int count = Mathf.Max(1, Mathf.CeilToInt(curve.length / piece.length));

            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var tris = new List<int>();

            for (int i = 0; i < count; i++)
                AppendDeformedPiece(source, piece, curve, i, verts, normals, tris);

            return CreateMesh(verts, normals, tris);
        }

        private static Mesh CreateMesh(List<Vector3> verts, List<Vector3> normals, List<int> tris)
        {
            if (verts.Count == 0) return null;

            Mesh mesh = new Mesh { name = "Segment" };
            mesh.SetVertices(verts);

            if (normals.Count == verts.Count)
                mesh.SetNormals(normals);

            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();

            if (normals.Count != verts.Count)
                mesh.RecalculateNormals();

            return mesh;
        }

        public static bool TryGetPieceInfo(SplineNodePrefab prefab, out PieceInfo info)
        {
            info = default;

            var startSocket = prefab.sockets.FirstOrDefault(s => s.id == "start");
            var endSocket = prefab.sockets.FirstOrDefault(s => s.id == "end");
            if (startSocket == null || endSocket == null) return false;

            Vector3 dir = endSocket.localPosition - startSocket.localPosition;
            float len = dir.magnitude;
            if (len < 0.001f) return false;

            info.start = startSocket.localPosition;
            info.forward = dir / len;
            info.length = len;
            return true;
        }

        public static bool TryGetSourceMesh(SplineNodePrefab prefab, out Mesh mesh)
        {
            mesh = null;
            var meshFilter = prefab.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null) return false;
            mesh = meshFilter.sharedMesh;
            return true;
        }

        public static bool TryGetCurve(Spline spline, SplineSegment segment, int lod, out CurveInfo curve)
        {
            curve = default;

            curve.p0 = GetSegmentEndLocalPosition(spline, segment.startNodeIndex, segment.startSocket);
            curve.p3 = GetSegmentEndLocalPosition(spline, segment.endNodeIndex, segment.endSocket);
            curve.p1 = curve.p0 + segment.startHandleOffset;
            curve.p2 = curve.p3 + segment.endHandleOffset;

            curve.startRotation = spline.nodes[segment.startNodeIndex].rotation;
            curve.endRotation = spline.nodes[segment.endNodeIndex].rotation;

            curve.length = SplineMath.EstimateCurveLength(curve.p0, curve.p1, curve.p2, curve.p3, lod);
            return curve.length >= 0.001f;
        }

        private static Vector3 GetSegmentEndLocalPosition(Spline spline, int nodeIndex, string socketId)
        {
            var node = spline.nodes[nodeIndex];
            Vector3 pos = node.position;

            if (node.prefab == null || string.IsNullOrEmpty(socketId))
                return pos;

            var socket = node.prefab.sockets.FirstOrDefault(s => s.id == socketId);
            if (socket == null) return pos;

            return pos + node.rotation * socket.localPosition;
        }

        public static void AppendDeformedPiece(
            Mesh source,
            PieceInfo piece,
            CurveInfo curve,
            int pieceIndex,
            List<Vector3> verts,
            List<Vector3> normals,
            List<int> tris)
        {
            Vector3[] srcVerts = source.vertices;
            Vector3[] srcNormals = source.normals;
            int[] srcTris = source.triangles;

            float distBase = pieceIndex * piece.length;
            int baseIndex = verts.Count;

            for (int v = 0; v < srcVerts.Length; v++)
            {
                Vector3 deformed = DeformVertex(srcVerts[v], piece, curve, distBase);
                verts.Add(deformed);

                if (srcNormals != null && srcNormals.Length == srcVerts.Length)
                    normals.Add(DeformNormal(srcNormals[v], piece, curve, distBase, srcVerts[v]));
            }

            for (int i = 0; i < srcTris.Length; i++)
                tris.Add(srcTris[i] + baseIndex);
        }

        public static Vector3 DeformVertex(Vector3 vert, PieceInfo piece, CurveInfo curve, float distBase)
        {
            float along = Vector3.Dot(vert - piece.start, piece.forward);
            float u = Mathf.Clamp01(along / piece.length);
            Vector3 lateral = vert - piece.start - piece.forward * along;

            float t = Mathf.Clamp01((distBase + u * piece.length) / curve.length);

            Vector3 pathPos = SplineMath.EvaluateCubicBezier(curve.p0, curve.p1, curve.p2, curve.p3, t);
            SplineMath.GetFrame(curve.p0, curve.p1, curve.p2, curve.p3, t, curve.startRotation, curve.endRotation, out Vector3 tangent, out Vector3 normal, out Vector3 binormal);

            return pathPos + normal * lateral.y + binormal * lateral.x;
        }

        public static Vector3 DeformNormal(Vector3 normal, PieceInfo piece, CurveInfo curve, float distBase, Vector3 vert)
        {
            float along = Vector3.Dot(vert - piece.start, piece.forward);
            float u = Mathf.Clamp01(along / piece.length);
            float t = Mathf.Clamp01((distBase + u * piece.length) / curve.length);

            SplineMath.GetFrame(curve.p0, curve.p1, curve.p2, curve.p3, t, curve.startRotation, curve.endRotation, out Vector3 tangent, out Vector3 n, out Vector3 binormal);
            return (n * normal.y + binormal * normal.x + tangent * normal.z).normalized;
        }
    }
}
