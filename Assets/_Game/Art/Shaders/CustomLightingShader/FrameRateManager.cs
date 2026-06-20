using UnityEngine;

namespace Art.Shaders.CustomLightingShader
{
    public class FrameRateManager : MonoBehaviour
    {
        [SerializeField] private int targetFPS = 60;

        private void Awake()
        {
            Debug.LogError("This script(FrameRateManager) should remove later !!!", gameObject);
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFPS;
        }
    }
}