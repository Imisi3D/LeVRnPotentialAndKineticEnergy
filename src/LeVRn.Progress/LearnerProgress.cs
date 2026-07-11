using System;
using System.Collections.Generic;
using System.Globalization;

namespace LeVRn.Progress
{
    /// <summary>Which embodied station a logged event came from.</summary>
    public enum StationKind
    {
        Throw,             // Handover: a thrown ball, with release speed and KE
        HillRun,           // Just Enough Hill: a release, with h0 and outcome
        PendulumShortfall  // Nose Test: how far short of the nose the tire returned
    }

    /// <summary>
    /// One entry in the station log. A single chronological list holds all three
    /// kinds; only the fields relevant to <see cref="Kind"/> are populated. Values
    /// come from the LeVRn.Physics models at run time.
    /// </summary>
    public sealed class StationEvent
    {
        public StationKind Kind { get; set; }
        public DateTime AtUtc { get; set; }

        // Throw
        public double? VelocityMetresPerSecond { get; set; }
        public double? KineticEnergyJoules { get; set; }

        // HillRun
        public double? ReleaseHeightMetres { get; set; }
        public string HillOutcome { get; set; }

        // PendulumShortfall
        public double? ShortfallRadians { get; set; }

        public static StationEvent MakeThrow(double v, double ke, DateTime atUtc) =>
            new StationEvent { Kind = StationKind.Throw, AtUtc = atUtc, VelocityMetresPerSecond = v, KineticEnergyJoules = ke };

        public static StationEvent MakeHillRun(double h0, string outcome, DateTime atUtc) =>
            new StationEvent { Kind = StationKind.HillRun, AtUtc = atUtc, ReleaseHeightMetres = h0, HillOutcome = outcome };

        public static StationEvent MakePendulumShortfall(double shortfallRadians, DateTime atUtc) =>
            new StationEvent { Kind = StationKind.PendulumShortfall, AtUtc = atUtc, ShortfallRadians = shortfallRadians };
    }

    /// <summary>Per-objective asked/scored tracking, mirroring LeVRn.Tutor.Mastery.</summary>
    public sealed class ObjectiveProgress
    {
        public string Id { get; set; }
        public int Asked { get; set; }
        public double Scored { get; set; }
        public int Attempts { get; set; } // total answers (right or wrong) on this objective

        /// <summary>Rounded mastery percent; 0 when never asked.</summary>
        public int MasteryPercent =>
            Asked == 0 ? 0 : (int)Math.Round(Scored / Asked * 100.0, MidpointRounding.AwayFromZero);

        /// <summary>Needs review when asked and below 50 percent, as in the engine.</summary>
        public bool FlaggedForReview => Asked > 0 && MasteryPercent < 50;
    }

    /// <summary>
    /// The on-device progress record for one learner session. It holds NO personal
    /// data of any kind: only a device-local anonymous slot id, timestamps, and
    /// learning telemetry. The schema is versioned; unknown fields from a future
    /// version are ignored on load (see <see cref="FromJson"/>).
    /// </summary>
    public sealed class LearnerProgress
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion { get; set; } = CurrentSchemaVersion;

        /// <summary>
        /// Device-local anonymous slot id (for example "slot-03"). It identifies a
        /// headset seat, not a person. There is deliberately no name, age, class,
        /// or any other personal field anywhere in this record.
        /// </summary>
        public string LearnerSlotId { get; set; }

        public DateTime StartedUtc { get; set; }
        public DateTime LastUpdatedUtc { get; set; }

        public bool Completed { get; set; }
        /// <summary>Overall percent recorded at completion, or null while in progress.</summary>
        public int? OverallPercentAtCompletion { get; set; }

        public int LensSwitches { get; set; }

        public List<ObjectiveProgress> Objectives { get; } = new List<ObjectiveProgress>();
        public List<StationEvent> StationEvents { get; } = new List<StationEvent>();

        /// <summary>Number of questions resolved so far (sum of per-objective Asked),
        /// which doubles as the resume cursor: a replay skips this many resolved
        /// questions before continuing to record.</summary>
        public int ResolvedQuestions
        {
            get
            {
                int total = 0;
                foreach (var o in Objectives) total += o.Asked;
                return total;
            }
        }

