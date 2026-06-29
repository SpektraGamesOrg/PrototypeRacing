#if UNITY_EDITOR || !DISABLE_SRDEBUGGER
#endif
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Analytics;
using Firebase.Crashlytics;
using SpektraGames.BuildAutomation.Runtime;
using SpektraGames.SpektraUtilities.Runtime;
using UnityEngine;

namespace Analytics
{
    public class FirebaseAnalyticsService : IAnalyticsService
    {
        public static Action OnAnyEventReported;
        public bool isFirebaseInitialized = false;
        public bool isFirebaseInitializedWithError = false;
        public bool isDependenciesChecked;

        public InfoLogger InfoLogger { get; }
        private Dictionary<string, string> userPropertyCache = new Dictionary<string, string>();

        public FirebaseAnalyticsService()
        {
            InfoLogger = new InfoLogger("Firebase", "blue");
        }

        public bool IsInitialized => isFirebaseInitialized;

        public async UniTask InitializeAsync()
        {
            try
            {
                // Await Firebase dependency resolution
                var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
                isDependenciesChecked = true;
                InfoLogger.Log($"Dependencies checked. Status: {dependencyStatus.ToString()}");

                if (dependencyStatus == DependencyStatus.Available)
                {
                    var app = FirebaseApp.DefaultInstance;
                    Crashlytics.IsCrashlyticsCollectionEnabled = true;
                    Crashlytics.ReportUncaughtExceptionsAsFatal = false;
                    FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                    FirebaseAnalytics.SetSessionTimeoutDuration(new TimeSpan(0, 30, 0));

                    // Switch execution back to the main thread
                    await UniTask.SwitchToMainThread();

                    isFirebaseInitialized = true;
                    isFirebaseInitializedWithError = false;
                    InfoLogger.Log("Firebase initialized");

                    // Unity IAP v5 moved iOS to StoreKit 2, which Firebase no longer auto-logs as in_app_purchase
                    // (it did under StoreKit 1). Subscribe so each verified Apple transaction is logged manually.
                    // The -= before += guards against a duplicate subscription if init ever runs more than once.
                    // ShopService.OnAppleStoreKitTransactionVerified -= LogAppleStoreKitTransaction;
                    // ShopService.OnAppleStoreKitTransactionVerified += LogAppleStoreKitTransaction;

                    FirebaseAnalytics.GetAnalyticsInstanceIdAsync().AsUniTask().ContinueWith(instanceIdTask =>
                    {
                        // if (instanceIdTask.IsCompleted)
                        {
                            try
                            {
                                MainThreadDispatcher.Enqueue(() =>
                                {
                                    string firebaseInstanceId = instanceIdTask;
                                    InfoLogger.Log("Firebase Instance ID: " + firebaseInstanceId);
                                    SetUserProperty("fiba_id", firebaseInstanceId);

                                    string customSessionId = Guid.NewGuid().ToString();

                                    SetUserProperty("custom_session_id", customSessionId);
                                    SetUserProperty("fiba_id", firebaseInstanceId);
                                    ReportEvent("custom_session_info", new Dictionary<string, AnalyticsEventParameter>()
                                    {
                                        { "custom_session_id", AnalyticsEventParameter.StringParam(customSessionId) },
                                    });
                                });
                            }
                            catch (Exception e)
                            {
                                Debug.LogError(e.ToString());
                            }
                        }
                    });

                    ProcessCachedUserProperties();

                    SetUserProperty("memory_size", SystemInfo.systemMemorySize.ToString());

                    string internetConnType = Application.internetReachability switch
                    {
                        NetworkReachability.ReachableViaCarrierDataNetwork => "cellular",
                        NetworkReachability.ReachableViaLocalAreaNetwork => "wifi",
                        _ => "no_connection"
                    };

                    SetUserProperty("network_conn_type", internetConnType);

                    SetUserProperty("test_user",
                        Debug.isDebugBuild || GameEnvironment.CurrentEnvironment == GameEnvironmentType.Development
                            ? "1"
                            : "0");
                }
                else
                {
                    InfoLogger.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                    isFirebaseInitialized = false;
                    isFirebaseInitializedWithError = true;
                }
            }
            catch (System.Exception e)
            {
                InfoLogger.LogError($"Firebase initialization failed: {e.Message}");
                isFirebaseInitialized = false;
                isFirebaseInitializedWithError = true;
            }
        }

