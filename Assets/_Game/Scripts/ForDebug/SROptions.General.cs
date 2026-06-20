using System.ComponentModel;
using Save;
using UI;
using UIManager;

// ReSharper disable once CheckNamespace
public partial class SROptions
{
    private const string GeneralCategory = "General";

    [Category(GeneralCategory)]
    public void AddCurrency()
    {
        int half = int.MaxValue / 2;
        int current = SaveManager.Coins;
        SaveManager.Coins = current > int.MaxValue - half ? int.MaxValue : current + half;
        SaveManager.Save();
        GameUIManager.Instance?.GetScreen<MainMenuScreen>()?.RefreshCoinDisplay();
    }

    [Category(GeneralCategory)]
    public void ClearPlayerPrefs()
    {
        SaveManager.ResetAll();
        GameUIManager.Instance?.GetScreen<MainMenuScreen>()?.RefreshCoinDisplay();
    }
}
