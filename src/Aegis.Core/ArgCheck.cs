namespace Aegis.Core
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// A collection of argument checking helpers.
    /// </summary>
    public static class ArgCheck
    {
        /// <summary>
        /// Verifies the argument is not null.
        /// </summary>
        /// <param name="arg">The argument to verify.</param>
        /// <param name="argName">The name of the argument (usually obtained using the nameof operator).</param>
        /// <exception cref="ArgumentNullException">Thrown when the constraint is violated.</exception>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotNull<T>([ValidatedNotNull]T arg, string argName)
            where T : class
        {
            if (arg is null)
            {
                throw new ArgumentNullException(argName);
            }
        }

        /// <summary>
        /// Verifies the argument is not null.
        /// </summary>
        /// <param name="arg">The argument to verify.</param>
        /// <param name="argName">The name of the argument (usually obtained using the nameof operator).</param>
        /// <param name="allowWhitespace">Whether or not to allow whitespace as a non-empty string. Default is false.</param>
        /// <exception cref="ArgumentNullException">Thrown when the constraint is violated.</exception>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NonEmptyString([ValidatedNotNull]string arg, string argName, bool allowWhitespace = false)
        {
            if (allowWhitespace ? string.IsNullOrEmpty(arg) : string.IsNullOrWhiteSpace(arg))
            {
                throw new ArgumentNullException(argName, "The string must not be empty!");
            }
        }

        /// <summary>
        /// Verifies the argument is not equal to a given invalid value.
        /// </summary>
        /// <param name="invalidValue">The invalid value to check against.</param>
        /// <param name="arg">The argument to verify.</param>
        /// <param name="argName">The name of the argument (usually obtained using the nameof operator).</param>
        /// <exception cref="ArgumentException">Thrown when the constraint is violated.</exception>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsNot<T>(T invalidValue, [ValidatedNotNull]T arg, string argName)
            where T : IComparable
        {
            if (arg.CompareTo(invalidValue) == 0)
            {
                throw new ArgumentException($"The input value must not be {invalidValue}!", argName);
            }
        }

        /// <summary>
        /// Marker that lets code analysis know that a method will ensure the argument is not null.
        /// </summary>
        [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
        internal sealed class ValidatedNotNullAttribute : Attribute
        {
        }
    }
}
