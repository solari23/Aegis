﻿using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Aegis.Core;

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
    public static void NotNull<T>([ValidatedNotNull] T arg, [CallerArgumentExpression("arg")] string argName = null)
        where T : class
    {
        if (arg is null)
        {
            throw new ArgumentNullException(argName);
        }
    }

    /// <summary>
    /// Verifies the given string is not null or empty.
    /// </summary>
    /// <param name="arg">The argument to verify.</param>
    /// <param name="argName">The name of the argument (usually obtained using the nameof operator).</param>
    /// <param name="allowWhitespace">Whether or not to consider whitespace as non-empty. Default is false.</param>
    /// <exception cref="ArgumentNullException">Thrown when the constraint is violated.</exception>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NotEmpty(
        [ValidatedNotNull] string arg,
        [CallerArgumentExpression("arg")] string argName = null,
        bool allowWhitespace = false)
    {
        if (allowWhitespace ? string.IsNullOrEmpty(arg) : string.IsNullOrWhiteSpace(arg))
        {
            throw new ArgumentNullException(argName, "The input string must not be empty!");
        }
    }

    /// <summary>
    /// Verifies the argument is not empty.
    /// </summary>
    /// <param name="arg">The argument to verify.</param>
    /// <param name="argName">The name of the argument (usually obtained using the nameof operator).</param>
    /// <exception cref="ArgumentNullException">Thrown when the constraint is violated because the argument is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the constraint is violated.</exception>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NotEmpty<T>(IEnumerable<T> arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        var hasElements = arg?.Any() ?? throw new ArgumentNullException(argName, "The input must not be null or empty!");

        if (!hasElements)
        {
            throw new ArgumentException("The input string must not be empty!", argName);
        }
    }

    /// <summary>
    /// Verifies the argument is not empty.
    /// </summary>
    /// <param name="arg">The argument to verify.</param>
    /// <param name="argName">The name of the argument (usually obtained using the nameof operator).</param>
    /// <exception cref="ArgumentException">Thrown when the constraint is violated.</exception>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NotEmpty<T>(Span<T> arg, [CallerArgumentExpression("arg")] string argName = null)
        => NotEmpty((ReadOnlySpan<T>)arg, argName);

    /// <summary>
    /// Verifies the argument is not empty.
    /// </summary>
    /// <param name="arg">The argument to verify.</param>
    /// <param name="argName">The name of the argument (usually obtained using the nameof operator).</param>
    /// <exception cref="ArgumentException">Thrown when the constraint is violated.</exception>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NotEmpty<T>(ReadOnlySpan<T> arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        if (arg.IsEmpty)
        {
            throw new ArgumentException("The input span must not be empty!", argName);
        }
    }

    /// <summary>
    /// Verifies the argument contains exactly a given number of elements.
    /// </summary>
    /// <param name="requiredLength">The exact number of elements required.</param>
    /// <param name="arg">The argument to verify.</param>
    /// <param name="argName">The name of the argument (usually obtained using the nameof operator).</param>
    /// <exception cref="ArgumentNullException">Thrown when the constraint is violated because the argument is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the constraint is violated.</exception>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void HasLength<T>(int requiredLength, IEnumerable<T> arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        var elementCount = arg?.Count()
            ?? throw new ArgumentNullException(argName, $"The input must not be null and must be of length {requiredLength}!");

        if (elementCount != requiredLength)
        {
            throw new ArgumentException($"The input must be of length {requiredLength}!", argName);
        }
    }

    /// <summary>
    /// Verifies the argument contains exactly a given number of elements.
    /// </summary>
    /// <param name="requiredLength">The exact number of elements required.</param>
    /// <param name="arg">The argument to verify.</param>
    /// <param name="argName">The name of the argument (usually obtained using the nameof operator).</param>
    /// <exception cref="ArgumentException">Thrown when the constraint is violated.</exception>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void HasLength<T>(int requiredLength, Span<T> arg, [CallerArgumentExpression("arg")] string argName = null)
        => HasLength(requiredLength, (ReadOnlySpan<T>)arg, argName);

    /// <summary>
    /// Verifies the argument contains exactly a given number of elements.
    /// </summary>
    /// <param name="requiredLength">The exact number of elements required.</param>
    /// <param name="arg">The argument to verify.</param>
    /// <param name="argName">The name of the argument (usually obtained using the nameof operator).</param>
    /// <exception cref="ArgumentException">Thrown when the constraint is violated.</exception>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void HasLength<T>(int requiredLength, ReadOnlySpan<T> arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        if (arg.Length != requiredLength)
        {
            throw new ArgumentException($"The input span must be of length {requiredLength}!", argName);
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
    public static void IsNot<T>(T invalidValue, T arg, [CallerArgumentExpression("arg")] string argName = null)
        where T : IComparable
    {
        if (ReferenceEquals(arg, invalidValue) || arg.CompareTo(invalidValue) == 0)
        {
            throw new ArgumentException($"The input value must not be {invalidValue}!", argName);
        }
    }

    /// <summary>
    /// Verifies the argument is not negative.
    /// </summary>
    /// <param name="arg">The argument to verify.</param>
    /// <param name="argName">The name of the argument (usually obtained using the nameof operator).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the constraint is violated.</exception>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NotNegative(long arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        if (arg < 0)
        {
            throw new ArgumentOutOfRangeException(argName, "The input value must not be negative!");
        }
    }

    /// <summary>
    /// Verifies the argument is greater than 0.
    /// </summary>
    /// <param name="arg">The argument to verify.</param>
    /// <param name="argName">The name of the argument (usually obtained using the nameof operator).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the constraint is violated.</exception>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GreaterThanZero(long arg, [CallerArgumentExpression("arg")] string argName = null)
    {
        if (arg <= 0)
        {
            throw new ArgumentOutOfRangeException(argName, "The input value must be greater than zero!");
        }
    }

    /// <summary>
    /// Verifies that the <see cref="IValidatableObject"/> argument is valid.
    /// </summary>
    /// <param name="validatableArg">The argument to verify.</param>
    /// <param name="argName">The name of the argument (usually obtained using the nameof operator).</param>
    /// <param name="isNullValid">Whether or not to treat null as a valid value for the argument.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the argument is null and <paramref name="isNullValid"/> is false.
    /// </exception>
    /// <exception cref="ArgumentException">Thrown when the argument is invalid.</exception>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void IsValid(
        [ValidatedNotNull] IValidatableObject validatableArg,
        [CallerArgumentExpression("validatableArg")] string argName = null,
        bool isNullValid = false)
    {
        if (validatableArg is null)
        {
            if (isNullValid)
            {
                return;
            }

            throw new ArgumentNullException(argName);
        }

        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(
            validatableArg,
            new ValidationContext(validatableArg),
            validationResults);

        if (!isValid)
        {
            throw new ArgumentException(
                $"The input is invalid. Validation errors:{Environment.NewLine}{string.Join(Environment.NewLine, validationResults)}",
                argName);
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
