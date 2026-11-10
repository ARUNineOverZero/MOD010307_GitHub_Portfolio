using System.Collections.Generic;
using System.Linq;
using MobileGameProject.SplineNetwork;
using UnityEditor;
using UnityEngine;

namespace MobileGameProjectEditor.SplineNetwork
{
    public class SplineMeshDrawer
    {
        private readonly Spline _spline;
        private Mesh[] _segmentMeshes;

        public SplineMeshDrawer(Spline spline)
        {
            _spline = spline;
        }

        public void DrawSceneGUI()
        {
            DrawNodePrefabse();
            DrawSegments();
        }

        private void DrawNodePrefabse()
        {
            for (int i = 0; i < _spline.nodes.Count; i++)
            {
                SplineNode node = _spline.nodes[i];
                if (node.prefab == null) continue;

                Vector3 worldPosition = _spline.GetNodeWorldPosition(i);
                Quaternion worldRotation = _spline.transform.rotation * node.rotation;

                DrawPrefabMesh(node.prefab, worldPosition, worldRotation, _spline.transform.lossyScale);
            }
        }

        private void DrawSegments()
        {
            if (_spline.segments.Count == 0) return;

            if (_segmentMeshes == null || _segmentMeshes.Length != _spline.segments.Count)
                _segmentMeshes = new Mesh[_spline.segments.Count];

            Camera cam = SceneView.currentDrawingSceneView?.camera;

            for (int i = 0; i < _spline.segments.Count; i++)
            {
                SplineSegment segment = _spline.segments[i];
                if (segment.prefab == null) continue;

                if (segment.isDirty || _segmentMeshes[i] == null)
                {
                    RebuildSegmentMesh(i);
                    segment.isDirty = false;
                }

                if (_segmentMeshes[i] == null) continue;

                MeshRenderer mr = segment.prefab.GetComponent<MeshRenderer>();
                if (mr == null || mr.sharedMaterial == null) continue;

                Graphics.DrawMesh(_segmentMeshes[i], Matrix4x4.identity, mr.sharedMaterial, 0, cam);
            }
        }

        #region Segment Calc

        private void RebuildSegmentMesh(int index)
        {
            SplineSegment segment = _spline.segments[index];
            if (segment.prefab == null) return;

            if (!TryGetPieceInfo(segment.prefab, out PieceInfo piece)) return;
            if (!TryGetSourceMesh(segment.prefab, out Mesh source)) return;
            if (!TryGetCurve(segment, out CurveInfo curve)) return;

            int count = Mathf.Max(1, Mathf.CeilToInt(curve.length / piece.length));

            var allVerts = new List<Vector3>();
            var allNormals = new List<Vector3>();
            var allTris = new List<int>();

            for (int pieceIndex = 0; pieceIndex < count; pieceIndex++)
                AppendDeformedPiece(source, piece, curve, pieceIndex, allVerts, allNormals, allTris);

            AssignSegmentMesh(index, allVerts, allNormals, allTris);
        }

        #region Segment Mesh Generation

        private struct PieceInfo
        {
            public Vector3 start;
            public Vector3 forward;
            public float length;
        }

        private struct CurveInfo
        {
            public Vector3 p0, p1, p2, p3;
            public float length;
            public Quaternion startRotation;
            public Quaternion endRotation;
        }

        private bool TryGetPieceInfo(SplineNodePrefab prefab, out PieceInfo info)
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

        private bool TryGetSourceMesh(SplineNodePrefab prefab, out Mesh mesh)
        {
            mesh = null;
            var mf = prefab.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) return false;
            mesh = mf.sharedMesh;
            return true;
        }

        private bool TryGetCurve(SplineSegment segment, out CurveInfo curve)
        {
            curve = default;

            curve.p0 = _spline.GetSegmentEndWorldPosition(segment.startNodeIndex, segment.startSocket);
            curve.p3 = _spline.GetSegmentEndWorldPosition(segment.endNodeIndex, segment.endSocket);
            curve.p1 = curve.p0 + _spline.transform.TransformVector(segment.startHandleOffset);
            curve.p2 = curve.p3 + _spline.transform.TransformVector(segment.endHandleOffset);

            curve.startRotation = _spline.nodes[segment.startNodeIndex].rotation;
            curve.endRotation = _spline.nodes[segment.endNodeIndex].rotation;

            curve.length = EstimateCurveLength(curve.p0, curve.p1, curve.p2, curve.p3, SplineEditorSettings.segmentLevelOfDetail);
            return curve.length >= 0.001f;
        }

