using System;
using System.Collections.Generic;
using LeVRn.Progress;
using LeVRn.Tutor;

namespace LeVRn.Progress.Tests
{
    /// <summary>
    /// Bridges the Task 01 tutor engine to the progress layer: it subscribes to
    /// the engine's events, updates a <see cref="LearnerProgress"/>, and saves
    /// after every scored event. This adapter lives in the test project on purpose,
    /// so the LeVRn.Progress library keeps no dependency on LeVRn.Tutor.
    ///
    /// Resume support: the tutor engine has no serialized cursor, but it is
    /// deterministic, so a resumed session is reconstructed by replaying the same
    /// answer profile on a fresh engine. To avoid double-counting the head that was
    /// already persisted, the adapter is told how many scored events to skip; it
    /// applies attempts, lens switches, and scored events only once the replay has
    /// passed that cursor.
    /// </summary>
    public sealed class TutorProgressAdapter
    {
        private readonly LearnerProgress _progress;
        private readonly ProgressStore _store;
        private readonly Func<DateTime> _clock;
        private readonly int _skipScored;

        /// <summary>Total scored events observed so far (including skipped ones).</summary>
        public int SeenScored { get; private set; }

        public TutorProgressAdapter(LearnerProgress progress, ProgressStore store, Func<DateTime> clock, int skipScored = 0)
        {
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _skipScored = skipScored;
        }

        public void Attach(TutorEngine engine) => engine.EventEmitted += OnEvent;

        private bool PastCursor => SeenScored >= _skipScored;

        private void OnEvent(TutorEvent e)
        {
            switch (e.Kind)
            {
                case TutorEventKind.AnswerReceived:
                    // Every answer (right or wrong) is an attempt on its objective.
                    if (PastCursor) _progress.RecordAttempt(e.Objective, _clock());
                    break;

                case TutorEventKind.LensSwitched:
                    if (PastCursor) _progress.RecordLensSwitch(_clock());
                    break;

                case TutorEventKind.AffirmGiven:
                    ApplyScored(e.Objective, e.Gain);
                    break;

                case TutorEventKind.WalkthroughGiven:
                    // A walkthrough resolves the question with a score of 0.
                    ApplyScored(e.Objective, 0.0);
                    break;

                case TutorEventKind.SessionEnded:
                    _progress.Complete(e.OverallPercent, _clock());
                    _store.Save(_progress);
                    break;
            }
        }

        private void ApplyScored(string objective, double gain)
        {
            SeenScored += 1;
            if (SeenScored <= _skipScored) return; // already persisted in the loaded head
            _progress.RecordResolved(objective, gain, _clock());
            _store.Save(_progress); // save after every scored event
        }
    }

    /// <summary>
    /// Deterministically drives a tutor engine from an answer profile: for each
    /// objective, how many attempts the learner needs before answering correctly
    /// (1 = right first time; 3 = two wrong then right; 4+ = never right, triggers
    /// the walkthrough). Mirrors the Task 01 test harness.
    /// </summary>
    public static class LearnerDriver
    {
        /// <summary>Drive to completion, or stop early once the adapter has seen
        /// <paramref name="stopAfterScored"/> scored events (to simulate a quit).</summary>
        public static void Drive(TutorEngine engine, IReadOnlyDictionary<string, int> attemptsNeeded,
                                 TutorProgressAdapter adapter = null, int? stopAfterScored = null)
        {
            engine.Start();
            while (!engine.IsEnded)
            {
                if (stopAfterScored.HasValue && adapter != null && adapter.SeenScored >= stopAfterScored.Value)
                    return; // simulate the app quitting mid-session

                switch (engine.Phase)
                {
                    case EnginePhase.AwaitingAnswer:
                        var q = engine.CurrentQuestion;
                        int needed = attemptsNeeded.TryGetValue(q.Objective, out int n) ? n : 1;
                        bool answerCorrect = (engine.CurrentAttempts + 1) >= needed;
                        int index = answerCorrect ? q.Correct : (q.Correct + 1) % q.Options.Count;
                        engine.Answer(index);
                        break;

                    case EnginePhase.AwaitingSayContinue:
                    case EnginePhase.AwaitingAffirmContinue:
                    case EnginePhase.AwaitingWalkthroughContinue:
                        engine.Continue();
                        break;

                    default:
                        throw new InvalidOperationException("Unexpected phase: " + engine.Phase);
                }
            }
        }
    }
}
