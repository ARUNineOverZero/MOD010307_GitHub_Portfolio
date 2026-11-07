using System.Linq;
using MobileGameProject.SplineNetwork;
using UnityEditor;
using UnityEngine;

namespace MobileGameProjectEditor.SplineNetwork
{
    public class SplineManipulator
    {
        private readonly Spline _spline;
        private readonly SplineSelector _selector;

        public SplineManipulator(Spline spline, SplineSelector selector)
        {
            _spline = spline;
            _selector = selector;
        }

        public void HandleMovement()
        {
            if (_selector.SelectededCount == 0) return;

            if (_selector.SelectedHandle.HasValue)
                HandleNodeHandleMovement();
            else
                HandleNodeMovement();
        }

        public void HandleRotation()
        {
            if (_selector.SelectededCount == 0) return;

            if (!_selector.SelectedHandle.HasValue)
                HandleNodeRotation();
        }

        private void HandleNodeMovement()
        {
            Vector3 worldCenter = Vector3.zero;
            foreach (int index in _selector.SelectedNodeIndices)
                worldCenter += GetSplineWorldPosition(index);

            worldCenter /= _selector.SelectededCount;

            int pivotIndex = _selector.SelectedNodeIndices.First();
            Quaternion handleRot = GetHandleRotation(pivotIndex);

            Vector3 newWorldCenter = Handles.PositionHandle(worldCenter, handleRot);
            if (newWorldCenter != worldCenter)
            {
                Vector3 delta = newWorldCenter - worldCenter;

                Undo.RecordObject(_spline, "Move Spline Nodes");
                foreach (int index in _selector.SelectedNodeIndices)
                {
                    Vector3 worldPos = GetSplineWorldPosition(index);
                    Vector3 newWorldPos = worldPos + delta;
                    _spline.nodes[index].position = _spline.transform.InverseTransformPoint(newWorldPos);
                }
                EditorUtility.SetDirty(_spline);
            }
        }

        private void HandleNodeHandleMovement()
        {
            var segmentHandle = _selector.SelectedHandle.Value;
            var segment = _spline.segments[segmentHandle.segmentIndex];
            int nodeIndex = segmentHandle.isStart ? segment.startNodeIndex : segment.endNodeIndex;

            Vector3 worldPos = GetSplineWorldPosition(nodeIndex);
            Vector3 localOffset = segmentHandle.isStart ? segment.startHandleOffset : segment.endHandleOffset;
            Vector3 handleWorld = worldPos + _spline.transform.TransformVector(localOffset);

            Quaternion handleRot = GetHandleRotation(nodeIndex);

            Vector3 newHandleWorld = Handles.PositionHandle(handleWorld, handleRot);
            if (newHandleWorld != handleWorld)
            {
                Undo.RecordObject(_spline, "Move Spline Handle");

                Vector3 worldOffset = newHandleWorld - worldPos;
                Vector3 newLocalOffset = _spline.transform.InverseTransformVector(worldOffset);

                if (segmentHandle.isStart)
                    segment.startHandleOffset = newLocalOffset;
                else
                    segment.endHandleOffset = newLocalOffset;

                EditorUtility.SetDirty(_spline);
            }
        }

        private void HandleNodeRotation()
        {
            if (_selector.SelectededCount != 1) return;

            int index = _selector.SelectedNodeIndices.First();
            var node = _spline.nodes[index];

            Vector3 worldPos = GetSplineWorldPosition(index);
            Quaternion worldRot = _spline.transform.rotation * node.rotation;

            Quaternion newWorldRot = Handles.RotationHandle(worldRot, worldPos);
            if (newWorldRot != worldRot)
            {
                Undo.RecordObject(_spline, "Rotate Spline Node");
                node.rotation = Quaternion.Inverse(_spline.transform.rotation) * newWorldRot;
                EditorUtility.SetDirty(_spline);
            }
        }

        private Vector3 GetSplineWorldPosition(int index) =>
            _spline.transform.TransformPoint(_spline.nodes[index].position);

        private Quaternion GetHandleRotation(int nodeIndex = -1)
        {
            if (Tools.pivotRotation == PivotRotation.Global)
                return Quaternion.identity;

            if (nodeIndex == -1)
                return _spline.transform.rotation;

            var node = _spline.nodes[nodeIndex];
            return _spline.transform.rotation * node.rotation;
        }
    }
}