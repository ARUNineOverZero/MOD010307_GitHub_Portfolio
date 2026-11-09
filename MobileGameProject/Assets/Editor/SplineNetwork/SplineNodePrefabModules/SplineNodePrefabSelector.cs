using System;
using System.Collections.Generic;
using MobileGameProject.SplineNetwork;
using UnityEditor;
using UnityEngine;

namespace MobileGameProjectEditor.SplineNetwork
{
    public class SplineNodePrefabSelector
    {
        private SplineNodePrefab _splineNodePrefab;

        private int _hoverSocketIndex = -1;
        public int HoverSocketIndex => _hoverSocketIndex;

        private HashSet<int> _selectedSocketIndices = new HashSet<int>();
        public HashSet<int> SelectedSocketIndices => _selectedSocketIndices;

        public int SelectedCount => _selectedSocketIndices.Count;

        public SplineNodePrefabSelector(SplineNodePrefab splineNodePrefab)
        {
            _splineNodePrefab = splineNodePrefab;
        }

        public void UpdateSocketHover()
        {
            _hoverSocketIndex = -1;
            if(_splineNodePrefab == null) return;

            for(int i = 0; i < _splineNodePrefab.sockets.Count; i++)
            {
                Vector3 worldPosition = _splineNodePrefab.GetSocketWorldPosition(i);
                float size = HandleUtility.GetHandleSize(worldPosition) * SplineEditorSettings.nodeBaseSize;

                if(HandleUtility.DistanceToCircle(worldPosition, size * SplineEditorSettings.hoverTestRadiusFactor) < SplineEditorSettings.hoverPixelThreshold)
                {
                    _hoverSocketIndex = i;
                    break;
                }
            }
        }

        public void HandleSelectionInput(Event e)
        {
            if(e.type != EventType.MouseDown || e.button != 0) return;

            if(_hoverSocketIndex != -1)
            {
                if(e.shift)
                {
                    if(_selectedSocketIndices.Contains(_hoverSocketIndex))
                        _selectedSocketIndices.Remove(_hoverSocketIndex);
                    else
                        _selectedSocketIndices.Add(_hoverSocketIndex);
                }
                else
                {
                    _selectedSocketIndices.Clear();
                    _selectedSocketIndices.Add(_hoverSocketIndex);
                }

                e.Use();
            }
            else if (!e.shift)
                ClearSelection(e);
        }

        public void ClearSelection(Event e = null)
        {
            _selectedSocketIndices.Clear();
            e?.Use();
        }

        public bool IsNodeSelected(int i)
        {
            return _selectedSocketIndices.Contains(i);
        }
    }
}