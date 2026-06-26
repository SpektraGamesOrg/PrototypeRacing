//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------


using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Root customization component for a vehicle. Coordinates all sub-managers (paint, wheels, upgrades, spoilers, sirens, decals, neons, customization)
/// and manages save/load of the complete customization loadout via JSON serialization to PlayerPrefs.
/// </summary>
[DefaultExecutionOrder(10)]
public class RCC_Customizer : RCC_Core {

    /// <summary>
    /// Save file name of the vehicle.
    /// </summary>
    [Tooltip("Save file name of the vehicle.")]
    public string saveFileName = "";

    /// <summary>
    /// Whether to automatically load the latest saved loadout from PlayerPrefs on initialization.
    /// </summary>
    [Tooltip("Whether to automatically load the latest saved loadout from PlayerPrefs on initialization.")]
    public bool autoLoadLoadout = true;

    /// <summary>
    /// Whether to automatically save the loadout to PlayerPrefs whenever a customization change is made.
    /// </summary>
    [Tooltip("Whether to automatically save the loadout to PlayerPrefs whenever a customization change is made.")]
    public bool autoSave = true;

    /// <summary>
    /// Determines when all customization managers are initialized during the Unity lifecycle.
    /// </summary>
    public enum InitializeMethod {

        /// <summary>
        /// Initialize during Awake.
        /// </summary>
        Awake,

        /// <summary>
        /// Initialize during OnEnable.
        /// </summary>
        OnEnable,

        /// <summary>
        /// Initialize during Start.
        /// </summary>
        Start,

        /// <summary>
        /// Initialize after one FixedUpdate frame for physics-dependent setups.
        /// </summary>
        DelayedWithFixedUpdate,

        /// <summary>
        /// Do not auto-initialize. Call Initialize() manually.
        /// </summary>
        None

    }

    /// <summary>
    /// Selected initialization timing for all customization managers.
    /// </summary>
    [Tooltip("Selected initialization timing for all customization managers.")]
    public InitializeMethod initializeMethod = InitializeMethod.Start;

    /// <summary>
    /// Loadout class.
    /// </summary>
    [Tooltip("Loadout class.")]
    public RCC_Customizer_Loadout loadout = new RCC_Customizer_Loadout();

    #region All upgrade managers

    /// <summary>
    /// Paint manager.
    /// </summary>
    private RCC_Customizer_PaintManager _paintManager;

    /// <summary>
    /// Gets the paint sub-manager for this vehicle. Lazy-initialized via GetComponentInChildren on first access; returns null if no paint manager child exists.
    /// </summary>
    public RCC_Customizer_PaintManager PaintManager {

        get {

            if (_paintManager == null)
                _paintManager = GetComponentInChildren<RCC_Customizer_PaintManager>(true);

            return _paintManager;

        }

    }

    /// <summary>
    /// Wheel Manager.
    /// </summary>
    private RCC_Customizer_WheelManager _wheelManager;

    /// <summary>
    /// Gets the wheel-customization sub-manager for this vehicle. Lazy-initialized via GetComponentInChildren on first access; returns null if no wheel manager child exists.
    /// </summary>
    public RCC_Customizer_WheelManager WheelManager {

        get {

            if (_wheelManager == null)
                _wheelManager = GetComponentInChildren<RCC_Customizer_WheelManager>(true);

            return _wheelManager;

        }

    }

    /// <summary>
    /// Upgrade Manager.
    /// </summary>
    private RCC_Customizer_UpgradeManager _upgradeManager;

    /// <summary>
    /// Gets the upgrade sub-manager (engine / brake / handling / speed) for this vehicle. Lazy-initialized via GetComponentInChildren on first access; returns null if no upgrade manager child exists.
    /// </summary>
    public RCC_Customizer_UpgradeManager UpgradeManager {

        get {

            if (_upgradeManager == null)
                _upgradeManager = GetComponentInChildren<RCC_Customizer_UpgradeManager>(true);

            return _upgradeManager;

        }

    }

    /// <summary>
    /// Spoiler Manager.
    /// </summary>
    private RCC_Customizer_SpoilerManager _spoilerManager;

