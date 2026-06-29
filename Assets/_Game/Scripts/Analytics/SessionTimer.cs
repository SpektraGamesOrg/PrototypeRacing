using System;
using System.Globalization;
using System.Threading;
using _Game.Scripts.Utils.VContainer;
using Cysharp.Threading.Tasks;
using SpektraGames.SpektraUtilities.Runtime;
using UnityEngine;

namespace Analytics
{
    public class SessionTimer : SingletonComponent<SessionTimer>
    {
        private static bool _hasFocus;

        private const int TimeoutDuration = 60;

        public static float SessionTime = 0f;

        private CancellationTokenSource _timeoutCts;

        protected override void Awake()
        {
            base.Awake();
            ResetTimer();
            FirebaseAnalyticsService.OnAnyEventReported += ResetTimer;
        }

        private async UniTask SendDurationIdle(CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(TimeoutDuration), cancellationToken: token);

                if (!token.IsCancellationRequested)
                {
                    if (ServiceLocator.TryGetService(out IAnalyticsService analyticsService))
                    {
                        analyticsService.SetUserProperty("current_duration", SessionTime.ToString(CultureInfo.InvariantCulture));
                        ResetTimer();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // no-op, the operation was cancelled
            }
        }

        public void ResetTimer()
        {
            _timeoutCts?.Cancel();
            _timeoutCts?.Dispose();

            _timeoutCts = new CancellationTokenSource();
            SendDurationIdle(_timeoutCts.Token).Forget();
        }

        private void Update()
        {
            if (_hasFocus)
            {
                SessionTime += Time.unscaledDeltaTime;
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            _hasFocus = hasFocus;

            if (!hasFocus)
            {
                _timeoutCts?.Cancel();
            }
            else
            {
                ResetTimer();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _timeoutCts?.Cancel();
            _timeoutCts?.Dispose();
            FirebaseAnalyticsService.OnAnyEventReported -= ResetTimer;
        }
    }
}