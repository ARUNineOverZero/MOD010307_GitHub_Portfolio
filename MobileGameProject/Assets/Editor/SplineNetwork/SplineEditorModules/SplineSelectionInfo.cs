using System.Linq;
using MobileGameProject.SplineNetwork;
using UnityEditor;
using UnityEngine;

namespace MobileGameProjectEditor.SplineNetwork
{
    public class SplineSelectionInfo
    {
        private readonly Spline _spline;
        private readonly SplineManipulator _manipulator;
        private readonly SplineSelector _selector;

        public SplineSelectionInfo(Spline spline, SplineManipulator manipulator, SplineSelector selector)
        {
            _spline = spline;
            _manipulator = manipulator;
            _selector = selector;
        }

        public void DrawGUI()
        {
            if (!_manipulator.HasSelectionPivot) return;

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Selection", EditorStyles.boldLabel);

            if (_selector.SelectedHandle.HasValue)
            {
                DrawHandleOffset();
                return;
            }

            Vector3 localPos = _spline.transform.InverseTransformPoint(_manipulator.SelectionPivotPosition);
            Quaternion localRot = Quaternion.Inverse(_spline.transform.rotation) * _manipulator.SelectionPivotRotation;

            EditorGUI.BeginChangeCheck();
            Vector3 newLocalPos = EditorGUILayout.Vector3Field("Position", localPos);
            Vector3 newEuler = EditorGUILayout.Vector3Field("Rotation", localRot.eulerAngles);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_spline, "Edit Selection");

                if (newLocalPos != localPos)
                {
                    Vector3 worldDelta = _spline.transform.TransformVector(newLocalPos - localPos);
                    ApplyNodePositionDelta(worldDelta);
                }

                if (_selector.SelectedCount == 1)
                {
                    SplineNode node = _spline.nodes[_selector.SelectedNodeIndices.First()];
                    node.rotation = Quaternion.Euler(newEuler);
                }

                EditorUtility.SetDirty(_spline);
            }

            DrawPrefabField();
        }

        private void DrawPrefabField()
        {
            Debug.Log($"Entered DrawPrefabField");
            if (_selector.SelectedCount == 1 && !_selector.SelectedHandle.HasValue)
            {
                Debug.Log($"Draw Node DrawPrefabField");
                SplineNode node = _spline.nodes[_selector.SelectedNodeIndices.First()];

                EditorGUI.BeginChangeCheck();
                SplineNodePrefab prefab = (SplineNodePrefab)EditorGUILayout.ObjectField("Node Prefab", node.prefab, typeof(SplineNodePrefab), false);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_spline, "Set Node Prefab");
                    node.prefab = prefab;
                    EditorUtility.SetDirty(_spline);
                }
                return;
            }

            if (_selector.SelectedCount == 2)
            {
                SplineSegment segment = _spline.segments.FirstOrDefault(s =>
                    _selector.IsNodeSelected(s.startNodeIndex) &&
                    _selector.IsNodeSelected(s.endNodeIndex));

                if (segment == null) return;

                EditorGUI.BeginChangeCheck();
                SplineNodePrefab prefab = (SplineNodePrefab)EditorGUILayout.ObjectField("Segment Prefab", segment.prefab, typeof(SplineNodePrefab), false);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_spline, "Set Segment Prefab");
                    segment.prefab = prefab;
                    EditorUtility.SetDirty(_spline);
                }
            }
        }

        private void DrawHandleOffset()
        {
            HandleSelection hs = _selector.SelectedHandle.Value;
            SplineSegment segment = _spline.segments[hs.segmentIndex];

            Vector3 offset = hs.isStart ? segment.startHandleOffset : segment.endHandleOffset;

            EditorGUI.BeginChangeCheck();
            Vector3 newOffset = EditorGUILayout.Vector3Field("Handle Offset", offset);

            if (!EditorGUI.EndChangeCheck()) return;

            Undo.RecordObject(_spline, "Edit Handle Offset");

            if (hs.isStart)
                segment.startHandleOffset = newOffset;
            else
                segment.endHandleOffset = newOffset;

            EditorUtility.SetDirty(_spline);
        }

        private void ApplyNodePositionDelta(Vector3 worldDelta)
        {
            foreach (int index in _selector.SelectedNodeIndices)
            {
                Vector3 worldPos = _spline.GetNodeWorldPosition(index);
                _spline.nodes[index].position = _spline.transform.InverseTransformPoint(worldPos + worldDelta);
            }
        }
    }
}