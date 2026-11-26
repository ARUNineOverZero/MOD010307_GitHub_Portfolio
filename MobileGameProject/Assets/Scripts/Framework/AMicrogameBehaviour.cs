using System;
using UnityEngine;

namespace MobileGameProject.Framework
{
    public abstract class AMicrogameBehaviour : MonoBehaviour
    {
        //Microgame session
        private bool _isRunning = false;
        public bool IsRunning => _isRunning;

        private MicrogameSession _session;
        protected MicrogameSession Session => _session;

        [SerializeField] protected Camera _mainCamera;

        [SerializeField] protected Orientation _orientation = Orientation.Verticle;
        
        public virtual void Begin(MicrogameSession session)
        {
            _session = session;
            _isRunning = true;
        }

        public virtual void End()
        {
            _isRunning = false;
        }

        protected void Win()
        {
            if(_isRunning) 
                _session.Finish(true);
        }

        protected void Lose()
        {
            if(_isRunning) 
                _session.Finish(false);
        }

        public virtual void EndGame()
        {
        }

        public virtual void Awake()
        {
            if (_orientation == Orientation.Horizontal)
                SetHorizontal();
            else
                SetVertical();
        }
        
        private void SetHorizontal()
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
        }
        
        private void SetVertical()
        {
            Screen.orientation = ScreenOrientation.Portrait;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
        }
    }

    public enum Orientation
    {
        Horizontal,
        Verticle
    }
}