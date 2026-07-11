using System;

namespace LeVRn.Tutor
{
    /// <summary>
    /// The authored responses attached to one Ask node. The tutor never
    /// generates text; it retrieves one of these by state (attempt count and
    /// whether a lens switch has happened). Mirrors the reference fields:
    /// affirm, hint, switchTo, switchText, walkthrough.
    /// </summary>
    public sealed class ResponseBank
    {
        /// <summary>Spoken when the learner answers correctly.</summary>
        public string Affirm { get; }

        /// <summary>Spoken after the first wrong attempt, same guide, then re-ask.</summary>
        public string Hint { get; }

        /// <summary>Id of the guide the lens switches to after the second wrong attempt.</summary>
        public string SwitchTo { get; }

        /// <summary>The re-ask framing spoken by the switch guide after the second wrong attempt.</summary>
        public string SwitchText { get; }

        /// <summary>Spoken after the third wrong attempt; the objective is flagged for review.</summary>
        public string Walkthrough { get; }

        public ResponseBank(string affirm, string hint, string switchTo, string switchText, string walkthrough)
        {
            Affirm = affirm ?? throw new ArgumentNullException(nameof(affirm));
            Hint = hint ?? throw new ArgumentNullException(nameof(hint));
            SwitchTo = switchTo ?? throw new ArgumentNullException(nameof(switchTo));
            SwitchText = switchText ?? throw new ArgumentNullException(nameof(switchText));
            Walkthrough = walkthrough ?? throw new ArgumentNullException(nameof(walkthrough));
        }
    }
}