    /// <summary>
    /// Gets the spoiler sub-manager for this vehicle. Lazy-initialized via GetComponentInChildren on first access; returns null if no spoiler manager child exists.
    /// </summary>
    public RCC_Customizer_SpoilerManager SpoilerManager {

        get {

            if (_spoilerManager == null)
                _spoilerManager = GetComponentInChildren<RCC_Customizer_SpoilerManager>(true);

            return _spoilerManager;

        }

    }

    /// <summary>
    /// Siren Manager.
    /// </summary>
    private RCC_Customizer_SirenManager _sirenManager;

    /// <summary>
    /// Gets the siren sub-manager for this vehicle (police / emergency siren attachments). Lazy-initialized via GetComponentInChildren on first access; returns null if no siren manager child exists.
    /// </summary>
    public RCC_Customizer_SirenManager SirenManager {

        get {

            if (_sirenManager == null)
                _sirenManager = GetComponentInChildren<RCC_Customizer_SirenManager>(true);

            return _sirenManager;

        }

    }

    /// <summary>
    /// Customization Manager.
    /// </summary>
    private RCC_Customizer_CustomizationManager _customizationManager;

    /// <summary>
    /// Gets the customization data sub-manager (suspension, camber, driving aids, transmission) for this vehicle. Lazy-initialized via GetComponentInChildren on first access; returns null if no customization manager child exists.
    /// </summary>
    public RCC_Customizer_CustomizationManager CustomizationManager {

        get {

            if (_customizationManager == null)
                _customizationManager = GetComponentInChildren<RCC_Customizer_CustomizationManager>(true);

            return _customizationManager;

        }

    }

    /// <summary>
    /// Decal Manager.
    /// </summary>
    private RCC_Customizer_DecalManager _decalManager;

    /// <summary>
    /// Gets the decal sub-manager for this vehicle (livery / sticker placement). Lazy-initialized via GetComponentInChildren on first access; returns null if no decal manager child exists.
    /// </summary>
    public RCC_Customizer_DecalManager DecalManager {

        get {

            if (_decalManager == null)
                _decalManager = GetComponentInChildren<RCC_Customizer_DecalManager>(true);

            return _decalManager;

        }

    }

    /// <summary>
    /// Neon Manager.
    /// </summary>
    private RCC_Customizer_NeonManager _neonManager;

    /// <summary>
    /// Gets the neon sub-manager for this vehicle (under-glow neon lights). Lazy-initialized via GetComponentInChildren on first access; returns null if no neon manager child exists.
    /// </summary>
    public RCC_Customizer_NeonManager NeonManager {

        get {

            if (_neonManager == null)
                _neonManager = GetComponentInChildren<RCC_Customizer_NeonManager>(true);

            return _neonManager;

        }

    }

    #endregion

    public void Awake() {

        //  Return if initialize method is set to none.
        if (initializeMethod == InitializeMethod.None)
            return;

        //  Loads the latest loadout.
        if (autoLoadLoadout)
            Load();

        //  Initializes all managers.
        if (initializeMethod == InitializeMethod.Awake)
            Initialize();

    }

    public void OnEnable() {

        //  Return if initialize method is set to none.
        if (initializeMethod == InitializeMethod.None)
            return;

        //  Initializes all managers.
        if (initializeMethod == InitializeMethod.OnEnable)
            Initialize();

    }

    public void Start() {

        //  Return if initialize method is set to none.
        if (initializeMethod == InitializeMethod.None)
            return;

        //  Initializes all managers.
        if (initializeMethod == InitializeMethod.Start)
            Initialize();

        //  Initializes all managers.
        if (initializeMethod == InitializeMethod.DelayedWithFixedUpdate)
            StartCoroutine(Delayed());

    }

    /// <summary>
    /// Initializing all managers delayed.
    /// </summary>
    /// <returns></returns>
    private IEnumerator Delayed() {

        yield return new WaitForFixedUpdate();
        Initialize();

    }

