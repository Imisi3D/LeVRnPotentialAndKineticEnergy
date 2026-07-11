using System;

namespace LeVRn.Physics
{
    /// <summary>Position and velocity of the thrown ball, in metres and m/s.</summary>
    public struct ProjectileState
    {
        /// <summary>Horizontal position, metres.</summary>
        public double X;
        /// <summary>Height above the ground reference, metres.</summary>
        public double Y;
        /// <summary>Horizontal velocity, m/s.</summary>
        public double Vx;
        /// <summary>Vertical velocity, m/s, +up.</summary>
        public double Vy;

        public ProjectileState(double x, double y, double vx, double vy)
        {
            X = x;
            Y = y;
            Vx = vx;
            Vy = vy;
        }
    }

    /// <summary>
    /// Ballistic flight of the handover ball with a live PE/KE ledger, matching
    /// the reference Station 1 update(). No drag: the only force is gravity.
    /// Integration is semi-implicit (symplectic) Euler exactly as the reference:
    /// update the vertical velocity first, then integrate position with the new
    /// velocity. dt is injectable so tests are deterministic.
    /// </summary>
    public sealed class ProjectileModel
    {
        /// <summary>
        /// Default ball mass in kg. Pedagogical reason: the client's own worked
        /// example uses a 0.5 kg ball, so KE = 0.5 * 0.5 * v^2 lands on round exam
        /// numbers the learner can check by hand.
        /// </summary>
        public const double DefaultMass = 0.5;

        public double Mass { get; }
        public ProjectileState State;

        public ProjectileModel(double mass = DefaultMass)
        {
            if (mass <= 0.0) throw new ArgumentOutOfRangeException(nameof(mass), "Mass must be positive.");
            Mass = mass;
        }

        /// <summary>Place the ball and give it a launch velocity.</summary>
        public void Launch(double x, double y, double vx, double vy)
        {
            State = new ProjectileState(x, y, vx, vy);
        }

        /// <summary>
        /// Advance one step of length <paramref name="dt"/> seconds. Semi-implicit
        /// Euler: vy -= G*dt, then position integrates with the updated velocity.
        /// </summary>
        public void Step(double dt)
        {
            State.Vy -= Constants.G * dt;
            State.X += State.Vx * dt;
            State.Y += State.Vy * dt;
        }

        /// <summary>Current speed magnitude, m/s.</summary>
        public double Speed => Math.Sqrt(State.Vx * State.Vx + State.Vy * State.Vy);

        /// <summary>Height above ground used for potential energy (never negative).</summary>
        public double Height => Math.Max(0.0, State.Y);

        /// <summary>Live energy ledger: PE = m*G*h, KE = 0.5*m*speed^2.</summary>
        public EnergyLedger Ledger()
        {
            return new EnergyLedger(Energy.Potential(Mass, State.Y), Energy.Kinetic(Mass, Speed));
        }

        /// <summary>True once the ball is at or below ground and no longer rising.</summary>
        public bool HasLanded => State.Y <= 0.0 && State.Vy <= 0.0;
    }
}
