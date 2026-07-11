using System;

namespace LeVRn.Physics
{
    /// <summary>
    /// The Nose Test tire swing: a damped pendulum whose return apex can never
    /// meet its release angle. Matches the reference Station 2 update(). The
    /// angular equation of motion is alpha = -(G/L)*sin(theta); each step applies
    /// a constant velocity-damping factor (the "whisper the air steals"). dt is
    /// injectable for deterministic tests.
    ///
    /// Angle convention (from the reference geometry): theta is measured from the
    /// downward vertical; the nose marker sits at a negative angle and the swing
    /// travels through positive angles on the far side.
    /// </summary>
    public sealed class PendulumModel
    {
        /// <summary>
        /// Rope length in metres. Pedagogical reason: 2.3 m is a believable
        /// mango-tree tire swing, and it sets a swing period slow enough to watch
        /// the tire stop a breath short of the nose.
        /// </summary>
        public const double DefaultLength = 2.3;

        /// <summary>
        /// Tire mass in kg. It affects only the energy ledger numbers, never the
        /// motion (the pendulum equation is mass independent). 6 kg reads as a real
        /// car tire.
        /// </summary>
        public const double DefaultMass = 6.0;

        /// <summary>
        /// Per-step angular-velocity retention. Pedagogical reason: this is the
        /// small energy loss to air that guarantees the tire returns lower than it
        /// started, so it can never touch the learner's nose. 0.9985 is gentle
        /// enough that the loss is visible only as a small, honest gap.
        /// </summary>
        public const double DefaultDamping = 0.9985;

        /// <summary>Release angle in radians, the nose marker (pulled back to the face).</summary>
        public const double DefaultReleaseAngle = -1.05;

        /// <summary>The swing must pass this angle (swung away) before a return apex counts.</summary>
        public const double SwungAwayAngle = 0.15;

        /// <summary>Only look for the return apex once the tire is back on the nose side.</summary>
        public const double ReturnGateAngle = -0.5;

        /// <summary>|omega| below this marks the turning point (apex) has been reached.</summary>
        public const double ApexOmega = 0.02;

        public double Length { get; }
        public double Mass { get; }
        public double Damping { get; }

        public double Theta { get; private set; }
        public double Omega { get; private set; }
        public bool Released { get; private set; }

        /// <summary>The detected return-apex angle, or null until it is detected.</summary>
        public double? ReturnApex { get; private set; }

        private bool _swungAway;

        public PendulumModel(double length = DefaultLength, double mass = DefaultMass, double damping = DefaultDamping)
        {
            if (length <= 0.0) throw new ArgumentOutOfRangeException(nameof(length), "Length must be positive.");
            if (mass <= 0.0) throw new ArgumentOutOfRangeException(nameof(mass), "Mass must be positive.");
            if (damping <= 0.0 || damping > 1.0)
                throw new ArgumentOutOfRangeException(nameof(damping), "Damping must be in (0, 1].");
            Length = length;
            Mass = mass;
            Damping = damping;
        }

        /// <summary>Pull the tire to <paramref name="angle"/> radians and let go from rest.</summary>
        public void Release(double angle)
        {
            Theta = angle;
            Omega = 0.0;
            Released = true;
            _swungAway = false;
            ReturnApex = null;
        }

        /// <summary>Advance one step of length <paramref name="dt"/> seconds.</summary>
        public void Step(double dt)
        {
            if (!Released) return;

            double alpha = -(Constants.G / Length) * Math.Sin(Theta);
            Omega += alpha * dt;
            Omega *= Damping;
            Theta += Omega * dt;

            if (!_swungAway && Theta > SwungAwayAngle) _swungAway = true;

            // Return apex: the first turning point after the tire has swung away and
            // come back onto the nose side. Detected at the turning point (|omega|
            // near zero). With damping < 1 this apex is strictly higher (less
            // negative) than the release angle; with damping == 1 it returns to the
            // release angle within numerical tolerance.
            if (ReturnApex == null && _swungAway && Theta < ReturnGateAngle && Math.Abs(Omega) < ApexOmega)
            {
                ReturnApex = Theta;
            }
        }

        /// <summary>Height of the tire above its lowest point, metres: L*(1-cos(theta)).</summary>
        public double Height => Length * (1.0 - Math.Cos(Theta));

        /// <summary>Linear speed of the tire, m/s: |omega|*L.</summary>
        public double LinearSpeed => Math.Abs(Omega) * Length;

        /// <summary>Live energy ledger for the swing.</summary>
        public EnergyLedger Ledger()
        {
            return new EnergyLedger(Energy.Potential(Mass, Height), Energy.Kinetic(Mass, LinearSpeed));
        }

        /// <summary>
        /// Step until the return apex is detected and return its angle. Throws if it
        /// is not detected within <paramref name="maxSteps"/> (a safety bound, not a
        /// physical limit).
        /// </summary>
        public double RunToReturnApex(double dt, int maxSteps = 1_000_000)
        {
            if (!Released) throw new InvalidOperationException("Release the pendulum before running it.");
            for (int i = 0; i < maxSteps && ReturnApex == null; i++) Step(dt);
            if (ReturnApex == null)
                throw new InvalidOperationException("Return apex was not detected within maxSteps.");
            return ReturnApex.Value;
        }
    }
}
