using System.Collections;
using TMPro;
using UnityEngine;

namespace MobileGameProject.Framework
{
    public sealed class CountDownMicrogameSession : MicrogameSession
    {
        [SerializeField] private TextMeshProUGUI _timerText;

        [SerializeField, Min(1f)] private float _durationSeconds = 10f;
        protected Coroutine _countDownCoroutine;

        protected override void Awake()
        {
            base.Awake();
            _timerText.text = string.Empty;
        }

        public override void StartExtention()
        {
            _countDownCoroutine = StartCoroutine(CountDown());
        }

        public override void FinishExtention()
        {
            if (_countDownCoroutine != null)
            {
                StopCoroutine(_countDownCoroutine);
                _countDownCoroutine = null;
            }
        }

        private IEnumerator CountDown()
        {
            var remainingSeconds = _durationSeconds;

            while (remainingSeconds > 0)
            {
                remainingSeconds -= Time.deltaTime;
                UpdateTimer(Mathf.Max(0, remainingSeconds));
                yield return null;
            }

            _countDownCoroutine = null;

            Finish(false);
        }

        private void UpdateTimer(float seconds) => _timerText.text = $"Time: {seconds:0.0}";
    }
}