using System.Collections;
using UnityEngine;

namespace MobileGameProject.Pinball
{
    public class LightBlock : Block
    {
        [SerializeField, Min(0.001f)] private float _lightIntensitySpeed = 1f;
        [SerializeField, Min(0.001f)] private float _lightIntensity = 1f;
        [SerializeField] private Light _light;
        private float _current = 0;

        private Coroutine _blinkLightCoroutine;

        protected override void TriggerCoroutines()
        {
            base.TriggerCoroutines();

            if(_blinkLightCoroutine == null)
                _blinkLightCoroutine = StartCoroutine(BlinkLight());
            else if (_current > 1)
                _current =  1 - (_current - 1);
        }

        private IEnumerator BlinkLight()
        {
            _light.enabled = true;
            while (_current != 2)
            {
                _current += Time.deltaTime / _lightIntensitySpeed;

                if (_current > 2)
                    _current = 2;

                float currentPosition = _current > 1 ? 1 - (_current - 1) : _current;

                _light.intensity = _lightIntensity * currentPosition;

                yield return null;
            }


            _light.enabled = false;
            _blinkLightCoroutine = null;
        }
    }
}
