using MobileGameProject.DragRacer;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace DragRacer.UI
{
    public class GearShifter : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("References")]
        [SerializeField] private RectTransform _stick;
        [SerializeField] private RectTransform _gate;

        [Header("Layout")]
        [SerializeField] private float _horizontalSpacing = 80f;
        [SerializeField] private float _verticalSpacing = 90f;
        [SerializeField] private float _centerThreshold = 25f;

        [Header("Gears")]
        [SerializeField] private Gear _currentGear = Gear.Neutral;

        public Gear CurrentGear => _currentGear;
        public UnityEvent<Gear> OnGearChanged;

        private Vector2[] _gearPositions;
        private bool _isDragging;

        private void Awake()
        {
            BuildGatePositions();
            SnapToGear(_currentGear, true);
        }

        private void BuildGatePositions()
        {
            _gearPositions = new Vector2[6];

            _gearPositions[0]   = new Vector2(-_horizontalSpacing,  _verticalSpacing);
            _gearPositions[1]  = new Vector2(-_horizontalSpacing, -_verticalSpacing);
            _gearPositions[2]   = new Vector2(0f,                   _verticalSpacing);
            _gearPositions[3]  = new Vector2(0f,                  -_verticalSpacing);
            _gearPositions[4]   = new Vector2( _horizontalSpacing,  _verticalSpacing);
            _gearPositions[5] = new Vector2( _horizontalSpacing, -_verticalSpacing);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isDragging = true;
            Debug.Log("Pointer Down");
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging || _stick == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _gate, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);

            Vector2 constrained = ConstrainToGate(localPoint);
            _stick.anchoredPosition = constrained;

            Gear nearest = GetNearestGear(constrained);
            if (nearest != _currentGear)
            {
                _currentGear = nearest;
                OnGearChanged?.Invoke(_currentGear);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isDragging = false;

            if (_stick.anchoredPosition.magnitude <= _centerThreshold)
            {
                _currentGear = Gear.Neutral;
                SnapToGear(Gear.Neutral);
                OnGearChanged?.Invoke(_currentGear);
                return;
            }

            SnapToGear(_currentGear);
        }

        private Vector2 ConstrainToGate(Vector2 point)
        {
            float[] rails = { -_horizontalSpacing, 0f, _horizontalSpacing };

            float closestRail = rails[0];
            float minDist = Mathf.Abs(point.x - rails[0]);

            for (int i = 1; i < rails.Length; i++)
            {
                float d = Mathf.Abs(point.x - rails[i]);
                if (d < minDist)
                {
                    minDist = d;
                    closestRail = rails[i];
                }
            }

            bool inCenterBand = Mathf.Abs(point.y) <= _centerThreshold;

            if (inCenterBand)
            {
                point.y = 0f;
                point.x = Mathf.Clamp(point.x, -_horizontalSpacing, _horizontalSpacing);
            }
            else
            {
                point.x = closestRail;
                point.y = Mathf.Clamp(point.y, -_verticalSpacing, _verticalSpacing);
            }

            return point;
        }

        private Gear GetNearestGear(Vector2 position)
        {
            if (position.magnitude <= _centerThreshold)
                return Gear.Neutral;

            Gear best = Gear.Neutral;
            float bestDist = float.MaxValue;

            foreach (Gear gear in System.Enum.GetValues(typeof(Gear)))
            {
                if (gear == Gear.Neutral) continue;

                Vector2 gearPos = _gearPositions[GearToIndex(gear)];
                float dist = Vector2.Distance(position, gearPos);

                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = gear;
                }
            }

            return best;
        }

        private void SnapToGear(Gear gear, bool instant = false)
        {
            Vector2 target = gear == Gear.Neutral
                ? Vector2.zero
                : _gearPositions[GearToIndex(gear)];

            _stick.anchoredPosition = target;
        }

        public void SetGear(Gear gear)
        {
            _currentGear = gear;
            SnapToGear(gear);
            OnGearChanged?.Invoke(_currentGear);
        }
        
        private int GearToIndex(Gear gear)
        {
            return gear switch
            {
                Gear.First   => 0,
                Gear.Second  => 1,
                Gear.Third   => 2,
                Gear.Fourth  => 3,
                Gear.Fifth   => 4,
                Gear.Reverse => 5,
                _            => -1
            };
        }
    }
}