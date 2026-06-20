using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core
{
    /// <summary>
    /// Post-load step for the Game scene. There is no gameplay screen yet, so this only completes the bar.
    ///
    /// Per the "a screen is always active" rule, this deliberately does NOT clear the screen layer - the
    /// loading screen stays up until a real game screen exists. When one is added, mirror
    /// <see cref="MainMenuSceneLoader"/>: prepare the scene here, then
    /// <c>GameUIManager.Instance.SwitchScreen&lt;GameScreen&gt;()</c> to replace the loading screen.
    /// </summary>
    public sealed class GameSceneLoader : SceneLoaderBase
    {
        public override SceneType SceneType => SceneType.Game;

        public override UniTask LoadAsync(IProgress<float> progress, CancellationToken token)
        {
            progress?.Report(1f);

            // TODO: when a gameplay screen exists, prepare it and SwitchScreen<GameScreen>() here so the
            // loading screen is replaced by a real screen instead of staying up.
            return UniTask.CompletedTask;
        }
    }
}
