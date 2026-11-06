using System;
using System.Collections.Generic;
using System.Linq;
using MobileGameProject.SplineNetwork;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace MobileGameProjectEditor.SplineNetwork
{
    [CustomEditor(typeof(Spline))]
    public class SplineEditor : Editor
    {
        private Spline _spline;
        private HashSet<int> _selectedNodeIndices = new HashSet<int>();
        private int _hoverNode = -1;
        private float _nodeBaseSize = .06f;
        private float _hoverSizeMultiplier = 1.35f;
        private int _segmentLevelOfDetail = 10;
        private Color _normalColor = Color.white;
        private Color _hoverColor = Color.yellow;
        private Color _selectedColor = Color.green;
        private const float HoverTestRadiusFactor = 0.6f;
        private const float HoverPixelThreshold = 8f;
        private bool _isEditing = false;

        private struct HandleSelection
        {
            public int segmentIndex;
            public bool isStart;
        }

        private HandleSelection? _selectedHandle = null;
        private int _hoverHandleSegment = -1;
        private bool _hoverHandleIsStart;

        private void OnEnable()
        {
            _spline = (Spline)target;
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
            DrawNodes();
            DrawHandles();
            DrawSegments();

            if (_isEditing)
            {
                Tools.hidden = true;

                if (e.control)
                {
                    HandleCreateInput(e);
                }
                else
                {
                    HandleNodeMovement();
                    HandleSelectionInput(e);
                }
            }
            else
            {
                Tools.hidden = false;
            }

            RequestRepaint(e);
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space(8);
            DrawEditGUI();
            GUI.backgroundColor = Color.white;
        }

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

        #region Create

        private void HandleCreateInput(Event e)
        {
            if (e.type != EventType.MouseDown || e.button != 0) return;

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

        private void HandleNodeMovement()
        {
            if (_selectedNodeIndices.Count == 0) return;

            Vector3 worldCenter = Vector3.zero;
            foreach (int index in _selectedNodeIndices)
                worldCenter += _spline.transform.TransformPoint(_spline.nodes[index].position);

            worldCenter /= _selectedNodeIndices.Count;

            Vector3 newWorldCenter = Handles.PositionHandle(worldCenter, Quaternion.identity);
            if (newWorldCenter != worldCenter)
            {
                Vector3 delta = newWorldCenter - worldCenter;

                Undo.RecordObject(_spline, "Move Spline Nodes");
                foreach (int index in _selectedNodeIndices)
                {
                    Vector3 worldPos = _spline.transform.TransformPoint(_spline.nodes[index].position);
                    Vector3 newWorldPos = worldPos + delta;
                    _spline.nodes[index].position = _spline.transform.InverseTransformPoint(newWorldPos);
                }
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

            if (_hoverNode != -1)
            {
                _selectedHandle = null;

                if (e.shift)
                {
                    if (_selectedNodeIndices.Contains(_hoverNode))
                        _selectedNodeIndices.Remove(_hoverNode);
                    else
                        _selectedNodeIndices.Add(_hoverNode);
                }
                else
                {
                    _selectedNodeIndices.Clear();
                    _selectedNodeIndices.Add(_hoverNode);
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
            _hoverNode = -1;

            for (int i = 0; i < _spline.nodes.Count; i++)
            {
                Vector3 worldPos = _spline.transform.TransformPoint(_spline.nodes[i].position);
                float size = HandleUtility.GetHandleSize(worldPos) * _nodeBaseSize;

                if (HandleUtility.DistanceToCircle(worldPos, size * HoverTestRadiusFactor) < HoverPixelThreshold)
                {
                    _hoverNode = i;
                    break;
                }
            }
        }

        private void ClearSelection(Event e)
        {
            _selectedNodeIndices.Clear();
            _selectedHandle = null;              // also clear handle
            e.Use();
            Repaint();
        }

        private bool IsHoveringHandle(int nodeIndex, Vector3 localOffset)
        {
            Vector3 handleWorld = GetSplineWorldPosition(nodeIndex) + _spline.transform.TransformVector(localOffset);
            float size = HandleUtility.GetHandleSize(handleWorld) * _nodeBaseSize * 0.7f;
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

        private void DrawNodes()
        {
            for (int i = 0; i < _spline.nodes.Count; i++)
            {
                Vector3 worldPos = GetSplineWorldPosition(i);
                bool isSelected = _selectedNodeIndices.Contains(i);
                bool isHovered = _hoverNode == i;

                Color color = _normalColor;
                float size = HandleUtility.GetHandleSize(worldPos) * _nodeBaseSize;

                if (isSelected)
                {
                    color = _selectedColor;
                }
                else if (isHovered)
                {
                    color = _hoverColor;
                    size *=  _hoverSizeMultiplier;
                }

                DrawNodeSquare(worldPos, size, color);
            }
        }

        private void DrawNodeSquare(Vector3 center, float size, Color color)
        {
            var cam = SceneView.currentDrawingSceneView.camera;
            Vector3 right = cam.transform.right * size;
            Vector3 up = cam.transform.up * size;

            Vector3[] verts = new Vector3[]
            {
                center - right - up,
                 center - right + up,
                  center + right + up,
                   center + right - up
            };

            Handles.DrawSolidRectangleWithOutline(verts, color, color);
        }

        private void DrawHandles()
        {
            if (_selectedNodeIndices.Count == 0) return;

            for (int i = 0; i < _spline.segments.Count; i++)
            {
                var seg = _spline.segments[i];

                if (_selectedNodeIndices.Contains(seg.startNodeIndex))
                    DrawHandle(i, true, seg.startNodeIndex, seg.startHandleOffset);

                if (_selectedNodeIndices.Contains(seg.endNodeIndex))
                    DrawHandle(i, false, seg.endNodeIndex, seg.endHandleOffset);
            }
        }

        private void DrawHandle(int segmentIndex, bool isStart, int nodeIndex, Vector3 localOffset)
        {
            Vector3 worldPos = GetSplineWorldPosition(nodeIndex);
            Vector3 handleWorld = worldPos + _spline.transform.TransformVector(localOffset);

            bool isHovered = _hoverHandleSegment == segmentIndex && _hoverHandleIsStart == isStart;
            bool isSelected = _selectedHandle.HasValue &&
                              _selectedHandle.Value.segmentIndex == segmentIndex &&
                              _selectedHandle.Value.isStart == isStart;

            Handles.color = _hoverColor;
            Handles.DrawLine(worldPos, handleWorld);

            float size = HandleUtility.GetHandleSize(worldPos) * _nodeBaseSize;
            Color color = _normalColor;

            if (isSelected)
            {
                color = _selectedColor;
            }
            else if (isHovered)
            {
                color = _hoverColor;
                size *= _hoverSizeMultiplier;
            }

            DrawNodeSquare(handleWorld, size, color);
        }
        private void DrawSegments()
        {
            foreach (var seg in _spline.segments)
                DrawSegmentLine(seg, _normalColor);
        }

        private void DrawSegmentLine(SplineSegment segment, Color color)
        {
            var nodes = _spline.nodes;

            if (_segmentLevelOfDetail <= 2)
            {
                Handles.color = color;
                Handles.DrawLine(GetSplineWorldPosition(segment.startNodeIndex), GetSplineWorldPosition(segment.endNodeIndex));
                return;
            }

            Vector3 p0 = GetSplineWorldPosition(segment.startNodeIndex);
            Vector3 p3 = GetSplineWorldPosition(segment.endNodeIndex);
            Vector3 p1 = p0 + _spline.transform.TransformVector(segment.startHandleOffset);
            Vector3 p2 = p3 + _spline.transform.TransformVector(segment.endHandleOffset);

            Vector3[] points = new Vector3[_segmentLevelOfDetail + 1];
            for (int i = 0; i <= _segmentLevelOfDetail; i++)
                points[i] = EvaluateCubicBezier(p0, p1, p2, p3, i / (float)_segmentLevelOfDetail);

            Handles.color = color;
            Handles.DrawPolyLine(points);
        }

        private Vector3 EvaluateCubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float u = 1f - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            return uuu * p0
                 + 3f * uu * t * p1
                 + 3f * u * tt * p2
                 + ttt * p3;
        }

        private void RequestRepaint(Event e)
        {
            if (e.type == EventType.MouseMove || e.type == EventType.MouseDrag)
                SceneView.RepaintAll();
        }

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