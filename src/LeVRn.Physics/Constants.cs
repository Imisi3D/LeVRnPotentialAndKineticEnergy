namespace LeVRn.Physics
{
    /// <summary>
    /// Shared physical constants for the embodied stations.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Acceleration due to gravity, in metres per second squared.
        ///
        /// Pedagogical reason: the Nigerian JSS 2 Basic Science curriculum (and every
        /// worked answer in this module) uses the school convention g = 10, NOT the
        /// physical 9.81. This is a curriculum value: do not "correct" it. Changing it
        /// would make the ledger numbers disagree with the learner's exam answers.
        /// </summary>
        public const double G = 10.0;
    }
}
