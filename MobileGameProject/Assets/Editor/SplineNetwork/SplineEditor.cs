using System;
using System.Collections.Generic;
using MobileGameProject.SplineNetwork;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace MobileGameProjectEditor.SplineNetwork
{
    [CustomEditor(typeof(Spline))]
    public class SplineEditor : Editor
    {
        private Spline _spline;
        private HashSet<int> _selectedNodes = new HashSet<int>();
        private int _hoverNode = -1;

        private float _nodeBaseSize = .12f;
        private float _hoverSizeMultiplier = 1.35f;
        private Color _normalColor = Color.white;
        private Color _hoverColor = Color.yellow;
        private Color _selectedColor = Color.green;

        private const float HoverTestRadiusFactor = 0.6f;
        private const float HoverPixelThreshold = 8f;

        private void OnEnable() => _spline = (Spline)target;

        private void OnSceneGUI()
        {
            if (_spline == null) return;

            Event e = Event.current;

            UpdateHover();
            DrawNodes();
            HandleNodeMovement();
            HandleSelectionInput(e);
            RequestRepaint(e);
        }

        private void UpdateHover()
        {
            _hoverNode = -1;

            for (int i = 0; i < _spline.nodes.Count; i++)
            {
                Vector3 pos = _spline.nodes[i].position;
                float size = HandleUtility.GetHandleSize(pos) * _nodeBaseSize;

                if (HandleUtility.DistanceToCircle(pos, size * HoverTestRadiusFactor) < HoverPixelThreshold)
                {
                    _hoverNode = i;
                    break;
                }
            }
        }

        private void DrawNodes()
        {
            for (int i = 0; i < _spline.nodes.Count; i++)
            {
                Vector3 pos = _spline.nodes[i].position;
                float baseSize = HandleUtility.GetHandleSize(pos) * _nodeBaseSize;
                bool isSelected = _selectedNodes.Contains(i);
                bool isHovered = _hoverNode == i;

                Color color = _normalColor;
                float size = baseSize;

                if (isSelected)
                {
                    color = _selectedColor;
                    size = baseSize;
                }
                else if (isHovered)
                {
                    color = _hoverColor;
                    size = baseSize * _hoverSizeMultiplier;
                }

                DrawNodeSquare(pos, size, color);
            }
        }

        private void HandleNodeMovement()
        {
            if (_selectedNodes.Count == 0) return;

            Vector3 center = Vector3.zero;
            foreach (int index in _selectedNodes)
                center += _spline.nodes[index].position;
            center /= _selectedNodes.Count;

            Vector3 newCenter = Handles.PositionHandle(center, quaternion.identity);

            if (newCenter != center)
            {
                Vector3 delta = newCenter - center;

                Undo.RecordObject(_spline, "Move Spline Nodes");
                foreach (int index in _selectedNodes)
                    _spline.nodes[index].position += delta;

                EditorUtility.SetDirty(_spline);
            }
        }

        private void HandleSelectionInput(Event e)
        {
            if (e.type != EventType.MouseDown || e.button != 0) return;

            if (_hoverNode != -1)
            {
                if (e.shift)
                {
                    if (_selectedNodes.Contains(_hoverNode))
                        _selectedNodes.Remove(_hoverNode);
                    else
                        _selectedNodes.Add(_hoverNode);
                }
                else
                {
                    _selectedNodes.Clear();
                    _selectedNodes.Add(_hoverNode);
                }

                e.Use();
                Repaint();
            }
            else if (!e.shift)
            {
                ClearSelection(e);
            }
        }

        private void RequestRepaint(Event e)
        {
            if (e.type == EventType.MouseMove || e.type == EventType.MouseDrag)
                SceneView.RepaintAll();
        }

        private void ClearSelection(Event e)
        {
            _selectedNodes.Clear();
            e.Use();
            Repaint();
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
    }
}