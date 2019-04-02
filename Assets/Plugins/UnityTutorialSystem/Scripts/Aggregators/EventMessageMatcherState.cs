namespace UnityTutorialSystem.Aggregators
{
    /// <summary>
    ///   Indicates the current state of the matching process for a <see cref="EventMessageAggregator"/>.
    /// </summary>
    /// <seealso cref="EventMessageAggregator.MatchResult"/>
    public enum EventMessageMatcherState
    {
        /// <summary>
        ///   Indicates that matching has not yet finished.
        /// </summary>
        Waiting,
        /// <summary>
        ///  Indicates that matching has finished successfully.
        /// </summary>
        Success,
        /// <summary>
        ///   Signals that matching failed at some point in the process.
        /// </summary>
        Failure
    }
}