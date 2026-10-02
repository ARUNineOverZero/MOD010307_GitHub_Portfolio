using System;
using TMPro;
using UnityEngine;
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
        [SerializeField] private TextMeshProUGUI _targetText;

        [Header("Rules")]
        [Tooltip("Points needed to win before the timer runs out.")]
        [SerializeField, Min(1)] private int _scoreToWin = 10;
        [Tooltip("Chance (0 = never, 1 = always) that the next target is a decoy.")]
        [SerializeField, Min(0f)] private float _decoyChance = 0.25f;
        [Tooltip("Points lost for tapping a decoy.")]
        [SerializeField, Min(0f)] private int _decoyPenalty = 2;

        [Header("Reaction scoring (seconds)")]
        [SerializeField, Min(0f)] private float _perfectTime = 0.5f;
        [SerializeField, Min(0f)] private float _greatTime = 1.0f;

        [Header("Shrinking Target")]
        [SerializeField, Min(10f)] private float _startSize = 240f;
        [SerializeField, Min(10f)] private float _minimumSize = 100f;
        [Tooltip("How many pixels the target loses from its width and height every second.")]
        [SerializeField, Range(0f, 300f)] private float _shrinkPetSecond = 80f;

        [Header("Presentation")]
        [SerializeField] private Color _safeColor = new Color(.2f, .8f, .4f);
        [SerializeField] private Color _decoyColor = new Color(.9f, .2f, .2f);
        [SerializeField] private bool _showReactionTime = true;

        private int _tapCount = 1;

        private int _score;
        private float _reactionTimer;
        private float _currentSize;
        private bool _isDecoy;

        public override void Begin(MicrogameSession session)
        {
            base.Begin(session);
            _score = 0;
            _isDecoy = false;
            SetTargetColor(_safeColor);
            ShowFeedback("Go!");
            UpdateProgress();
            ShowNextTarget();
        }


        private void Update()
        {
            if (!IsRunning) return;

            _reactionTimer += Time.deltaTime;
            SetTargetSize(_currentSize - _shrinkPetSecond * Time.deltaTime);

            if (IsTargetTooSmall())
                TargetExpired();
        }

        public void TapTarget()
        {
            if (!IsRunning) return;
            
            if(_isDecoy)
            {
                AddScore(-_decoyPenalty);
                ShowFeedback($"Decoy! -{_decoyPenalty}");
                return;
            }

            _tapCount -= 1;
            
            if(_tapCount == 0)
            {
                int points = CalculatePoints(_reactionTimer);
                AddScore(points);

                string message = $"{GetRating(points)} +{points}";

                if(_showReactionTime)
                    message += $"   ({_reactionTimer:0.00s})";
            
                ShowFeedback(message);

                if (_score >= _scoreToWin)
                    Win();
                else
                    ShowNextTarget();
            }
            else 
                _targetText.text = _tapCount.ToString();
        }

        private void TargetExpired()
        {
            ShowFeedback(_isDecoy ? "Good Dodge!" : "Too Slow!");

            ShowNextTarget();
        }

        private void ShowNextTarget()
        {
            bool wasDecoy = _isDecoy;

            _isDecoy = _score == 0 || wasDecoy 
                        ? false 
                        : _isDecoy = UnityEngine.Random.value < _decoyChance;

            _tapCount = UnityEngine.Random.Range(1,5);

            _targetText.text = _isDecoy ? "TAP!" : _tapCount.ToString();

            SetTargetColor(_isDecoy ? _decoyColor : _safeColor);

            SetTargetSize(_startSize);
            _target.anchoredPosition = GetRandomPosition(_playArea, _startSize);
            _reactionTimer = 0f;
        }

        

        private int CalculatePoints(float reactionTime) => reactionTime <= _perfectTime
                                                                ? 3
                                                                : reactionTime <= _greatTime
                                                                    ? 2
                                                                    : 1;
                                                                    
        private object GetRating(int points) 
        {
            switch(points)
            {
                case 3:
                    return "Perfect!";
                case 2: 
                    return"Great!";
                default:
                return "Good";
            }
        }

        private void AddScore(int amount = 1)
        {
            _score += amount;

            if(_score <= 0)
                _score = 0;

            UpdateProgress();
        }

        private void SetTargetSize(float size)
        {
            _currentSize = size;
            _target.sizeDelta = new Vector2(size, size);
        }

        private bool IsTargetTooSmall() => _currentSize <= _minimumSize;

        private void SetTargetColor(Color color) => _targetImage.color = color;

        private void ShowFeedback(string message) => _feedbackText.text = message;

        private void UpdateProgress() => _progressText.text = $"Score: {_score} / {_scoreToWin}";

        private Vector2 GetRandomPosition(RectTransform playArea, float startSize)
        {
            _currentSize = startSize;
            _target.sizeDelta = new Vector2(_currentSize, _currentSize);
            _reactionTimer = 0f;

            float maxX = (playArea.rect.width - _target.rect.width) * .5f;
            float maxY = (playArea.rect.height - _target.rect.height) * .5f;
            float x = UnityEngine.Random.Range(-maxX, maxX);
            float y = UnityEngine.Random.Range(-maxY, maxY);
            return new Vector2(x, y);
        }
    }
}