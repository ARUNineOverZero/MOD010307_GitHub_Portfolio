using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MobileGameProject.KitchenWaiter
{
    public class TestTrigger : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _sr;
        [SerializeField] private Color _off;
        [SerializeField] private Color _on;
        [SerializeField] private float _current = 0;
        [SerializeField] private float _speed = .25f;


        private Coroutine _shiftColor;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.transform.root.GetComponent<Waiter>() != null)
            {
                if (_shiftColor != null)
                {
                    StopCoroutine(_shiftColor);
                    _shiftColor = null;
                }

                _shiftColor = StartCoroutine(ShiftColor(1));
                ShiftColor(1);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.transform.root.GetComponent<Waiter>() != null)
            {
                if (_shiftColor != null)
                {
                    if (_shiftColor != null)
                    {
                        StopCoroutine(_shiftColor);
                        _shiftColor = null;
                    }

                    _shiftColor = StartCoroutine(ShiftColor(0));
                }
            }
        }

        private IEnumerator ShiftColor(float target)
        {
            while (true)
            {
                if (_current == target) break;

                float diff = _speed * Time.deltaTime;
                if (_current < target)
                {
                    _current += diff;
                    if (_current > target)
                        _current = target;
                }
                else
                {
                    _current -= diff;
                    if (_current < target)
                        _current = target;
                }

                _sr.color = Color.Lerp(_off, _on, _current);
                yield return null;
            }
        }
    }
}