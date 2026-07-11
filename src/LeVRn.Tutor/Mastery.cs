using System;
using System.Collections.Generic;

namespace LeVRn.Tutor
{
    /// <summary>
    /// Per-objective asked and scored tracking, plus derived mastery percent and
    /// the review flag. Mirrors the reference: percent = round(scored/asked*100),
    /// review = asked &gt; 0 and percent &lt; 50.
    /// </summary>
    public sealed class Mastery
    {
        private readonly IReadOnlyList<Objective> _objectives;
        private readonly Dictionary<string, int> _asked = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly Dictionary<string, double> _scored = new Dictionary<string, double>(StringComparer.Ordinal);

        public Mastery(IReadOnlyList<Objective> objectives)
        {
            _objectives = objectives ?? throw new ArgumentNullException(nameof(objectives));
            foreach (var o in _objectives)
            {
                _asked[o.Id] = 0;
                _scored[o.Id] = 0.0;
            }
        }

        public IReadOnlyList<Objective> Objectives => _objectives;

        public int GetAsked(string objective) => _asked[objective];
        public double GetScored(string objective) => _scored[objective];

        /// <summary>Rounded mastery percent for one objective; 0 when never asked.</summary>
        public int GetPercent(string objective)
        {
            int asked = _asked[objective];
            if (asked == 0) return 0;
            return (int)Math.Round(_scored[objective] / asked * 100.0, MidpointRounding.AwayFromZero);
        }

        /// <summary>Objective needs review when it has been asked and mastery is below 50 percent.</summary>
        public bool IsFlaggedForReview(string objective)
        {
            return _asked[objective] > 0 && GetPercent(objective) < 50;
        }

        /// <summary>Record one asked question and the score gained on it.</summary>
        public void Record(string objective, double gain)
        {
            _asked[objective] += 1;
            _scored[objective] += gain;
        }

        /// <summary>
        /// Overall mastery percent: mean of each objective's ratio, matching the
        /// reference showEnd() computation.
        /// </summary>
        public int OverallPercent()
        {
            double total = 0.0;
            foreach (var o in _objectives)
            {
                int asked = _asked[o.Id];
                total += asked > 0 ? _scored[o.Id] / asked : 0.0;
            }
            return (int)Math.Round(total / _objectives.Count * 100.0, MidpointRounding.AwayFromZero);
        }
    }
}
