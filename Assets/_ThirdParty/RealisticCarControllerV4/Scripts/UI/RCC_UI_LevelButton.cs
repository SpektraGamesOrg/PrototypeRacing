//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------


using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Per-button hover handler for AIO demo scene selector buttons.
/// Shows a tooltip description when the user hovers over a level button,
/// helping beginners understand what each demo scene demonstrates.
/// </summary>
public class RCC_UI_LevelButton : RCC_Core, IPointerEnterHandler, IPointerExitHandler {

    /// <summary>
    /// Tooltip description displayed when hovering over this button.
    /// </summary>
    [Tooltip("Description shown in the tooltip when hovering over this button.")]
    [TextArea(2, 4)]
    public string description = "";

    /// <summary>
    /// Called when the pointer enters this button. Shows the tooltip.
    /// </summary>
    /// <param name="eventData">Pointer event data from the EventSystem.</param>
    public void OnPointerEnter(PointerEventData eventData) {

        if (RCC_UI_Tooltip.Instance && !string.IsNullOrEmpty(description))
            RCC_UI_Tooltip.Instance.Show(description);

    }

    /// <summary>
    /// Called when the pointer exits this button. Hides the tooltip.
    /// </summary>
    /// <param name="eventData">Pointer event data from the EventSystem.</param>
    public void OnPointerExit(PointerEventData eventData) {

        if (RCC_UI_Tooltip.Instance)
            RCC_UI_Tooltip.Instance.Hide();

    }

    private void OnDisable() {

        //  Hide tooltip when this button is disabled to prevent stale tooltips.
        if (RCC_UI_Tooltip.Instance)
            RCC_UI_Tooltip.Instance.Hide();

    }

}
