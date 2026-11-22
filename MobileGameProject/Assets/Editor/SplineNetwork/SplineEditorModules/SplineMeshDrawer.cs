using System.Collections.Generic;
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
            DrawNodePrefabs();
            DrawSegments();
        }

        private void DrawNodePrefabs()
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

            Camera camera = SceneView.currentDrawingSceneView?.camera;

            for (int i = 0; i < _spline.segments.Count; i++)
            {
                SplineSegment segment = _spline.segments[i];
                if (segment.prefab == null) continue;

                if (segment.isDirty || _segmentMeshes[i] == null)
                {
                    if (_segmentMeshes[i] != null)
                        Object.DestroyImmediate(_segmentMeshes[i]);

                    _segmentMeshes[i] = SplineSegmentMeshBuilder.Build(
                        _spline, segment, SplineEditorSettings.segmentLevelOfDetail);

                    segment.isDirty = false;
                }

                if (_segmentMeshes[i] == null) continue;

                MeshRenderer meshRenderer = segment.prefab.GetComponent<MeshRenderer>();
                if (meshRenderer == null || meshRenderer.sharedMaterial == null) continue;

                Graphics.DrawMesh(
                    _segmentMeshes[i],
                    _spline.transform.localToWorldMatrix,
                    meshRenderer.sharedMaterial,
                    0,
                    camera);
            }
        }

        private void DrawPrefabMesh(SplineNodePrefab prefab, Vector3 worldPosition, Quaternion worldRotation, Vector3 scale)
        {
            MeshFilter meshFilter = prefab.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null) return;

            MeshRenderer meshRenderer = prefab.GetComponent<MeshRenderer>();
            if (meshRenderer == null || meshRenderer.sharedMaterial == null) return;

            Matrix4x4 matrix = Matrix4x4.TRS(worldPosition, worldRotation, scale);
            Camera camera = SceneView.currentDrawingSceneView.camera;

            Graphics.DrawMesh(meshFilter.sharedMesh, matrix, meshRenderer.sharedMaterial, 0, camera);
        }

        public Mesh GenerateCombinedMesh()
        {
            List<CombineInstance> combines = new List<CombineInstance>();

            for (int i = 0; i < _spline.nodes.Count; i++)
            {
                SplineNode node = _spline.nodes[i];
                if (node.prefab == null) continue;

                MeshFilter meshFilter = node.prefab.GetComponent<MeshFilter>();
                if (meshFilter == null || meshFilter.sharedMesh == null) continue;

                Vector3 worldPos = _spline.GetNodeWorldPosition(i);
                Quaternion worldRot = _spline.transform.rotation * node.rotation;
                Vector3 scale = _spline.transform.lossyScale;

                combines.Add(new CombineInstance
                {
                    mesh = meshFilter.sharedMesh,
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
                        transform = _spline.transform.localToWorldMatrix
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
            for (int i = 0; i < _spline.nodes.Count; i++)
            {
                if (_spline.nodes[i].prefab == null) continue;
                MeshRenderer meshRenderer = _spline.nodes[i].prefab.GetComponent<MeshRenderer>();
                if (meshRenderer != null && meshRenderer.sharedMaterial != null)
                    return meshRenderer.sharedMaterial;
            }

            for (int i = 0; i < _spline.segments.Count; i++)
            {
                if (_spline.segments[i].prefab == null) continue;
                MeshRenderer meshRenderer = _spline.segments[i].prefab.GetComponent<MeshRenderer>();
                if (meshRenderer != null && meshRenderer.sharedMaterial != null)
                    return meshRenderer.sharedMaterial;
            }

            return null;
        }
    }
}