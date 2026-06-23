using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.Core
{
    public static class GameInitializeChecker
    {
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static async void Check()
        {
            if (!Application.isPlaying)
                return;

            var currentScene = SceneManager.GetActiveScene();

            if (currentScene.name != nameof(SceneType.Starter))
            {
                Debug.LogError("The game was not started from Starter scene.");
                //SceneManager.LoadScene(nameof(SceneType.Starter));
            }
        }
#endif
    }
}