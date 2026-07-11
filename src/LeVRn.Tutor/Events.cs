using System;
using System.Globalization;

namespace LeVRn.Tutor
{
    public enum TutorEventKind
    {
        SessionStarted,
        SayPresented,
        QuestionPresented,
        AnswerReceived,
        AffirmGiven,
        HintGiven,
        LensSwitched,
        WalkthroughGiven,
        SceneTriggered,
        ExtrasInserted,
        ExtrasSkipped,
        SessionEnded
    }

    /// <summary>How an Ask node is being presented at this moment.</summary>
    public enum AskPresentation
    {
        Initial,
        HintReask,
        SwitchReask
    }

    /// <summary>
    /// A single observable event from the engine. The UI layer (Unity, web, or a
    /// test harness) subscribes to these and never needs to read engine
    /// internals. Every field is set only when meaningful for the kind; a stable
    /// <see cref="Signature"/> string is provided so runs can be compared for
    /// determinism.
    /// </summary>
    public sealed class TutorEvent
    {
        public TutorEventKind Kind { get; }
        public string GuideId { get; }
        public string ToGuideId { get; }
        public string Objective { get; }
        public string Text { get; }
        public string Scene { get; }
        public AskPresentation Presentation { get; }
        public int OptionIndex { get; }
        public bool IsCorrect { get; }

        /// <summary>1-based attempt number this event relates to.</summary>
        public int Attempt { get; }
        public double Gain { get; }
        public int Count { get; }
        public int OverallPercent { get; }

        private TutorEvent(TutorEventKind kind, string guideId = null, string toGuideId = null,
                           string objective = null, string text = null, string scene = null,
                           AskPresentation presentation = AskPresentation.Initial,
                           int optionIndex = -1, bool isCorrect = false, int attempt = 0,
                           double gain = 0.0, int count = 0, int overallPercent = 0)
        {
            Kind = kind;
            GuideId = guideId;
            ToGuideId = toGuideId;
            Objective = objective;
            Text = text;
            Scene = scene;
            Presentation = presentation;
            OptionIndex = optionIndex;
            IsCorrect = isCorrect;
            Attempt = attempt;
            Gain = gain;
            Count = count;
            OverallPercent = overallPercent;
        }

        public static TutorEvent SessionStarted() =>
            new TutorEvent(TutorEventKind.SessionStarted);

        public static TutorEvent SayPresented(string guideId, string text) =>
            new TutorEvent(TutorEventKind.SayPresented, guideId: guideId, text: text);

        public static TutorEvent QuestionPresented(string objective, string guideId, string text, AskPresentation presentation) =>
            new TutorEvent(TutorEventKind.QuestionPresented, guideId: guideId, objective: objective, text: text, presentation: presentation);

        public static TutorEvent AnswerReceived(string objective, int optionIndex, bool isCorrect, int attempt) =>
            new TutorEvent(TutorEventKind.AnswerReceived, objective: objective, optionIndex: optionIndex, isCorrect: isCorrect, attempt: attempt);

        public static TutorEvent AffirmGiven(string objective, string guideId, string text, int attempt, double gain) =>
            new TutorEvent(TutorEventKind.AffirmGiven, guideId: guideId, objective: objective, text: text, attempt: attempt, gain: gain);

        public static TutorEvent HintGiven(string objective, string guideId, string text) =>
            new TutorEvent(TutorEventKind.HintGiven, guideId: guideId, objective: objective, text: text);

        public static TutorEvent LensSwitched(string objective, string fromGuideId, string toGuideId, string text) =>
            new TutorEvent(TutorEventKind.LensSwitched, guideId: fromGuideId, toGuideId: toGuideId, objective: objective, text: text);

        public static TutorEvent WalkthroughGiven(string objective, string guideId, string text) =>
            new TutorEvent(TutorEventKind.WalkthroughGiven, guideId: guideId, objective: objective, text: text);

        public static TutorEvent SceneTriggered(string scene) =>
            new TutorEvent(TutorEventKind.SceneTriggered, scene: scene);

        public static TutorEvent ExtrasInserted(int count) =>
            new TutorEvent(TutorEventKind.ExtrasInserted, count: count);

        public static TutorEvent ExtrasSkipped() =>
            new TutorEvent(TutorEventKind.ExtrasSkipped);

        public static TutorEvent SessionEnded(int overallPercent) =>
            new TutorEvent(TutorEventKind.SessionEnded, overallPercent: overallPercent);

        /// <summary>
        /// A compact, culture-invariant string identifying this event and its
        /// salient fields. Used to compare two runs for determinism. It does not
        /// include authored text, only structural decisions, so it stays readable.
        /// </summary>
        public string Signature()
        {
            switch (Kind)
            {
                case TutorEventKind.SessionStarted:
                    return "SessionStarted";
                case TutorEventKind.SayPresented:
                    return "Say:" + GuideId;
                case TutorEventKind.QuestionPresented:
                    return "Ask:" + Objective + ":" + GuideId + ":" + Presentation;
                case TutorEventKind.AnswerReceived:
                    return "Answer:" + Objective + ":opt" + OptionIndex.ToString(CultureInfo.InvariantCulture)
                           + ":" + (IsCorrect ? "correct" : "wrong") + ":a" + Attempt.ToString(CultureInfo.InvariantCulture);
                case TutorEventKind.AffirmGiven:
                    return "Affirm:" + Objective + ":" + GuideId + ":a" + Attempt.ToString(CultureInfo.InvariantCulture)
                           + ":+" + Gain.ToString("0.0", CultureInfo.InvariantCulture);
                case TutorEventKind.HintGiven:
                    return "Hint:" + Objective + ":" + GuideId;
                case TutorEventKind.LensSwitched:
                    return "Switch:" + Objective + ":" + GuideId + "->" + ToGuideId;
                case TutorEventKind.WalkthroughGiven:
                    return "Walkthrough:" + Objective + ":" + GuideId;
                case TutorEventKind.SceneTriggered:
                    return "Scene:" + Scene;
                case TutorEventKind.ExtrasInserted:
                    return "ExtrasInserted:" + Count.ToString(CultureInfo.InvariantCulture);
                case TutorEventKind.ExtrasSkipped:
                    return "ExtrasSkipped";
                case TutorEventKind.SessionEnded:
                    return "SessionEnded:" + OverallPercent.ToString(CultureInfo.InvariantCulture);
                default:
                    return Kind.ToString();
            }
        }

        public override string ToString() => Signature();
    }
}
