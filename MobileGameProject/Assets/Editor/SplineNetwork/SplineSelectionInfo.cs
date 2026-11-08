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

            if (!EditorGUI.EndChangeCheck()) return;

            Undo.RecordObject(_spline, "Edit Selection");

            if (newLocalPos != localPos)
            {
                Vector3 worldDelta = _spline.transform.TransformVector(newLocalPos - localPos);
                ApplyNodePositionDelta(worldDelta);
            }

            if (_selector.SelectededCount == 1)
            {
                var node = _spline.nodes[_selector.SelectedNodeIndices.First()];
                node.rotation = Quaternion.Euler(newEuler);
            }

            EditorUtility.SetDirty(_spline);
        }

        private void DrawHandleOffset()
        {
            var hs = _selector.SelectedHandle.Value;
            var segment = _spline.segments[hs.segmentIndex];

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