        /// <summary>Create a session and seed the full objective set so overall
        /// mastery divides by the same denominator the engine uses.</summary>
        public static LearnerProgress NewSession(string slotId, IEnumerable<string> objectiveIds, DateTime nowUtc)
        {
            var p = new LearnerProgress
            {
                LearnerSlotId = slotId,
                StartedUtc = nowUtc,
                LastUpdatedUtc = nowUtc
            };
            if (objectiveIds != null)
                foreach (var id in objectiveIds)
                    p.Objectives.Add(new ObjectiveProgress { Id = id });
            return p;
        }

        public ObjectiveProgress GetOrAddObjective(string id)
        {
            foreach (var o in Objectives)
                if (o.Id == id) return o;
            var added = new ObjectiveProgress { Id = id };
            Objectives.Add(added);
            return added;
        }

        /// <summary>Record one answer (right or wrong) as an attempt on an objective.</summary>
        public void RecordAttempt(string objectiveId, DateTime nowUtc)
        {
            GetOrAddObjective(objectiveId).Attempts += 1;
            LastUpdatedUtc = nowUtc;
        }

        /// <summary>Record a resolved question: asked + 1, scored + gain. This is the
        /// scored event after which the store should save.</summary>
        public void RecordResolved(string objectiveId, double gain, DateTime nowUtc)
        {
            var o = GetOrAddObjective(objectiveId);
            o.Asked += 1;
            o.Scored += gain;
            LastUpdatedUtc = nowUtc;
        }

        public void RecordLensSwitch(DateTime nowUtc)
        {
            LensSwitches += 1;
            LastUpdatedUtc = nowUtc;
        }

