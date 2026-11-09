using System;
using System.Linq;
using MobileGameProject.SplineNetwork;
using UnityEditor;
using UnityEngine;

namespace MobileGameProjectEditor.SplineNetwork
{
    public class SplineNodePrefabManipulator
    {
        private readonly SplineNodePrefab _splineNodePrefab;
        private readonly SplineNodePrefabSelector _selector;
        private Vector3 _selectionPivotPosition;
        public Vector3 SelectionPivotPosition => _selectionPivotPosition;
        private Quaternion _selectionPivotRotation;
        public Quaternion SelectionPivotRotation => _selectionPivotRotation;
        private bool _hasSelectionPivot;
        public bool HasSelectionPivot => _hasSelectionPivot;

        public SplineNodePrefabManipulator(SplineNodePrefab splineNodePrefab, SplineNodePrefabSelector selector)
        {
            _splineNodePrefab = splineNodePrefab;
            _selector = selector;
        }

        public void HandleMovement()
        {
            if (_selector.SelectedCount == 0) return;

            if (!_hasSelectionPivot) return;

            Quaternion handleRot = GetHandleRotation(_selector.SelectedCount == 1 ? _selector.SelectedSocketIndices.First() : -1);

            Vector3 newWorldCenter = Handles.PositionHandle(_selectionPivotPosition, handleRot);
            if (newWorldCenter != _selectionPivotPosition)
            {
                Vector3 delta = newWorldCenter - _selectionPivotPosition;

                Undo.RecordObject(_splineNodePrefab, "Move Spline Prefab Socket");
                foreach (int index in _selector.SelectedSocketIndices)
                {
                    Vector3 worldPosition = _splineNodePrefab.GetSocketWorldPosition(index);
                    _splineNodePrefab.sockets[index].localPosition = _splineNodePrefab.transform.InverseTransformPoint(worldPosition + delta);
                }
                EditorUtility.SetDirty(_splineNodePrefab);
            }
        }

        public void HandleRotation()
        {

        }

        private Quaternion GetHandleRotation(int socketIndex = -1)
        {
            if (Tools.pivotRotation == PivotRotation.Global)
                return Quaternion.identity;

            if (socketIndex == -1)
                return _splineNodePrefab.transform.rotation;

            var socket = _splineNodePrefab.sockets[socketIndex];
            return _splineNodePrefab.transform.rotation * socket.localRotation;
        }

        public void RefreshPivot()
        {
            _hasSelectionPivot = false;
            if (_selector.SelectedCount == 0) return;

            if (_selector.SelectedCount == 1)
            {
                var index = _selector.SelectedSocketIndices.First();
                _selectionPivotPosition = _splineNodePrefab.GetSocketWorldPosition(index);
                _selectionPivotRotation = GetHandleRotation(index);
            }
            else
            {
                Vector3 center = Vector3.zero;
                foreach (int index in _selector.SelectedSocketIndices)
                    center += _splineNodePrefab.GetSocketWorldPosition(index);
                center /= _selector.SelectedCount;

                _selectionPivotPosition = center;
                _selectionPivotRotation = GetHandleRotation();
            }

            _hasSelectionPivot = true;
        }
    }
}
