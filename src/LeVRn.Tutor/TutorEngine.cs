using System;
using System.Collections.Generic;

namespace LeVRn.Tutor
{
    /// <summary>What input the engine is currently waiting for.</summary>
    public enum EnginePhase
    {
        NotStarted,
        AwaitingSayContinue,
        AwaitingAnswer,
        AwaitingAffirmContinue,
        AwaitingWalkthroughContinue,
        Ended
    }

    /// <summary>
    /// The offline, grounded tutor: a state machine that retrieves authored
    /// responses. There is no language model and no network. Adaptivity comes
    /// from attempt counts, mastery and pacing selecting which authored response
    /// is emitted next. Behaviour mirrors reference/tutor_prototype.html exactly.
    ///
    /// The engine is engine-agnostic (no UnityEngine, no I/O). It is driven by
    /// three inputs, Start, Continue and Answer, and reports everything through
    /// <see cref="TutorEvent"/>s so a Unity layer, a web layer or a headless test
    /// harness can all drive it identically.
    /// </summary>
    public sealed class TutorEngine
    {
        // The adaptation rule from the brief is specifically: if any DEFINE
        // objective question needed more than one attempt, insert the extra
        // classification items immediately before NIYAT's formula introduction.
        // These identifiers encode that rule; they are not learner-facing text.
        private const string StruggleObjectiveId = "define";
        private const string AdaptAnchorGuideId = "niyat";

        private readonly TutorScript _content;
        private readonly List<TutorNode> _script;
        private readonly IReadOnlyList<AskNode> _extraClassify;
        private readonly Mastery _mastery;
        private readonly List<TutorEvent> _events = new List<TutorEvent>();

        private int _i;
        private int _attempts;      // wrong attempts on the current question
        private bool _switched;     // lens switched on the current question
        private int _switches;      // total lens switches this session
        private int _answers;       // total answers received this session
        private bool _defineStruggle;
        private bool _adaptationResolved; // guard: the adaptation decision was made
        private bool _extrasInserted;     // observable: extras were actually spliced in
        private AskNode _currentAsk;
        private EnginePhase _phase = EnginePhase.NotStarted;

        public TutorEngine(TutorScript content)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _script = new List<TutorNode>(content.Nodes);
            _extraClassify = content.ExtraClassify;
            _mastery = new Mastery(content.Objectives);
        }

        /// <summary>Fires for every event, in order, as the session progresses.</summary>
        public event Action<TutorEvent> EventEmitted;

        public TutorScript Content => _content;
        public Mastery Mastery => _mastery;
        public EnginePhase Phase => _phase;
        public int LensSwitches => _switches;
        public int AnswersReceived => _answers;

        /// <summary>Wrong attempts recorded so far on the current question. The
        /// next presentation number is this value plus one.</summary>
        public int CurrentAttempts => _attempts;

        public bool ExtrasInserted => _extrasInserted;
        public bool IsEnded => _phase == EnginePhase.Ended;

        /// <summary>The complete ordered event log for this session.</summary>
        public IReadOnlyList<TutorEvent> Events => _events;

        /// <summary>
        /// The question currently awaiting an answer, or null. A production UI
        /// does not need this; a headless harness reads it (including
        /// <see cref="AskNode.Correct"/>) to simulate a learner profile.
        /// </summary>
        public AskNode CurrentQuestion => _phase == EnginePhase.AwaitingAnswer ? _currentAsk : null;

        /// <summary>Begin the session and present the first node.</summary>
        public void Start()
        {
            if (_phase != EnginePhase.NotStarted)
                throw new InvalidOperationException("Session already started.");
            Emit(TutorEvent.SessionStarted());
            Step();
        }

        /// <summary>Advance past a Say, an affirmation or a walkthrough.</summary>
        public void Continue()
        {
            switch (_phase)
            {
                case EnginePhase.AwaitingSayContinue:
                    _i++;
                    Step();
                    return;

                case EnginePhase.AwaitingAffirmContinue:
                case EnginePhase.AwaitingWalkthroughContinue:
                    // Post-question advance: reset per-question state, move on,
                    // then evaluate difficulty adaptation, exactly as the reference.
                    _attempts = 0;
                    _switched = false;
                    _i++;
                    MaybeAdapt();
                    Step();
                    return;

                default:
                    throw new InvalidOperationException("Continue is not valid in phase " + _phase + ".");
            }
        }

