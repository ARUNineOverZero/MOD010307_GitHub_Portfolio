using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace MobileGameProject.Pinball
{
    public class Block : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _moveDown = 1f;
        [SerializeField, Min(0f)] private float _moveSpeed = 1f;
        private float _current = 0f;
        private Coroutine _moveDownCoroutine;

        protected virtual void TriggerCoroutines()
        {
            if (_moveDownCoroutine == null)
                _moveDownCoroutine = StartCoroutine(MoveDown());
        }

        private IEnumerator MoveDown()
        {
            if (_moveDown == 0f || _moveSpeed == 0f) yield break;

            while (_current != 1)
            {
                _current += Time.deltaTime / _moveSpeed;

                if (_current > 1)
                    _current = 1;

                var localPosition = transform.localPosition;
                localPosition.y = _current * -_moveDown;
                transform.localPosition = localPosition;

                yield return null;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            Ball ball = collision.gameObject.GetComponent<Ball>();
            if (ball != null)
                TriggerCoroutines();
        }
    }
}
