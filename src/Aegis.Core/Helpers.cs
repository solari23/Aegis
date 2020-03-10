using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace Aegis.Core
{
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

        /// <summary>
        /// Encodes the given data in Base64URL format.
        /// </summary>
        /// <param name="data">The data to encode.</param>
        /// <returns>The Base64URL encoding of the data.</returns>
        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "False positive in analyzer.")]
        public static string Base64UrlEncode(ReadOnlySpan<byte> data)
        {
            if (data.IsEmpty)
            {
                return string.Empty;
            }

            var base64String = new StringBuilder(Convert.ToBase64String(data));
            base64String.Replace('+', '-');
            base64String.Replace('/', '_');

            // Figure out how many '=' are on the end and trim them.
            int numEquals = 0;
            for (int i = base64String.Length - 1; i >= 0 && base64String[i] == '='; i--, numEquals++);

            base64String.Length -= numEquals;

            return base64String.ToString();
        }
    }
}
