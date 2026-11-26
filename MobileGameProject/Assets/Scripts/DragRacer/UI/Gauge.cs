using UnityEngine;

namespace DragRacer.UI
{
    public class Gauge : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform _needle;
        [SerializeField] private RectTransform _marker;

        [Header("Markers")]
        [SerializeField] private int _markerCount = 14;
        [SerializeField] private float _markerDistance = 80f;

        private RectTransform[] _markers;

        [Header("Range")]
        [SerializeField] private float _minValue = 0f;
        [SerializeField] private float _maxValue = 7000f;

        [Header("Needle Rotation")]
        [SerializeField] private float _minAngle = 135f;
        [SerializeField] private float _maxAngle = -135f;

        [Header("Smoothing")]
        [SerializeField] private float _smoothSpeed = 8f;

        private float _currentDisplayValue;


        [ContextMenu("Create Markers")]
        private void CreateMarkers()
        {
            for (int i = 1; i < _markers.Length; i++)
                DestroyImmediate(_markers[i]);
            
            if (_marker == null || _markerCount < 2) return;

            _markers = new RectTransform[_markerCount];
            _markers[0] = _marker;

            float angleStep = (_maxAngle - _minAngle) / (_markerCount - 1);

            for (int i = 0; i < _markerCount; i++)
            {
                RectTransform marker;

                if (i == 0)
                    marker = _marker;
                else
                {
                    marker = Instantiate(_marker, _marker.parent);
                    marker.name = $"Marker_{i}";
                }

                float angle = _minAngle + angleStep * i;
                marker.localRotation = Quaternion.Euler(0f, 0f, angle);
                _markers[i] = marker;
            }
        }

        public void SetValue(float value)
        {
            float target = Mathf.Clamp(value, _minValue, _maxValue);
            _currentDisplayValue = Mathf.Lerp(_currentDisplayValue, target, Time.deltaTime * _smoothSpeed);

            float t = Mathf.InverseLerp(_minValue, _maxValue, _currentDisplayValue);

            if (_needle != null)
            {
                float angle = Mathf.Lerp(_minAngle, _maxAngle, t);
                _needle.localRotation = Quaternion.Euler(0f, 0f, angle);
            }
        }
    }
}