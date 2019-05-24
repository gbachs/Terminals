using System;

namespace Terminals.Data.History
{
    /// <summary>
    ///     Uniform access to the current DateTime in Universal format.
    ///     Allows to define custom time moments by unit tests.
    /// </summary>
    internal static class Moment
    {
        // don't make it read only, is used by tests.
        private static readonly IDateService service = new NowService();

        /// <summary>
        ///     Gets current time in UTC provided by Service
        /// </summary>
        internal static DateTime Now => service.UtcNow;

        private class NowService : IDateService
        {
            public DateTime UtcNow => DateTime.UtcNow;
        }
    }
}