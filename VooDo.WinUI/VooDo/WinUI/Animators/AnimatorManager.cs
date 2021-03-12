using Microsoft.UI.Xaml.Media;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using VooDo.Runtime;

namespace VooDo.WinUI.Animators
{

    internal static class AnimatorManager
    {

        private static readonly HashSet<IAnimator> s_animators = new();
        private static readonly Dictionary<IProgram, int> s_programReferenceCount = new();
        private static readonly List<(IAnimator animator, bool add)> s_edits = new();
        private static bool s_updating;

        private static bool s_running;
        private static int s_lastTick;
        private const double c_maxDeltaTime = 1.0 / 2.0;
        private static double s_fps;
        private static double s_renderingTime;
        private static readonly Stopwatch s_stopwatch = new Stopwatch();
        private static readonly EventHandler<object> s_renderingEventHandler = CompositionTarget_Rendering;

        static AnimatorManager()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    Debug.WriteLine($"{s_animators.Count} active animators at {s_fps:F2} fps with average render time of {s_renderingTime:F4}ms");
                    await Task.Delay(1000, CancellationToken.None);
                }
            });
        }

        private static void UpdateRunningState()
        {
            bool shouldBeRunning = s_animators.Count > 0;
            if (shouldBeRunning != s_running)
            {
                s_running = shouldBeRunning;
                if (s_running)
                {
                    s_lastTick = Environment.TickCount;
                    CompositionTarget.Rendering += s_renderingEventHandler;
                }
                else
                {
                    CompositionTarget.Rendering -= s_renderingEventHandler;
                }
            }
        }

        private static void CompositionTarget_Rendering(object? _sender, object _e)
        {
            s_updating = true;
            double deltaTime = Math.Min((Environment.TickCount - s_lastTick) / 1000.0, c_maxDeltaTime);
            s_stopwatch.Restart();
            ImmutableArray<ILocker> locks = s_programReferenceCount.Keys.Select(_p => _p.Lock(true)).ToImmutableArray();
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
            }
            s_updating = false;
            foreach ((IAnimator animator, bool add) in s_edits)
            {
                if (add)
                {
                    RegisterAnimator(animator);
                }
                else
                {
                    UnregisterAnimator(animator);
                }
            }
            s_edits.Clear();
            s_stopwatch.Stop();
            if (deltaTime > 0)
            {
                s_fps = (s_fps + (1 / deltaTime)) / 2;
            }
            s_renderingTime = (s_renderingTime + s_stopwatch.ElapsedMilliseconds) / 2;
            s_lastTick = Environment.TickCount;
        }

        internal static void RegisterAnimator(IAnimator _animator)
        {
            if (s_updating)
            {
                s_edits.Add((_animator, true));
                return;
            }
            if (s_animators.Add(_animator))
            {
                IProgram program = _animator.Program;
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
            if (s_updating)
            {
                s_edits.Add((_animator, false));
                return;
            }
            if (s_animators.Remove(_animator))
            {
                IProgram program = _animator.Program;
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
