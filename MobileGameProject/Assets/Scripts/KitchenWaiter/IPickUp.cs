using UnityEngine;

namespace MobileGameProject.KitchenWaiter
{
    public interface IPickUp
    {
        
    } 

    public interface IHeldItem : IPickUp
    {
        Transform Transform { get; }

        void Holding();
    }
}