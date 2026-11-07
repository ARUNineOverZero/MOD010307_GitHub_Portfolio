using System.Collections.Generic;
using System.Linq;
using MobileGameProject.SplineNetwork;
using UnityEditor;
using UnityEngine;

namespace MobileGameProjectEditor.SplineNetwork
{
    public class SplineActions
    {
        private readonly Spline _spline;
        private readonly SplineSelector _selector;

        public SplineActions(Spline spline, SplineSelector selector)
        {
            _spline = spline;
            _selector = selector;
        }

        public void HandleCreateInput(Event e)
        {
            if (e.type != EventType.MouseDown || e.button != 0) return;

            if (_selector.SelectedHandle.HasValue)
                _selector.ClearSelectedHandle();

            if (TryGetClickedWorldPosition(e, out Vector3 worldPos))
            {
                CreateNodeAt(worldPos);
                e.Use();
            }
        }

        public void CreateNodeAt(Vector3 worldPos)
        {
            Undo.RecordObject(_spline, "Add Spline Node");

            Vector3 localPos = _spline.transform.InverseTransformPoint(worldPos);
            int newIndex = _spline.AddNode(localPos, _selector.SelectedNodeIndices.ToArray());

            _selector.SelectedNodeIndices.Clear();
            _selector.SelectedNodeIndices.Add(newIndex);

            EditorUtility.SetDirty(_spline);
        }

        public void DeleteSelectedNodes()
        {
            if (_selector.SelectededCount == 0) return;

            Undo.RecordObject(_spline, "Delete Spline Nodes");

            _spline.RemoveAllNodesAt(_selector.SelectedNodeIndices.ToArray());
            _selector.ClearSelection();

            EditorUtility.SetDirty(_spline);
        }

        public void CreateSegmentBetweenSelected()
        {
            if (_selector.SelectededCount != 2) return;

            var segment = _spline.segments.FirstOrDefault(x =>
                _selector.IsNodeSelected(x.startNodeIndex) &&
                _selector.IsNodeSelected(x.endNodeIndex));

            if (segment != null) return;

            Undo.RecordObject(_spline, "Create Spline Segment");

            var indices = _selector.SelectedNodeIndices.ToArray();
            _spline.AddSegment(indices[0], indices[1]);

            EditorUtility.SetDirty(_spline);
        }

        public void DeleteSegmentBetweenSelected()
        {
            if (_selector.SelectededCount != 2) return;

            var segment = _spline.segments.FirstOrDefault(x =>
                _selector.IsNodeSelected(x.startNodeIndex) &&
                _selector.IsNodeSelected(x.endNodeIndex));

            if (segment == null) return;

            Undo.RecordObject(_spline, "Delete Spline Segment");

            _spline.RemoveSegment(segment);

            EditorUtility.SetDirty(_spline);
        }

        public void Clear()
        {
            Undo.RecordObject(_spline, "Clear Spline Data");

            _spline.Clear();
            _selector.ClearSelection();

            EditorUtility.SetDirty(_spline);
        }

        private bool TryGetClickedWorldPosition(Event e, out Vector3 worldPos)
        {
            var cam = SceneView.currentDrawingSceneView?.camera;
            if (cam == null)
            {
                worldPos = Vector3.zero;
                return false;
            }

            var position = e.mousePosition;
            position.y = cam.scaledPixelHeight - position.y;
            var ray = cam.ScreenPointToRay(position);

            if (!Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                worldPos = Vector3.zero;
                return false;
            }

            worldPos = hit.point;
            if (EditorSnapSettings.gridSnapActive && EditorSnapSettings.gridSnapEnabled)
                worldPos = Snapping.Snap(worldPos, EditorSnapSettings.move);
            
            return true;
        }
        
        public void ResetHandlesOnSelectedSegments()
        {
            if (_selector.SelectededCount < 2) return;

            Undo.RecordObject(_spline, "Reset Segment Handles");

            bool changed = false;

            foreach (var segment in _selector.GetSelectedSegments())
            {
                Vector3 startPos = _spline.nodes[segment.startNodeIndex].position;
                Vector3 endPos = _spline.nodes[segment.endNodeIndex].position;
                Vector3 delta = endPos - startPos;

                segment.startHandleOffset = delta * Spline.DefaultHandleOffsetFactor;
                segment.endHandleOffset = -delta * Spline.DefaultHandleOffsetFactor;

                changed = true;
            }

            if (changed)
                EditorUtility.SetDirty(_spline);
        }

        public void DropSelectedNodesToSurface()
        {
            if (_selector.SelectededCount == 0) return;

            Undo.RecordObject(_spline, "Drop Nodes to Surface");

            bool changed = false;
            foreach (int index in _selector.SelectedNodeIndices)
            {
                Vector3 worldPos = _spline.GetNodeWorldPosition(index);
                Ray ray = new Ray(worldPos, Vector3.down);

                if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
                {
                    _spline.nodes[index].position = _spline.transform.InverseTransformPoint(hit.point);
                    changed = true;
                }
            }

            if (changed)
                EditorUtility.SetDirty(_spline);
        }

        public void ResetNodeHandles()
        {
            if (_selector.SelectededCount == 0) return;

            Undo.RecordObject(_spline, "Reset Node Handles");

            bool changed = false;

            foreach (var segment in _spline.segments)
            {
                bool startSelected = _selector.IsNodeSelected(segment.startNodeIndex);
                bool endSelected = _selector.IsNodeSelected(segment.endNodeIndex);

                if (!startSelected && !endSelected)
                    continue;

                Vector3 startPos = _spline.nodes[segment.startNodeIndex].position;
                Vector3 endPos = _spline.nodes[segment.endNodeIndex].position;
                Vector3 delta = endPos - startPos;

                if (startSelected)
                    segment.startHandleOffset = delta * Spline.DefaultHandleOffsetFactor;

                if (endSelected)
                    segment.endHandleOffset = -delta * Spline.DefaultHandleOffsetFactor;

                changed = true;
            }

            if (changed)
                EditorUtility.SetDirty(_spline);
        }
    }
}