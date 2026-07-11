using System;

namespace LeVRn.Physics
{
    /// <summary>
    /// The two energy formulae the whole module teaches, in one place so the
    /// stations and the tests compute them identically. These mirror the inline
    /// expressions in reference/embodied_prototype.html: KE = 0.5*m*v^2 and
    /// PE = m*G*h (height clamped at ground level).
    /// </summary>
    public static class Energy
    {
        /// <summary>Kinetic energy 0.5 * m * v^2, in joules.</summary>
        public static double Kinetic(double mass, double speed)
        {
            return 0.5 * mass * speed * speed;
        }

        /// <summary>
        /// Gravitational potential energy m * G * h, in joules. Height below the
        /// ground reference contributes zero (h is clamped at 0), matching the
        /// reference ledger which never shows negative potential energy.
        /// </summary>
        public static double Potential(double mass, double height)
        {
            return mass * Constants.G * Math.Max(0.0, height);
        }
    }

    /// <summary>
    /// A snapshot of the live energy ledger the aside panel shows: potential,
    /// kinetic, and their conserved-in-flight total.
    /// </summary>
    public readonly struct EnergyLedger
    {
        public readonly double Potential;
        public readonly double Kinetic;
        public readonly double Total;

        public EnergyLedger(double potential, double kinetic)
        {
            Potential = potential;
            Kinetic = kinetic;
            Total = potential + kinetic;
        }
    }
}
