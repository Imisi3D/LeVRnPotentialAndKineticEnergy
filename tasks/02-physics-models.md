# Task 02 · Port the verified physics models to C#

## Goal
Reimplement the three verified physics models from
`reference/embodied_prototype.html` as an engine-agnostic C# library with
numerical tests. These power the embodied stations (Coding Brief Section 4).

## Deliverables
```
src/LeVRn.Physics/           netstandard2.1, no UnityEngine
  Constants.cs               G = 10 (school convention), documented
  ProjectileModel.cs         ballistic flight + live PE/KE ledger values
  ReleaseVelocityEstimator.cs velocity from a timestamped position trail
  PendulumModel.cs           damped pendulum, Nose Test
  HillModel.cs               energy-method rolling track, Just Enough Hill
tests/LeVRn.Physics.Tests/
TASK_REPORT.md
```

## Models and behaviour to match
### ReleaseVelocityEstimator
Given a trail of (x, y, tMillis) points in pixels and a pixelsPerMetre scale:
velocity from the displacement over the last ~6 samples (minimum dt 16 ms),
y inverted (screen down = physics up negative), speed capped at 15 m/s with
direction preserved.

### ProjectileModel (Handover station)
Ball mass 0.5 kg. Step: vy -= G*dt; position integrates. Ledger at any
instant: PE = m*G*h (h >= 0 above ground), KE = 0.5*m*speed^2.

### PendulumModel (Nose Test)
Length L = 2.3 m, angular EOM alpha = -(G/L)*sin(theta), per-step angular
velocity damping factor 0.9985, dt injectable. Release angle -1.05 rad.
Apex detection: only after the swing has passed theta > 0.15 (swung away)
and returned to theta < -0.5 with |omega| < 0.02.

### HillModel (Just Enough Hill)
Track height (x in metres, 0..14):
  valley = 5.4, crest x = 9.6, crest height = 2.2
  x < valley:      h = 3.4 * ((valley - x)/valley)^1.5
  valley..crest:   h = 2.2 * ((x - valley)/(crest - valley))^2
  x > crest:       h = max(0, 2.2 - 2.6*(x - crest))
Motion by energy method: kineticE = m*G*h0 - mu*distance - m*G*h(x), with
m = 1, friction mu = 0.12 J per metre, speed floor 0.12 m/s so the cart
creeps off its release mark, x advances by dir*v*dt*1.15.
Stall rule: only when ke <= 0.02 AND distance travelled > 0.05; on the rise
(x > 5.4, dir +1) flip to -1; on the left slope (x < 5.4, dir -1) flip to +1;
after 3 stalls the run settles (retry). Crest outcome: crossing x >= 9.6
moving right is JUST ENOUGH if crest speed < 1.6 m/s, else OVERSHOOT.

## Acceptance tests (pre-verified expected results)
1. Formula spot checks: KE(m=0.5, v=10) == 25 J; KE(2 kg, 3 m/s) == 9 J;
   PE(0.2 kg, h=5) == 10 J with G = 10.
2. Projectile conservation: with no drag, PE + KE stays within 0.5% of the
   launch total across a full simulated flight (dt = 1/90 s).
3. Velocity estimator: a synthetic straight-line trail of known speed
   recovers that speed within 5%; a 60 m/s trail caps at 15 m/s.
4. Pendulum: released at -1.05 rad with damping 0.9985, the detected return
   apex is strictly greater than -1.05 rad (it can NEVER meet or exceed the
   release angle); with damping 1.0 and small dt, the return apex is within
   0.02 rad of release (energy conservation sanity).
5. Hill outcome sweep, release x from 0.4 to 4.8 step 0.1 (dt = 0.008):
   all three outcomes occur; every h0 <= 2.09 m rolls back; h0 values near
   2.33 and 2.42 m are JUST ENOUGH; h0 >= 2.76 m overshoots. The success
   band must be non-empty and sit above crest height 2.2 m (friction tax).
6. Determinism: identical inputs and dt produce identical trajectories.

## Notes
- Keep dt injectable everywhere; tests use fixed dt.
- Document every constant with its pedagogical reason (e.g. speed floor,
  friction) so tuning later is informed, not archaeological.
