using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using LeVRn.Tutor;

namespace LeVRn.Tutor.Tests
{
    /// <summary>
    /// The five acceptance tests from tasks/01-tutor-engine.md. Expected values
    /// are pre-verified against reference/tutor_prototype.html.
    /// </summary>
    [TestFixture]
    public sealed class TutorEngineAcceptanceTests
    {
        private static readonly string[] ObjectiveOrder = { "define", "classify", "formula", "convert", "conserve" };

        private static string ContentFile =>
            Path.Combine(AppContext.BaseDirectory, "content", "pe-ke.json");

        private static TutorScript LoadContent() => TutorScriptLoader.LoadFromFile(ContentFile);

        private static int[] AskedPerObjective(TutorEngine engine) =>
            ObjectiveOrder.Select(o => engine.Mastery.GetAsked(o)).ToArray();

        private static int[] MasteryPerObjective(TutorEngine engine) =>
            ObjectiveOrder.Select(o => engine.Mastery.GetPercent(o)).ToArray();

        private static List<string> Signatures(TutorEngine engine) =>
            engine.Events.Select(e => e.Signature()).ToList();

        // -------------------------------------------------------------------
        // Acceptance test 1: STRONG learner, every answer correct first try.
        // -------------------------------------------------------------------
        [Test]
        public void Strong_learner_masters_everything_with_no_adaptation()
        {
            var engine = new TutorEngine(LoadContent());
            var profile = new Dictionary<string, int>
            {
                ["define"] = 1, ["classify"] = 1, ["formula"] = 1, ["convert"] = 1, ["conserve"] = 1
            };
            new LearnerSimulator(profile).Run(engine);

            Assert.That(AskedPerObjective(engine), Is.EqualTo(new[] { 2, 2, 2, 3, 1 }),
                "questions asked per objective");
            Assert.That(MasteryPerObjective(engine), Is.EqualTo(new[] { 100, 100, 100, 100, 100 }),
                "mastery per objective");
            Assert.That(engine.LensSwitches, Is.EqualTo(0), "lens switches");
            Assert.That(engine.ExtrasInserted, Is.False, "extra items must not be inserted");
            Assert.That(engine.Events.Count(e => e.Kind == TutorEventKind.ExtrasInserted), Is.EqualTo(0));
        }

        // -------------------------------------------------------------------
        // Acceptance test 2: WEAK learner.
        // define needs 3 attempts, classify 2, formula never, convert 2, conserve 1.
        // -------------------------------------------------------------------
        [Test]
        public void Weak_learner_triggers_adaptation_switches_and_review_flag()
        {
            var engine = new TutorEngine(LoadContent());
            var profile = new Dictionary<string, int>
            {
                ["define"] = 3,
                ["classify"] = 2,
                ["formula"] = LearnerSimulator.Never,
                ["convert"] = 2,
                ["conserve"] = 1
            };
            new LearnerSimulator(profile).Run(engine);

            // Extra classification items ARE inserted -> classify asked == 4.
            Assert.That(engine.ExtrasInserted, Is.True, "extras should be inserted");
            Assert.That(engine.Mastery.GetAsked("classify"), Is.EqualTo(4), "classify asked");

            // Lens switches >= 4.
            Assert.That(engine.LensSwitches, Is.GreaterThanOrEqualTo(4), "lens switches");

            // Formula flagged for review: score 0 but asked > 0.
            Assert.That(engine.Mastery.GetAsked("formula"), Is.GreaterThan(0), "formula asked");
            Assert.That(engine.Mastery.GetScored("formula"), Is.EqualTo(0.0), "formula scored");
            Assert.That(engine.Mastery.IsFlaggedForReview("formula"), Is.True, "formula flagged for review");

            // Mastery pattern within rounding.
            Assert.That(MasteryPerObjective(engine), Is.EqualTo(new[] { 30, 60, 0, 60, 100 }),
                "mastery pattern");
        }

        // -------------------------------------------------------------------
        // Acceptance test 3: adaptation idempotence.
        // -------------------------------------------------------------------
        [Test]
        public void Extras_are_inserted_at_most_once()
        {
            var engine = new TutorEngine(LoadContent());
            var profile = new Dictionary<string, int>
            {
                ["define"] = 3,
                ["classify"] = 1,
                ["formula"] = 1,
                ["convert"] = 1,
                ["conserve"] = 1
            };
            new LearnerSimulator(profile).Run(engine);

            Assert.That(engine.Events.Count(e => e.Kind == TutorEventKind.ExtrasInserted), Is.EqualTo(1),
                "exactly one insertion event");
            // 2 original classify + 2 extra, never 6.
            Assert.That(engine.Mastery.GetAsked("classify"), Is.EqualTo(4),
                "classify asked reflects a single insertion of two items");
        }

        // -------------------------------------------------------------------
        // Acceptance test 4: content integrity.
        // -------------------------------------------------------------------
        [Test]
        public void Content_yields_the_same_number_of_authored_responses_as_the_reference()
        {
            var content = LoadContent();

            // Reference countBank: Say +1, Ask +4, over script and extras.
            // 6 Say nodes, 12 Ask nodes (10 in script + 2 extra) => 6 + 48 = 54.
            Assert.That(content.CountAuthoredResponses(), Is.EqualTo(54));
        }

        // -------------------------------------------------------------------
        // Acceptance test 5: determinism.
        // -------------------------------------------------------------------
        [Test]
        public void Same_profile_produces_identical_event_sequences()
        {
            var profile = new Dictionary<string, int>
            {
                ["define"] = 2,
                ["classify"] = 3,
                ["formula"] = LearnerSimulator.Never,
                ["convert"] = 1,
                ["conserve"] = 2
            };

            var a = new TutorEngine(LoadContent());
            new LearnerSimulator(profile).Run(a);

            var b = new TutorEngine(LoadContent());
            new LearnerSimulator(profile).Run(b);

            Assert.That(Signatures(b), Is.EqualTo(Signatures(a)));
            Assert.That(a.Events.Count, Is.GreaterThan(0));
        }
    }
}
