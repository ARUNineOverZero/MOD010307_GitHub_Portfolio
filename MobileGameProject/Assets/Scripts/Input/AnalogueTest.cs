using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MobileGameProject.MGPInputSystem
{
    public class AnalogueTest : MonoBehaviour
    {
        public AnalogueStick analogue;

        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        public void Update()
        {
            if(Input.GetMouseButtonDown(0))
            {
                //analogue.StartInteraction(_mainCamera.ScreenToWorldPoint(Pointer.current.position.ReadValue()));
                analogue.gameObject.SetActive(true);
            }

            if(Input.GetMouseButtonUp(0))
            {
                analogue.FinishInteraction();
                analogue.gameObject.SetActive(false);
            }


            Debug.Log($"{((IVector2Value)analogue).Value.x}, {((IVector2Value)analogue).Value.y}");
        }
    }
}