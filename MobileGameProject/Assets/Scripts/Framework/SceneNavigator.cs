using UnityEngine;
using UnityEngine.SceneManagement;

namespace MobileGameProject.Framework
{
    public class SceneNavigator : MonoBehaviour
    {
        public void OpenMainMenu()
        {
            LoadScene("MainMenu");
        }

        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
        
        public void Replay()
        {
            LoadScene(SceneManager.GetActiveScene().name);
        }
    }

}