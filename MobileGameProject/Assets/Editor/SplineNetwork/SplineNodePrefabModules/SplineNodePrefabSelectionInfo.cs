using System.Linq;
using MobileGameProject.SplineNetwork;
using UnityEditor;
using UnityEngine;

namespace MobileGameProjectEditor.SplineNetwork
{
    public class SplineNodePrefabSelectionInfo
    {
        private readonly SplineNodePrefab _splineNodePrefab;
        private readonly SplineNodePrefabSelector _selector;
        private readonly SplineNodePrefabManipulator _manipulator;

        public SplineNodePrefabSelectionInfo(SplineNodePrefab splineNodePrefab, SplineNodePrefabManipulator manipulator, SplineNodePrefabSelector selector)
        {
            _splineNodePrefab = splineNodePrefab;
            _manipulator = manipulator;
            _selector = selector;
        }

        public void DrawGUI()
        {
            if (!_manipulator.HasSelectionPivot) return;

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Selection", EditorStyles.boldLabel);

            Vector3 localPos = _splineNodePrefab.transform.InverseTransformPoint(_manipulator.SelectionPivotPosition);
            Quaternion localRot = Quaternion.Inverse(_splineNodePrefab.transform.rotation) * _manipulator.SelectionPivotRotation;

            EditorGUI.BeginChangeCheck();

            NodeSocket socket = null;
            string id = string.Empty;

            if(_selector.SelectedCount == 1)
            {
                socket = _splineNodePrefab.sockets[_selector.SelectedSocketIndices.First()];
                id = EditorGUILayout.TextField("ID", socket.id);
            }

            Vector3 newLocalPos = EditorGUILayout.Vector3Field("Position", localPos);
            Vector3 newEuler = EditorGUILayout.Vector3Field("Rotation", localRot.eulerAngles);

            if (!EditorGUI.EndChangeCheck()) return;

            Undo.RecordObject(_splineNodePrefab, "Edit Selection");

            if (newLocalPos != localPos)
            {
                Vector3 worldDelta = _splineNodePrefab.transform.TransformVector(newLocalPos - localPos);
                ApplyNodePositionDelta(worldDelta);
            }

            if (socket != null)
            {
                socket.id = id;
                var node = _splineNodePrefab.sockets[_selector.SelectedSocketIndices.First()];
                node.localRotation = Quaternion.Euler(newEuler);
            }

            EditorUtility.SetDirty(_splineNodePrefab);
        }

        private void ApplyNodePositionDelta(Vector3 worldDelta)
        {
            foreach (int index in _selector.SelectedSocketIndices)
            {
                Vector3 worldPosition = _splineNodePrefab.GetSocketWorldPosition(index);
                _splineNodePrefab.sockets[index].localPosition = _splineNodePrefab.transform.InverseTransformPoint(worldPosition + worldDelta);
            }
        }
    }
}