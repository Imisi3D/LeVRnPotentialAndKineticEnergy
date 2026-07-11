using System;
using System.Collections.Generic;
using LeVRn.Tutor;

namespace LeVRn.Tutor.Tests
{
    /// <summary>
    /// Drives a <see cref="TutorEngine"/> the way a UI would, but according to a
    /// scripted learner profile: for each objective, the attempt on which the
    /// learner finally answers correctly (1, 2 or 3), or <see cref="Never"/> to
    /// keep answering wrong until the walkthrough. Purely deterministic: no
    /// randomness, no time.
    /// </summary>
    public sealed class LearnerSimulator
    {
        /// <summary>Sentinel meaning the learner never answers this objective correctly.</summary>
        public const int Never = 0;

        private readonly IReadOnlyDictionary<string, int> _profile;

        public LearnerSimulator(IReadOnlyDictionary<string, int> profileByObjective)
        {
            _profile = profileByObjective ?? throw new ArgumentNullException(nameof(profileByObjective));
        }

        /// <summary>Run the whole session to its end.</summary>
        public void Run(TutorEngine engine)
        {
            if (engine == null) throw new ArgumentNullException(nameof(engine));
            engine.Start();

            // Guard against a runaway loop if behaviour ever regresses.
            int guard = 0;
            while (!engine.IsEnded)
            {
                if (++guard > 100000)
                    throw new InvalidOperationException("Simulator did not terminate; possible engine regression.");

                switch (engine.Phase)
                {
                    case EnginePhase.AwaitingSayContinue:
                    case EnginePhase.AwaitingAffirmContinue:
                    case EnginePhase.AwaitingWalkthroughContinue:
                        engine.Continue();
                        break;

                    case EnginePhase.AwaitingAnswer:
                        AnswerCurrent(engine);
                        break;

                    default:
                        throw new InvalidOperationException("Unexpected phase while running: " + engine.Phase);
                }
            }
        }

        private void AnswerCurrent(TutorEngine engine)
        {
            var q = engine.CurrentQuestion;
            int target = _profile.TryGetValue(q.Objective, out var t) ? t : 1;
            int presentationNumber = engine.CurrentAttempts + 1; // 1-based

            bool answerCorrectNow = target != Never && presentationNumber == target;
            if (answerCorrectNow)
                engine.Answer(q.Correct);
            else
                engine.Answer(WrongIndex(q)); // deliberately wrong, deterministic
        }

        private static int WrongIndex(AskNode q)
        {
            // Any index other than the correct one; options always have >= 2 entries.
            return (q.Correct + 1) % q.Options.Count;
        }
    }
}
