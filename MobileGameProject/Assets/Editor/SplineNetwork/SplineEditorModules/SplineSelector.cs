using System.Collections.Generic;
using System.Linq;
using MobileGameProject.SplineNetwork;
using UnityEditor;
using UnityEngine;

namespace MobileGameProjectEditor.SplineNetwork
{
    public struct HandleSelection
    {
        public int segmentIndex;
        public bool isStart;
        
        public bool Equals(HandleSelection other) =>
            segmentIndex == other.segmentIndex && isStart == other.isStart;

        public override bool Equals(object obj) => obj is HandleSelection other && Equals(other);

        public override int GetHashCode() => (segmentIndex, isStart).GetHashCode();

        public static bool operator ==(HandleSelection a, HandleSelection b) => a.Equals(b);
        public static bool operator !=(HandleSelection a, HandleSelection b) => !a.Equals(b);
    }
    
    public class SplineSelector
    {
        private readonly Spline _spline;
        private int _hoverNodeIndex = -1;
        public int HoverNodeIndex => _hoverNodeIndex;
        private int _hoverHandleSegmentIndex = -1;
        public int HoverHandleSegmentIndex => _hoverHandleSegmentIndex;
        private bool _hoverHandleIsStart;
        public bool HoverHandleIsStart => _hoverHandleIsStart;

        private HashSet<int> _selectedNodeIndices = new HashSet<int>();
        public HashSet<int> SelectedNodeIndices => _selectedNodeIndices;

        private HandleSelection? _selectedHandle;
        public HandleSelection? SelectedHandle => _selectedHandle;
        
        public int SelectedCount => _selectedNodeIndices.Count;
        
        public SplineSelector(Spline spline)
        {
            _spline = spline;
        }

        public void UpdateHoverSelection()
        {
            UpdateHover();
            UpdateHandleHover();
        }

        public void UpdateHover()
        {
            _hoverNodeIndex = -1;
            if (_spline == null) return;

            for (int i = 0; i < _spline.nodes.Count; i++)
            {
                Vector3 worldPos = _spline.transform.TransformPoint(_spline.nodes[i].position);
                float size = HandleUtility.GetHandleSize(worldPos) * SplineEditorSettings.nodeBaseSize;

                if (HandleUtility.DistanceToCircle(worldPos, size * SplineEditorSettings.hoverTestRadiusFactor) < SplineEditorSettings.hoverPixelThreshold)
                {
                    _hoverNodeIndex = i;
                    break;
                }
            }
        }

        public void UpdateHandleHover()
        {
            _hoverHandleSegmentIndex = -1;
            if (_spline == null || _selectedNodeIndices == null || _selectedNodeIndices.Count == 0) return;

            for (int i = 0; i < _spline.segments.Count; i++)
            {
                var seg = _spline.segments[i];

                if (_selectedNodeIndices.Contains(seg.startNodeIndex) &&
                    IsHoveringHandle(seg.startNodeIndex, seg.startHandleOffset))
                {
                    _hoverHandleSegmentIndex = i;
                    _hoverHandleIsStart = true;
                    return;
                }

                if (_selectedNodeIndices.Contains(seg.endNodeIndex) &&
                    IsHoveringHandle(seg.endNodeIndex, seg.endHandleOffset))
                {
                    _hoverHandleSegmentIndex = i;
                    _hoverHandleIsStart = false;
                    return;
                }
            }
        }

        public void HandleSelectionInput(Event e)
        {
            if (e.type != EventType.MouseDown || e.button != 0) return;

            if (_hoverHandleSegmentIndex != -1)
            {
                _selectedHandle = new HandleSelection
                {
                    segmentIndex = _hoverHandleSegmentIndex,
                    isStart = HoverHandleIsStart
                };
                e.Use();
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
            }
            else if (!e.shift)
                ClearSelection(e);
            
        }

        public void ClearSelection(Event e = null)
        {
            _selectedNodeIndices.Clear();
            _selectedHandle = null;
            e?.Use();
        }

        public void ClearSelectedHandle()
        {
            _selectedHandle = null;
        }

        private bool IsHoveringHandle(int nodeIndex, Vector3 localOffset)
        {
            Vector3 handleWorld = _spline.transform.TransformPoint(_spline.nodes[nodeIndex].position)
                                  + _spline.transform.TransformVector(localOffset);
            float size = HandleUtility.GetHandleSize(handleWorld) * SplineEditorSettings.nodeBaseSize * 0.7f;
            return HandleUtility.DistanceToCircle(handleWorld, size * SplineEditorSettings.hoverTestRadiusFactor) < SplineEditorSettings.hoverPixelThreshold;
        }

        public bool IsNodeSelected(int index) => _selectedNodeIndices.Contains(index);
        
        public IEnumerable<SplineSegment> GetSelectedSegments()
        {
            if (_selectedNodeIndices.Count < 2) yield break;

            foreach (var segment in _spline.segments)
                if (_selectedNodeIndices.Contains(segment.startNodeIndex) && _selectedNodeIndices.Contains(segment.endNodeIndex))
                    yield return segment;
        }

        public void SelectAll()
        {
            _selectedNodeIndices.Clear();
            for (int i = 0; i < _spline.nodes.Count; i++)
                _selectedNodeIndices.Add(i);
        }

        public void SelectSegmentsFromSelectedNodes()
        {
            var oldSelectedNodeIndices = new HashSet<int>(_selectedNodeIndices);
            foreach (var segment in _spline.segments)
            {
                if (oldSelectedNodeIndices.Contains(segment.startNodeIndex) || oldSelectedNodeIndices.Contains(segment.endNodeIndex))
                {
                    if(!_selectedNodeIndices.Contains(segment.startNodeIndex))
                        _selectedNodeIndices.Add(segment.startNodeIndex);
                    else if(!_selectedNodeIndices.Contains(segment.endNodeIndex))
                        _selectedNodeIndices.Add(segment.endNodeIndex);
                }
            }
        }
    }
}