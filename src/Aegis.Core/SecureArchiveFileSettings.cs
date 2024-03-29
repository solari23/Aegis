﻿using System.ComponentModel.DataAnnotations;

namespace Aegis.Core;

/// <summary>
/// Settings for where the <see cref="SecureArchive"/> and related files are stored.
/// </summary>
public class SecureArchiveFileSettings : IValidatableObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SecureArchiveFileSettings"/> class.
    /// </summary>
    /// <param name="archiveFilePath">The path to the <see cref="SecureArchive"/> on disk.</param>
    /// <param name="tempDirectory">The path to the directory where secured files can be temporarily checked out.</param>
    public SecureArchiveFileSettings(string archiveFilePath, string tempDirectory)
    {
        ArgCheck.NotEmpty(archiveFilePath);
        ArgCheck.NotEmpty(tempDirectory);

        this.Path = archiveFilePath;
        this.TempDirectory = tempDirectory;
    }

    /// <summary>
    /// Gets or sets the path to the <see cref="SecureArchive"/> on disk.
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// Gets or sets the path to the directory where secured files can be temporarily checked out.
    /// </summary>
    public string TempDirectory { get; set; }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        if (string.IsNullOrWhiteSpace(this.Path))
        {
            yield return new ValidationResult(
                $"Property {nameof(this.Path)} can't be empty.");
        }

        if (string.IsNullOrWhiteSpace(this.TempDirectory))
        {
            yield return new ValidationResult(
                $"Property {nameof(this.TempDirectory)} can't be empty.");
        }
    }
}
