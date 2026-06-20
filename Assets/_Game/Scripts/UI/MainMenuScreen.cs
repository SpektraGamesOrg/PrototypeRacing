using System;
using Save;
using TMPro;
using UIManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Vehicles;

namespace UI
{
    /// <summary>
    /// Garage / main menu screen. Lets the player browse the vehicle roster (left/right arrows),
    /// buy the displayed vehicle from the shop, jump into customization, and start the game.
    /// All button references are wired in the inspector; click handlers are attached in code.
    ///
    /// This is intentionally self-contained: vehicle data comes from <see cref="VehicleContainer"/>
    /// and ownership / currency come from <see cref="SaveManager"/>. The actual 3D car swap in the
    /// garage is left to listeners of <see cref="VehicleDisplayChanged"/>.
    /// </summary>
    public class MainMenuScreen : ScreenBase
    {
        [Header("Vehicle Navigation")]
        [SerializeField] private Button leftArrowButton;
        [SerializeField] private Button rightArrowButton;
        [SerializeField] private TMP_Text vehicleNameText;
        [SerializeField] private GameObject lockedBadge;

        [Header("Top Bar")]
        [SerializeField] private TMP_Text coinAmountText;
        [SerializeField] private Button settingsButton;

        [Header("Actions")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button customizeButton;
        [SerializeField] private Button buyButton;
        [SerializeField] private TMP_Text buyPriceText;
        [SerializeField] private GameObject buyCoinIcon;

        [Header("Config")]
        [SerializeField] private int defaultVehiclePrice = 5000;
        [SerializeField] private string gameSceneName = "Game";

        /// <summary>Raised whenever the displayed (browsed) vehicle changes. Hook this to swap the 3D car.</summary>
        public event Action<VehicleID> VehicleDisplayChanged;
        /// <summary>Raised when the player taps Customize (no customization screen exists yet).</summary>
        public event Action CustomizeRequested;
        /// <summary>Raised when the player taps Settings (no settings screen exists yet).</summary>
        public event Action SettingsRequested;

        private int _displayedIndex = -1;

        private static VehicleContainer Container => VehicleContainer.Instance;

        protected override void Awake()
        {
            base.Awake();

            EnsureStarterVehicle();

            if (leftArrowButton) leftArrowButton.onClick.AddListener(ShowPreviousVehicle);
            if (rightArrowButton) rightArrowButton.onClick.AddListener(ShowNextVehicle);
            if (playButton) playButton.onClick.AddListener(OnPlayClicked);
            if (customizeButton) customizeButton.onClick.AddListener(OnCustomizeClicked);
            if (buyButton) buyButton.onClick.AddListener(OnBuyClicked);
            if (settingsButton) settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        private void OnDestroy()
        {
            if (leftArrowButton) leftArrowButton.onClick.RemoveListener(ShowPreviousVehicle);
            if (rightArrowButton) rightArrowButton.onClick.RemoveListener(ShowNextVehicle);
            if (playButton) playButton.onClick.RemoveListener(OnPlayClicked);
            if (customizeButton) customizeButton.onClick.RemoveListener(OnCustomizeClicked);
            if (buyButton) buyButton.onClick.RemoveListener(OnBuyClicked);
            if (settingsButton) settingsButton.onClick.RemoveListener(OnSettingsClicked);
        }

        protected override void OnBeforeShowing(bool immediate, object uiData = null)
        {
            base.OnBeforeShowing(immediate, uiData);

            if (_displayedIndex < 0)
                _displayedIndex = Mathf.Max(0, IndexOf(SaveManager.SelectedVehicle));

            Refresh();
        }

        // ---------------------------------------------------------------------
        // Vehicle browsing
        // ---------------------------------------------------------------------

        public void ShowNextVehicle() => StepVehicle(1);
        public void ShowPreviousVehicle() => StepVehicle(-1);

        private void StepVehicle(int direction)
        {
            int count = VehicleCount;
            if (count == 0)
                return;

            _displayedIndex = (_displayedIndex + direction + count) % count;
            Refresh();
            VehicleDisplayChanged?.Invoke(DisplayedVehicle);
        }

        // ---------------------------------------------------------------------
        // Button handlers
        // ---------------------------------------------------------------------

        private void OnBuyClicked()
        {
            VehicleID id = DisplayedVehicle;
            if (id == VehicleID.None || SaveManager.IsOwned(id))
                return;

            if (SaveManager.Coins < defaultVehiclePrice)
            {
                Debug.LogError($"[MainMenu] Not enough coins to buy {id}. Need {defaultVehiclePrice}, have {SaveManager.Coins}.");
                return;
            }

            SaveManager.Coins -= defaultVehiclePrice;
            SaveManager.AddOwned(id);
            SaveManager.SelectVehicle(id);
            SaveManager.Save();

            Refresh();
        }

        private void OnPlayClicked()
        {
            SaveManager.Save();

            if (string.IsNullOrEmpty(gameSceneName))
            {
                Debug.LogError("[MainMenu] No game scene configured on the Play button.");
                return;
            }

            SceneManager.LoadScene(gameSceneName);
        }

        private void OnCustomizeClicked()
        {
            // No customization screen exists yet; surface the intent so it can be hooked up later.
            CustomizeRequested?.Invoke();
            Debug.Log($"[MainMenu] Customize requested for {DisplayedVehicle}.");
        }

        private void OnSettingsClicked()
        {
            // No settings screen exists yet; surface the intent so it can be hooked up later.
            SettingsRequested?.Invoke();
            Debug.Log("[MainMenu] Settings requested.");
        }

        // ---------------------------------------------------------------------
        // View refresh
        // ---------------------------------------------------------------------

        private void Refresh()
        {
            if (coinAmountText)
                coinAmountText.text = SaveManager.Coins.ToString("N0");

            VehicleID id = DisplayedVehicle;
            bool owned = id != VehicleID.None && SaveManager.IsOwned(id);

            if (vehicleNameText)
                vehicleNameText.text = PrettyName(id);

            if (lockedBadge)
                lockedBadge.SetActive(!owned);

            if (owned)
            {
                // Browsing an owned car selects it so the garage reflects the choice.
                SaveManager.SelectVehicle(id);

                if (buyButton) buyButton.interactable = false;
                if (buyCoinIcon) buyCoinIcon.SetActive(false);
                if (buyPriceText) buyPriceText.text = "OWNED";
            }
            else
            {
                if (buyButton) buyButton.interactable = SaveManager.Coins >= defaultVehiclePrice;
                if (buyCoinIcon) buyCoinIcon.SetActive(true);
                if (buyPriceText) buyPriceText.text = defaultVehiclePrice.ToString("N0");
            }
        }

        // ---------------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------------

        private int VehicleCount => Container != null ? Container.Vehicles.Count : 0;

        private VehicleID DisplayedVehicle
        {
            get
            {
                if (Container == null || _displayedIndex < 0 || _displayedIndex >= Container.Vehicles.Count)
                    return VehicleID.None;

                return Container.Vehicles[_displayedIndex].ID;
            }
        }

        private int IndexOf(VehicleID id)
        {
            if (Container == null)
                return -1;

            for (int i = 0; i < Container.Vehicles.Count; i++)
            {
                if (Container.Vehicles[i].ID == id)
                    return i;
            }

            return -1;
        }

        private void EnsureStarterVehicle()
        {
            if (Container == null || Container.Vehicles.Count == 0)
                return;

            if (SaveManager.GetOwnedVehicles().Count > 0)
                return;

            // Grant the first roster entry so the player always has a drivable car.
            VehicleID starter = Container.Vehicles[0].ID;
            SaveManager.AddOwned(starter);
            SaveManager.SelectVehicle(starter);
            SaveManager.Save();
        }

        // Turns "GTR_R35" into "GTR R35" for display. Cheap, allocation-light, only runs on swaps.
        private static string PrettyName(VehicleID id)
        {
            if (id == VehicleID.None)
                return "-";

            return id.ToString().Replace('_', ' ');
        }
    }
}
