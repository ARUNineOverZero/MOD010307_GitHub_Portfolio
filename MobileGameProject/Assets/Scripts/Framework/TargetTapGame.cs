using System;
using Mono.Cecil;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

namespace MobileGameProject.Framework
{
    public class TargetTapGame : AMicrogameBehaviour
    {
        [Header("Scene Reference")]

        [SerializeField] private RectTransform _playArea;
        [SerializeField] private RectTransform _target;
        [SerializeField] private Image _targetImage;
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField, Min(1)] private int _tapsToWin = 5;
        [SerializeField] private TextMeshProUGUI _feedbackText;

        [Header("Rules")]
        [Tooltip("Points needed to win before the timer runs out.")]
        [SerializeField, Min(1)] private int _scoreToWin = 10;

        [Header("Shrinking Target")]

        [SerializeField, Min(10f)] private float _startSize = 240f ;
        [SerializeField, Min(10f)] private float _minimumSize = 100f;
        [Tooltip("How many pixels the target loses from its width and height every second.")]
        [SerializeField, Range(0f, 300f)] private float _shrinkPetSecond = 80f;

        [Header("Presentation")]
        [SerializeField] private Color _safeColor = new Color(.2f, .8f, .4f);
        [SerializeField] private bool _showReactionTime = true;

        private int _score;
        private float _reactionTimer;

        //private int _tapsRemaining;
        private float _currentSize;

        public override void Begin(MicrogameSession session)
        {
            base.Begin(session);
            _score = 0;
            _targetImage.color = _safeColor;
            _feedbackText.text = "Go!";
            //_tapsRemaining = _tapsToWin;
            UpdateProgress();
            MoveTarget();
        }

        private void Update()
        {
            if(!IsRunning) return;

            _reactionTimer += Time.deltaTime;
            _currentSize -= _shrinkPetSecond * Time.deltaTime;
            _target.sizeDelta = new Vector2(_currentSize,_currentSize);
            if(_currentSize <= _minimumSize)
            {
                _feedbackText.text = "Too slow!";
                MoveTarget();
            }
        }

        public void TapTarget()
        {
            if(!IsRunning) return;

            //_tapsRemaining--;
            _score++;
            UpdateProgress();

            if(_showReactionTime)
                _feedbackText.text = _showReactionTime ? $"Hit! {_reactionTimer:0.00}s" : "Hit!.";

            if(_score >= _scoreToWin)
                Win();
            else
                MoveTarget();
        }

        private void UpdateProgress()
        {
            _progressText.text = $"Score: {_score} / {_scoreToWin}";
        }

        private void MoveTarget()
        {
            _currentSize = _startSize;
            _target.sizeDelta = new Vector2(_currentSize, _currentSize);
            _reactionTimer = 0f;

            float maxX = (_playArea.rect.width - _target.rect.width) * .5f;
            float maxY = (_playArea.rect.height - _target.rect.height) * .5f;
            float x = UnityEngine.Random.Range(-maxX, maxX);
            float y = UnityEngine.Random.Range(-maxY, maxY);
            _target.anchoredPosition = new Vector2(x,y);

        }

       
    }

}