using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace MobileGameProject.Pinball
{
    public class Block : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _moveDown = 1f;
        [SerializeField, Min(0f)] private float _moveSpeed = 1f;
        [SerializeField] protected Renderer _renderer;
        [SerializeField] private Color _color = Color.white;
        private float _current = 0f;
        private Coroutine _moveDownCoroutine;
        private MaterialPropertyBlock _propertyBlock;
        private static readonly int ColorId = Shader.PropertyToID("_BaseColor");

        protected virtual void OnValidate()
        {
            if (_renderer != null)
                ApplyColor(ref _propertyBlock, ColorId, _color);
        }

        protected void ApplyColor(ref MaterialPropertyBlock propertyBlock, int id, Color color)
        {
            if (propertyBlock == null)
                propertyBlock = new MaterialPropertyBlock();

            _renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(id, color);
            _renderer.SetPropertyBlock(propertyBlock);
        }

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
