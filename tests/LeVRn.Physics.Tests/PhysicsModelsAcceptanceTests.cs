using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using LeVRn.Physics;

namespace LeVRn.Physics.Tests
{
    /// <summary>
    /// The six acceptance tests from tasks/02-physics-models.md. Expected values
    /// are pre-verified against reference/embodied_prototype.html.
    /// </summary>
    [TestFixture]
    public sealed class PhysicsModelsAcceptanceTests
    {
        // ===================================================================
        // Acceptance test 1: formula spot checks.
        // ===================================================================
        [Test]
        public void Energy_formulae_match_the_worked_examples()
        {
            // KE = 0.5 * m * v^2
            Assert.That(Energy.Kinetic(0.5, 10.0), Is.EqualTo(25.0).Within(1e-9), "KE(0.5 kg, 10 m/s)");
            Assert.That(Energy.Kinetic(2.0, 3.0), Is.EqualTo(9.0).Within(1e-9), "KE(2 kg, 3 m/s)");
            // PE = m * G * h, with G = 10
            Assert.That(Energy.Potential(0.2, 5.0), Is.EqualTo(10.0).Within(1e-9), "PE(0.2 kg, 5 m)");
            Assert.That(Constants.G, Is.EqualTo(10.0), "school-convention gravity");
        }

        // ===================================================================
        // Acceptance test 2: projectile energy conservation.
        // With no drag, PE + KE stays within 0.5% of the launch total across a
        // full simulated flight (dt = 1/90 s).
        //
        // Note: the reference integrator is semi-implicit (symplectic) Euler,
        // which introduces a small constant energy drift per step. A fast, flat
        // throw keeps that drift a tiny fraction of the (large) horizontal KE, so
        // it stays well inside 0.5% over the whole flight.
        // ===================================================================
        [Test]
        public void Projectile_conserves_energy_within_half_a_percent()
        {
            const double dt = 1.0 / 90.0;
            var ball = new ProjectileModel(); // 0.5 kg
            ball.Launch(x: 0.0, y: 1.5, vx: 15.0, vy: 0.0);

            double launchTotal = ball.Ledger().Total;
            double maxRelDeviation = 0.0;
            int guard = 0;

            while (!ball.HasLanded && guard++ < 100_000)
            {
                ball.Step(dt);
                double total = ball.Ledger().Total;
                double rel = Math.Abs(total - launchTotal) / launchTotal;
                if (rel > maxRelDeviation) maxRelDeviation = rel;
            }

            TestContext.WriteLine($"launch total = {launchTotal:F3} J, max relative deviation = {maxRelDeviation:P3}");
            Assert.That(ball.HasLanded, Is.True, "the flight must complete");
            Assert.That(maxRelDeviation, Is.LessThan(0.005), "PE + KE must stay within 0.5% of the launch total");
        }

        // ===================================================================
        // Acceptance test 3: release-velocity estimator.
        // A synthetic straight-line trail of known speed recovers that speed
        // within 5%; a 60 m/s trail caps at 15 m/s with direction preserved.
        // ===================================================================
        [Test]
        public void Velocity_estimator_recovers_known_speed_within_five_percent()
        {
            const double ppm = 46.0; // reference pixels-per-metre
            var trail = HorizontalTrail(speedMetresPerSecond: 8.0, pixelsPerMetre: ppm,
                                        sampleCount: 8, intervalMillis: 20.0);

            ReleaseVelocity rv = ReleaseVelocityEstimator.Estimate(trail, ppm);

            Assert.That(rv.Speed, Is.EqualTo(8.0).Within(8.0 * 0.05), "recovered speed within 5%");
            Assert.That(rv.Vy, Is.EqualTo(0.0).Within(1e-6), "purely horizontal trail has no vertical component");
            Assert.That(rv.Vx, Is.GreaterThan(0.0), "direction preserved (moving right)");
        }

