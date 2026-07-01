using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Vehicles;

namespace Save.Editor
{
    /// <summary>
    /// SILENT, editor-only forensic recorder for the save system. Runs in the background across play
    /// sessions and domain reloads, snapshotting the save state at every meaningful boundary (boot, play
    /// mode enter/exit, script recompile, editor quit) plus a light periodic sample while playing. Nothing
    /// is printed to the Console - all data goes to files under persistentDataPath/save_forensics/. When the
    /// save-reset bug is reproduced, open Tools > Save System > Forensics and read/copy the full report.
    ///
    /// Detection is based on the two DEFINITIVE, monotonic invariants of this game's save system:
    ///   1. save_counter only ever increases (every SaveManager.Save bumps it) - a DECREASE means PlayerPrefs
    ///      rolled back to an older snapshot.
    ///   2. Vehicle ownership is only ever added (there is no selling) - a previously-owned car becoming
    ///      un-owned means ownership was lost.
    /// Gold VALUE is deliberately NOT used as a trigger (it legitimately drops when you buy a car). Every
    /// snapshot still records gold, user_id presence, and the macOS plist file timestamp so an incident
    /// report can pinpoint whether the whole store was cleared (user_id also gone) or only the later-written
    /// keys rolled back (user_id survives), and whether the on-disk plist lagged the in-memory value.
    /// </summary>
    [InitializeOnLoad]
    public static class SaveSystemForensics
    {
        private const double PeriodicSampleSeconds = 3.0;
        private const long LedgerTrimBytes = 1_000_000; // keep the ledger from growing without bound

        private static readonly string Dir = Path.Combine(Application.persistentDataPath, "save_forensics");
        private static readonly string LedgerFile = Path.Combine(Dir, "ledger.jsonl");
        private static readonly string WatermarkFile = Path.Combine(Dir, "watermark.json");
        private static readonly string IncidentsFile = Path.Combine(Dir, "incidents.jsonl");
        private static readonly string LastIncidentSigFile = Path.Combine(Dir, "last_incident_sig.txt");

        private static readonly string SessionId = Guid.NewGuid().ToString("N").Substring(0, 8);

        private static double _lastPeriodic;
        private static string _lastSignature;

        static SaveSystemForensics()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
            EditorApplication.quitting += OnQuitting;
            EditorApplication.update += OnUpdate;
        }

        // ---------------------------------------------------------------------
        // Event hooks (all silent)
        // ---------------------------------------------------------------------

        private static void OnPlayModeStateChanged(PlayModeStateChange state) => Capture(state.ToString(), true);
        private static void OnAfterAssemblyReload() => Capture("AfterAssemblyReload", true);
        private static void OnQuitting() => Capture("EditorQuitting", true);

        private static void OnUpdate()
        {
            if (!EditorApplication.isPlaying)
                return;

            double now = EditorApplication.timeSinceStartup;
            if (now - _lastPeriodic < PeriodicSampleSeconds)
                return;

            _lastPeriodic = now;
            Capture("Periodic", false);
        }

        // ---------------------------------------------------------------------
        // Capture + detection
        // ---------------------------------------------------------------------

        private static void Capture(string trigger, bool forceAppend)
        {
            try
            {
                ForensicSnapshot snap = BuildSnapshot(trigger);

                // Dedupe the noisy periodic sampler: only append when the meaningful state changed.
                string signature = Signature(snap);
                if (!forceAppend && signature == _lastSignature)
                    return;

                _lastSignature = signature;
                AppendLine(LedgerFile, JsonUtility.ToJson(snap));
                TrimIfLarge(LedgerFile);

                // Only JUDGE steady in-play reads. Editor PlayerPrefs reads during domain reloads / edit-mode
                // transitions momentarily return empty/stale values (the counter flickers 14 -> 0 -> 15), which
                // are NOT real data loss - detecting on them produces false positives. The timeline still keeps
                // every sample for context; incident detection runs only on stable samples.
                if (IsStableForDetection(snap))
                    Detect(snap);
            }
            catch
            {
                // Forensics must never disturb the editor or the test. Swallow everything.
            }
        }

        // A read we trust enough to raise incidents from: taken while actually in Play Mode and after boot has
        // settled (a periodic sample or the clean end-of-play read). Edit-mode and reload-transient reads are
        // excluded because the editor returns unstable PlayerPrefs values during those windows.
        private static bool IsStableForDetection(ForensicSnapshot snap)
        {
            return snap.trigger == "Periodic" || snap.trigger == "ExitingPlayMode";
        }

