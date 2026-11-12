using UnityEditor;
using UnityEngine;

namespace MobileGameProjectEditor.SplineNetwork
{
    public abstract class AActions
    {
        public bool TryGetClickedWorldPosition(Event e, out Vector3 worldPos, float distance = 1000f)
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
            if (EditorSnapSettings.snapEnabled && EditorSnapSettings.gridSnapEnabled)
                worldPos = Snapping.Snap(worldPos, EditorSnapSettings.gridSize);

            return true;
        }

        public void HandleCreateInput(Event e)
        {
            if (e.type != EventType.MouseDown || e.button != 0) return;

            if (TryGetClickedWorldPosition(e, out Vector3 worldPos))
            {
                CreateNodeAt(worldPos);
                e.Use();
            }
        }

        public abstract void CreateNodeAt(Vector3 worldPos);
    }
}
