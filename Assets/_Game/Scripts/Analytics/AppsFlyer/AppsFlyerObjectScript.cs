using System.Collections.Generic;
using AppsFlyerSDK;
using UnityEngine;

namespace Analytics.AppsFlyer
{
    [AddComponentMenu("AppsFlyerObjectScript")]
    public class AppsFlyerObjectScript : MonoBehaviour,
        IAppsFlyerConversionData, // For conversion data callbacks
        IAppsFlyerPurchaseValidation, // For purchase validation callbacks  
        IAppsFlyerPurchaseRevenueDataSource, // For StoreKit 1 additional parameters 
        IAppsFlyerPurchaseRevenueDataSourceStoreKit2 // For StoreKit 2 additional parameters 
    {
        private string _uwpAppID;
        private string _macOSAppID;
        private bool _isDebug;
        private bool _getConversionData;

        // ---------------------------------------------------------------------
        // Diagnostic instrumentation (read-only). These flags record how far the
        // AppsFlyer init in Start() progressed on this launch and are surfaced via
        // SRDebugger (see SROptions.AppsFlyer) to pinpoint, on a real device, where
        // AppsFlyer initialization stops — WITHOUT changing any init behaviour.
        // If StartSdkCalled stays false, startSDK() never ran and the install/launch
        // event was never sent (e.g. the unguarded PurchaseConnector block threw).
        // ---------------------------------------------------------------------
        public static bool AwakeCalled { get; private set; }
        public static bool ConfigLoaded { get; private set; }
        public static bool InitSdkCalled { get; private set; }
        public static bool PurchaseConnectorConfigured { get; private set; }
        public static bool StartSdkCalled { get; private set; }

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void AFODM_Initialize();
#endif

        private void Awake()
        {
#if UNITY_IOS && !UNITY_EDITOR
        // 1) Load GoogleAdsOnDeviceConversion (ODM)
        AFODM_Initialize();
#endif

#if DEV_GAME_ENVIRONMENT
            _isDebug = true;
#else
        _isDebug = false;
#endif
            DontDestroyOnLoad(this);
            AwakeCalled = true;
        }

        private void Start()
        {
            var appsFlyerConfig = AppsFlyerConfig.Instance;
            if (appsFlyerConfig == null)
            {
                Debug.LogError("AppsFlyerConfig is null");
                return;
            }

            ConfigLoaded = true;

            if (_isDebug)
                Debug.Log("<color=blue>[AppsFlyer]</color> AppsFlyerObjectScript Start");

#if UNITY_IOS && !UNITY_EDITOR
        AppsFlyerSDK.AppsFlyer.waitForATTUserAuthorizationWithTimeoutInterval(60);
#endif
#if UNITY_WSA_10_0 && !UNITY_EDITOR
        AppsFlyerSDK.AppsFlyer.initSDK(appsFlyerConfig.devKey, _uwpAppID, getConversionData ? this : null);
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
        //AppsFlyerSDK.AppsFlyer.initSDK(appsFlyerConfig.devKey, _macOSAppID, getConversionData ? this : null);
#elif UNITY_IOS
        AppsFlyerSDK.AppsFlyer.initSDK(appsFlyerConfig.devKey, appsFlyerConfig.iOSAppId, _getConversionData ? this : null);
#elif UNITY_ANDROID
            AppsFlyerSDK.AppsFlyer.initSDK(appsFlyerConfig.devKey, appsFlyerConfig.androidAppId, _getConversionData ? this : null);
#endif

            InitSdkCalled = true;

            AppsFlyerSDK.AppsFlyer.setIsDebug(_isDebug);
#if UNITY_IOS
        AppsFlyerSDK.AppsFlyer.setUseUninstallSandbox(_isDebug);
#endif

            // Purchase connector setup, kept in AppsFlyer's documented order (connector built and
            // observing BEFORE startSDK). The block is guarded only so that a connector failure —
            // e.g. the Google Play Billing version bundled by Unity IAP conflicting with the one
            // AppsFlyer's purchase-connector expects — cannot abort Start() before startSDK() runs.
            // That unguarded throw is what silently killed install attribution on Android only
            // (iOS uses StoreKit and was unaffected). This is a containment guard, NOT the real fix
            // for the billing conflict; purchase-revenue logging stays broken until that is resolved.
            try
            {
                AppsFlyerPurchaseConnector.init(this, Store.GOOGLE);
                ConfigurePurchaseConnector();
                AppsFlyerPurchaseConnector.build();
                AppsFlyerPurchaseConnector.startObservingTransactions();
                PurchaseConnectorConfigured = true;
            }
            catch (System.Exception e)
            {
                Debug.LogError("[AppsFlyer] PurchaseConnector init failed; continuing to startSDK so install tracking still fires: " + e);
            }

            AppsFlyerSDK.AppsFlyer.startSDK();
            StartSdkCalled = true;
        }

        private void ConfigurePurchaseConnector()
        {
            // Set sandbox mode for testing
            AppsFlyerPurchaseConnector.setIsSandbox(_isDebug);

            // Configure StoreKit version (iOS only) - SK1 is the default
            AppsFlyerPurchaseConnector.setStoreKitVersion(StoreKitVersion.SK2);

            // Enable automatic logging for subscriptions and in-app purchases
            AppsFlyerPurchaseConnector.setAutoLogPurchaseRevenue(
                AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsAutoRenewableSubscriptions,
                AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsInAppPurchases
            );

            // Enable purchase validation callbacks
            AppsFlyerPurchaseConnector.setPurchaseRevenueValidationListeners(true);

            // Set data sources for additional parameters (iOS) - SK1
            AppsFlyerPurchaseConnector.setPurchaseRevenueDataSource(this);
            // Set data sources for additional parameters (iOS) - SK2
            AppsFlyerPurchaseConnector.setPurchaseRevenueDataSourceStoreKit2(this);
        }

        // Mark AppsFlyer CallBacks
        public void onConversionDataSuccess(string conversionData)
        {
            AppsFlyerSDK.AppsFlyer.AFLog("didReceiveConversionData", conversionData);
            Dictionary<string, object> conversionDataDictionary = AppsFlyerSDK.AppsFlyer.CallbackStringToDictionary(conversionData);
            // add deferred deeplink logic here
        }

        public void onConversionDataFail(string error)
        {
            AppsFlyerSDK.AppsFlyer.AFLog("didReceiveConversionDataWithError", error);
        }

        public void onAppOpenAttribution(string attributionData)
        {
            AppsFlyerSDK.AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
            Dictionary<string, object> attributionDataDictionary = AppsFlyerSDK.AppsFlyer.CallbackStringToDictionary(attributionData);
            // add direct deeplink logic here
        }

        public void onAppOpenAttributionFailure(string error)
        {
            AppsFlyerSDK.AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
        }

        public void onValidateAndLogComplete(string result)
        {
            AppsFlyerSDK.AppsFlyer.AFLog("onValidateAndLogComplete", result);
        }

        public void onValidateAndLogFailure(string error)
        {
            AppsFlyerSDK.AppsFlyer.AFLog("onValidateAndLogFailure", error);
        }

        public void didFinishValidateReceipt(string result)
        {
            AppsFlyerSDK.AppsFlyer.AFLog("didFinishValidateReceipt", result);
        }

        public void didFinishValidateReceiptWithError(string error)
        {
            AppsFlyerSDK.AppsFlyer.AFLog("didFinishValidateReceiptWithError", error);
        }

        public void didReceivePurchaseRevenueValidationInfo(string validationInfo)
        {
        }

        public void didReceivePurchaseRevenueError(string error)
        {
        }

        public Dictionary<string, object> PurchaseRevenueAdditionalParametersForProducts(HashSet<object> products, HashSet<object> transactions)
        {
            // Add custom parameters to purchase events
            return new Dictionary<string, object>
            {
                ["purchase_source"] = "main_store"
            };
        }

        public Dictionary<string, object> PurchaseRevenueAdditionalParametersStoreKit2ForProducts(HashSet<object> products, HashSet<object> transactions)
        {
            // Add custom parameters specifically for StoreKit 2 purchases
            return new Dictionary<string, object>
            {
                ["sk2_custom_param"] = "sk2_value"
            };
        }
    }
}