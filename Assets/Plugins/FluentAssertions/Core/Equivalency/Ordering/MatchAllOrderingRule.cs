namespace FluentAssertions.Equivalency.Ordering
{
    /// <summary>
    /// An ordering rule that basically states that the order of items in all collections is important.
    /// </summary>
    internal class MatchAllOrderingRule : IOrderingRule
    {
        /// <summary>
        /// Determines if ordering of the member referred to by the current <paramref name="subjectInfo"/> is relevant.
        /// </summary>
        public OrderStrictness Evaluate(ISubjectInfo subjectInfo)
        {
            return OrderStrictness.Strict;
        }

        public override string ToString()
        {
            return "Always be strict about the collection order";
        }
    }
}