        public void ReportEvent(string eventName, Dictionary<string, AnalyticsEventParameter> paramsToSend)
        {
            if (string.IsNullOrEmpty(eventName) || eventName.Length < 2)
                return;

            if (!isDependenciesChecked)
            {
                InfoLogger.LogError($"FirebaseAnalyticsProvider.ReportEvent: Dependencies are not checked yet. Event name: {eventName}");
                return;
            }

            if (paramsToSend != null)
            {
                List<string> paramsToRemove = null;
                foreach (var keyValuePair in paramsToSend)
                {
                    if (
                        string.IsNullOrEmpty(keyValuePair.Key) ||
                        (keyValuePair.Value.parameterType == AnalyticsEventParameter.ParameterType.STRING &&
                         string.IsNullOrEmpty(keyValuePair.Value.stringValue))
                    )
                    {
                        if (paramsToRemove == null)
                            paramsToRemove = new List<string>();

                        paramsToRemove.Add(keyValuePair.Key);
                    }
                }

                if (paramsToRemove != null)
                {
                    for (int i = 0; i < paramsToRemove.Count; i++)
                    {
                        paramsToSend.Remove(paramsToRemove[i]);
                        InfoLogger.Log($"Removing event param {paramsToRemove[i]} since param is null or empty");
                    }
                }
            }

            if (paramsToSend == null || paramsToSend.Count <= 0)
            {
                ReportEvent(eventName);
                return;
            }

            try
            {
                eventName = FixEventName(eventName);

                if (string.IsNullOrEmpty(eventName) || eventName.Length < 2)
                    return;
            }
            catch (Exception e)
            {
                Debug.LogError("ReportEvent.ReportEvent0: " + e.ToString());
                return;
            }

            if (Application.isEditor)
            {
                try
                {
                    InfoLogger.Log("FirebaseAnalyticsProvider.ReportEventWithParam: " +
                                   Environment.NewLine + eventName +
                                   Environment.NewLine + "params: " +
                                   paramsToSend.SerializeObject(true));
                    SetSessionTimeProperty();
                }
                catch (Exception e)
                {
                    Debug.LogError("FirebaseAnalyticsProvider.ReportEventWithParam: " + e.ToString());
                }
            }

            if (Application.isEditor)
                return;

            try
            {
                //if (IsFirebaseInitialized)
                {
                    var eventStringForFirebase = eventName.Replace(":", "_");
                    var parameters = new Parameter[paramsToSend.Count];
                    int i = 0;
                    foreach (var eventParameter in paramsToSend)
                    {
                        string eventParamName = eventParameter.Key;
                        eventParamName = FixEventName(eventParamName);

                        if (string.IsNullOrEmpty(eventParamName))
                        {
                            var val = eventParameter.Value.ConvertToString();
                            parameters[i] = new Parameter("_NAN_", string.IsNullOrEmpty(val) ? "_NAN_" : val);
                        }
                        else
                        {
                            if (eventParameter.Value.parameterType == AnalyticsEventParameter.ParameterType.LONG)
                                parameters[i] = new Parameter(eventParamName, eventParameter.Value.longValue);
                            else if (eventParameter.Value.parameterType == AnalyticsEventParameter.ParameterType.DOUBLE)
                                parameters[i] = new Parameter(eventParamName, eventParameter.Value.doubleValue);
                            else if (eventParameter.Value.parameterType == AnalyticsEventParameter.ParameterType.STRING)
                                parameters[i] = new Parameter(eventParamName,
                                    FixStringEventParamValue(eventParameter.Value.stringValue));
                        }

                        ++i;
                    }

                    SetSessionTimeProperty();
                    FirebaseAnalytics.LogEvent(eventStringForFirebase, parameters);
                    OnAnyEventReported?.Invoke();
                    try
                    {
                        InfoLogger.Log("FirebaseAnalyticsProvider.SendEventWithParam: " +
                                       Environment.NewLine + eventName +
                                       Environment.NewLine + "params: " +
                                       paramsToSend.SerializeObject(true));
                    }
                    catch (Exception e)
                    {
                        InfoLogger.LogError("FirebaseAnalyticsProvider.ReportEvent.ReportEvent2: " + e.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                InfoLogger.LogError("FirebaseAnalyticsProvider.ReportEvent.FirebaseAnalytics: " + e.ToString());
            }
        }


        public void ReportErrorEvent(string eventName, string errorType, string errorMessage, string exceptionMessage, string details)
        {
            var parameters = new Dictionary<string, AnalyticsEventParameter>
            {
                { "error_type", AnalyticsEventParameter.StringParam(errorType) },
                { "error_message", AnalyticsEventParameter.StringParam(errorMessage) },
                { "exception_message", AnalyticsEventParameter.StringParam(exceptionMessage) },
                { "details", AnalyticsEventParameter.StringParam(details) }
            };

            ReportEvent(eventName, parameters);
        }

        // Unity IAP v5 moved iOS to StoreKit 2, which Firebase no longer auto-logs as the in_app_purchase event
        // (it did automatically under StoreKit 1). Manually log the App-Store-signed transaction (JWS) so that
        // event is restored on iOS. Invoked from ShopService for every successful Apple IAP. No-op in editor,
        // matching the rest of this service (Firebase is not called in the editor).
        private void LogAppleStoreKitTransaction(string appleTransactionJws)
        {
            if (Application.isEditor)
                return;

            if (string.IsNullOrEmpty(appleTransactionJws))
                return;

            try
            {
                FirebaseAnalytics.LogAppleTransactionAsync(appleTransactionJws);
                InfoLogger.Log("Logged Apple StoreKit 2 transaction to Firebase (in_app_purchase).");
            }
            catch (Exception e)
            {
                InfoLogger.LogError($"Failed to log Apple StoreKit 2 transaction to Firebase: {e}");
            }
        }

        public void SetUserId(string userId)
        {
            if (Application.isEditor)
                return;

            if (string.IsNullOrEmpty(userId))
                return;

            try
            {
                //if (IsInitializedFirebase)
                {
                    FirebaseAnalytics.SetUserId(userId);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("SetUserID.Firebase0: " + e.ToString());
            }

            try
            {
                //if (IsInitializedFirebase)
                {
                    Crashlytics.SetUserId(userId);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("SetUserID.Firebase1: " + e.ToString());
            }

            try
            {
                //if (IsInitializedFirebase)
                {
                    SetUserProperty("user_id", userId);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("SetUserID.Firebase2: " + e.ToString());
            }
        }

        public void SetUserProperty(string name, string property)
        {
            if (string.IsNullOrEmpty(name.Trim()))
            {
                Debug.LogError("SetUserProperty:: name is null or empty. Event will not send");
                return;
            }

            if (string.IsNullOrEmpty(property.Trim()))
            {
                Debug.LogError("SetUserProperty:: property is null or empty. Event will send still");
            }

            if (isFirebaseInitialized)
            {
                InfoLogger.Log($"Set firebase user property. {name}: {property}");
                try
                {
                    FirebaseAnalytics.SetUserProperty(name, property);
                    Crashlytics.SetCustomKey(name, property);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }
            }
            else
            {
                userPropertyCache[name] = property;
                InfoLogger.Log($"Firebase not initialized. Caching user property: {name} = {property}");
            }
        }

        public void ReportEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName) || eventName.Length < 2)
            {
                InfoLogger.LogError("FirebaseAnalyticsProvider.ReportEvent: eventName is not valid");
                return;
            }

            if (!isDependenciesChecked)
            {
                InfoLogger.LogError($"FirebaseAnalyticsProvider.ReportEvent: Dependencies are not checked yet. Event name: {eventName}");
                return;
            }

            try
            {
                eventName = FixEventName(eventName);

                if (string.IsNullOrEmpty(eventName) || eventName.Length < 2)
                    return;
            }
            catch (Exception e)
            {
                InfoLogger.LogError("FirebaseAnalyticsProvider.ReportEvent: " + e.ToString());
                return;
            }

            try
            {
                InfoLogger.Log("FirebaseAnalyticsProvider.ReportEvent: " + Environment.NewLine + eventName);
            }
            catch (Exception)
            {
            }

            if (Application.isEditor)
                return;

            try
            {
                //if (IsFirebaseInitialized)
                {
                    var eventNameForFirebase = eventName.Replace(":", "_");
                    FirebaseAnalytics.LogEvent(eventNameForFirebase);
                    OnAnyEventReported?.Invoke();
                }
            }
            catch (Exception e)
            {
                InfoLogger.LogError("FirebaseAnalyticsProvider.ReportEvent: " + e.ToString());
            }
        }

        private string FixEventName(string input)
        {
            string response = input
                    .Replace(" ", "_")
                    .Replace("ş", "s")
                    .Replace("Ş", "S")
                    .Replace("ç", "c")
                    .Replace("Ç", "C")
                    .Replace("ğ", "g")
                    .Replace("Ğ", "G")
                    .Replace("ı", "i")
                    .Replace("İ", "I")
                    .Replace("ö", "o")
                    .Replace("Ö", "O")
                    .Replace("ü", "u")
                    .Replace("Ü", "U")
                ;

            if (response.Length > 39)
            {
                response = response.Substring(0, 39);
                Debug.LogError("Event name too long: " + response);
            }

            return response;
        }

        private string FixStringEventParamValue(string input)
        {
            if (!string.IsNullOrEmpty(input) && input.Length > 99)
            {
                input = input.Substring(0, 99);
                Debug.LogError("Event param value too long: " + input);
            }

            return input;
        }

        private void ProcessCachedUserProperties()
        {
            if (userPropertyCache.Count == 0)
                return;

            InfoLogger.Log("Processing cached user properties...");
            foreach (var kvp in userPropertyCache)
            {
                FirebaseAnalytics.SetUserProperty(kvp.Key, kvp.Value);
                InfoLogger.Log($"Sent cached property: {kvp.Key} = {kvp.Value}");
            }

            userPropertyCache.Clear();
        }

        private void SetSessionTimeProperty()
        {
            SetUserProperty("current_duration", SessionTimer.SessionTime.ToString(CultureInfo.InvariantCulture));
        }
    }
}