using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace MobileGameProject.Framework
{
    public sealed class MicrogameSession : MonoBehaviour
    {
        private enum Phase
        {
            Ready = 0,
            Plaiying = 1,
            Result = 2
        }

        [SerializeField] private AMicrogameBehaviour _game;
        [SerializeField] private GameObject _readyPanel;
        [SerializeField] private GameObject _playArea;
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private Text _timerText;
        [SerializeField] private Text _resultText;
        [SerializeField, Min(1f)] private float _durationSeconds = 10f;
        private Phase _currentPhase;
        private float _remainingSeconds;

        private bool _won;

        private void Awake()
        {
            _currentPhase = Phase.Ready;
            _readyPanel.SetActive(true);
            _playArea.SetActive(false);
            _resultPanel.SetActive(false);
            _timerText.text = string.Empty;
        }

        private void Update()
        {
            if(_currentPhase != Phase.Plaiying)
                return;

            _remainingSeconds = Mathf.Max(0, _remainingSeconds -Time.deltaTime);
            ShowTime();

            if(_remainingSeconds == 0)
                Finish(false);
        }

        public void Finish(bool v)
        {
            if(_currentPhase != Phase.Plaiying)
                return;

            _currentPhase = Phase.Result;
            _game.End();
            _playArea.SetActive(false);
            _resultPanel.SetActive(true);
            _resultText.text = _won ? "You Win!" 
                                    : "You LOST!";
        }

        public void StartGame()
        {
            if(_currentPhase != Phase.Ready || gameObject == null)
                return;


            _remainingSeconds = _durationSeconds;
            _readyPanel.SetActive(false);
            _playArea.SetActive(true);
            _resultPanel.SetActive(false);
            _currentPhase = Phase.Plaiying;

            ShowTime();    
        }

      

        private void ShowTime()
        {
            _timerText.text = $"Time: {_remainingSeconds:0.0}"; //String interpolation
        }
    }
}