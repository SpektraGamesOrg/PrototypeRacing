using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SRDebugger.Services;
using SRF.Service;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SRDebugger.UI.Other
{
    using Controls;
    using SRF;
    using SRF.UI;
    using UnityEngine;
    using UnityEngine.Serialization;

    public class TriggerRoot : SRMonoBehaviourEx
    {
        [RequiredField] public Canvas Canvas;

        [RequiredField] public LongPressButton TapHoldButton;

        [RequiredField] public RectTransform TriggerTransform;

        [RequiredField] public ErrorNotifier ErrorNotifier;

        [RequiredField] [FormerlySerializedAs("TriggerButton")]
        public MultiTapButton TripleTapButton;

#if UNITY_EDITOR

        private EventSystem _eventSystem = null;
        private readonly Dictionary<GameObject, bool> _inputFields = new Dictionary<GameObject, bool>();

        protected override void Update()
        {
            base.Update();

            if (!ShouldToggleThisFrame()) return;
            if (IsTypingIntoUI()) return;

            var svc = SRServiceManager.GetService<IDebugPanelService>();
            if (svc == null) return;

            if (svc.IsVisible)
                SRDebug.Instance.HideDebugPanel();
            else
                SRDebug.Instance.ShowDebugPanel();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ShouldToggleThisFrame()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.DoubleQuote))
                return true;

            // Avoid touching inputString when nothing happened.
            if (!Input.anyKeyDown) return false;

            var s = Input.inputString;
            if (string.IsNullOrEmpty(s)) return false;

            // Fast char scan (no allocations, faster than multiple Contains)
            for (int i = 0; i < s.Length; i++)
            {
                switch (s[i])
                {
                    case '<':
                    case '>':
                    case '"':
                    case '\u00E9': // é
                        return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsTypingIntoUI()
        {
            var es = _eventSystem ? _eventSystem : (_eventSystem = EventSystem.current);
            if (!es) return false;

            var go = es.currentSelectedGameObject;
            if (!go) return false;

            if (!_inputFields.TryGetValue(go, out var isInputField))
            {
                isInputField = go.TryGetComponent<TMP_InputField>(out _) || go.TryGetComponent<InputField>(out _);
                _inputFields[go] = isInputField;
            }

            return isInputField;
        }
#endif
    }
}