using System.Collections.Generic;
using _Game.Scripts.Utils.VContainer;
using Cysharp.Threading.Tasks;
using SpektraGames.SpektraUtilities.Runtime;
using UnityEngine;

namespace Analytics.AppsFlyer
{
    public static class AppsFlyerIdObtainer
    {
        private static FirebaseAnalyticsService _firebaseAnalyticsService;
        private static FirebaseAnalyticsService FirebaseAnalyticsService
        {
            get
            {
                if (_firebaseAnalyticsService == null)
                    _firebaseAnalyticsService = ServiceLocator.GetService<FirebaseAnalyticsService>();
                return _firebaseAnalyticsService;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            InitializeAsync().Forget();
        }

        private static async UniTaskVoid InitializeAsync()
        {
            if (Application.isEditor)
                return;

            var infoLogger = new InfoLogger("AppsFlyer", "blue");
            string appsFlyerId = string.Empty;
            while (string.IsNullOrEmpty(appsFlyerId) || !FirebaseAnalyticsService.isFirebaseInitialized ||
                   FirebaseAnalyticsService.isFirebaseInitializedWithError)
            {
                await UniTask.WaitForSeconds(5f);
                appsFlyerId = AppsFlyerSDK.AppsFlyer.getAppsFlyerId();
            }

            infoLogger.Log($"MMP id: {appsFlyerId}");
            if (FirebaseAnalyticsService.isFirebaseInitializedWithError)
            {
                infoLogger.LogError("Could not send apps flyer id since firebase is initialized with error");
            }
            else
            {
                FirebaseAnalyticsService.SetUserProperty("mmp_id", appsFlyerId);
            }

            //TODO: Write mmpId(appsFlyerId) to user properties of clutch like this:
            //ClutchAnalyticsService.SetUserProperty("mmpId", appsFlyerId);
        }
    }
}