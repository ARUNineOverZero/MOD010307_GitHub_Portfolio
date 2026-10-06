using UnityEngine;

namespace MobileGameProject.MGPInputSystem
{
    public interface IVector2Value
    {
        Vector2 Value {get;}
        Vector2 Normalized {get;}
        float Magnitude {get;}
    }
}