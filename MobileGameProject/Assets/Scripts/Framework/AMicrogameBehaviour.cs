namespace MobileGameProject.Framework
{
    public abstract class AMicrogameBehaviour
    {
        //Microgame session
        private bool _isRunning = false;
        public bool IsRunning => _isRunning;

        private MicrogameSession _session;
        protected MicrogameSession Session => _session;
        
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
    }
}