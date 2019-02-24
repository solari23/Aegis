namespace Aegis.Core
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// A collection of static helper utilities and extension methods.
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Gets the Unix timestamp representation of a <see cref="DateTime"/>.
        /// </summary>
        /// <param name="dateTime">The <see cref="DateTime"/> to convert.</param>
        /// <returns>The Unix timestamp representation of the <see cref="DateTime"/>.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ToUnixTime(this DateTime dateTime) => new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeSeconds();

        /// <summary>
        /// Converts a Unix timestamp to a <see cref="DateTime"/> object.
        /// </summary>
        /// <param name="unixTime">The Unix timestamp to convert.</param>
        /// <returns>A <see cref="DateTime"/> representation of the Unix timestamp.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime ToDateTime(long unixTime) => DateTimeOffset.FromUnixTimeSeconds(unixTime).UtcDateTime;

        /// <summary>
        /// Gets the current Unix time.
        /// </summary>
        /// <returns>The current Unix time.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetCurrentUnixTime() => DateTime.UtcNow.ToUnixTime();
    }
}
