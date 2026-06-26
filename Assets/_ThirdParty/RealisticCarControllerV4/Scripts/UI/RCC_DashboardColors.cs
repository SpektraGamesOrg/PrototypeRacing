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
using UnityEngine.UI;

/// <summary>
/// Changes HUD image colors dynamically using RGB UI Sliders.
/// Applies the chosen color to all assigned HUD images each frame while preserving their individual alpha values.
/// </summary>
public class RCC_DashboardColors : RCC_Core {

    /// <summary>
    /// Array of HUD Image components whose colors will be updated each frame.
    /// </summary>
    [Tooltip("Array of HUD Image components whose colors will be updated each frame.")]
    public Image[] huds;

    /// <summary>
    /// Current HUD color applied to all images. Updated from slider values each frame.
    /// </summary>
    [Tooltip("Current HUD color applied to all images. Updated from slider values each frame.")]
    public Color hudColor = Color.white;

    /// <summary>
    /// UI Slider controlling the red channel of the HUD color.
    /// </summary>
    [Tooltip("UI Slider controlling the red channel of the HUD color.")]
    public Slider hudColor_R;

    /// <summary>
    /// UI Slider controlling the green channel of the HUD color.
    /// </summary>
    [Tooltip("UI Slider controlling the green channel of the HUD color.")]
    public Slider hudColor_G;

    /// <summary>
    /// UI Slider controlling the blue channel of the HUD color.
    /// </summary>
    [Tooltip("UI Slider controlling the blue channel of the HUD color.")]
    public Slider hudColor_B;

    private void Start() {

        if (huds == null || huds.Length < 1)
            enabled = false;

        if (hudColor_R && hudColor_G && hudColor_B) {

            hudColor_R.value = hudColor.r;
            hudColor_G.value = hudColor.g;
            hudColor_B.value = hudColor.b;

        }

    }

    private void Update() {

        if (hudColor_R && hudColor_G && hudColor_B)
            hudColor = new Color(hudColor_R.value, hudColor_G.value, hudColor_B.value);

        for (int i = 0; i < huds.Length; i++) {

            if (huds[i] != null)
                huds[i].color = new Color(hudColor.r, hudColor.g, hudColor.b, huds[i].color.a);

        }

    }

}
