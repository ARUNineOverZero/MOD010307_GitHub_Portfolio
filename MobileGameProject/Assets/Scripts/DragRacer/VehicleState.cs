namespace MobileGameProject.DragRacer
{
    public abstract class VehicleState
    {
        protected Vehicle _vehicle;
        public abstract void Enter();
        public abstract void Update(float deltaTime);
        public abstract void Exit();

        public VehicleState(Vehicle vehicle) => _vehicle = vehicle;
    }
}