        /// <summary>Answer the current question by option index.</summary>
        public void Answer(int index)
        {
            if (_phase != EnginePhase.AwaitingAnswer)
                throw new InvalidOperationException("Answer is not valid in phase " + _phase + ".");

            var node = _currentAsk;
            if (index < 0 || index >= node.Options.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            _answers++;
            bool correct = index == node.Correct;
            int attemptNumber = _attempts + 1; // 1-based presentation number
            Emit(TutorEvent.AnswerReceived(node.Objective, index, correct, attemptNumber));

            if (correct)
            {
                double gain = _attempts == 0 ? 1.0 : _attempts == 1 ? 0.6 : 0.3;
                _mastery.Record(node.Objective, gain);
                if (node.Objective == StruggleObjectiveId && _attempts > 0)
                    _defineStruggle = true;

                // Affirmation is spoken by the switch guide if a switch happened,
                // otherwise by the original guide.
                string speaker = _switched ? node.Bank.SwitchTo : node.GuideId;
                Emit(TutorEvent.AffirmGiven(node.Objective, speaker, node.Bank.Affirm, attemptNumber, gain));
                _phase = EnginePhase.AwaitingAffirmContinue;
                return;
            }

            _attempts++;
            if (_attempts == 1)
            {
                // Wrong once: hint from the same guide, then re-ask.
                Emit(TutorEvent.HintGiven(node.Objective, node.GuideId, node.Bank.Hint));
                _phase = EnginePhase.AwaitingAnswer;
                Emit(TutorEvent.QuestionPresented(node.Objective, node.GuideId, node.Bank.Hint, AskPresentation.HintReask));
            }
            else if (_attempts == 2)
            {
                // Wrong twice: lens switch to a different guide, then re-ask.
                _switched = true;
                _switches++;
                Emit(TutorEvent.LensSwitched(node.Objective, node.GuideId, node.Bank.SwitchTo, node.Bank.SwitchText));
                _phase = EnginePhase.AwaitingAnswer;
                Emit(TutorEvent.QuestionPresented(node.Objective, node.Bank.SwitchTo, node.Bank.SwitchText, AskPresentation.SwitchReask));
            }
            else
            {
                // Wrong three times: walkthrough by the switch guide; objective
                // counted as asked with score 0, which flags it for review.
                _mastery.Record(node.Objective, 0.0);
                if (node.Objective == StruggleObjectiveId)
                    _defineStruggle = true;
                Emit(TutorEvent.WalkthroughGiven(node.Objective, node.Bank.SwitchTo, node.Bank.Walkthrough));
                _phase = EnginePhase.AwaitingWalkthroughContinue;
            }
        }

        private void Step()
        {
            while (true)
            {
                if (_i >= _script.Count)
                {
                    EndSession();
                    return;
                }

                var node = _script[_i];
                switch (node.Type)
                {
                    case NodeType.Scene:
                        Emit(TutorEvent.SceneTriggered(((SceneNode)node).Fn));
                        _i++;
                        continue; // scenes auto-advance

                    case NodeType.End:
                        EndSession();
                        return;

                    case NodeType.Say:
                        var say = (SayNode)node;
                        _phase = EnginePhase.AwaitingSayContinue;
                        Emit(TutorEvent.SayPresented(say.GuideId, say.Text));
                        return;

                    case NodeType.Ask:
                        var ask = (AskNode)node;
                        _currentAsk = ask;
                        _phase = EnginePhase.AwaitingAnswer;
                        Emit(TutorEvent.QuestionPresented(ask.Objective, ask.GuideId, ask.Text, AskPresentation.Initial));
                        return;

                    default:
                        throw new InvalidOperationException("Unknown node type: " + node.Type);
                }
            }
        }

        /// <summary>
        /// Difficulty adaptation: if the learner was shaky on definitions, splice
        /// the two extra classification items in immediately before Niyat's
        /// formula introduction, exactly once per session. Idempotent: repeated
        /// evaluation never inserts twice.
        /// </summary>
        private void MaybeAdapt()
        {
            if (_adaptationResolved) return;
            if (_i >= _script.Count) return;

            var next = _script[_i];
            if (next.Type == NodeType.Say && ((SayNode)next).GuideId == AdaptAnchorGuideId)
            {
                if (_defineStruggle)
                {
                    _script.InsertRange(_i, _extraClassify);
                    _extrasInserted = true;
                    _adaptationResolved = true;
                    Emit(TutorEvent.ExtrasInserted(_extraClassify.Count));
                }
                else
                {
                    _adaptationResolved = true;
                    Emit(TutorEvent.ExtrasSkipped());
                }
            }
        }

        private void EndSession()
        {
            if (_phase == EnginePhase.Ended) return;
            _phase = EnginePhase.Ended;
            Emit(TutorEvent.SessionEnded(_mastery.OverallPercent()));
        }

        private void Emit(TutorEvent e)
        {
            _events.Add(e);
            EventEmitted?.Invoke(e);
        }
    }
}