        [Test]
        public void Velocity_estimator_caps_a_sixty_metre_per_second_trail_at_fifteen()
        {
            const double ppm = 46.0;
            var trail = HorizontalTrail(speedMetresPerSecond: 60.0, pixelsPerMetre: ppm,
                                        sampleCount: 8, intervalMillis: 20.0);

            ReleaseVelocity rv = ReleaseVelocityEstimator.Estimate(trail, ppm);

            Assert.That(rv.Speed, Is.EqualTo(ReleaseVelocityEstimator.MaxSpeed).Within(1e-6), "capped at 15 m/s");
            Assert.That(rv.Vx, Is.EqualTo(ReleaseVelocityEstimator.MaxSpeed).Within(1e-6), "cap preserves the horizontal direction");
            Assert.That(rv.Vy, Is.EqualTo(0.0).Within(1e-6), "no vertical component introduced by the cap");
        }

        // ===================================================================
        // Acceptance test 4: pendulum apex.
        // Released at -1.05 rad with damping 0.9985, the detected return apex is
        // strictly greater than -1.05 (it can NEVER meet or exceed the release
        // angle). With damping 1.0 and small dt, the return apex is within 0.02
        // rad of release (energy-conservation sanity).
        // ===================================================================
        [Test]
        public void Damped_pendulum_never_returns_to_its_release_angle()
        {
            const double dt = 1.0 / 240.0;
            var swing = new PendulumModel(damping: PendulumModel.DefaultDamping);
            swing.Release(PendulumModel.DefaultReleaseAngle);

            double apex = swing.RunToReturnApex(dt);

            TestContext.WriteLine($"damped return apex = {apex:F5} rad (release {PendulumModel.DefaultReleaseAngle})");
            Assert.That(apex, Is.GreaterThan(PendulumModel.DefaultReleaseAngle),
                "the tire must return lower than it started: it can never touch the nose");
        }

        [Test]
        public void Undamped_pendulum_returns_to_its_release_angle_within_tolerance()
        {
            const double dt = 1.0 / 1000.0; // small dt: energy-conservation sanity
            var swing = new PendulumModel(damping: 1.0);
            swing.Release(PendulumModel.DefaultReleaseAngle);

            double apex = swing.RunToReturnApex(dt);

            TestContext.WriteLine($"undamped return apex = {apex:F5} rad (release {PendulumModel.DefaultReleaseAngle})");
            Assert.That(apex, Is.EqualTo(PendulumModel.DefaultReleaseAngle).Within(0.02),
                "with no damping the swing returns to (essentially) its release angle");
        }

        // ===================================================================
        // Acceptance test 5: hill outcome sweep.
        // Release x from 0.4 to 4.8 step 0.1 (dt = 0.008): all three outcomes
        // occur; every h0 <= 2.09 m rolls back; h0 near 2.33 and 2.42 m are
        // JUST ENOUGH; h0 >= 2.76 m overshoots. The success band must be
        // non-empty and sit above crest height 2.2 m (friction tax).
        // ===================================================================
        [Test]
        public void Hill_outcome_sweep_matches_the_verified_bands()
        {
            const double dt = 0.008;
            var hill = new HillModel();

            var results = new List<HillResult>();
            for (int i = 4; i <= 48; i++) // 0.4 .. 4.8 step 0.1, via integers to avoid float drift
            {
                double releaseX = i / 10.0;
                results.Add(hill.Run(releaseX, dt));
            }

            // All three outcomes must occur across the sweep.
            var outcomes = results.Select(r => r.Outcome).Distinct().ToList();
            Assert.That(outcomes, Does.Contain(HillOutcome.RolledBack), "some releases roll back");
            Assert.That(outcomes, Does.Contain(HillOutcome.JustEnough), "some releases just crest");
            Assert.That(outcomes, Does.Contain(HillOutcome.Overshoot), "some releases overshoot");

            // Every low release rolls back.
            foreach (var r in results.Where(r => r.ReleaseHeight <= 2.09))
                Assert.That(r.Outcome, Is.EqualTo(HillOutcome.RolledBack),
                    $"h0 = {r.ReleaseHeight:F3} m (<= 2.09) must roll back");

            // Every high release overshoots.
            foreach (var r in results.Where(r => r.ReleaseHeight >= 2.76))
                Assert.That(r.Outcome, Is.EqualTo(HillOutcome.Overshoot),
                    $"h0 = {r.ReleaseHeight:F3} m (>= 2.76) must overshoot");

            // The two named "just enough" heights.
            AssertJustEnough(hill.Run(1.2, dt), expectedHeight: 2.33);
            AssertJustEnough(hill.Run(1.1, dt), expectedHeight: 2.42);

            // The success band is non-empty and sits above the crest (friction tax).
            var justEnough = results.Where(r => r.Outcome == HillOutcome.JustEnough).ToList();
            Assert.That(justEnough, Is.Not.Empty, "the success band must be non-empty");
            foreach (var r in justEnough)
                Assert.That(r.ReleaseHeight, Is.GreaterThan(HillModel.CrestHeight),
                    $"a just-enough release (h0 = {r.ReleaseHeight:F3} m) must bank more than the crest height");
        }

