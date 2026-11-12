using System.Collections;
using UnityEngine;

namespace MobileGameProject.Pinball
{
    public class Block : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _moveDown = 1f;
        [SerializeField, Min(0f)] private float _speed = 1f;
        private float _currentPosition = 0f;
        private Coroutine _moveDownCoroutine;

        protected virtual void TriggerCoroutines()
        {
            if(_moveDownCoroutine == null)
                _moveDownCoroutine = StartCoroutine(MoveDown());
        }

        private IEnumerator MoveDown()
        {
            if(_moveDown == 0f || _speed == 0f) yield break;

            while(_currentPosition != 1)
            {
                _currentPosition += Time.fixedDeltaTime * _speed;

                if(_currentPosition > 1)
                    _currentPosition = 1;

                var localPosition = transform.localPosition;
                localPosition.y = _currentPosition * -_moveDown;
                transform.localPosition = localPosition;

                yield return null;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            Ball ball = collision.gameObject.GetComponent<Ball>();
            if(ball != null)
                TriggerCoroutines();
        }
    }
}