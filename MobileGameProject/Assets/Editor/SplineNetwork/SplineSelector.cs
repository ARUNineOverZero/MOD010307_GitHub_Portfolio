using System.Collections.Generic;
using MobileGameProject.SplineNetwork;
using UnityEditor;
using UnityEngine;

namespace MobileGameProjectEditor.SplineNetwork
{
    public struct HandleSelection
    {
        public int segmentIndex;
        public bool isStart;
    }
    
    public class SplineSelector
    {
        private readonly SplineEditorSettings _settings;

        public int _hoverNodeIndex = -1;
        public int HoverNodeIndex => _hoverNodeIndex;
        public int _hoverHandleSegmentIndex = -1;
        public int HoverHandleSegmentIndex => _hoverHandleSegmentIndex;
        private bool _hoverHandleIsStart;
        public bool HoverHandleIsStart => _hoverHandleIsStart;

        private HashSet<int> _selectedNodeIndices = new HashSet<int>();
        public HashSet<int> SelectedNodeIndices => _selectedNodeIndices;

        private HandleSelection? _selectedHandle;
        public HandleSelection? SelectedHandle => _selectedHandle;
        
        public int SelectededCount => _selectedNodeIndices.Count;
        

        public SplineSelector(SplineEditorSettings settings) => _settings = settings;

        public void UpdateHover(Spline spline)
        {
            _hoverNodeIndex = -1;
            if (spline == null) return;

            for (int i = 0; i < spline.nodes.Count; i++)
            {
                Vector3 worldPos = spline.transform.TransformPoint(spline.nodes[i].position);
                float size = HandleUtility.GetHandleSize(worldPos) * _settings.nodeBaseSize;

                if (HandleUtility.DistanceToCircle(worldPos, size * _settings.hoverTestRadiusFactor) < _settings.hoverPixelThreshold)
                {
                    _hoverNodeIndex = i;
                    break;
                }
            }
        }

        public void UpdateHandleHover(Spline spline)
        {
            _hoverHandleSegmentIndex = -1;
            if (spline == null || _selectedNodeIndices == null || _selectedNodeIndices.Count == 0) return;

            for (int i = 0; i < spline.segments.Count; i++)
            {
                var seg = spline.segments[i];

                if (_selectedNodeIndices.Contains(seg.startNodeIndex) &&
                    IsHoveringHandle(spline, seg.startNodeIndex, seg.startHandleOffset))
                {
                    _hoverHandleSegmentIndex = i;
                    _hoverHandleIsStart = true;
                    return;
                }

                if (_selectedNodeIndices.Contains(seg.endNodeIndex) &&
                    IsHoveringHandle(spline, seg.endNodeIndex, seg.endHandleOffset))
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

            if (HoverNodeIndex != -1)
            {
                _selectedHandle = null;

                if (e.shift)
                {
                    if (_selectedNodeIndices.Contains(HoverNodeIndex))
                        _selectedNodeIndices.Remove(HoverNodeIndex);
                    else
                        _selectedNodeIndices.Add(HoverNodeIndex);
                }
                else
                {
                    _selectedNodeIndices.Clear();
                    _selectedNodeIndices.Add(HoverNodeIndex);
                }

                e.Use();
            }
            else if (!e.shift)
            {
                ClearSelection(e);
            }
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

        private bool IsHoveringHandle(Spline spline, int nodeIndex, Vector3 localOffset)
        {
            Vector3 handleWorld = spline.transform.TransformPoint(spline.nodes[nodeIndex].position)
                                  + spline.transform.TransformVector(localOffset);
            float size = HandleUtility.GetHandleSize(handleWorld) * _settings.nodeBaseSize * 0.7f;
            return HandleUtility.DistanceToCircle(handleWorld, size * _settings.hoverTestRadiusFactor) < _settings.hoverPixelThreshold;
        }

        public bool IsNodeSelected(int index) => _selectedNodeIndices.Contains(index);
    }
}