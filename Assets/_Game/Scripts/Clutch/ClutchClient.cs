using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Clutch
{
    /// <summary>
    /// The only networking in the Clutch layer: a single static call that evaluates PUBLIC feature flags.
    ///
    /// PrototypeRacing has no Nakama and no user auth, so we hit Clutch's public endpoint with NO
    /// Authorization header — the server returns only flags marked public (which is all this game uses).
    /// Each returned flag value is handed back as its raw JSON string (our flags are JSON maps such as
    /// {"R35":200}); the caller deserializes as needed. UnityWebRequest keeps this on the Unity main
    /// thread and is the right fit for mobile.
    /// </summary>
    public static class ClutchClient
    {
        private const int TimeoutSeconds = 10;

        /// <summary>
        /// POSTs an evaluate-batch request for <paramref name="keys"/> and returns the flags Clutch
        /// returned, each value serialized to its JSON string. Throws on transport error, non-2xx, or
        /// unparseable body — callers treat any throw as "Clutch failed, use cache/fallback".
        /// </summary>
        /// <param name="baseUrl">Clutch API base URL (e.g. https://api.clutch.spektragames.com).</param>
        /// <param name="environmentId">Clutch environment id.</param>
        /// <param name="keys">Flag keys to evaluate (e.g. "VehicleConfig", "AdConfig").</param>
        /// <param name="properties">Optional targeting attributes (camelCase) forwarded as "properties".</param>
        public static async UniTask<Dictionary<string, string>> EvaluatePublicAsync(
            string baseUrl,
            string environmentId,
            IReadOnlyList<string> keys,
            JObject properties = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(environmentId))
                throw new InvalidOperationException("Clutch base URL / environment id is not configured.");

            if (keys == null || keys.Count == 0)
                return new Dictionary<string, string>();

            string url = $"{baseUrl}/v1/client/environments/{environmentId}/features/evaluate-batch";

            JObject body = new JObject { ["keys"] = new JArray(keys) };
            if (properties != null && properties.Count > 0)
                body["properties"] = properties;
            byte[] payload = Encoding.UTF8.GetBytes(body.ToString(Formatting.None));

            using UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(payload);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");
            // REQUIRED: the Clutch edge/WAF returns 403 for requests with no User-Agent. Set one
            // explicitly (UnityWebRequest does not always send a UA the edge accepts).
            request.SetRequestHeader("User-Agent", "ClutchSDK-Unity/1.0");
            // No Authorization header on purpose: public flags only.
            request.timeout = TimeoutSeconds;

            await request.SendWebRequest().WithCancellation(cancellationToken);

            if (request.result != UnityWebRequest.Result.Success)
                throw new Exception($"Clutch evaluate failed: {request.responseCode} {request.error}");

            string responseText = request.downloadHandler.text;
            if (string.IsNullOrEmpty(responseText))
                return new Dictionary<string, string>();

            return ParseFeatures(responseText);
        }

        // Reads the "features" object ({ "<key>": <raw json value> }) and serializes each value to a
        // JSON string. Objects/arrays/scalars are all preserved verbatim so callers can deserialize the
        // exact shape Clutch returned.
        private static Dictionary<string, string> ParseFeatures(string responseText)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            JObject parsed = JObject.Parse(responseText);
            if (parsed["features"] is JObject features)
            {
                foreach (KeyValuePair<string, JToken> kvp in features)
                {
                    if (kvp.Value == null || kvp.Value.Type == JTokenType.Null)
                        continue;

                    result[kvp.Key] = kvp.Value.ToString(Formatting.None);
                }
            }

            return result;
        }
    }
}
