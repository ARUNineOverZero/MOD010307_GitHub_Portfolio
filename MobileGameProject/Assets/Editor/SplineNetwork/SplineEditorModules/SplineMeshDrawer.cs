using System;
using System.Linq;
using Codice.CM.Common.Matcher;
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
            SplineNodePrefab prefab = segment.prefab;

            NodeSocket startSocket = prefab.sockets.FirstOrDefault(s => s.id == "start");
            NodeSocket endSocket = prefab.sockets.FirstOrDefault(s => s.id == "end");
            if (startSocket == null || endSocket == null) return;

            float pieceLength = Vector3.Distance(startSocket.localPosition, endSocket.localPosition);
            if (pieceLength < 0.001f) return;

            MeshFilter mf = prefab.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) return;

            Vector3 p0 = _spline.GetSegmentEndWorldPosition(segment.startNodeIndex, segment.startSocket);
            Vector3 p3 = _spline.GetSegmentEndWorldPosition(segment.endNodeIndex, segment.endSocket);
            Vector3 p1 = p0 + _spline.transform.TransformVector(segment.startHandleOffset);
            Vector3 p2 = p3 + _spline.transform.TransformVector(segment.endHandleOffset);

            float curveLength = EstimateCurveLength(p0, p1, p2, p3, SplineEditorSettings.segmentLevelOfDetail);
            int count = Mathf.Max(1, Mathf.CeilToInt(curveLength / pieceLength));

            Vector3 localStart = startSocket.localPosition;
            Vector3 pieceForward = (endSocket.localPosition - startSocket.localPosition).normalized;

            CombineInstance[] combines = new CombineInstance[count];

            for (int i = 0; i < count; i++)
            {
                //working out 
            }

            if (_segmentMeshes[index] != null)
                GameObject.DestroyImmediate(_segmentMeshes[index]);

            Mesh mesh = new Mesh { name = "Segment_" + index };
            mesh.CombineMeshes(combines, true, true);
            _segmentMeshes[index] = mesh;
        }

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
    }
}