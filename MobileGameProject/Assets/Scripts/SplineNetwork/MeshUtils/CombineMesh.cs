using System.Collections.Generic;
using UnityEngine;

namespace MobileGameProject.SplineNetwork.MeshUtils
{
    public class CombineMesh : MonoBehaviour
    {
        public Mesh CombineChildren()
        {
            List<MeshFilter> filters = new List<MeshFilter>();
            GetComponentsInChildren(filters);

            filters.RemoveAll(f => f == null || f.sharedMesh == null || f.gameObject == gameObject);
            if (filters.Count == 0) return null;

            CombineInstance[] combines = new CombineInstance[filters.Count];

            for (int i = 0; i < filters.Count; i++)
            {
                MeshFilter f = filters[i];
                combines[i] = new CombineInstance
                {
                    mesh = f.sharedMesh,
                    transform = transform.worldToLocalMatrix * f.transform.localToWorldMatrix
                };
            }

            Mesh combined = new Mesh { name = gameObject.name + "_Combined" };
            combined.CombineMeshes(combines, true, true);
            combined.RecalculateBounds();
            return combined;
        }
    }
}
