using System;
using Mono.Cecil;
using TMPro;
using UnityEditor;
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

        [SerializeField] private float _startSize = 240f ;
        [SerializeField] private float _minimumSize = 100f;
        [SerializeField] private float _shrinkPetSecond = 80f;

        private int _tapsRemaining;
        private float _currentSize;

        public override void Begin(MicrogameSession session)
        {
            base.Begin(session);
            _tapsRemaining = _tapsToWin;
            UpdateProgress();
            MoveTarget();
        }

        private void Update()
        {
            if(!IsRunning) return;

            _currentSize -= _shrinkPetSecond * Time.deltaTime;
            _target.sizeDelta = new Vector2(_currentSize,_currentSize);
            if(_currentSize <= _minimumSize)
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
            _currentSize = _startSize;
            _target.sizeDelta = new Vector2(_currentSize, _currentSize);
            float maxX = (_playArea.rect.width - _target.rect.width) * .5f;
            float maxY = (_playArea.rect.height - _target.rect.height) * .5f;
            float x = UnityEngine.Random.Range(-maxX, maxX);
            float y = UnityEngine.Random.Range(-maxY, maxY);
            _target.anchoredPosition = new Vector2(x,y);

        }

       
    }

}