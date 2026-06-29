using System;
using System.Collections.Generic;
using SpektraGames.SpektraUtilities.Runtime;
using UnityEngine;

namespace Analytics.AppsFlyer
{
    public class AppsFlyerMMPService : IMMPService
    {
        public InfoLogger InfoLogger { get; }

        public AppsFlyerMMPService()
        {
            InfoLogger = new InfoLogger("AppsFlyer", "blue");
        }

        public void SetUserId(string userId)
        {
            if (Application.isEditor)
                return;

            if (string.IsNullOrEmpty(userId))
                return;

            try
            {
                AppsFlyerSDK.AppsFlyer.setCustomerUserId(userId);
            }
            catch (Exception e)
            {
                Debug.LogError("SetUserID.AppsFlyer: " + e.ToString());
            }
        }

        public void ReportEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
                return;

            try
            {
                AppsFlyerSDK.AppsFlyer.sendEvent(eventName, null);
            }
            catch (Exception e)
            {
                InfoLogger.LogError("AppsflyerAnalyticsProvider.ReportEventForAppsFlyer: " + e.ToString());
            }

            InfoLogger.Log("AppsflyerAnalyticsProvider ReportEvent: " + Environment.NewLine + eventName);
        }

        public void ReportEvent(string eventName, Dictionary<string, AnalyticsEventParameter> paramsToSend)
        {
            if (string.IsNullOrEmpty(eventName))
                return;

            try
            {
                var stringParamsDictionary = new Dictionary<string, string>();
                foreach (var analyticsEventParameter in paramsToSend)
                {
                    stringParamsDictionary.Add(analyticsEventParameter.Key, analyticsEventParameter.Value.stringValue);
                }

                AppsFlyerSDK.AppsFlyer.sendEvent(eventName, stringParamsDictionary);
            }
            catch (Exception e)
            {
                Debug.LogError("AppsflyerAnalyticsProvider: " + e.ToString());
            }

            InfoLogger.Log("AppsflyerAnalyticsProvider: " +
                           Environment.NewLine + eventName +
                           Environment.NewLine + (paramsToSend != null ? paramsToSend.SerializeObject() : "null"));
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

            ReportEvent("custom_error", parameters);
        }
    }
}