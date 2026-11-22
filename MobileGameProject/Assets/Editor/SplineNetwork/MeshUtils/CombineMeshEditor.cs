using MobileGameProject.SplineNetwork.MeshUtils;
using UnityEditor;
using UnityEngine;

namespace MobileGameProjectEditor.SplineNetwork
{
    [CustomEditor(typeof(CombineMesh))]
    public class CombineMeshEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(8);

            if (GUILayout.Button("Combine Children", GUILayout.Height(30)))
            {
                CombineMesh combiner = (CombineMesh)target;
                Mesh mesh = combiner.CombineChildren();
                if (mesh == null)
                    return;
                

                string path = EditorUtility.SaveFilePanelInProject(
                    "Save Combined Mesh",
                    combiner.gameObject.name + "_Combined",
                    "asset",
                    "Choose where to save the combined mesh");

                if (string.IsNullOrEmpty(path)) return;

                AssetDatabase.CreateAsset(mesh, path);
                AssetDatabase.SaveAssets();

                MeshFilter meshFilter = combiner.GetComponent<MeshFilter>();
                if (meshFilter == null) meshFilter = combiner.gameObject.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = mesh;

                MeshRenderer meshRenderer = combiner.GetComponent<MeshRenderer>();
                if (meshRenderer == null) meshRenderer = combiner.gameObject.AddComponent<MeshRenderer>();
            }
        }
    }
}