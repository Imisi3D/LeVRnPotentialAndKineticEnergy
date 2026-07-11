using System;
using System.Collections.Generic;
using System.Globalization;

namespace LeVRn.Progress
{
    /// <summary>Outcome of validating an export bundle against the Task 06 schema.</summary>
    public sealed class ValidationResult
    {
        public bool Ok => Errors.Count == 0;
        public List<string> Errors { get; } = new List<string>();
        public override string ToString() => Ok ? "valid" : string.Join("; ", Errors);
    }

    /// <summary>
    /// Builds the teacher-view export bundle from a <see cref="LearnerProgress"/>
    /// and validates bundles against the schema the offline teacher page (Task 06)
    /// consumes. The bundle is a read-only, aggregated view: per-objective mastery
    /// plus the three station logs grouped by kind. It contains no personal data,
    /// for the same reason the progress record does not.
    ///
    /// This is the single source of truth for that schema; Task 06 must read
    /// exactly these fields.
    /// </summary>
    public static class Export
    {
        public const int ExportSchemaVersion = 1;

        public static string BuildJson(LearnerProgress progress, DateTime generatedUtc, bool indent = true) =>
            Json.Serialize(BuildObject(progress, generatedUtc), indent);

        public static Dictionary<string, object> BuildObject(LearnerProgress progress, DateTime generatedUtc)
        {
            if (progress == null) throw new ArgumentNullException(nameof(progress));

            var objectives = new List<object>();
            foreach (var o in progress.Objectives)
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

            var throws = new List<object>();
            var hillRuns = new List<object>();
            var pendulums = new List<object>();
            foreach (var e in progress.StationEvents)
            {
                switch (e.Kind)
                {
                    case StationKind.Throw:
                        throws.Add(new Dictionary<string, object>(StringComparer.Ordinal)
                        {
                            ["velocityMetresPerSecond"] = e.VelocityMetresPerSecond ?? 0.0,
                            ["kineticEnergyJoules"] = e.KineticEnergyJoules ?? 0.0,
                            ["atUtc"] = Iso(e.AtUtc)
                        });
                        break;
                    case StationKind.HillRun:
                        hillRuns.Add(new Dictionary<string, object>(StringComparer.Ordinal)
                        {
                            ["releaseHeightMetres"] = e.ReleaseHeightMetres ?? 0.0,
                            ["outcome"] = e.HillOutcome ?? "",
                            ["atUtc"] = Iso(e.AtUtc)
                        });
                        break;
                    case StationKind.PendulumShortfall:
                        pendulums.Add(new Dictionary<string, object>(StringComparer.Ordinal)
                        {
                            ["shortfallRadians"] = e.ShortfallRadians ?? 0.0,
                            ["atUtc"] = Iso(e.AtUtc)
                        });
                        break;
                }
            }

            var stations = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["throws"] = throws,
                ["hillRuns"] = hillRuns,
                ["pendulumShortfalls"] = pendulums
            };

            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["exportSchemaVersion"] = ExportSchemaVersion,
                ["generatedUtc"] = Iso(generatedUtc),
                ["learnerSlotId"] = progress.LearnerSlotId,
                ["startedUtc"] = Iso(progress.StartedUtc),
                ["lastUpdatedUtc"] = Iso(progress.LastUpdatedUtc),
                ["completed"] = progress.Completed,
                ["overallPercent"] = progress.OverallPercent(),
                ["lensSwitches"] = progress.LensSwitches,
                ["objectives"] = objectives,
                ["stations"] = stations
            };
        }

        /// <summary>Validate a bundle JSON string against the Task 06 schema.</summary>
        public static ValidationResult Validate(string json)
        {
            var result = new ValidationResult();
            object parsed;
            try { parsed = Json.Parse(json); }
            catch (Exception ex) { result.Errors.Add("not parseable JSON: " + ex.Message); return result; }
            if (!(parsed is IDictionary<string, object> root)) { result.Errors.Add("root is not an object"); return result; }

            RequireInt(root, "exportSchemaVersion", result);
            RequireString(root, "generatedUtc", result);
            RequireString(root, "learnerSlotId", result);
            RequireString(root, "startedUtc", result);
            RequireString(root, "lastUpdatedUtc", result);
            RequireBool(root, "completed", result);
            RequireNumber(root, "overallPercent", result);
            RequireInt(root, "lensSwitches", result);

            // Explicit privacy guard: these keys must never appear in a bundle.
            foreach (var banned in new[] { "name", "firstName", "lastName", "fullName", "age", "dob", "dateOfBirth", "email", "phone", "gender", "address" })
                if (root.ContainsKey(banned)) result.Errors.Add("bundle contains forbidden personal field '" + banned + "'");

            if (!(GetList(root, "objectives") is IEnumerable<object> objectives))
                result.Errors.Add("missing 'objectives' array");
            else
            {
                int idx = 0;
                foreach (var item in objectives)
                {
                    if (!(item is IDictionary<string, object> o)) { result.Errors.Add("objectives[" + idx + "] is not an object"); idx++; continue; }
                    RequireString(o, "id", result, "objectives[" + idx + "].");
                    RequireInt(o, "asked", result, "objectives[" + idx + "].");
                    RequireNumber(o, "scored", result, "objectives[" + idx + "].");
                    RequireInt(o, "attempts", result, "objectives[" + idx + "].");
                    RequireInt(o, "masteryPercent", result, "objectives[" + idx + "].");
                    RequireBool(o, "flaggedForReview", result, "objectives[" + idx + "].");
                    idx++;
                }
            }

            if (!(GetDict(root, "stations") is IDictionary<string, object> stations))
                result.Errors.Add("missing 'stations' object");
            else
            {
                ValidateThrows(stations, result);
                ValidateHillRuns(stations, result);
                ValidatePendulums(stations, result);
            }

            return result;
        }

        private static void ValidateThrows(IDictionary<string, object> stations, ValidationResult result)
        {
            if (!(GetList(stations, "throws") is IEnumerable<object> arr)) { result.Errors.Add("stations.throws missing"); return; }
            int i = 0;
            foreach (var item in arr)
            {
                if (!(item is IDictionary<string, object> t)) { result.Errors.Add("stations.throws[" + i + "] not an object"); i++; continue; }
                RequireNumber(t, "velocityMetresPerSecond", result, "stations.throws[" + i + "].");
                RequireNumber(t, "kineticEnergyJoules", result, "stations.throws[" + i + "].");
                RequireString(t, "atUtc", result, "stations.throws[" + i + "].");
                i++;
            }
        }

        private static void ValidateHillRuns(IDictionary<string, object> stations, ValidationResult result)
        {
            if (!(GetList(stations, "hillRuns") is IEnumerable<object> arr)) { result.Errors.Add("stations.hillRuns missing"); return; }
            int i = 0;
            foreach (var item in arr)
            {
                if (!(item is IDictionary<string, object> h)) { result.Errors.Add("stations.hillRuns[" + i + "] not an object"); i++; continue; }
                RequireNumber(h, "releaseHeightMetres", result, "stations.hillRuns[" + i + "].");
                RequireString(h, "outcome", result, "stations.hillRuns[" + i + "].");
                RequireString(h, "atUtc", result, "stations.hillRuns[" + i + "].");
                i++;
            }
        }

        private static void ValidatePendulums(IDictionary<string, object> stations, ValidationResult result)
        {
            if (!(GetList(stations, "pendulumShortfalls") is IEnumerable<object> arr)) { result.Errors.Add("stations.pendulumShortfalls missing"); return; }
            int i = 0;
            foreach (var item in arr)
            {
                if (!(item is IDictionary<string, object> p)) { result.Errors.Add("stations.pendulumShortfalls[" + i + "] not an object"); i++; continue; }
                RequireNumber(p, "shortfallRadians", result, "stations.pendulumShortfalls[" + i + "].");
                RequireString(p, "atUtc", result, "stations.pendulumShortfalls[" + i + "].");
                i++;
            }
        }

        private static object GetList(IDictionary<string, object> m, string k) => m.TryGetValue(k, out var v) ? v : null;
        private static object GetDict(IDictionary<string, object> m, string k) => m.TryGetValue(k, out var v) ? v : null;

        private static void RequireString(IDictionary<string, object> m, string k, ValidationResult r, string prefix = "")
        {
            if (!m.TryGetValue(k, out var v) || !(v is string)) r.Errors.Add("missing/invalid string '" + prefix + k + "'");
        }
        private static void RequireBool(IDictionary<string, object> m, string k, ValidationResult r, string prefix = "")
        {
            if (!m.TryGetValue(k, out var v) || !(v is bool)) r.Errors.Add("missing/invalid bool '" + prefix + k + "'");
        }
        private static void RequireNumber(IDictionary<string, object> m, string k, ValidationResult r, string prefix = "")
        {
            if (!m.TryGetValue(k, out var v) || !(v is double)) r.Errors.Add("missing/invalid number '" + prefix + k + "'");
        }
        private static void RequireInt(IDictionary<string, object> m, string k, ValidationResult r, string prefix = "")
        {
            // Numbers parse to double; an integer is a double with no fractional part.
            if (!m.TryGetValue(k, out var v) || !(v is double d) || d != Math.Floor(d))
                r.Errors.Add("missing/invalid integer '" + prefix + k + "'");
        }

        private static string Iso(DateTime dt) => dt.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
    }
}
