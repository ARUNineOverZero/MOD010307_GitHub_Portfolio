using System.Linq;
using MobileGameProject.SplineNetwork;
using UnityEditor;
using UnityEngine;

namespace MobileGameProjectEditor.SplineNetwork
{
    public class SplineSelectionInfo
    {
        private readonly Spline _spline;
        private readonly SplineSelector _selector;

        public SplineSelectionInfo(Spline spline, SplineSelector selector)
        {
            _spline = spline;
            _selector = selector;
        }

        public void DrawGUI()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Selection (Local to Spline)", EditorStyles.boldLabel);

            if (_selector.SelectedHandle.HasValue)
            {
                DrawHandleInfo();
                return;
            }

            if (_selector.SelectededCount == 1)
                DrawNodeInfo();
            else if (_selector.SelectededCount > 1)
                DrawNodesInfo();
        }

        private void DrawNodeInfo()
        {
            int index = _selector.SelectedNodeIndices.First();
            var node = _spline.nodes[index];

            EditorGUI.BeginChangeCheck();
            Vector3 newPos = EditorGUILayout.Vector3Field("Position", node.position);
            Vector3 newEuler = EditorGUILayout.Vector3Field("Rotation", node.rotation.eulerAngles);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_spline, "Edit Node");
                node.position = newPos;
                node.rotation = Quaternion.Euler(newEuler);
                EditorUtility.SetDirty(_spline);
            }
        }
        
        private void DrawNodesInfo()
        {
            var nodes = _selector.SelectedNodeIndices.Select(x => _spline.nodes[x]).ToArray();
            Vector3 center = Vector3.zero;

            foreach (SplineNode node in nodes)
                center += node.position;

            center /= nodes.Length;

            EditorGUI.BeginChangeCheck();
            Vector3 newCenter = EditorGUILayout.Vector3Field("Position", center);
            Vector3 newEuler = EditorGUILayout.Vector3Field("Rotation", Quaternion.identity.eulerAngles);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_spline, "Edit Node");

                var delta = newCenter - center;
                
                foreach (SplineNode node in nodes)
                    node.position += delta;
                
                EditorUtility.SetDirty(_spline);
            }
        }

        private void DrawHandleInfo()
        {
            var selectedHandle = _selector.SelectedHandle.Value;
            var segment = _spline.segments[selectedHandle.segmentIndex];

            Vector3 offset = selectedHandle.isStart ? segment.startHandleOffset : segment.endHandleOffset;

            EditorGUI.BeginChangeCheck();
            Vector3 newOffset = EditorGUILayout.Vector3Field("Handle Offset", offset);
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_spline, "Move Handle");
                if (selectedHandle.isStart)
                    segment.startHandleOffset = newOffset;
                else
                    segment.endHandleOffset = newOffset;
                
                EditorUtility.SetDirty(_spline);
            }

            using (var gui = new EditorGUI.DisabledGroupScope(true))
                EditorGUILayout.Vector3Field("Node Rotation (Eular)", Quaternion.identity.eulerAngles);
        }
    }
}