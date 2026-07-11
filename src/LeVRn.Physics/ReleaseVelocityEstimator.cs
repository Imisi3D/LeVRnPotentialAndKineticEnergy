using System;
using System.Collections.Generic;

namespace LeVRn.Physics
{
    /// <summary>
    /// One sampled point from a controller / pointer motion trail. X and Y are in
    /// pixels in screen space (Y increases downward, as in the reference canvas);
    /// <see cref="TimeMillis"/> is a timestamp in milliseconds.
    /// </summary>
    public readonly struct TrailPoint
    {
        public readonly double X;
        public readonly double Y;
        public readonly double TimeMillis;

        public TrailPoint(double x, double y, double timeMillis)
        {
            X = x;
            Y = y;
            TimeMillis = timeMillis;
        }
    }

    /// <summary>An estimated release velocity in metres per second.</summary>
    public readonly struct ReleaseVelocity
    {
        /// <summary>Horizontal velocity, +x to screen right.</summary>
        public readonly double Vx;
        /// <summary>Vertical velocity, +y is physics up (screen up).</summary>
        public readonly double Vy;
        /// <summary>Speed magnitude, capped at <see cref="ReleaseVelocityEstimator.MaxSpeed"/>.</summary>
        public readonly double Speed;

        public ReleaseVelocity(double vx, double vy, double speed)
        {
            Vx = vx;
            Vy = vy;
            Speed = speed;
        }
    }

    /// <summary>
    /// Estimates a release velocity from a timestamped position trail, matching
    /// the reference releaseVelocity() exactly. The learner's tracked-controller
    /// gesture becomes the launch velocity of the thrown ball.
    /// </summary>
    public static class ReleaseVelocityEstimator
    {
        /// <summary>
        /// Speed cap in m/s. Pedagogical reason: a real arm cannot fling the ball
        /// arbitrarily fast, and an uncapped gesture on a laggy frame could spike
        /// the ledger to nonsense. 15 m/s is a hard, human-plausible ceiling; the
        /// direction of the gesture is preserved when the cap bites.
        /// </summary>
        public const double MaxSpeed = 15.0;

        /// <summary>
        /// Minimum time window in milliseconds. Pedagogical reason: dividing by a
        /// near-zero dt on two samples that arrived in the same frame would blow the
        /// speed up. 16 ms is roughly one 60 Hz frame, the smallest honest dt.
        /// </summary>
        public const double MinDeltaMillis = 16.0;

        /// <summary>
        /// How many trailing samples to measure displacement over (~6). Using a short
        /// window rather than the whole trail reads the speed at the moment of
        /// release, not the average of the whole drag.
        /// </summary>
        public const int SampleWindow = 6;

        /// <summary>
        /// Estimate the release velocity from the trail. Returns zero if there are
        /// fewer than two samples.
        /// </summary>
        /// <param name="trail">Samples in chronological order; the last is the release instant.</param>
        /// <param name="pixelsPerMetre">Screen scale used to convert pixel displacement to metres.</param>
        public static ReleaseVelocity Estimate(IReadOnlyList<TrailPoint> trail, double pixelsPerMetre)
        {
            if (trail == null) throw new ArgumentNullException(nameof(trail));
            if (pixelsPerMetre <= 0.0)
                throw new ArgumentOutOfRangeException(nameof(pixelsPerMetre), "pixelsPerMetre must be positive.");

            if (trail.Count < 2) return new ReleaseVelocity(0.0, 0.0, 0.0);

            int last = trail.Count - 1;
            // Reference: a = tr[max(0, len-6)]. Span the last SampleWindow samples.
            int first = Math.Max(0, last - (SampleWindow - 1));
            TrailPoint a = trail[first];
            TrailPoint b = trail[last];

            double dt = Math.Max(MinDeltaMillis, b.TimeMillis - a.TimeMillis) / 1000.0;
            double vx = (b.X - a.X) / dt / pixelsPerMetre;
            // Y is inverted: screen-down is physics-down, so an upward drag (screen Y
            // decreasing) becomes a positive (upward) velocity.
            double vy = -(b.Y - a.Y) / dt / pixelsPerMetre;

            double v = Math.Sqrt(vx * vx + vy * vy);
            if (v > MaxSpeed)
            {
                vx *= MaxSpeed / v;
                vy *= MaxSpeed / v;
            }
            return new ReleaseVelocity(vx, vy, Math.Min(v, MaxSpeed));
        }
    }
}
