namespace Aegis.Core
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Helpers used by the Bond runtime to automatically convert types during serialization.
    /// These allow for more C# idiomatic usage of the code generated types.
    /// </summary>
    public static class BondTypeAliasConverter
    {
        /// <summary>
        /// Converts a <see cref="DateTime"/> into a Unix timestamp for serialization.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="unused">Required by the Bond type conversion interface, but not used.</param>
        /// <returns>The converted value.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Signature is required by Bond.")]
        public static long Convert(DateTime value, long unused) => value.ToUnixTime();

        /// <summary>
        /// Converts a Unix timestamp into a <see cref="DateTime"/> for deserialization.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="unused">Required by the Bond type conversion interface, but not used.</param>
        /// <returns>The converted value.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Signature is required by Bond.")]
        public static DateTime Convert(long value, DateTime unused) => Helpers.ToDateTime(value);
    }
}
