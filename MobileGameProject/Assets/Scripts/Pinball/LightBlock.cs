using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace MobileGameProject.Pinball
{
    public class LightBlock : Block
    {
        [SerializeField] private AnimationCurve _lightIntensityCurve; 
        [SerializeField, Min(0.001f)] private float _lightIntensitySpeed = 1f;
        [SerializeField, Min(0.001f)] private float _lightIntensity = 1f;
        [SerializeField] private Light _light;
        private MaterialPropertyBlock _propertyBlock;
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private float _current = 0;

        private Coroutine _blinkLightCoroutine;

        protected override void OnValidate()
        {
            base.OnValidate();
            
            if(_light != null)
                _light.color = _color;
        }

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
                _current += Time.deltaTime / _lightIntensitySpeed / 2;

                if (_current > 2)
                    _current = 2;

                float currentPosition = _current > 1 ? 1 - (_current - 1) : _current;
                float intensity = _lightIntensity * _lightIntensityCurve.Evaluate(currentPosition);
                _light.intensity = intensity;
                ApplyColor(ref _propertyBlock, EmissionColorId, _color * intensity);

                yield return null;
            }

            Debug.Log("Exit Blink Light");
            
            _current = 0;
            _light.enabled = false;
            _blinkLightCoroutine = null;
        }
    }
}