    /// <summary>
    /// Initialize all managers.
    /// </summary>
    public void Initialize() {

        if (loadout == null)
            loadout = new RCC_Customizer_Loadout();

        //  Initializes paint manager.
        if (PaintManager)
            PaintManager.Initialize();

        //  Initializes wheel manager.
        if (WheelManager)
            WheelManager.Initialize();

        //  Initializes upgrade manager.
        if (UpgradeManager)
            UpgradeManager.Initialize();

        //  Initializes spoiler manager.
        if (SpoilerManager)
            SpoilerManager.Initialize();

        //  Initializes siren manager.
        if (SirenManager)
            SirenManager.Initialize();

        //  Initializes customization manager.
        if (CustomizationManager)
            CustomizationManager.Initialize();

        //  Initializes decal manager.
        if (DecalManager)
            DecalManager.Initialize();

        //  Initializes neon manager.
        if (NeonManager)
            NeonManager.Initialize();

    }

    /// <summary>
    /// Gets the current customization loadout. Creates a new empty loadout if none exists.
    /// </summary>
    /// <returns>The current customization loadout.</returns>
    public RCC_Customizer_Loadout GetLoadout() {

        if (loadout != null) {

            return loadout;

        } else {

            loadout = new RCC_Customizer_Loadout();
            return loadout;

        }

    }

    /// <summary>
    /// Saves the current loadout with Json.
    /// </summary>
    public void Save() {

        if (loadout == null)
            loadout = new RCC_Customizer_Loadout();

        PlayerPrefs.SetString(saveFileName, JsonUtility.ToJson(loadout));

    }

    /// <summary>
    /// Loads the latest saved loadout with Json.
    /// </summary>
    public void Load() {

        if (PlayerPrefs.HasKey(saveFileName))
            loadout = (RCC_Customizer_Loadout)JsonUtility.FromJson(PlayerPrefs.GetString(saveFileName), typeof(RCC_Customizer_Loadout));

    }

    /// <summary>
    /// Deletes the latest saved loadout and restores the vehicle setup.
    /// </summary>
    public void Delete() {

        PlayerPrefs.DeleteKey(saveFileName);

        loadout = new RCC_Customizer_Loadout();

        //  Restores paint manager.
        if (PaintManager)
            PaintManager.Restore();

        //  Restores wheel manager.
        if (WheelManager)
            WheelManager.Restore();

        //  Restores upgrade manager.
        if (UpgradeManager)
            UpgradeManager.Restore();

        //  Restores spoiler manager.
        if (SpoilerManager)
            SpoilerManager.Restore();

        //  Restores siren manager.
        if (SirenManager)
            SirenManager.Restore();

        //  Restores customization manager.
        if (CustomizationManager)
            CustomizationManager.Restore();

        //  Restores decal manager.
        if (DecalManager)
            DecalManager.Restore();

        //  Restores neon manager.
        if (NeonManager)
            NeonManager.Restore();

    }

    /// <summary>
    /// Updates the loadout and all managers.
    /// </summary>
    /// <param name="component"></param>
    public void Refresh(MonoBehaviour component) {

        if (loadout == null)
            return;

        loadout.UpdateLoadout(component);

    }

    /// <summary>
    /// Hides all visual upgraders.
    /// </summary>
    public void HideAll() {

        //  Restores spoiler manager.
        if (SpoilerManager)
            SpoilerManager.DisableAll();

        //  Restores siren manager.
        if (SirenManager)
            SirenManager.DisableAll();

        //  Restores decal manager.
        if (DecalManager)
            DecalManager.DisableAll();

        //  Restores neon manager.
        if (NeonManager)
            NeonManager.DisableAll();

    }

    /// <summary>
    /// Shows all visual upgrades.
    /// </summary>
    public void ShowAll() {

        //  Restores spoiler manager.
        if (SpoilerManager)
            SpoilerManager.EnableAll();

        //  Restores siren manager.
        if (SirenManager)
            SirenManager.EnableAll();

        //  Restores decal manager.
        if (DecalManager)
            DecalManager.EnableAll();

        //  Restores neon manager.
        if (NeonManager)
            NeonManager.EnableAll();

    }

    private void Reset() {

        RCC_CarControllerV4 carController = GetComponentInParent<RCC_CarControllerV4>(true);

        if (carController)
            saveFileName = carController.transform.name;

    }

}

