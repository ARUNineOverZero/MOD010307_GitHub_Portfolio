using UnityEngine;
using UnityEngine.EventSystems;

namespace MobileGameProject.DragRacer
{
    public class Pedal : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Visual")]
        [SerializeField] private RectTransform _bar;

        [Header("Settings")]
        [SerializeField] private bool _returnToZero = true;

        private float _value;
        public float Value => _value;

        private RectTransform _rect;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
        }

        public void UpdateKeyboarValue(float value) => SetValue(value);

        public void SetValue(float value)
        {
            _value = Mathf.Clamp01(value);
            UpdateBar();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            UpdateValueFromPointer(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            UpdateValueFromPointer(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_returnToZero)
                SetValue(0f);
        }

        private void UpdateValueFromPointer(PointerEventData eventData)
        {
            if (_rect == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);

            float height = _rect.rect.height;
            float normalized = Mathf.InverseLerp(-height * 0.5f, height * 0.5f, localPoint.y);

            SetValue(normalized);
        }

        private void UpdateBar()
        {
            if (_bar == null) return;

            _bar.anchorMin = new Vector2(0.09854014f, 0f);
            _bar.anchorMax = new Vector2(0.9014599f, _value * 0.9423077f);
            _bar.offsetMin = Vector2.zero;
            _bar.offsetMax = Vector2.zero;
        }
    }
}