using System.Collections.Generic;
using System.Linq;
using MobileGameProject.SplineNetwork;
using UnityEditor;
using UnityEngine;

namespace MobileGameProjectEditor.SplineNetwork
{
    [CustomEditor(typeof(Spline))]
    public class SplineEditor : Editor
    {
        #region Fields

        private Spline _spline;
        private HashSet<int> _selectedNodeIndices = new HashSet<int>();
        private int _hoverNodeIndex = -1;
        private const float HoverTestRadiusFactor = 0.6f;
        private const float HoverPixelThreshold = 8f;
        private bool _isEditing = false;

        private SplineDrawer _drawer;
        private SplineSelector _selector;
        private SplineManipulator _manipulator;

        private HandleSelection? _selectedHandle = null;
        private int _hoverHandleSegment = -1;
        private bool _hoverHandleIsStart;

        #endregion Fields

        #region Unity Callbacks

        private void OnEnable()
        {
            _spline = (Spline)target;
            _drawer = new SplineDrawer();
            _selector = new SplineSelector();
            _manipulator = new SplineManipulator();
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnDisable()
        {
            Tools.hidden = false;
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        private void OnSceneGUI()
        {
            if (_spline == null) return;

            Event e = Event.current;

            UpdateHover();
            UpdateHandleHover();
            _drawer.DrawNodes(_spline, _selectedNodeIndices, _hoverNodeIndex);
            //DrawHandles();
            //DrawSegments();

            if (_isEditing)
            {
                Tools.hidden = true;

                if (e.control)
                {
                    HandleCreateInput(e);
                }
                else
                {
                    switch (Tools.current)
                    {
                        case Tool.Move:
                            HandleMovement();
                            break;
                        case Tool.Rotate:
                            HandleRotation();
                            break;
                        default:
                            break;
                    }

                    HandleSelectionInput(e);
                }
            }
            else
            {
                Tools.hidden = false;
            }

            _drawer.RequestRepaint(e);
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space(8);
            DrawEditGUI();
            GUI.backgroundColor = Color.white;
        }

        #endregion Unity Callbacks

        #region Inspector

        private void DrawEditGUI()
        {
            GUI.backgroundColor = _isEditing ? Color.green : Color.white;
            if (GUILayout.Button(_isEditing ? "Exit Edit Mode" : "Enter Edit Mode", GUILayout.Height(30)))
            {
                _isEditing = !_isEditing;
                Tools.hidden = _isEditing;
                _selectedNodeIndices.Clear();
                SceneView.RepaintAll();
            }

            if (_isEditing)
            {
                GUI.backgroundColor = Color.white;

                EditorGUILayout.LabelField("Spline Edit", EditorStyles.boldLabel);
                EditorGUILayout.Space(4);

                EditorGUILayout.LabelField("Node Edit", EditorStyles.boldLabel);
                EditorGUILayout.Space(4);

                if (GUILayout.Button("Delete Selected", GUILayout.Height(30)))
                    DeleteSelectedNodes();

                EditorGUILayout.LabelField("Segment Edit", EditorStyles.boldLabel);
                EditorGUILayout.Space(4);

                if (GUILayout.Button("Create Segmment", GUILayout.Height(30)))
                    CreateSegmment();

                if (GUILayout.Button("Delete Segment", GUILayout.Height(30)))
                    DeleteSegmment();

                if (GUILayout.Button("Clear Spline Data", GUILayout.Height(30)))
                    ClearSplineData();
            }
        }

        private void DeleteSelectedNodes()
        {
            if (_selectedNodeIndices.Count == 0) return;

            Undo.RecordObject(_spline, "Delete Spline Nodes");

            _spline.RemoveAllNodesAt(_selectedNodeIndices.ToArray());
            _selectedNodeIndices.Clear();
            EditorUtility.SetDirty(_spline);
            SceneView.RepaintAll();
        }

        private void CreateSegmment()
        {
            if (_selectedNodeIndices.Count != 2) return;

            var segment = _spline.segments.FirstOrDefault(x => _selectedNodeIndices.Contains(x.startNodeIndex) && _selectedNodeIndices.Contains(x.endNodeIndex));
            if (segment == null)
            {
                Undo.RecordObject(_spline, "Create Spline Segment");

                var indices = _selectedNodeIndices.ToArray();
                _spline.AddSegment(indices[0], indices[1]);
            }

            EditorUtility.SetDirty(_spline);
            SceneView.RepaintAll();
        }

        private void DeleteSegmment()
        {
            if (_selectedNodeIndices.Count != 2) return;

            var segment = _spline.segments.FirstOrDefault(x => _selectedNodeIndices.Contains(x.startNodeIndex) && _selectedNodeIndices.Contains(x.endNodeIndex));
            if (segment == null) return;

            Undo.RecordObject(_spline, "Delete Spline Segment");

            _spline.RemoveSegment(segment);

            EditorUtility.SetDirty(_spline);
            SceneView.RepaintAll();
        }

        private void ClearSplineData()
        {
            Undo.RecordObject(_spline, "Clear Spline Data");

            _spline.Clear();
            _selectedNodeIndices.Clear();
            EditorUtility.SetDirty(_spline);
            SceneView.RepaintAll();
        }

        #endregion Inspector

        #region Helpers

        private bool TryGetClickedWorldPosition(Event e, out Vector3 worldPos)
        {
            var cam = SceneView.currentDrawingSceneView.camera;
            var position = e.mousePosition;

            position.y = cam.scaledPixelHeight - position.y;
            var ray = cam.ScreenPointToRay(position);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                worldPos = hit.point;
                return true;
            }

            worldPos = Vector3.zero;
            return false;
        }

        private Vector3 GetSplineWorldPosition(int index) => _spline.transform.TransformPoint(_spline.nodes[index].position);

        private Quaternion GetHandleRotation(int nodeIndex = -1)
        {
            if (Tools.pivotRotation == PivotRotation.Global)
                return Quaternion.identity;
            
            if (nodeIndex == -1)
                return _spline.transform.rotation;

            var node = _spline.nodes[nodeIndex];
            return _spline.transform.rotation * node.rotation;
        }

        #endregion Helpers

        #region Create

        private void HandleCreateInput(Event e)
        {
            if (e.type != EventType.MouseDown || e.button != 0) return;

            if (_selectedHandle.HasValue)
                _selectedHandle = null;
            
            if (TryGetClickedWorldPosition(e, out Vector3 worldPos))
            {
                CreateNodeAt(worldPos);
                e.Use();
                Repaint();
            }
        }

        private void CreateNodeAt(Vector3 worldPos)
        {
            Undo.RecordObject(_spline, "Add Spline Node");

            var localpos = _spline.transform.InverseTransformPoint(worldPos);
            int newIndex = _spline.AddNode(localpos, _selectedNodeIndices.ToArray());

            _selectedNodeIndices.Clear();
            _selectedNodeIndices.Add(newIndex);

            EditorUtility.SetDirty(_spline);
        }

        #endregion Create

        #region Editing

        private void HandleMovement()
        {
            if (_selectedNodeIndices.Count == 0) return;

            if (_selectedHandle.HasValue)
                HandleNodeHandleMovement();
            else
                HandleNodeMovement();
        }

        private void HandleNodeMovement()
        {
            Vector3 worldCenter = Vector3.zero;
            foreach (int index in _selectedNodeIndices)
                worldCenter += GetSplineWorldPosition(index);

            worldCenter /= _selectedNodeIndices.Count;
            
            int pivotIndex = _selectedNodeIndices.First();
            Quaternion handleRot = GetHandleRotation(pivotIndex);

            Vector3 newWorldCenter = Handles.PositionHandle(worldCenter, handleRot);
            if (newWorldCenter != worldCenter)
            {
                Vector3 delta = newWorldCenter - worldCenter;

                Undo.RecordObject(_spline, "Move Spline Nodes");
                foreach (int index in _selectedNodeIndices)
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
            var segmentHandle = _selectedHandle.Value;
            var segment = _spline.segments[segmentHandle.segmentIndex];
            var nodeIndex = segmentHandle.isStart ? segment.startNodeIndex : segment.endNodeIndex;
            Vector3 worldPos = GetSplineWorldPosition(nodeIndex);
            Vector3 handleWorld = worldPos + _spline.transform.TransformVector(segmentHandle.isStart ? segment.startHandleOffset : segment.endHandleOffset);
            Quaternion handleRot = GetHandleRotation();
            Vector3 newHandleWorld = Handles.PositionHandle(handleWorld, handleRot);
            if (newHandleWorld != handleWorld)
            {
                Undo.RecordObject(_spline, "Move Spline Nodes Handle");

                Vector3 worldOffset = newHandleWorld - worldPos;
                Vector3 localOffset = _spline.transform.InverseTransformVector(worldOffset);

                if (segmentHandle.isStart)
                    segment.startHandleOffset = localOffset;
                else
                    segment.endHandleOffset = localOffset;

                EditorUtility.SetDirty(_spline);
            }
        }

        private void HandleRotation()
        {
            if (_selectedNodeIndices.Count == 0) return;

            if (!_selectedHandle.HasValue)
                HandleNodeRotation();
        }

        private void HandleNodeRotation()
        {
            if (_selectedNodeIndices.Count != 1) return;

            int index = _selectedNodeIndices.First();
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

        #endregion Editing

        #region Selection

        private void HandleSelectionInput(Event e)
        {
            if (e.type != EventType.MouseDown || e.button != 0) return;

            if (_hoverHandleSegment != -1)
            {
                _selectedHandle = new HandleSelection
                {
                    segmentIndex = _hoverHandleSegment,
                    isStart = _hoverHandleIsStart
                };
                e.Use();
                Repaint();
                return;
            }

            if (_hoverNodeIndex != -1)
            {
                _selectedHandle = null;

                if (e.shift)
                {
                    if (_selectedNodeIndices.Contains(_hoverNodeIndex))
                        _selectedNodeIndices.Remove(_hoverNodeIndex);
                    else
                        _selectedNodeIndices.Add(_hoverNodeIndex);
                }
                else
                {
                    _selectedNodeIndices.Clear();
                    _selectedNodeIndices.Add(_hoverNodeIndex);
                }

                e.Use();
                Repaint();
            }
            else if (!e.shift)
            {
                ClearSelection(e);
            }
        }

        private void UpdateHover()
        {
            _hoverNodeIndex = -1;

            for (int i = 0; i < _spline.nodes.Count; i++)
            {
                Vector3 worldPos = _spline.transform.TransformPoint(_spline.nodes[i].position);
                float size = HandleUtility.GetHandleSize(worldPos) * _drawer.nodeBaseSize;

                if (HandleUtility.DistanceToCircle(worldPos, size * HoverTestRadiusFactor) < HoverPixelThreshold)
                {
                    _hoverNodeIndex = i;
                    break;
                }
            }
        }

        private void ClearSelection(Event e)
        {
            _selectedNodeIndices.Clear();
            _selectedHandle = null;
            e.Use();
            Repaint();
        }

        private bool IsHoveringHandle(int nodeIndex, Vector3 localOffset)
        {
            Vector3 handleWorld = GetSplineWorldPosition(nodeIndex) + _spline.transform.TransformVector(localOffset);
            float size = HandleUtility.GetHandleSize(handleWorld) * _drawer.nodeBaseSize * 0.7f;
            return HandleUtility.DistanceToCircle(handleWorld, size * HoverTestRadiusFactor) < HoverPixelThreshold;
        }

        private void UpdateHandleHover()
        {
            _hoverHandleSegment = -1;

            if (_selectedNodeIndices.Count == 0) return;

            for (int i = 0; i < _spline.segments.Count; i++)
            {
                var seg = _spline.segments[i];

                if (_selectedNodeIndices.Contains(seg.startNodeIndex) &&
                    IsHoveringHandle(seg.startNodeIndex, seg.startHandleOffset))
                {
                    _hoverHandleSegment = i;
                    _hoverHandleIsStart = true;
                    return;
                }

                if (_selectedNodeIndices.Contains(seg.endNodeIndex) &&
                    IsHoveringHandle(seg.endNodeIndex, seg.endHandleOffset))
                {
                    _hoverHandleSegment = i;
                    _hoverHandleIsStart = false;
                    return;
                }
            }
        }

        #endregion Selection

        #region Draw

        

        


        

        

        

        #endregion Draw

        #region Events

        private void OnUndoRedo()
        {
            _selectedNodeIndices.RemoveWhere(i => i < 0 || i >= _spline.nodes.Count);
            SceneView.RepaintAll();
        }

        #endregion Events
    }
}