        public void AddStationEvent(StationEvent e, DateTime nowUtc)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            StationEvents.Add(e);
            LastUpdatedUtc = nowUtc;
        }

        public void Complete(int overallPercent, DateTime nowUtc)
        {
            Completed = true;
            OverallPercentAtCompletion = overallPercent;
            LastUpdatedUtc = nowUtc;
        }

        /// <summary>Overall mastery: mean of each objective's ratio across the full
        /// objective set, matching LeVRn.Tutor.Mastery.OverallPercent.</summary>
        public int OverallPercent()
        {
            if (Objectives.Count == 0) return 0;
            double total = 0.0;
            foreach (var o in Objectives)
                total += o.Asked > 0 ? o.Scored / o.Asked : 0.0;
            return (int)Math.Round(total / Objectives.Count * 100.0, MidpointRounding.AwayFromZero);
        }

        // ---------------- JSON mapping ----------------

        public string ToJson(bool indent = true) => Json.Serialize(ToJsonObject(), indent);

        public Dictionary<string, object> ToJsonObject()
        {
            var objectives = new List<object>();
            foreach (var o in Objectives)
            {
                objectives.Add(new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["id"] = o.Id,
                    ["asked"] = o.Asked,
                    ["scored"] = o.Scored,
                    ["attempts"] = o.Attempts,
                    ["masteryPercent"] = o.MasteryPercent,
                    ["flaggedForReview"] = o.FlaggedForReview
                });
            }

            var stations = new List<object>();
            foreach (var e in StationEvents)
            {
                var m = new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["kind"] = e.Kind.ToString(),
                    ["atUtc"] = ToIso(e.AtUtc)
                };
                if (e.VelocityMetresPerSecond.HasValue) m["velocityMetresPerSecond"] = e.VelocityMetresPerSecond.Value;
                if (e.KineticEnergyJoules.HasValue) m["kineticEnergyJoules"] = e.KineticEnergyJoules.Value;
                if (e.ReleaseHeightMetres.HasValue) m["releaseHeightMetres"] = e.ReleaseHeightMetres.Value;
                if (e.HillOutcome != null) m["hillOutcome"] = e.HillOutcome;
                if (e.ShortfallRadians.HasValue) m["shortfallRadians"] = e.ShortfallRadians.Value;
                stations.Add(m);
            }

            var root = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["schemaVersion"] = SchemaVersion,
                ["learnerSlotId"] = LearnerSlotId,
                ["startedUtc"] = ToIso(StartedUtc),
                ["lastUpdatedUtc"] = ToIso(LastUpdatedUtc),
                ["completed"] = Completed,
                ["lensSwitches"] = LensSwitches,
                ["objectives"] = objectives,
                ["stationEvents"] = stations
            };
            if (OverallPercentAtCompletion.HasValue) root["overallPercentAtCompletion"] = OverallPercentAtCompletion.Value;
            return root;
        }

        /// <summary>Rebuild a record from parsed JSON. Unknown keys are ignored,
        /// so a file written by a newer schema still loads.</summary>
        public static LearnerProgress FromJson(string json) => FromJsonObject(Json.Parse(json) as IDictionary<string, object>);

        public static LearnerProgress FromJsonObject(IDictionary<string, object> root)
        {
            if (root == null) throw new Json.ParseException("Root is not a JSON object.");
            var p = new LearnerProgress
            {
                SchemaVersion = GetInt(root, "schemaVersion", CurrentSchemaVersion),
                LearnerSlotId = GetString(root, "learnerSlotId", null),
                StartedUtc = ParseIso(GetString(root, "startedUtc", null)),
                LastUpdatedUtc = ParseIso(GetString(root, "lastUpdatedUtc", null)),
                Completed = GetBool(root, "completed", false),
                LensSwitches = GetInt(root, "lensSwitches", 0)
            };
            if (root.TryGetValue("overallPercentAtCompletion", out var ov) && ov != null)
                p.OverallPercentAtCompletion = Convert.ToInt32(ov, CultureInfo.InvariantCulture);

            if (root.TryGetValue("objectives", out var objsRaw) && objsRaw is IEnumerable<object> objs)
            {
                foreach (var item in objs)
                {
                    if (item is IDictionary<string, object> od)
                    {
                        p.Objectives.Add(new ObjectiveProgress
                        {
                            Id = GetString(od, "id", null),
                            Asked = GetInt(od, "asked", 0),
                            Scored = GetDouble(od, "scored", 0.0),
                            Attempts = GetInt(od, "attempts", 0)
                            // masteryPercent / flaggedForReview are derived, not read back
                        });
                    }
                }
            }

            if (root.TryGetValue("stationEvents", out var stRaw) && stRaw is IEnumerable<object> sts)
            {
                foreach (var item in sts)
                {
                    if (item is IDictionary<string, object> sd)
                    {
                        var e = new StationEvent
                        {
                            Kind = ParseKind(GetString(sd, "kind", "Throw")),
                            AtUtc = ParseIso(GetString(sd, "atUtc", null)),
                            HillOutcome = GetString(sd, "hillOutcome", null)
                        };
                        if (sd.TryGetValue("velocityMetresPerSecond", out var v) && v != null) e.VelocityMetresPerSecond = Convert.ToDouble(v, CultureInfo.InvariantCulture);
                        if (sd.TryGetValue("kineticEnergyJoules", out var ke) && ke != null) e.KineticEnergyJoules = Convert.ToDouble(ke, CultureInfo.InvariantCulture);
                        if (sd.TryGetValue("releaseHeightMetres", out var h) && h != null) e.ReleaseHeightMetres = Convert.ToDouble(h, CultureInfo.InvariantCulture);
                        if (sd.TryGetValue("shortfallRadians", out var s) && s != null) e.ShortfallRadians = Convert.ToDouble(s, CultureInfo.InvariantCulture);
                        p.StationEvents.Add(e);
                    }
                }
            }
            return p;
        }

        private static StationKind ParseKind(string s) =>
            Enum.TryParse(s, out StationKind k) ? k : StationKind.Throw;

        private static string ToIso(DateTime dt) => dt.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);

        private static DateTime ParseIso(string s)
        {
            if (string.IsNullOrEmpty(s)) return default;
            return DateTime.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }

        private static string GetString(IDictionary<string, object> m, string key, string fallback) =>
            m.TryGetValue(key, out var v) && v is string s ? s : fallback;

        private static int GetInt(IDictionary<string, object> m, string key, int fallback) =>
            m.TryGetValue(key, out var v) && v != null ? Convert.ToInt32(v, CultureInfo.InvariantCulture) : fallback;

        private static double GetDouble(IDictionary<string, object> m, string key, double fallback) =>
            m.TryGetValue(key, out var v) && v != null ? Convert.ToDouble(v, CultureInfo.InvariantCulture) : fallback;

        private static bool GetBool(IDictionary<string, object> m, string key, bool fallback) =>
            m.TryGetValue(key, out var v) && v is bool b ? b : fallback;
    }
}
