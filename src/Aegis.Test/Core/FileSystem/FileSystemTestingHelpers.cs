using System;

using Aegis.Core.FileSystem;
using Aegis.Models;

namespace Aegis.Test.Core.FileSystem
{
    public static class FileSystemTestingHelpers
    {
        public static AegisFileInfo CreateFileInfo(string virtualFilePath, Guid? fileId = null)
            => new AegisFileInfo(
                new AegisVirtualFilePath(virtualFilePath),
                new FileIndexEntry
                {
                    FileId = fileId ?? Guid.NewGuid(),
                    FilePath = virtualFilePath,
                    AddedTime = DateTimeOffset.UtcNow,
                    LastModifiedTime = DateTimeOffset.UtcNow,
                });
    }
}