        private static void AssertJustEnough(HillResult r, double expectedHeight)
        {
            Assert.That(r.ReleaseHeight, Is.EqualTo(expectedHeight).Within(0.05),
                $"release height should be near {expectedHeight} m");
            Assert.That(r.Outcome, Is.EqualTo(HillOutcome.JustEnough),
                $"release at h0 = {r.ReleaseHeight:F3} m should be just enough, crest speed {r.CrestSpeed:F3} m/s");
            Assert.That(r.CrestSpeed, Is.LessThan(HillModel.JustEnoughSpeed), "crest speed below the just-enough threshold");
        }

        // ===================================================================
        // Acceptance test 6: determinism.
        // Identical inputs and dt produce identical trajectories.
        // ===================================================================
        [Test]
        public void Identical_inputs_produce_identical_trajectories()
        {
            const double dt = 1.0 / 90.0;

            // Projectile: identical state sequences.
            var seqA = ProjectileStates(dt);
            var seqB = ProjectileStates(dt);
            Assert.That(seqA.Count, Is.EqualTo(seqB.Count), "same number of projectile steps");
            for (int i = 0; i < seqA.Count; i++)
            {
                Assert.That(seqA[i].X, Is.EqualTo(seqB[i].X), $"projectile x at step {i}");
                Assert.That(seqA[i].Y, Is.EqualTo(seqB[i].Y), $"projectile y at step {i}");
                Assert.That(seqA[i].Vx, Is.EqualTo(seqB[i].Vx), $"projectile vx at step {i}");
                Assert.That(seqA[i].Vy, Is.EqualTo(seqB[i].Vy), $"projectile vy at step {i}");
            }

            // Hill: identical x trajectories and identical outcome.
            var hill = new HillModel();
            var trajA = new List<double>();
            var trajB = new List<double>();
            var resA = hill.Run(1.2, 0.008, trajA);
            var resB = hill.Run(1.2, 0.008, trajB);
            Assert.That(trajA, Is.EqualTo(trajB), "identical hill trajectories");
            Assert.That(resA.Outcome, Is.EqualTo(resB.Outcome), "identical hill outcome");
            Assert.That(resA.CrestSpeed, Is.EqualTo(resB.CrestSpeed), "identical crest speed");
        }

        // ---------------- helpers ----------------

        private static List<TrailPoint> HorizontalTrail(double speedMetresPerSecond, double pixelsPerMetre,
                                                        int sampleCount, double intervalMillis)
        {
            var trail = new List<TrailPoint>(sampleCount);
            double pxPerSample = speedMetresPerSecond * pixelsPerMetre * (intervalMillis / 1000.0);
            for (int i = 0; i < sampleCount; i++)
                trail.Add(new TrailPoint(x: i * pxPerSample, y: 100.0, timeMillis: i * intervalMillis));
            return trail;
        }

        private static List<ProjectileState> ProjectileStates(double dt)
        {
            var ball = new ProjectileModel();
            ball.Launch(0.0, 1.5, 15.0, 0.0);
            var states = new List<ProjectileState>();
            int guard = 0;
            while (!ball.HasLanded && guard++ < 100_000)
            {
                ball.Step(dt);
                states.Add(ball.State);
            }
            return states;
        }
    }
}
