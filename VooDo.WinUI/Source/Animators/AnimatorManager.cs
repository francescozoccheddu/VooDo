using Microsoft.UI.Xaml.Media;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

using VooDo.Runtime;
using VooDo.Utils;

namespace VooDo.WinUI.Animators
{

    internal static class AnimatorManager
    {

        private static readonly HashSet<IAnimator> s_animators = new(new Identity.ReferenceComparer<IAnimator>());
        private static readonly Dictionary<Program, int> s_programReferenceCount = new(new Identity.ReferenceComparer<Program>());
        private static bool s_running;
        private static readonly Stopwatch s_stopwatch = new Stopwatch();
        private const double c_maxDeltaTime = 1.0 / 2.0;

        private static void UpdateRunningState()
        {
            bool shouldBeRunning = s_animators.Count > 0;
            if (shouldBeRunning != s_running)
            {
                s_running = shouldBeRunning;
                if (s_running)
                {
                    CompositionTarget.Rendering += CompositionTarget_Rendering;
                    s_stopwatch.Restart();
                }
                else
                {
                    CompositionTarget.Rendering -= CompositionTarget_Rendering;
                    s_stopwatch.Stop();
                }
            }
        }

        private static void CompositionTarget_Rendering(object? _sender, object _e)
        {
            double deltaTime = Math.Min(s_stopwatch.Elapsed.TotalSeconds, c_maxDeltaTime);
            ImmutableArray<IDisposable> locks = s_programReferenceCount.Keys.Select(_p => _p.Lock()).ToImmutableArray();
            try
            {
                foreach (IAnimator a in s_animators)
                {
                    a.Update(deltaTime);
                }
            }
            finally
            {
                foreach (IDisposable l in locks)
                {
                    l.Dispose();
                }
                s_stopwatch.Restart();
            }
        }

        internal static void RegisterAnimator(IAnimator _animator)
        {
            if (s_animators.Add(_animator))
            {
                Program program = _animator.Program;
                if (s_programReferenceCount.TryGetValue(program, out int referenceCount))
                {
                    s_programReferenceCount[program] = referenceCount + 1;
                }
                else
                {
                    s_programReferenceCount.Add(program, 1);
                }
                UpdateRunningState();
            }
        }

        internal static void UnregisterAnimator(IAnimator _animator)
        {
            if (s_animators.Remove(_animator))
            {
                Program program = _animator.Program;
                if (s_programReferenceCount.TryGetValue(program, out int referenceCount))
                {
                    if (referenceCount > 1)
                    {
                        s_programReferenceCount[program] = referenceCount - 1;
                    }
                    else
                    {
                        s_programReferenceCount.Remove(program);
                    }
                }
                UpdateRunningState();
            }
        }

    }

}
