using System.Collections;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace MobileGameProject.KitchenWaiter
{
    public class Table : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _sr;
        [SerializeField] private TextMeshProUGUI _text;

        [SerializeField] private Color _off;
        [SerializeField] private Color _incorrect;
        [SerializeField] private Color _correct;
        [SerializeField] private float _speed = .25f;

        [SerializeField] private int _tableNumber = -1;

        public int TableNumber => _tableNumber;
        private Coroutine _shiftColor;

        public UnityEvent<Table> onEnterTable;
        public  UnityEvent<Table> onLeaveTable;

        public void SetTableNumber(int tableNumber)
        {
            _tableNumber = tableNumber;
        }

        private void OnValidate()
        {
            _text.text = _tableNumber.ToString();
        }

        public void CorrectTableEntered()
        {
            SetColour(_correct);
        }

        public void IncorrectTableEntered()
        {
            SetColour(_incorrect);
        }

        public void TableExited()
        {
            if(_sr.color != _off)
                SetColour(_off);
        }

        private void SetColour(Color color)
        {
            if (_shiftColor != null)
            {
                StopCoroutine(_shiftColor);
                _shiftColor = null;
            }

            _shiftColor = StartCoroutine(ShiftColor(color));
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            var waiter = collision.transform.GetComponentInParent<Waiter>();

            if (waiter != null)
                onEnterTable?.Invoke(this);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            var waiter = collision.transform.GetComponentInParent<Waiter>();

            if (waiter != null)
                onLeaveTable?.Invoke(this);
        }

        private IEnumerator ShiftColor(Color target)
        {
            var cc = _sr.color;
            float cVal = 0f;
            while (true)
            {
                cVal += _speed * Time.deltaTime;
                if(cVal > 1)
                {
                    _sr.color = Color.Lerp(cc, target, 1);
                    break;
                }

                _sr.color = Color.Lerp(cc, target, cVal);
                yield return null;
            }
        }
    }

    public enum ItemTableState
    {
        NoItem,
        IncorrectTable,
        CorrectTable
    }
}