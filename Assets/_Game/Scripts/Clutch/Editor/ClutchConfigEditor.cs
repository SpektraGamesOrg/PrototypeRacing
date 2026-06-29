using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace _Game.Scripts.Clutch.Editor
{
    /// <summary>
    /// Editor tooling for the Clutch remote-config layer. Fetches live PUBLIC feature flags from a chosen
    /// environment and writes the values into the <see cref="global::Clutch.ClutchConfig"/> fallback asset
    /// AND the PlayerPrefs cache, so the authored fallbacks and the local cache stay current at edit time
    /// ("write them memory, prefs and SO in editor time"). Mirrors the runtime fetch shape in ClutchClient
    /// but runs synchronously via HttpClient (editor-only, so HttpClient/LINQ are fine here).
    /// </summary>
    public static class ClutchConfigEditor
    {
        private const int TimeoutSeconds = 15;

        [MenuItem("Tools/Clutch/Fetch & Write Config (Dev)")]
        private static void FetchDev() => FetchAndWrite(useDev: true);

        [MenuItem("Tools/Clutch/Fetch & Write Config (Prod)")]
        private static void FetchProd() => FetchAndWrite(useDev: false);

        [MenuItem("Tools/Clutch/Select Clutch Config (Fallback)")]
        private static void SelectFallbackAsset()
        {
            global::Clutch.ClutchConfig config = global::Clutch.ClutchConfig.Instance;
            if (!config)
            {
                Debug.LogError("[ClutchConfigEditor] ClutchConfig asset not found in Resources. Create one via Create > Clutch > Clutch Config (Fallback).");
                return;
            }

            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }

        private static void FetchAndWrite(bool useDev)
        {
            global::Clutch.ClutchSDKConfig sdkConfig = global::Clutch.ClutchSDKConfig.Instance;
            if (!sdkConfig)
            {
                Debug.LogError("[ClutchConfigEditor] ClutchSDKConfig asset not found in Resources. Create one via Create > Clutch > Clutch SDK Config.");
                return;
            }

            global::Clutch.ClutchConfig fallbackConfig = global::Clutch.ClutchConfig.Instance;
            if (!fallbackConfig)
            {
                Debug.LogError("[ClutchConfigEditor] ClutchConfig (fallback) asset not found in Resources. Create one via Create > Clutch > Clutch Config (Fallback).");
                return;
            }

            string baseUrl = useDev ? sdkConfig.DevBaseUrl : sdkConfig.ProdBaseUrl;
            string envId = useDev ? sdkConfig.DevEnvironmentId : sdkConfig.ProdEnvironmentId;
            string envLabel = useDev ? "Dev" : "Prod";

            if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(envId))
            {
                Debug.LogError($"[ClutchConfigEditor] {envLabel} base URL / environment id is empty in ClutchSDKConfig.");
                return;
            }

            string[] keys = global::Clutch.ClutchFlagKeys.All;

            Dictionary<string, string> fetched;
            try
            {
                EditorUtility.DisplayProgressBar("Clutch", $"Fetching flags from {envLabel}...", 0.5f);
                fetched = EvaluatePublic(baseUrl, envId, keys);
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"[ClutchConfigEditor] Fetch from {envLabel} failed: {e.Message}");
                return;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            if (fetched.Count == 0)
            {
                Debug.LogError($"[ClutchConfigEditor] {envLabel} returned no public flags for keys: {string.Join(", ", keys)}. " +
                               "Check the flags exist and are marked public in Clutch.");
                return;
            }

            // Write into the fallback SO and pre-warm the PlayerPrefs cache.
            Undo.RecordObject(fallbackConfig, "Write Clutch Config");
            foreach (KeyValuePair<string, string> kvp in fetched)
            {
                fallbackConfig.SetFallbackEditor(kvp.Key, kvp.Value);
                PlayerPrefsSetClutch(kvp.Key, kvp.Value);
                Debug.Log($"[ClutchConfigEditor] {envLabel} {kvp.Key} = {kvp.Value}");
            }

            EditorUtility.SetDirty(fallbackConfig);
            AssetDatabase.SaveAssets();
            PlayerPrefs.Save();

            string[] missing = keys.Where(k => !fetched.ContainsKey(k)).ToArray();
            if (missing.Length > 0)
                Debug.LogError($"[ClutchConfigEditor] {envLabel} did not return: {string.Join(", ", missing)} (kept existing fallback).");

            Debug.Log($"[ClutchConfigEditor] Wrote {fetched.Count} flag(s) from {envLabel} into ClutchConfig + PlayerPrefs cache.");
        }

        // Mirrors ClutchConfigCache's combined-blob layout (one PlayerPrefs key, key->json-string), without
        // referencing runtime save internals from editor code.
        private static void PlayerPrefsSetClutch(string flagKey, string json)
        {
            const string cacheKey = "clutch_config_cache";
            string raw = PlayerPrefs.GetString(cacheKey, string.Empty);
            JObject blob;
            try
            {
                blob = string.IsNullOrEmpty(raw) ? new JObject() : JObject.Parse(raw);
            }
            catch (JsonException)
            {
                blob = new JObject();
            }

            blob[flagKey] = json ?? string.Empty;
            PlayerPrefs.SetString(cacheKey, blob.ToString(Formatting.None));
        }

        // Synchronous public evaluate-batch for editor use. Same request/response shape as ClutchClient.
        private static Dictionary<string, string> EvaluatePublic(string baseUrl, string envId, IReadOnlyList<string> keys)
        {
            string url = $"{baseUrl}/v1/client/environments/{envId}/features/evaluate-batch";
            JObject body = new JObject { ["keys"] = new JArray(keys) };
            string jsonBody = body.ToString(Formatting.None);

            using HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(TimeoutSeconds) };
            using HttpContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = client.PostAsync(url, content).GetAwaiter().GetResult();
            string responseText = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"{(int)response.StatusCode} {response.ReasonPhrase}: {responseText}");

            Dictionary<string, string> result = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(responseText))
                return result;

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
