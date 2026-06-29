using SpektraGames.SpektraUtilities.Runtime;
using UnityEngine;

namespace Analytics.AppsFlyer
{
    [CreateAssetMenu(fileName = "AppsFlyerConfig", menuName = "Configs/AppsFlyer Config")]
    public class AppsFlyerConfig : SingletonScriptableObject<AppsFlyerConfig>
    {
        [Header("AppsFlyer Configuration")]
        [Tooltip("The App ID for the iOS version of the application.")]
        public string iOSAppId;

        [Tooltip("The App ID for the Android version of the application.")]
        public string androidAppId;

        [Tooltip("The developer key for AppsFlyer.")]
        public string devKey;
    }
}