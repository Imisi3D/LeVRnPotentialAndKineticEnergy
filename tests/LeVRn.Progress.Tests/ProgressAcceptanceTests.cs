using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using LeVRn.Progress;
using LeVRn.Tutor;
using LeVRn.Physics;

namespace LeVRn.Progress.Tests
{
    /// <summary>
    /// The four acceptance tests from tasks/05-progress-data.md.
    /// </summary>
    [TestFixture]
    public sealed class ProgressAcceptanceTests
    {
        private static readonly DateTime BaseUtc = new DateTime(2026, 7, 11, 9, 0, 0, DateTimeKind.Utc);

        // A profile with a mix: define is shaky (triggers the extra classify items),
        // formula needs a lens switch, the rest are clean.
        private static readonly Dictionary<string, int> Profile = new Dictionary<string, int>
        {
            ["define"] = 2,   // wrong once then right (defineStruggle -> extras inserted)
            ["classify"] = 1, // right first time
            ["formula"] = 3,  // wrong twice then right (one lens switch per formula question)
            ["convert"] = 1,
            ["conserve"] = 1
        };

        private string _dir;

        [SetUp]
        public void SetUp()
        {
            _dir = Path.Combine(Path.GetTempPath(), "levrn_progress_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
        }

        [TearDown]
        public void TearDown()
        {
            try { if (Directory.Exists(_dir)) Directory.Delete(_dir, true); } catch { /* best effort */ }
        }

        private static string ContentFile => Path.Combine(AppContext.BaseDirectory, "content", "pe-ke.json");
        private static TutorScript LoadContent() => TutorScriptLoader.LoadFromFile(ContentFile);
        private static IEnumerable<string> ObjectiveIds(TutorScript c) => c.Objectives.Select(o => o.Id).ToList();

        private sealed class FakeClock
        {
            private DateTime _t;
            public FakeClock(DateTime start) { _t = start; }
            public DateTime Now() { var r = _t; _t = _t.AddSeconds(1); return r; }
        }

        // -------------------------------------------------------------------
        // Acceptance test 1: round trip save mid-session, load, resume; final
        // mastery identical to an uninterrupted run with the same profile.
        // -------------------------------------------------------------------
        [Test]
        public void Round_trip_resume_matches_an_uninterrupted_run()
        {
            var content = LoadContent();
            var ids = ObjectiveIds(content).ToList();
            var storage = new FileStorage(_dir);
            var store = new ProgressStore(storage);

            // Uninterrupted reference run.
            var progressU = LearnerProgress.NewSession("slot-uninterrupted", ids, BaseUtc);
            var adapterU = new TutorProgressAdapter(progressU, store, new FakeClock(BaseUtc).Now);
            var engineU = new TutorEngine(content);
            adapterU.Attach(engineU);
            LearnerDriver.Drive(engineU, Profile);

            int n = progressU.ResolvedQuestions;
            int k = n / 2;
            Assert.That(n, Is.GreaterThan(1), "the profile must resolve several questions");
            Assert.That(progressU.Completed, Is.True);

            // Interrupted run, phase 1: drive until k scored events, then "quit".
            var progress1 = LearnerProgress.NewSession("slot-resume", ids, BaseUtc);
            var adapter1 = new TutorProgressAdapter(progress1, store, new FakeClock(BaseUtc).Now);
            var engine1 = new TutorEngine(content);
            adapter1.Attach(engine1);
            LearnerDriver.Drive(engine1, Profile, adapter1, stopAfterScored: k);
            Assert.That(progress1.Completed, Is.False, "phase 1 stops before completion");

            // Resume: load the saved head from disk, replay the tail on a new engine.
            var progressR = store.LoadOrNew("slot-resume", ids, BaseUtc);
            Assert.That(progressR.ResolvedQuestions, Is.EqualTo(k), "the loaded head has exactly k resolved questions");
            var adapter2 = new TutorProgressAdapter(progressR, store, new FakeClock(BaseUtc).Now, skipScored: k);
            var engine2 = new TutorEngine(content);
            adapter2.Attach(engine2);
            LearnerDriver.Drive(engine2, Profile);

            // Final mastery must be identical to the uninterrupted run.
            Assert.That(progressR.Completed, Is.True, "resumed session completes");
            Assert.That(progressR.ResolvedQuestions, Is.EqualTo(n), "same number of resolved questions");
            Assert.That(progressR.LensSwitches, Is.EqualTo(progressU.LensSwitches), "same lens switches");
            Assert.That(progressR.OverallPercent(), Is.EqualTo(progressU.OverallPercent()), "same overall mastery");
            Assert.That(progressR.OverallPercentAtCompletion, Is.EqualTo(progressU.OverallPercentAtCompletion));

            var byIdU = progressU.Objectives.ToDictionary(o => o.Id);
            foreach (var o in progressR.Objectives)
            {
                var u = byIdU[o.Id];
                Assert.That(o.Asked, Is.EqualTo(u.Asked), "asked for " + o.Id);
                Assert.That(o.Scored, Is.EqualTo(u.Scored).Within(1e-9), "scored for " + o.Id);
                Assert.That(o.Attempts, Is.EqualTo(u.Attempts), "attempts for " + o.Id);
                Assert.That(o.MasteryPercent, Is.EqualTo(u.MasteryPercent), "mastery for " + o.Id);
                Assert.That(o.FlaggedForReview, Is.EqualTo(u.FlaggedForReview), "review flag for " + o.Id);
            }

            TestContext.WriteLine($"n={n} k={k} overall={progressR.OverallPercent()} switches={progressR.LensSwitches}");
        }

        // -------------------------------------------------------------------
        // Acceptance test 2: a corrupt file loads as a fresh session, is logged,
        // and never crashes.
        // -------------------------------------------------------------------
        [Test]
        public void Corrupt_file_loads_as_a_fresh_session_and_is_logged()
        {
            var ids = new[] { "define", "classify" };
            var storage = new FileStorage(_dir);

            foreach (var junk in new[] { "{ this is not valid json ]", "\0\0\x01 binary", "[1,2,3]", "42", "" })
            {
                var logs = new List<string>();
                var store = new ProgressStore(storage, logs.Add);
                storage.Write("slot-corrupt", junk);

                LearnerProgress p = null;
                Assert.DoesNotThrow(() => p = store.LoadOrNew("slot-corrupt", ids, BaseUtc),
                    "load must never throw on corrupt input: <" + junk + ">");

                Assert.That(p, Is.Not.Null);
                Assert.That(p.LearnerSlotId, Is.EqualTo("slot-corrupt"));
                Assert.That(p.ResolvedQuestions, Is.EqualTo(0), "fresh session has no progress");
                Assert.That(p.Completed, Is.False);
                Assert.That(p.StartedUtc, Is.EqualTo(BaseUtc));
                // Whitespace/empty is treated as absent (fresh, no log needed); real
                // corruption must be logged.
                if (!string.IsNullOrWhiteSpace(junk))
                    Assert.That(logs, Is.Not.Empty, "corruption must be logged for <" + junk + ">");
            }
        }

        // -------------------------------------------------------------------
        // Acceptance test 3: serialized JSON contains no personal data fields.
        // -------------------------------------------------------------------
        [Test]
        public void Serialized_json_contains_no_personal_data()
        {
            var progress = BuildPopulatedProgress(out _);
            string json = progress.ToJson();

            foreach (var banned in new[] { "\"name\"", "\"firstname\"", "\"lastname\"", "\"fullname\"",
                                           "\"age\"", "\"dob\"", "\"dateofbirth\"", "\"birth\"",
                                           "\"email\"", "\"phone\"", "\"gender\"", "\"address\"" })
                Assert.That(json.ToLowerInvariant(), Does.Not.Contain(banned), "must not serialize field " + banned);

            // Sanity: the only identifier present is the device-local slot id.
            Assert.That(json, Does.Contain("\"learnerSlotId\""));
            Assert.That(json.ToLowerInvariant(), Does.Not.Contain("learnername"));
        }

        // -------------------------------------------------------------------
        // Acceptance test 4: the export bundle validates against the Task 06 schema.
        // -------------------------------------------------------------------
        [Test]
        public void Export_bundle_validates_against_the_task06_schema()
        {
            var progress = BuildPopulatedProgress(out var generatedUtc);
            string bundle = Export.BuildJson(progress, generatedUtc);

            var result = Export.Validate(bundle);
            Assert.That(result.Ok, Is.True, "bundle should validate, but: " + result);

            // The bundle actually carries the station data and objectives.
            Assert.That(bundle, Does.Contain("\"throws\""));
            Assert.That(bundle, Does.Contain("\"hillRuns\""));
            Assert.That(bundle, Does.Contain("\"pendulumShortfalls\""));
            Assert.That(bundle, Does.Contain("\"masteryPercent\""));

            // Negative control: a bundle carrying a personal field must be rejected.
            string tampered = bundle.Replace("\"lensSwitches\"", "\"name\": \"Ada\", \"lensSwitches\"");
            var bad = Export.Validate(tampered);
            Assert.That(bad.Ok, Is.False, "a bundle with a personal field must fail validation");
            Assert.That(string.Join(";", bad.Errors).ToLowerInvariant(), Does.Contain("name"));
        }

        // ---------------- helpers ----------------

        /// <summary>Run a full session, then append one real station event of each
        /// kind computed from the Task 02 physics models.</summary>
        private LearnerProgress BuildPopulatedProgress(out DateTime generatedUtc)
        {
            var content = LoadContent();
            var ids = ObjectiveIds(content).ToList();
            var storage = new FileStorage(_dir);
            var store = new ProgressStore(storage);

            var progress = LearnerProgress.NewSession("slot-07", ids, BaseUtc);
            var clock = new FakeClock(BaseUtc);
            var adapter = new TutorProgressAdapter(progress, store, clock.Now);
            var engine = new TutorEngine(content);
            adapter.Attach(engine);
            LearnerDriver.Drive(engine, Profile);

            // Throw (Handover): real projectile launch, ledger read at launch.
            var ball = new ProjectileModel(); // 0.5 kg
            ball.Launch(0, 1.5, 6.0, 4.0);
            var led = ball.Ledger();
            progress.AddStationEvent(StationEvent.MakeThrow(ball.Speed, led.Kinetic, clock.Now()), clock.Now());

            // Hill run (Just Enough Hill): a real release outcome.
            var hill = new HillModel();
            var run = hill.Run(1.2, 0.008);
            progress.AddStationEvent(StationEvent.MakeHillRun(run.ReleaseHeight, run.Outcome.ToString(), clock.Now()), clock.Now());

            // Pendulum shortfall (Nose Test): how far short of the nose it returned.
            var swing = new PendulumModel(damping: PendulumModel.DefaultDamping);
            swing.Release(PendulumModel.DefaultReleaseAngle);
            double apex = swing.RunToReturnApex(1.0 / 240.0);
            double shortfall = apex - PendulumModel.DefaultReleaseAngle; // positive: stopped short
            progress.AddStationEvent(StationEvent.MakePendulumShortfall(shortfall, clock.Now()), clock.Now());

            store.Save(progress);
            generatedUtc = BaseUtc.AddMinutes(5);
            return progress;
        }
    }
}