        private static void Detect(ForensicSnapshot snap)
        {
            ForensicWatermark hw = ReadWatermark();

            // First meaningful observation establishes the baseline.
            if (hw == null)
            {
                if (snap.saveCounter > 0 || snap.coins > 0 || snap.ownedVehicleIds.Count > 0)
                    WriteWatermark(WatermarkFrom(snap));
                return;
            }

            // Domain change: the editor is now reading a DIFFERENT PlayerPrefs store than the one that holds
            // our high-watermark (companyName/productName drives the macOS plist / registry key).
            if (snap.company != hw.company || snap.product != hw.product)
            {
                RecordIncident("DOMAIN_CHANGE", snap, hw,
                    $"PlayerPrefs identity changed from '{hw.company}.{hw.product}' to " +
                    $"'{snap.company}.{snap.product}'. The editor is reading a different store, so all values " +
                    "appear reset. Check ProjectSettings companyName/productName on the current branch.");
                WriteWatermark(WatermarkFrom(snap)); // new identity becomes the baseline
                return;
            }

            bool counterRegressed = snap.saveCounter < hw.saveCounter;
            List<int> lostCars = Subtract(hw.ownedVehicleIds, snap.ownedVehicleIds);

            // Full wipe: counter back to 0 and nothing owned, where we previously had progress. Looks like a
            // DeleteAll/ResetAll - could be a deliberate debug reset OR the bug wiping everything.
            if (snap.saveCounter == 0 && snap.ownedVehicleIds.Count == 0 &&
                (hw.saveCounter > 0 || hw.ownedVehicleIds.Count > 0))
            {
                RecordIncident("WIPE", snap, hw,
                    "Store fully wiped: save_counter=0 and no owned vehicles. If you did NOT click a " +
                    "'Clear/Reset Player Prefs' debug button, this is the bug wiping everything. " +
                    LayerDiagnosis(snap));
                ClearWatermark(); // reset baseline so we don't keep re-flagging the wiped state
                return;
            }

            if (counterRegressed || lostCars.Count > 0)
            {
                string kind = counterRegressed ? "ROLLBACK" : "OWNERSHIP_LOSS";
                string detail = counterRegressed
                    ? $"save_counter went BACKWARDS {hw.saveCounter} -> {snap.saveCounter} (it must only ever " +
                      "increase), so PlayerPrefs reverted to an older snapshot. "
                    : $"Previously-owned vehicle id(s) [{string.Join(",", lostCars)}] are no longer owned " +
                      "(ownership is never removed by the game). ";

                RecordIncident(kind, snap, hw, detail + LayerDiagnosis(snap));
                // Keep the high-watermark (the peak) so we still detect until the data recovers - the incident
                // dedupe prevents spamming the same episode.
                return;
            }

            // Normal progress: advance the high-watermark (max counter, union of owned cars).
            if (snap.saveCounter > hw.saveCounter || !IsSubset(snap.ownedVehicleIds, hw.ownedVehicleIds))
                WriteWatermark(Merge(hw, snap));
        }

        // Localizes the failure: did the WHOLE store go (user_id also gone) or only later-written keys roll
        // back (user_id survives - the reported profile), and did the on-disk macOS plist lag?
        private static string LayerDiagnosis(ForensicSnapshot snap)
        {
            var sb = new StringBuilder();
            sb.Append(snap.hasUserId
                ? "user_id SURVIVED, so only later-written keys (coins/vehicles) were lost - matches the reported bug. "
                : "user_id is ALSO missing, so the ENTIRE store was cleared, not a partial rollback. ");

            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                sb.Append(snap.plistExists
                    ? $"macOS plist last written {snap.plistLastWriteUtc} (compare to when you last earned/bought - " +
                      "an old timestamp means cfprefsd never flushed the newest value). "
                    : "macOS plist file is MISSING entirely. ");
            }

