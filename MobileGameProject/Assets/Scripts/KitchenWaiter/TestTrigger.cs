using System.Collections;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

namespace MobileGameProject.KitchenWaiter
{
    public class TestTrigger : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _sr;
        [SerializeField] private TextMeshProUGUI _text;

        [SerializeField] private Color _off;
        [SerializeField] private Color _wrong;
        [SerializeField] private Color _correct;
        [SerializeField] private float _speed = .25f;

        [SerializeField] private int _tableNumber = 0;

        private Coroutine _shiftColor;

        private void OnValidate()
        {
            _text.text = _tableNumber.ToString();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            var waiter = collision.transform.GetComponentInParent<Waiter>();

            if (waiter != null && waiter.HasTray)
            {
                if (_shiftColor != null)
                {
                    StopCoroutine(_shiftColor);
                    _shiftColor = null;
                }

                _shiftColor = StartCoroutine(ShiftColor(waiter.TableNumber == _tableNumber ? _correct : _wrong));

            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.transform.GetComponentInParent<Waiter>() != null)
            {
                if (_shiftColor != null)
                {
                    if (_shiftColor != null)
                    {
                        StopCoroutine(_shiftColor);
                        _shiftColor = null;
                    }

                    _shiftColor = StartCoroutine(ShiftColor(_off));
                }
            }
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
}