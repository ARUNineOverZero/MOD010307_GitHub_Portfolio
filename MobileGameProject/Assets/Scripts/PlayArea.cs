using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;


namespace MobileGameProject
{
    public class PlayArea : MonoBehaviour
    {
        [Header("Prefab Reference")]
        [SerializeField] private Transform _top;
        [SerializeField] private Transform _right;
        [SerializeField] private Transform _bottom;
        [SerializeField] private Transform _left;

        [Header("Scene Reference")]
        [SerializeField] private RectTransform _content;

        [ContextMenu("Execute")]
        public void SizeToContent()
        {
            var camera = Camera.main;

            Vector3 bl = camera.ViewportToWorldPoint(Vector3.forward);
            Vector3 tr = camera.ViewportToWorldPoint(Vector3.one);
            Vector2 size = tr - bl;
            Vector2 center = new Vector2(bl.x, bl.y) + size * 0.5f;

            float thickness = NativeSize(_top).y;
            float inset = thickness * 0.5f;

            Place(_top, new Vector3(center.x, tr.y - inset, 0f), size.x, thickness);
            Place(_bottom, new Vector3(center.x, bl.y + inset, 0f), size.x, thickness);
            Place(_left, new Vector3(bl.x + inset, center.y, 0f), thickness, size.y);
            Place(_right, new Vector3(tr.x - inset, center.y, 0f), thickness, size.y);
        }

        private Vector2 NativeSize(Transform t)
        {
            Sprite sprite = t.GetComponent<SpriteRenderer>().sprite;
            return new Vector2(sprite.rect.width / sprite.pixelsPerUnit, sprite.rect.height / sprite.pixelsPerUnit);
        }

        private void Place(Transform t, Vector3 worldPosition, float worldWidth, float worldHeight)
        {
            Vector2 nativeSize = NativeSize(t);
            t.position = worldPosition;
            t.localScale = new Vector3(worldWidth / nativeSize.x, worldHeight / nativeSize.y, 1f);
        }
    }
}