            return sb.ToString();
        }

        // ---------------------------------------------------------------------
        // Snapshot construction
        // ---------------------------------------------------------------------

        private static ForensicSnapshot BuildSnapshot(string trigger)
        {
            var snap = new ForensicSnapshot
            {
                ts = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
                trigger = trigger,
                session = SessionId,
                isPlaying = EditorApplication.isPlaying,
                company = Application.companyName,
                product = Application.productName,
                saveCounter = PlayerPrefs.GetInt(SaveKeys.SaveCounter, 0),
                hasCoins = PlayerPrefs.HasKey(SaveKeys.Gold),
                coins = PlayerPrefs.GetInt(SaveKeys.Gold, -1),
                selectedVehicle = PlayerPrefs.GetInt(SaveKeys.SelectedVehicle, -1),
                distanceDriven = PlayerPrefs.GetInt(SaveKeys.DistanceDriven, -1),
                milestonesClaimed = PlayerPrefs.GetInt(SaveKeys.DistanceMilestonesClaimed, -1),
                nitro = PlayerPrefs.GetInt(SaveKeys.NitroCount, -1),
                hasUserId = !string.IsNullOrEmpty(PlayerPrefs.GetString(SaveKeys.UserId, string.Empty)),
                userIdShort = Short(PlayerPrefs.GetString(SaveKeys.UserId, string.Empty)),
                ownedVehicleIds = OwnedFromPrefs(),
            };

            FillPlistInfo(snap);
            return snap;
        }

        private static List<int> OwnedFromPrefs()
        {
            var owned = new List<int>();
            try
            {
                string json = PlayerPrefs.GetString(SaveKeys.Vehicles, string.Empty);
                if (string.IsNullOrEmpty(json))
                    return owned;

                VehicleList list = JsonUtility.FromJson<VehicleList>(json);
                if (list?.vehicles == null)
                    return owned;

                for (var i = 0; i < list.vehicles.Count; i++)
                {
                    VehicleSaveData v = list.vehicles[i];
                    if (v != null && v.owned)
                        owned.Add((int)v.id);
                }
            }
            catch
            {
                // Corrupt vehicles blob is itself a finding; leave the list as-is and record what we can.
            }

            owned.Sort();
            return owned;
        }

        private static void FillPlistInfo(ForensicSnapshot snap)
        {
            try
            {
                if (Application.platform != RuntimePlatform.OSXEditor)
                {
                    snap.plistPath = "(not macOS editor)";
                    return;
                }

                string home = Environment.GetEnvironmentVariable("HOME");
                snap.plistPath = $"{home}/Library/Preferences/unity.{snap.company}.{snap.product}.plist";
                var info = new FileInfo(snap.plistPath);
                snap.plistExists = info.Exists;
                if (info.Exists)
                {
                    snap.plistLastWriteUtc = info.LastWriteTimeUtc.ToString("o", CultureInfo.InvariantCulture);
                    snap.plistSize = info.Length;
                }
            }
            catch
            {
                // ignore
            }
        }

        // ---------------------------------------------------------------------
        // Incident recording (deduped so a single episode is not logged repeatedly)
        // ---------------------------------------------------------------------

        private static void RecordIncident(string kind, ForensicSnapshot snap, ForensicWatermark hw, string diagnosis)
        {
            var incident = new ForensicIncident
            {
                ts = snap.ts,
                kind = kind,
                trigger = snap.trigger,
                diagnosis = diagnosis,
                before = hw,
                after = snap,
            };

            // Dedupe by the STABLE facts of the episode (the peak we fell from, the cars lost, the current
            // identity) - NOT the live counter, which climbs again after a reset and would re-log endlessly.
            List<int> lost = Subtract(hw.ownedVehicleIds, snap.ownedVehicleIds);
            string signature = $"{kind}|{hw.saveCounter}|{string.Join(",", lost)}|{snap.userIdShort}";
            if (signature == ReadLastIncidentSignature())
                return; // same ongoing episode - do not spam

            AppendLine(IncidentsFile, JsonUtility.ToJson(incident));
            WriteText(LastIncidentSigFile, signature);
        }

        private static string ReadLastIncidentSignature()
        {
            try
            {
                return File.Exists(LastIncidentSigFile) ? File.ReadAllText(LastIncidentSigFile) : null;
            }
            catch
            {
                return null;
            }
        }

        // ---------------------------------------------------------------------
        // Public API for the viewer window
        // ---------------------------------------------------------------------

        public static string ForensicsDirectory => Dir;

        public static void CaptureNow() => Capture("Manual", true);

        public static void ClearHistory()
        {
            try
            {
                if (Directory.Exists(Dir))
                    Directory.Delete(Dir, true);
                _lastSignature = null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystemForensics] Failed to clear history: {e.Message}");
            }
        }

        public static int IncidentCount()
        {
            try
            {
                return File.Exists(IncidentsFile) ? File.ReadAllLines(IncidentsFile).Length : 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>Builds the full, human-readable forensic report (also what the Copy button puts on the clipboard).</summary>
        public static string BuildReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine("==================== SAVE SYSTEM FORENSIC REPORT ====================");
            sb.AppendLine($"Generated (UTC): {DateTime.UtcNow:o}");
            sb.AppendLine($"Editor session:  {SessionId}");
            sb.AppendLine($"Data folder:     {Dir}");
            sb.AppendLine();

            sb.AppendLine("----- LIVE STATE (captured now) -----");
            AppendSnapshot(sb, BuildSnapshot("Report"));
            sb.AppendLine();

            sb.AppendLine("----- HIGH-WATERMARK (best progress ever seen) -----");
            ForensicWatermark hw = ReadWatermark();
            if (hw == null)
            {
                sb.AppendLine("(none recorded yet)");
            }
            else
            {
                sb.AppendLine($"saveCounter={hw.saveCounter}  coins={hw.coins}  owned=[{string.Join(",", hw.ownedVehicleIds)}]");
                sb.AppendLine($"identity={hw.company}.{hw.product}  userId={hw.userIdShort}  at={hw.ts}");
            }
            sb.AppendLine();

            int incidents = IncidentCount();
            sb.AppendLine($"----- INCIDENTS DETECTED: {incidents} -----");
            if (incidents == 0)
            {
                sb.AppendLine("No rollback / ownership-loss / wipe detected so far. If the bug just happened and");
                sb.AppendLine("nothing shows here, the loss did NOT regress the save_counter or ownership set -");
                sb.AppendLine("copy this whole report anyway; the live-vs-watermark values below still help.");
            }
            else
            {
                foreach (string line in SafeReadLines(IncidentsFile))
                {
                    ForensicIncident inc = TryParse<ForensicIncident>(line);
                    if (inc == null)
                        continue;

                    sb.AppendLine($"  [{inc.kind}] {inc.ts}  (trigger={inc.trigger})");
                    sb.AppendLine($"    diagnosis: {inc.diagnosis}");
                    if (inc.before != null)
                        sb.AppendLine($"    BEFORE: counter={inc.before.saveCounter} coins={inc.before.coins} owned=[{string.Join(",", inc.before.ownedVehicleIds)}]");
                    if (inc.after != null)
                        sb.AppendLine($"    AFTER:  counter={inc.after.saveCounter} coins={inc.after.coins} owned=[{string.Join(",", inc.after.ownedVehicleIds)}] " +
                                      $"userId={(inc.after.hasUserId ? inc.after.userIdShort : "MISSING")} " +
                                      $"plist(exists={inc.after.plistExists},written={inc.after.plistLastWriteUtc})");
                    sb.AppendLine();
                }
            }

            sb.AppendLine("----- RECENT TIMELINE (last 60 samples) -----");
            foreach (string line in TailLines(LedgerFile, 60))
            {
                ForensicSnapshot s = TryParse<ForensicSnapshot>(line);
                if (s == null)
                    continue;

                sb.AppendLine($"  {s.ts}  {s.trigger,-20} play={s.isPlaying,-5} counter={s.saveCounter,-4} " +
                              $"coins={s.coins,-8} owned=[{string.Join(",", s.ownedVehicleIds)}] " +
                              $"userId={(s.hasUserId ? s.userIdShort : "MISSING")} plistWritten={s.plistLastWriteUtc}");
            }

            sb.AppendLine("====================================================================");
            return sb.ToString();
        }

        private static void AppendSnapshot(StringBuilder sb, ForensicSnapshot s)
        {
            sb.AppendLine($"time={s.ts}  trigger={s.trigger}  playing={s.isPlaying}");
            sb.AppendLine($"identity={s.company}.{s.product}");
            sb.AppendLine($"PlayerPrefs: saveCounter={s.saveCounter}  coins={(s.hasCoins ? s.coins.ToString() : "MISSING")}  " +
                          $"owned=[{string.Join(",", s.ownedVehicleIds)}]  selected={s.selectedVehicle}  " +
                          $"distance={s.distanceDriven}  milestones={s.milestonesClaimed}  nitro={s.nitro}  " +
                          $"userId={(s.hasUserId ? s.userIdShort : "MISSING")}");
            sb.AppendLine($"macOS plist: {s.plistPath}");
            sb.AppendLine($"             exists={s.plistExists}  lastWriteUtc={s.plistLastWriteUtc}  size={s.plistSize}");
        }

        // ---------------------------------------------------------------------
        // Watermark helpers
        // ---------------------------------------------------------------------

        private static ForensicWatermark ReadWatermark()
        {
            try
            {
                return File.Exists(WatermarkFile) ? JsonUtility.FromJson<ForensicWatermark>(File.ReadAllText(WatermarkFile)) : null;
            }
            catch
            {
                return null;
            }
        }

        private static void WriteWatermark(ForensicWatermark wm) => WriteText(WatermarkFile, JsonUtility.ToJson(wm));

        private static void ClearWatermark()
        {
            try
            {
                if (File.Exists(WatermarkFile))
                    File.Delete(WatermarkFile);
            }
            catch
            {
                // ignore
            }
        }

        private static ForensicWatermark WatermarkFrom(ForensicSnapshot s) => new ForensicWatermark
        {
            ts = s.ts,
            company = s.company,
            product = s.product,
            saveCounter = s.saveCounter,
            coins = s.coins,
            userIdShort = s.userIdShort,
            ownedVehicleIds = new List<int>(s.ownedVehicleIds),
        };

        private static ForensicWatermark Merge(ForensicWatermark hw, ForensicSnapshot s)
        {
            var merged = WatermarkFrom(s);
            merged.saveCounter = Math.Max(hw.saveCounter, s.saveCounter);
            merged.ownedVehicleIds = Union(hw.ownedVehicleIds, s.ownedVehicleIds);
            return merged;
        }

        // ---------------------------------------------------------------------
        // Small utilities
        // ---------------------------------------------------------------------

        private static string Signature(ForensicSnapshot s) =>
            $"{s.saveCounter}|{s.coins}|{string.Join(",", s.ownedVehicleIds)}|{s.selectedVehicle}|{s.distanceDriven}|{s.company}.{s.product}";

        private static string Short(string s) => string.IsNullOrEmpty(s) ? string.Empty : s.Substring(0, Math.Min(8, s.Length));

        private static List<int> Subtract(List<int> a, List<int> b)
        {
            var result = new List<int>();
            for (var i = 0; i < a.Count; i++)
            {
                if (!b.Contains(a[i]))
                    result.Add(a[i]);
            }

            return result;
        }

        private static bool IsSubset(List<int> a, List<int> b)
        {
            for (var i = 0; i < a.Count; i++)
            {
                if (!b.Contains(a[i]))
                    return false;
            }

            return true;
        }

        private static List<int> Union(List<int> a, List<int> b)
        {
            var result = new List<int>(a);
            for (var i = 0; i < b.Count; i++)
            {
                if (!result.Contains(b[i]))
                    result.Add(b[i]);
            }

            result.Sort();
            return result;
        }

        private static void AppendLine(string path, string line)
        {
            Directory.CreateDirectory(Dir);
            File.AppendAllText(path, line + "\n");
        }

        private static void WriteText(string path, string text)
        {
            Directory.CreateDirectory(Dir);
            File.WriteAllText(path, text);
        }

        private static void TrimIfLarge(string path)
        {
            try
            {
                var info = new FileInfo(path);
                if (!info.Exists || info.Length < LedgerTrimBytes)
                    return;

                string[] lines = File.ReadAllLines(path);
                int keep = lines.Length / 2;
                File.WriteAllLines(path, new ArraySegment<string>(lines, lines.Length - keep, keep));
            }
            catch
            {
                // ignore
            }
        }

        private static IEnumerable<string> SafeReadLines(string path)
        {
            try
            {
                return File.Exists(path) ? File.ReadAllLines(path) : Array.Empty<string>();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        private static List<string> TailLines(string path, int count)
        {
            var all = new List<string>(SafeReadLines(path));
            int start = Math.Max(0, all.Count - count);
            return all.GetRange(start, all.Count - start);
        }

        private static T TryParse<T>(string json) where T : class
        {
            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch
            {
                return null;
            }
        }
    }

    [Serializable]
    public class ForensicSnapshot
    {
        public string ts;
        public string trigger;
        public string session;
        public bool isPlaying;
        public string company;
        public string product;
        public int saveCounter;
        public bool hasCoins;
        public int coins;
        public List<int> ownedVehicleIds = new List<int>();
        public int selectedVehicle;
        public int distanceDriven;
        public int milestonesClaimed;
        public int nitro;
        public bool hasUserId;
        public string userIdShort;
        public string plistPath;
        public bool plistExists;
        public string plistLastWriteUtc;
        public long plistSize;
    }

    [Serializable]
    public class ForensicWatermark
    {
        public string ts;
        public string company;
        public string product;
        public int saveCounter;
        public int coins;
        public string userIdShort;
        public List<int> ownedVehicleIds = new List<int>();
    }

    [Serializable]
    public class ForensicIncident
    {
        public string ts;
        public string kind;
        public string trigger;
        public string diagnosis;
        public ForensicWatermark before;
        public ForensicSnapshot after;
    }
}
