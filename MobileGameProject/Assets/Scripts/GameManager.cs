using UnityEditorInternal;
using UnityEngine;


namespace MobileGameProject
{
    public class GameManager : MonoBehaviour
    {
        #region Static

        private static GameManager _instance;
        public static GameManager Instance => _instance;

        public static bool CreateGameManager(GameManager gameManager)
        {
            if(_instance != null)
            {
                Destroy(gameManager.gameObject);
                return false;
            }

            _instance = gameManager;
            DontDestroyOnLoad(_instance);

               return true;
        }

        #endregion Static


        public void Awake()
        {
            if(!CreateGameManager(this))
                return;
        }
    }
}