using MobileGameProject.Framework;
using UnityEngine;

namespace MobileGameProject.DragRacer
{
    public class DragRacerGame : AMicrogameBehaviour
    {
        [Header("Scene Game Objects")] [SerializeField]
        private Vehicle _vehicle;

        public Vehicle Vehicle => _vehicle;

        public override void EndGame()
        {
            if (true)
                Win();
            else
                Lose();
        }
    }
}