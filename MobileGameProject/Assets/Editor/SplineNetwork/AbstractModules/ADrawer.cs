using MobileGameProject.SplineNetwork;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace MobileGameProjectEditor.SplineNetwork
{
    public abstract class ADrawer
    {
        protected void DrawNodeSquare(Vector3 center, float size, Color color)
        {
            var cam = SceneView.currentDrawingSceneView.camera;
            if (cam == null) return;

            Vector3 right = cam.transform.right * size;
            Vector3 up = cam.transform.up * size;

            Vector3[] verts =
            {
                center - right - up,
                center - right + up,
                center + right + up,
                center + right - up
            };

            Handles.DrawSolidRectangleWithOutline(verts, color, color);
        }

        protected void DrawWithOcclusion(System.Action draw, Color normalColor, Color occludedColor)
        {
            var prevZTest = Handles.zTest;
            var prevColor = Handles.color;

            Handles.zTest = CompareFunction.Greater;
            Handles.color = occludedColor;
            draw();

            Handles.zTest = CompareFunction.LessEqual;
            Handles.color = normalColor;
            draw();

            Handles.zTest = prevZTest;
            Handles.color = prevColor;
        }

        public void RequestRepaint(Event e)
        {
            if (e.type == EventType.MouseMove || e.type == EventType.MouseDrag)
                SceneView.RepaintAll();
        }
    }
}