        private void AppendDeformedPiece(
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

        private Vector3 DeformVertex(Vector3 vert, PieceInfo piece, CurveInfo curve, float distBase)
        {
            float along = Vector3.Dot(vert - piece.start, piece.forward);
            float u = Mathf.Clamp01(along / piece.length);
            Vector3 lateral = vert - piece.start - piece.forward * along;

            float t = Mathf.Clamp01((distBase + u * piece.length) / curve.length);

            Vector3 pathPos = EvaluateCubicBezier(curve.p0, curve.p1, curve.p2, curve.p3, t);
            GetFrame(curve, t, out Vector3 tangent, out Vector3 normal, out Vector3 binormal);

            return pathPos + normal * lateral.y + binormal * lateral.x;
        }

        private Vector3 DeformNormal(Vector3 normal, PieceInfo piece, CurveInfo curve, float distBase, Vector3 vert)
        {
            float along = Vector3.Dot(vert - piece.start, piece.forward);
            float u = Mathf.Clamp01(along / piece.length);
            float t = Mathf.Clamp01((distBase + u * piece.length) / curve.length);

            GetFrame(curve, t, out Vector3 tangent, out Vector3 n, out Vector3 binormal);
            return (n * normal.y + binormal * normal.x + tangent * normal.z).normalized;
        }

        private void GetFrame(CurveInfo curve, float t, out Vector3 tangent, out Vector3 normal, out Vector3 binormal)
        {
            tangent = EvaluateCubicBezierTangent(curve.p0, curve.p1, curve.p2, curve.p3, t).normalized;
            if (tangent.sqrMagnitude < 0.0001f)
                tangent = Vector3.forward;

            Quaternion nodeRot = Quaternion.Slerp(curve.startRotation, curve.endRotation, t);
            Vector3 up = nodeRot * Vector3.up;

            if (Mathf.Abs(Vector3.Dot(tangent, up)) > 0.99f)
                up = nodeRot * Vector3.forward;

            binormal = Vector3.Cross(tangent, up).normalized;
            normal = Vector3.Cross(binormal, tangent).normalized;
        }

        private void AssignSegmentMesh(int index, List<Vector3> verts, List<Vector3> normals, List<int> tris)
        {
            if (_segmentMeshes[index] != null)
                GameObject.DestroyImmediate(_segmentMeshes[index]);

            Mesh mesh = new Mesh { name = "Segment_" + index };
            mesh.SetVertices(verts);
            if (normals.Count == verts.Count)
                mesh.SetNormals(normals);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            if (normals.Count != verts.Count)
                mesh.RecalculateNormals();

            _segmentMeshes[index] = mesh;
        }



        #endregion Segment Mesh Generation

        private float EstimateCurveLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int steps = 32)
        {
            float length = 0f;
            Vector3 prev = p0;
            for (int i = 1; i <= steps; i++)
            {
                float t = i / (float)steps;
                Vector3 point = EvaluateCubicBezier(p0, p1, p2, p3, t);
                length += Vector3.Distance(prev, point);
                prev = point;
            }
            return length;
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

        private static Vector3 EvaluateCubicBezierTangent(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float u = 1f - t;
            return 3f * u * u * (p1 - p0)
                 + 6f * u * t * (p2 - p1)
                 + 3f * t * t * (p3 - p2);
        }

        #endregion Segment Calc

        private void DrawPrefabMesh(SplineNodePrefab prefab, Vector3 worldPosition, Quaternion worldRotation, Vector3 scale)
        {
            MeshFilter mf = prefab.GetComponent<MeshFilter>();

            if (mf == null || mf.sharedMesh == null) return;

            MeshRenderer mr = prefab.GetComponent<MeshRenderer>();
            if (mr == null) return;

            Material mat = mr.sharedMaterial;
            if (mat == null) return;

            Matrix4x4 matrix = Matrix4x4.TRS(worldPosition, worldRotation, scale);
            Camera cam = SceneView.currentDrawingSceneView.camera;

            Graphics.DrawMesh(mf.sharedMesh, matrix, mat, 0, cam);
        }

        public Mesh GenerateCombinedMesh()
        {
            var combines = new List<CombineInstance>();

            for (int i = 0; i < _spline.nodes.Count; i++)
            {
                var node = _spline.nodes[i];
                if (node.prefab == null) continue;

                var mf = node.prefab.GetComponent<MeshFilter>();
                if (mf == null || mf.sharedMesh == null) continue;

                Vector3 worldPos = _spline.GetNodeWorldPosition(i);
                Quaternion worldRot = _spline.transform.rotation * node.rotation;
                Vector3 scale = _spline.transform.lossyScale;

                combines.Add(new CombineInstance
                {
                    mesh = mf.sharedMesh,
                    transform = Matrix4x4.TRS(worldPos, worldRot, scale)
                });
            }

            if (_segmentMeshes != null)
            {
                for (int i = 0; i < _segmentMeshes.Length; i++)
                {
                    if (_segmentMeshes[i] == null) continue;

                    combines.Add(new CombineInstance
                    {
                        mesh = _segmentMeshes[i],
                        transform = Matrix4x4.identity
                    });
                }
            }

            if (combines.Count == 0) return null;

            Mesh combined = new Mesh { name = "SplineCombined" };
            combined.CombineMeshes(combines.ToArray(), true, true);
            combined.RecalculateBounds();
            return combined;
        }

        public Material GetFirstMaterial()
        {
            bool TryGetMaterialFromPrefab(GameObject gameObject, out Material material)
            {
                material = null;
                var meshRenderer = gameObject.GetComponent<MeshRenderer>();
                if (meshRenderer == null || meshRenderer.sharedMaterial == null) return false;


                material = meshRenderer.sharedMaterial;
                return true;
            }

            Material material;

            for (int i = 0; i < _spline.nodes.Count; i++)
                if (_spline.nodes[i].prefab != null && TryGetMaterialFromPrefab(_spline.nodes[i].prefab.gameObject, out material))
                    return material;

            for (int i = 0; i < _spline.segments.Count; i++)
                if (_spline.segments[i].prefab != null && TryGetMaterialFromPrefab(_spline.segments[i].prefab.gameObject, out material))
                    return material;

            return null;
        }
    }
}