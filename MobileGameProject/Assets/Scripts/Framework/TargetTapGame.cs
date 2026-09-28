using System;
using Mono.Cecil;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileGameProject.Framework
{
    public class TargetTapGame : AMicrogameBehaviour
    {
        [SerializeField] private RectTransform _playArea;
        [SerializeField] private RectTransform _target;
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField, Min(1)] private int _tapsToWin = 5;

        private int _tapsRemaining;

        public override void Begin(MicrogameSession session)
        {
            base.Begin(session);
            _tapsRemaining = _tapsToWin;
            UpdateProgress();
            MoveTarget();
        }

        public void TapTarget()
        {
            if(!IsRunning) return;

            _tapsRemaining--;
            UpdateProgress();

            if(_tapsRemaining == 0)
                Win();
            else
                MoveTarget();
        }

        private void UpdateProgress()
        {
            _progressText.text = $"Taps left: {_tapsRemaining}";
        }

        private void MoveTarget()
        {
            float maxX = (_playArea.rect.width - _target.rect.width) * .5f;
            float maxY = (_playArea.rect.height - _target.rect.height) * .5f;
            float x = UnityEngine.Random.Range(-maxX, maxX);
            float y = UnityEngine.Random.Range(-maxY, maxY);
            _target.anchoredPosition = new Vector2(x,y);

        }

       
    }

}