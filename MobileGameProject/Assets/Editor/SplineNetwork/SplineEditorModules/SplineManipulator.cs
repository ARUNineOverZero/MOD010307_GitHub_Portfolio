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
        private Vector3 _selectionPivotPosition;
        public Vector3 SelectionPivotPosition => _selectionPivotPosition;
        private Quaternion _selectionPivotRotation;
        public Quaternion SelectionPivotRotation => _selectionPivotRotation;
        private bool _hasSelectionPivot;
        public bool HasSelectionPivot => _hasSelectionPivot;

        public SplineManipulator(Spline spline, SplineSelector selector)
        {
            _spline = spline;
            _selector = selector;
        }

        public void HandleMovement()
        {
            if (_selector.SelectedCount == 0) return;

            if (_selector.SelectedHandle.HasValue)
                HandleNodeHandleMovement();
            else
                HandleNodeMovement();
        }

        public void HandleRotation()
        {
            if (_selector.SelectedCount == 0) return;

            if (!_selector.SelectedHandle.HasValue)
                HandleNodeRotation();
        }

        private void HandleNodeMovement()
        {
            if (!_hasSelectionPivot) return;

            Quaternion handleRot = GetHandleRotation(_selector.SelectedCount == 1 ? _selector.SelectedNodeIndices.First() : -1);

            Vector3 newWorldCenter = Handles.PositionHandle(_selectionPivotPosition, handleRot);
            if (newWorldCenter != _selectionPivotPosition)
            {
                Vector3 delta = newWorldCenter - _selectionPivotPosition;

                Undo.RecordObject(_spline, "Move Spline Nodes");
                var selectedNodeIndices = _selector.SelectedNodeIndices.ToArray();
                foreach (int index in selectedNodeIndices)
                {
                    Vector3 worldPos = _spline.GetNodeWorldPosition(index);
                    _spline.nodes[index].position = _spline.transform.InverseTransformPoint(worldPos + delta);
                }
                _spline.SetNodeSegmentsDirty(selectedNodeIndices);
                EditorUtility.SetDirty(_spline);
            }
        }

        private void HandleNodeHandleMovement()
        {
            if (!_hasSelectionPivot) return;

            var selectedHandle = _selector.SelectedHandle.Value;
            var segment = _spline.segments[selectedHandle.segmentIndex];
            int nodeIndex = selectedHandle.isStart ? segment.startNodeIndex : segment.endNodeIndex;
            Vector3 nodeWorld = _spline.GetNodeWorldPosition(nodeIndex);

            Vector3 newHandleWorld = Handles.PositionHandle(_selectionPivotPosition, _selectionPivotRotation);
            if (newHandleWorld != _selectionPivotPosition)
            {
                Undo.RecordObject(_spline, "Move Spline Handle");

                Vector3 worldOffset = newHandleWorld - nodeWorld;
                Vector3 newLocalOffset = _spline.transform.InverseTransformVector(worldOffset);

                if (selectedHandle.isStart)
                    segment.startHandleOffset = newLocalOffset;
                else
                    segment.endHandleOffset = newLocalOffset;

                segment.isDirty = true;
                EditorUtility.SetDirty(_spline);
            }
        }

        private void HandleNodeRotation()
        {
            if (_selector.SelectedCount != 1) return;

            int index = _selector.SelectedNodeIndices.First();
            var node = _spline.nodes[index];

            Vector3 worldPos = GetSplineWorldPosition(index);
            Quaternion worldRot = _selectionPivotRotation;

            Quaternion newWorldRot = Handles.RotationHandle(worldRot, worldPos);
            if (newWorldRot != worldRot)
            {
                Undo.RecordObject(_spline, "Rotate Spline Node");
                _spline.SetNodeSegmentsDirty(index);
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

        public void RefreshPivot()
        {
            _hasSelectionPivot = false;
            if (_selector.SelectedCount == 0 && !_selector.SelectedHandle.HasValue) return;

            if (_selector.SelectedHandle.HasValue)
            {
                var hs = _selector.SelectedHandle.Value;
                var segment = _spline.segments[hs.segmentIndex];
                int nodeIndex = hs.isStart ? segment.startNodeIndex : segment.endNodeIndex;
                string socketId = hs.isStart ? segment.startSocket : segment.endSocket;
                Vector3 localOffset = hs.isStart ? segment.startHandleOffset : segment.endHandleOffset;

                Vector3 origin = _spline.GetSegmentEndWorldPosition(nodeIndex, socketId);
                Quaternion socketRotation = _spline.GetSocketWorldRotation(nodeIndex, socketId);

                _selectionPivotPosition = origin + _spline.transform.TransformVector(localOffset);
                _selectionPivotRotation = GetHandleRotation(nodeIndex);
                _hasSelectionPivot = true;
                return;
            }

            if (_selector.SelectedCount == 1)
            {
                var index = _selector.SelectedNodeIndices.First();
                _selectionPivotPosition = _spline.GetNodeWorldPosition(index);
                _selectionPivotRotation = GetHandleRotation(index);
            }
            else
            {
                Vector3 center = Vector3.zero;
                foreach (int index in _selector.SelectedNodeIndices)
                    center += _spline.GetNodeWorldPosition(index);
                center /= _selector.SelectedCount;

                _selectionPivotPosition = center;

                _selectionPivotRotation = GetHandleRotation();
            }

            _hasSelectionPivot = true;
        }
    }
}