using System;
using TMPro;
using UnityEngine;

namespace MobileGameProject.Framework
{
    public class MicrogameSession : MonoBehaviour
    {
        private enum Phase
        {
            Ready = 0,
            Plaiying = 1,
            Result = 2
        }

        [SerializeField] protected AMicrogameBehaviour _game;
        [SerializeField] private GameObject _readyPanel;
        [SerializeField] private GameObject _playArea;
        [SerializeField] private GameObject _uiPlayArea;
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private TextMeshProUGUI _resultText;

        private Phase _currentPhase;


        private bool _won;

        protected virtual void Awake()
        {
            _currentPhase = Phase.Ready;
            _readyPanel.SetActive(true);
            _playArea.SetActive(false);

            if(_uiPlayArea != null)
                _uiPlayArea.SetActive(false);

            _resultPanel.SetActive(false);
        }

        public void Finish(bool v)
        {
            if (_currentPhase != Phase.Plaiying)
                return;

            _currentPhase = Phase.Result;
            _game.End();
            _playArea.SetActive(false);

            if(_uiPlayArea != null)
                _uiPlayArea.SetActive(false);

            _resultPanel.SetActive(true);
            _won = v;
            _resultText.text = _won ? "You Win!"
                                    : "You LOST!";

            FinishExtention();
        }

        public void StartGame()
        {
            if (_currentPhase != Phase.Ready || gameObject == null)
                return;

            _readyPanel.SetActive(false);
            _playArea.SetActive(true);

            if(_uiPlayArea != null)
                _uiPlayArea.SetActive(true);

            _resultPanel.SetActive(false);
            _currentPhase = Phase.Plaiying;

            StartExtention();

            _game.Begin(this); 
        }

        public virtual void StartExtention() { }
        public virtual void FinishExtention() { }
    }
}