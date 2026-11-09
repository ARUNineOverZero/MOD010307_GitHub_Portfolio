using System;
using Codice.CM.Common.Matcher;
using MobileGameProject.SplineNetwork;
using UnityEditor;
using UnityEngine;

namespace MobileGameProjectEditor.SplineNetwork
{
    public class SplineMeshDrawer
    {
        private readonly Spline _spline;

        public SplineMeshDrawer(Spline spline)
        {
            _spline = spline;
        }

        public void DrawSceneGUI()
        {
            DrawNodePrefabse();
        }

        private void DrawNodePrefabse()
        {
            for(int i = 0; i < _spline.nodes.Count; i++)
            {
                var node = _spline.nodes[i];
                if(node.prefab == null) continue;

                Vector3 worldPosition = _spline.GetNodeWorldPosition(i);
                Quaternion worldRotation = _spline.transform.rotation * node.rotation;

                DrawPrefabMesh(node.prefab, worldPosition, worldRotation, _spline.transform.lossyScale);
            }
        }

        private void DrawPrefabMesh(SplineNodePrefab prefab, Vector3 worldPosition, Quaternion worldRotation, Vector3 scale)
        {
            var mf = prefab.GetComponent<MeshFilter>();

            if(mf == null || mf.sharedMesh == null) return;

            var mr = prefab.GetComponent<MeshRenderer>();
            if(mr == null) return;

            Material mat = mr.sharedMaterial;
            if(mat == null) return; 

            Matrix4x4 matrix = Matrix4x4.TRS(worldPosition, worldRotation, scale);
            Camera camera = SceneView.currentDrawingSceneView.camera;

            Graphics.DrawMesh(mf.sharedMesh, matrix, mat, 0, camera);
        }
    }
}