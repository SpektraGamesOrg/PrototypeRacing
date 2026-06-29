using System.ComponentModel;
using System.Text;
using AppsFlyerSDK;
using SpektraGames.RuntimeUI.Runtime;
using SRDebugger;
using UnityEngine;

// NOTE: must stay in the GLOBAL namespace. SROptions is a partial class that SRDebugger
// discovers globally; the sibling SROptions.*.cs files are all global. Wrapping this in a
// namespace would create a separate type that never merges, so the buttons would not appear.
#if !DISABLE_SRDEBUGGER
public partial class SROptions
{
    // AppsFlyer SDK ID. Available once the native SDK object exists; empty if the
    // SDK never initialised (or in the editor).
    [Category("AppsFlyer")]
    [Sort(0)]
    [DisplayName("AppsFlyer Id")]
    public string AppsFlyerId => SafeGetAppsFlyerId();

    // The decisive signal for install tracking: did Start() reach AppsFlyer.startSDK()?
    // If this is false on a real Android build, the install/launch event was never sent.
    [Category("AppsFlyer")]
    [Sort(1)]
    [DisplayName("Initialized (startSDK reached)")]
    public bool AppsFlyerInitialized => Analytics.AppsFlyer.AppsFlyerObjectScript.StartSdkCalled;

    [Category("AppsFlyer")]
    [Sort(2)]
    [DisplayName("SDK Running (not stopped)")]
    public bool AppsFlyerRunning
    {
        get
        {
            try
            {
                return !AppsFlyer.isSDKStopped();
            }
            catch
            {
                return false;
            }
        }
    }

    [Category("AppsFlyer")]
    [Sort(3)]
    [DisplayName("SDK Version")]
    public string AppsFlyerSdkVersion
    {
        get
        {
            try
            {
                return AppsFlyer.getSdkVersion();
            }
            catch (System.Exception e)
            {
                return "<error: " + e.Message + ">";
            }
        }
    }

    [Category("AppsFlyer")]
    [Sort(4)]
    [DisplayName("Copy AppsFlyer Id")]
    public void CopyAppsFlyerId()
    {
        var id = SafeGetAppsFlyerId();
        GUIUtility.systemCopyBuffer = id;
        RuntimeUI.ShowToast(string.IsNullOrEmpty(id)
            ? "AppsFlyer Id is empty (SDK not initialised?)"
            : "AppsFlyer Id copied: " + id);
    }

    // Full readout of the init ladder so you can see, on-device, exactly where
    // AppsFlyer initialisation stopped. Logged to console and copied to clipboard.
    [Category("AppsFlyer")]
    [Sort(5)]
    [DisplayName("Dump AppsFlyer Status")]
    public void DumpAppsFlyerStatus()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== AppsFlyer Status ===");
        sb.AppendLine("Awake ran:                " + Analytics.AppsFlyer.AppsFlyerObjectScript.AwakeCalled);
        sb.AppendLine("Config loaded:            " + Analytics.AppsFlyer.AppsFlyerObjectScript.ConfigLoaded);
        sb.AppendLine("initSDK called:           " + Analytics.AppsFlyer.AppsFlyerObjectScript.InitSdkCalled);
        sb.AppendLine("PurchaseConnector built:  " + Analytics.AppsFlyer.AppsFlyerObjectScript.PurchaseConnectorConfigured);
        sb.AppendLine("startSDK called (INSTALL):" + Analytics.AppsFlyer.AppsFlyerObjectScript.StartSdkCalled);
        sb.AppendLine("SDK running (not stopped):" + SafeIsRunning());
        sb.AppendLine("SDK version:              " + AppsFlyerSdkVersion);
        sb.AppendLine("AppsFlyer Id:             " + SafeGetAppsFlyerId());
        sb.AppendLine("Interpretation:           " + Interpret());

        var report = sb.ToString();
        Debug.Log(report);
        GUIUtility.systemCopyBuffer = report;
        RuntimeUI.ShowToast("AppsFlyer status logged & copied. startSDK reached: "
                            + Analytics.AppsFlyer.AppsFlyerObjectScript.StartSdkCalled);
    }

    private static string SafeGetAppsFlyerId()
    {
        try
        {
            return AppsFlyer.getAppsFlyerId();
        }
        catch (System.Exception e)
        {
            return "<error: " + e.Message + ">";
        }
    }

    private static string SafeIsRunning()
    {
        try
        {
            return (!AppsFlyer.isSDKStopped()).ToString();
        }
        catch (System.Exception e)
        {
            return "<error: " + e.Message + ">";
        }
    }

    // Localises the failure based on the furthest init step reached.
    private static string Interpret()
    {
        if (!Analytics.AppsFlyer.AppsFlyerObjectScript.AwakeCalled)
            return "AppsFlyerObjectScript never ran (GameObject missing/disabled in scene).";
        if (!Analytics.AppsFlyer.AppsFlyerObjectScript.ConfigLoaded)
            return "AppsFlyerConfig.Instance was null (Resources/obfuscation issue).";
        if (!Analytics.AppsFlyer.AppsFlyerObjectScript.InitSdkCalled)
            return "Stopped before initSDK.";
        if (!Analytics.AppsFlyer.AppsFlyerObjectScript.PurchaseConnectorConfigured)
            return "PurchaseConnector threw before startSDK — install tracking is NOT sent. " +
                   "Matches the IAP/Play-Billing conflict (Android-only).";
        if (!Analytics.AppsFlyer.AppsFlyerObjectScript.StartSdkCalled)
            return "startSDK() itself threw — install tracking is NOT sent.";
        return "OK — startSDK reached; install/launch event was sent.";
    }
}
#endif