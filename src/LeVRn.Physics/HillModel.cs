using System;
using System.Collections.Generic;

namespace LeVRn.Physics
{
    /// <summary>Outcome of one Just Enough Hill release.</summary>
    public enum HillOutcome
    {
        /// <summary>The cart ran out of energy and settled without cresting (retry).</summary>
        RolledBack,
        /// <summary>The cart crossed the crest slowly: the target result.</summary>
        JustEnough,
        /// <summary>The cart flew over the crest with energy to spare.</summary>
        Overshoot
    }

    /// <summary>The result of a single release: outcome plus the numbers behind it.</summary>
    public readonly struct HillResult
    {
        public readonly HillOutcome Outcome;
        /// <summary>Release height h0 in metres (mgh the learner banked).</summary>
        public readonly double ReleaseHeight;
        /// <summary>Speed at the crest, m/s (0 if the crest was never reached).</summary>
        public readonly double CrestSpeed;
        /// <summary>Number of stalls (direction flips from running out of energy).</summary>
        public readonly int Stalls;
        /// <summary>Number of integration steps the run took.</summary>
        public readonly int Steps;

        public HillResult(HillOutcome outcome, double releaseHeight, double crestSpeed, int stalls, int steps)
        {
            Outcome = outcome;
            ReleaseHeight = releaseHeight;
            CrestSpeed = crestSpeed;
            Stalls = stalls;
            Steps = steps;
        }
    }

    /// <summary>
    /// The Just Enough Hill capstone: a rolling cart on an authored track, solved
    /// by the energy method exactly as the reference Station 3 update(). The cart's
    /// kinetic energy at any point is the banked mgh at release, minus the friction
    /// paid over distance travelled, minus the mgh of the current height. dt is
    /// injectable for deterministic tests.
    /// </summary>
    public sealed class HillModel
    {
        /// <summary>Cart mass in kg. 1 kg keeps the ledger arithmetic clean (PE = 10*h).</summary>
        public const double DefaultMass = 1.0;

        /// <summary>
        /// Friction in joules lost per metre travelled. Pedagogical reason: this is
        /// the "tax" that makes the answer sit a whisker ABOVE the crest height, not
        /// exactly on it, so the learner discovers energy in must exceed energy
        /// needed. 0.12 J/m is small enough that the lesson is about mgh, not drag.
        /// </summary>
        public const double DefaultFriction = 0.12;

        /// <summary>Track x of the valley (lowest point), metres.</summary>
        public const double Valley = 5.4;
        /// <summary>Track x of the crest (top of the far hill), metres.</summary>
        public const double CrestX = 9.6;
        /// <summary>Crest height, metres.</summary>
        public const double CrestHeight = 2.2;

        /// <summary>
        /// Floor applied to v^2 (NOT v). Pedagogical reason: without a floor the cart
        /// would freeze exactly on its release mark and never visibly start; flooring
        /// v^2 at 0.12 (so v is at least sqrt(0.12) ~= 0.346 m/s) makes it creep off
        /// the mark. This matches the reference sqrt(max(0.12, 2*ke/m)) exactly; note
        /// the floor is on the squared speed, so the effective minimum speed is
        /// sqrt(0.12), not 0.12.
        /// </summary>
        public const double SpeedFloorSquared = 0.12;

        /// <summary>Kinetic energy at or below this (J) counts as out of energy.</summary>
        public const double StallKineticEnergy = 0.02;

        /// <summary>The cart must have travelled more than this (m) before a stall counts.</summary>
        public const double StallMinDistance = 0.05;

        /// <summary>After this many stalls the run settles (retry).</summary>
        public const int StallLimit = 3;

        /// <summary>Distance-per-step scale factor (reference tuning: x += dir*v*dt*1.15).</summary>
        public const double StepScale = 1.15;

        /// <summary>Crest speed below this (m/s) is "just enough", at or above is overshoot.</summary>
        public const double JustEnoughSpeed = 1.6;

        public double Mass { get; }
        public double Friction { get; }

        public HillModel(double mass = DefaultMass, double friction = DefaultFriction)
        {
            if (mass <= 0.0) throw new ArgumentOutOfRangeException(nameof(mass), "Mass must be positive.");
            if (friction < 0.0) throw new ArgumentOutOfRangeException(nameof(friction), "Friction must be non-negative.");
            Mass = mass;
            Friction = friction;
        }

        /// <summary>
        /// Track height in metres for a track position x (metres, 0..14), exactly
        /// as authored in the reference:
        ///   x &lt; valley:    3.4 * ((valley - x)/valley)^1.5      (left slope)
        ///   valley..crest:  2.2 * ((x - valley)/(crest - valley))^2 (rise to crest)
        ///   x &gt; crest:     max(0, 2.2 - 2.6*(x - crest))         (down the far side)
        /// </summary>
        public double Track(double x)
        {
            if (x < Valley) return 3.4 * Math.Pow((Valley - x) / Valley, 1.5);
            if (x < CrestX) return 2.2 * Math.Pow((x - Valley) / (CrestX - Valley), 2.0);
            return Math.Max(0.0, 2.2 - 2.6 * (x - CrestX));
        }

        /// <summary>Simulate one release from track position <paramref name="releaseX"/>.</summary>
        public HillResult Run(double releaseX, double dt, int maxSteps = 1_000_000)
        {
            return RunInternal(releaseX, dt, maxSteps, null);
        }

        /// <summary>
        /// Simulate one release and record the cart's x position after every step
        /// into <paramref name="trajectoryOut"/>. Used to prove determinism.
        /// </summary>
        public HillResult Run(double releaseX, double dt, List<double> trajectoryOut, int maxSteps = 1_000_000)
        {
            return RunInternal(releaseX, dt, maxSteps, trajectoryOut);
        }

        private HillResult RunInternal(double releaseX, double dt, int maxSteps, List<double> trajectoryOut)
        {
            double h0 = Track(releaseX);
            double x = releaseX;
            int dir = 1;
            double dist = 0.0;
            int stalls = 0;

            for (int steps = 1; steps <= maxSteps; steps++)
            {
                double h = Track(x);
                double energy = Mass * Constants.G * h0 - Friction * dist;
                double ke = energy - Mass * Constants.G * h;

                // Genuine stall: out of energy, after having actually moved, before the crest.
                if (ke <= StallKineticEnergy && dist > StallMinDistance && x < CrestX)
                {
                    if (dir == 1 && x > Valley) { dir = -1; stalls++; }
                    else if (dir == -1 && x < Valley) { dir = 1; stalls++; }

                    if (stalls >= StallLimit)
                    {
                        return new HillResult(HillOutcome.RolledBack, h0, 0.0, stalls, steps);
                    }
                }

                double v = Math.Sqrt(Math.Max(SpeedFloorSquared, 2.0 * Math.Max(0.0, ke) / Mass));
                x += dir * v * dt * StepScale;
                dist += v * dt * StepScale;
                trajectoryOut?.Add(x);

                if (x >= CrestX && dir == 1)
                {
                    HillOutcome outcome = v < JustEnoughSpeed ? HillOutcome.JustEnough : HillOutcome.Overshoot;
                    return new HillResult(outcome, h0, v, stalls, steps);
                }
            }

            // Safety net: neither crest nor StallLimit reached within maxSteps.
            return new HillResult(HillOutcome.RolledBack, h0, 0.0, stalls, maxSteps);
        }
    }
}
