using System;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// An <see cref="IProgress{Single}"/> that maps a phase's <i>logical</i> 0..1 progress onto a fixed
    /// <c>[start, end]</c> slice of an underlying progress sink (usually the loading bar).
    ///
    /// This lets each loading phase report a clean 0..1 without knowing which slice of the overall bar it
    /// occupies: a phase always reports <c>Report(1f)</c> at its end, and the slice decides what that means
    /// on the real bar. Example: a phase scoped to <c>[0, 0.3]</c> turns <c>Report(0.3f)</c> into a real
    /// <c>0.09</c>, while the same call on a <c>[0, 1]</c> slice stays <c>0.3</c> - the caller never writes
    /// the scaled value by hand.
    /// </summary>
    public sealed class RangedProgress : IProgress<float>
    {
        private readonly IProgress<float> _inner;
        private readonly float _start;
        private readonly float _end;

        public RangedProgress(IProgress<float> inner, float start, float end)
        {
            _inner = inner;
            _start = Mathf.Clamp01(start);
            _end = Mathf.Clamp01(end);
        }

        /// <summary>Real bar value this slice begins at.</summary>
        public float Start => _start;

        /// <summary>Real bar value this slice ends at.</summary>
        public float End => _end;

        /// <summary>Reports logical 0..1 progress for this phase; mapped onto <c>[Start, End]</c> on the sink.</summary>
        public void Report(float logical01)
        {
            _inner?.Report(Mathf.Lerp(_start, _end, Mathf.Clamp01(logical01)));
        }
    }
}
