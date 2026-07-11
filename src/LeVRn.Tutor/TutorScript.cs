using System;
using System.Collections.Generic;

namespace LeVRn.Tutor
{
    public enum NodeType { Say, Ask, Scene, End }

    /// <summary>Base class for all authored script nodes.</summary>
    public abstract class TutorNode
    {
        public abstract NodeType Type { get; }
    }

    /// <summary>A guide speaks; the learner advances with Continue.</summary>
    public sealed class SayNode : TutorNode
    {
        public override NodeType Type => NodeType.Say;
        public string GuideId { get; }
        public string Text { get; }

        public SayNode(string guideId, string text)
        {
            GuideId = guideId ?? throw new ArgumentNullException(nameof(guideId));
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }
    }

    /// <summary>A multiple-choice question tied to a learning objective.</summary>
    public sealed class AskNode : TutorNode
    {
        public override NodeType Type => NodeType.Ask;
        public string GuideId { get; }
        public string Objective { get; }
        public string Text { get; }
        public IReadOnlyList<string> Options { get; }
        public int Correct { get; }
        public ResponseBank Bank { get; }

        public AskNode(string guideId, string objective, string text,
                       IReadOnlyList<string> options, int correct, ResponseBank bank)
        {
            GuideId = guideId ?? throw new ArgumentNullException(nameof(guideId));
            Objective = objective ?? throw new ArgumentNullException(nameof(objective));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Options = options ?? throw new ArgumentNullException(nameof(options));
            if (correct < 0 || correct >= options.Count)
                throw new ArgumentOutOfRangeException(nameof(correct));
            Correct = correct;
            Bank = bank ?? throw new ArgumentNullException(nameof(bank));
        }
    }

    /// <summary>A scene side-effect (rain, flood, mango). Auto-advances.</summary>
    public sealed class SceneNode : TutorNode
    {
        public override NodeType Type => NodeType.Scene;
        public string Fn { get; }

        public SceneNode(string fn)
        {
            Fn = fn ?? throw new ArgumentNullException(nameof(fn));
        }
    }

    /// <summary>Marks the end of the lesson.</summary>
    public sealed class EndNode : TutorNode
    {
        public override NodeType Type => NodeType.End;
    }

    /// <summary>A learning objective (id and learner-facing label).</summary>
    public sealed class Objective
    {
        public string Id { get; }
        public string Label { get; }

        public Objective(string id, string label)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Label = label ?? throw new ArgumentNullException(nameof(label));
        }
    }

    /// <summary>
    /// The full authored lesson: guides, objectives, the ordered node list, and
    /// the extra classification items that adaptation may splice in. This is a
    /// plain data container with no engine behaviour and no external
    /// dependencies, so any loader (JSON, Unity asset, hand-built) can produce it.
    /// </summary>
    public sealed class TutorScript
    {
        public string Id { get; }
        public string Title { get; }
        public string Grounding { get; }
        public GuideSet Guides { get; }
        public IReadOnlyList<Objective> Objectives { get; }
        public IReadOnlyList<TutorNode> Nodes { get; }
        public IReadOnlyList<AskNode> ExtraClassify { get; }

        public TutorScript(string id, string title, string grounding, GuideSet guides,
                           IReadOnlyList<Objective> objectives, IReadOnlyList<TutorNode> nodes,
                           IReadOnlyList<AskNode> extraClassify)
        {
            Id = id ?? string.Empty;
            Title = title ?? string.Empty;
            Grounding = grounding ?? string.Empty;
            Guides = guides ?? throw new ArgumentNullException(nameof(guides));
            Objectives = objectives ?? throw new ArgumentNullException(nameof(objectives));
            Nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
            ExtraClassify = extraClassify ?? new List<AskNode>();
        }

        /// <summary>
        /// Number of authored responses, matching the reference countBank():
        /// each Say node counts once; each Ask node counts four
        /// (affirm, hint, switchText, walkthrough). Counts the main script and
        /// the extra classification items, as the reference does.
        /// </summary>
        public int CountAuthoredResponses()
        {
            int n = 0;
            foreach (var node in Nodes)
            {
                if (node.Type == NodeType.Say) n += 1;
                else if (node.Type == NodeType.Ask) n += 4;
            }
            foreach (var _ in ExtraClassify) n += 4;
            return n;
        }
    }
}
