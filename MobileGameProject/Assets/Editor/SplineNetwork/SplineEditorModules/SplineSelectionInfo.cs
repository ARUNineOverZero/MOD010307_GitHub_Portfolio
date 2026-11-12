using System.Collections.Generic;
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

                var selectedNodeIndices = _selector.SelectedNodeIndices.ToArray();

                if (selectedNodeIndices.Length == 1)
                {
                    SplineNode node = _spline.nodes[selectedNodeIndices[0]];
                    node.rotation = Quaternion.Euler(newEuler);
                }

                _spline.SetNodeSegmentsDirty(selectedNodeIndices);
                EditorUtility.SetDirty(_spline);
            }

            DrawPrefabField();
        }

        private void DrawPrefabField()
        {
            if (_selector.SelectedCount == 1 && !_selector.SelectedHandle.HasValue)
            {
                SplineNode node = _spline.nodes[_selector.SelectedNodeIndices.First()];

                EditorGUI.BeginChangeCheck();
                SplineNodePrefab prefab = (SplineNodePrefab)EditorGUILayout.ObjectField("Node Prefab", node.prefab, typeof(SplineNodePrefab), false);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_spline, "Set Node Prefab");
                    node.prefab = prefab;
                    _spline.SetSegmentsDirty(0);
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
                    segment.isDirty = true;
                    EditorUtility.SetDirty(_spline);
                }

                DrawSegmentSockets(segment);
            }
        }

        private void DrawHandleOffset()
        {
            HandleSelection hs = _selector.SelectedHandle.Value;
            SplineSegment segment = _spline.segments[hs.segmentIndex];

            int nodeIndex = hs.isStart ? segment.startNodeIndex : segment.endNodeIndex;
            string socketId = hs.isStart ? segment.startSocket : segment.endSocket;

            Vector3 stored = hs.isStart ? segment.startHandleOffset : segment.endHandleOffset;
            Quaternion handleSpace;
            var node = _spline.nodes[nodeIndex];
            
            if (_spline.TryGetSocket(nodeIndex, socketId, out NodeSocket socket))
                handleSpace = node.rotation * socket.localRotation;
            else
                handleSpace = node.rotation;

            Vector3 socketLocal = Quaternion.Inverse(handleSpace) * stored;

            void SetZeroWhenLessThan(ref float value, float epsilon = 0.0001f)
            {
                if (Mathf.Abs(value) < epsilon)
                    value = 0;
            }

            SetZeroWhenLessThan(ref socketLocal.x);
            SetZeroWhenLessThan(ref socketLocal.y);
            SetZeroWhenLessThan(ref socketLocal.z);

            EditorGUI.BeginChangeCheck();
            Vector3 newSocketLocal = EditorGUILayout.Vector3Field("Handle Offset", socketLocal);

            if (GUILayout.Button("Zero X and Y", GUILayout.Height(30)))
            {
                newSocketLocal.x = 0;
                newSocketLocal.y = 0;
            }

            if (!EditorGUI.EndChangeCheck() && socketLocal == newSocketLocal) return;

            Undo.RecordObject(_spline, "Edit Handle Offset");

            Vector3 newStored = handleSpace * newSocketLocal;

            if (hs.isStart)
                segment.startHandleOffset = newStored;
            else
                segment.endHandleOffset = newStored;

            segment.isDirty = true;
            EditorUtility.SetDirty(_spline);
        }

        private void DrawSegmentSockets(SplineSegment segment)
        {
            var startNode = _spline.nodes[segment.startNodeIndex];
            var endNode = _spline.nodes[segment.endNodeIndex];

            DrawSocketPopup("Start Socket", startNode.prefab, segment.startSocket, value =>
            {
                Undo.RecordObject(_spline, "Set Start Socket");
                segment.startSocket = value;
                EditorUtility.SetDirty(_spline);
            });

            DrawSocketPopup("End Socket", endNode.prefab, segment.endSocket, value =>
            {
                Undo.RecordObject(_spline, "Set End Socket");
                segment.endSocket = value;
                EditorUtility.SetDirty(_spline);
            });
        }

        private void DrawSocketPopup(string label, SplineNodePrefab prefab, string currentId, System.Action<string> onChanged)
        {
            var options = new List<string> { "None" };

            if (prefab != null)
            {
                foreach (var socket in prefab.sockets)
                {
                    if (!string.IsNullOrEmpty(socket.id) && !options.Contains(socket.id))
                        options.Add(socket.id);
                }
            }

            int currentIndex = 0;
            if (!string.IsNullOrEmpty(currentId))
            {
                int found = options.IndexOf(currentId);
                currentIndex = found >= 0 ? found : 0;
            }

            int newIndex = EditorGUILayout.Popup(label, currentIndex, options.ToArray());
            string newValue = newIndex <= 0 ? null : options[newIndex];

            if (newValue != currentId)
                onChanged?.Invoke(newValue);
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