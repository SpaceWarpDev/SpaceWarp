using System;
using System.IO;

namespace SpaceWarp.InternalUtilities;

internal static class PathHelpers
{
    /// <summary>
    ///     Creates a relative path from one file or folder to another.
    /// </summary>
    /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
    /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
    /// <returns>The relative path from the start directory to the end path.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="fromPath" /> or <paramref name="toPath" /> is <c>null</c>.</exception>
    /// <exception cref="UriFormatException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    internal static string GetRelativePath(string fromPath, string toPath)
    {
        if (string.IsNullOrEmpty(fromPath))
        {
            throw new ArgumentNullException(nameof(fromPath));
        }

        if (string.IsNullOrEmpty(toPath))
        {
            throw new ArgumentNullException(nameof(toPath));
        }

        var fromUri = new Uri(AppendDirectorySeparatorChar(fromPath));
        var toUri = new Uri(AppendDirectorySeparatorChar(toPath));

        if (fromUri.Scheme != toUri.Scheme)
        {
            return toPath;
        }

        var relativeUri = fromUri.MakeRelativeUri(toUri);
        var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

        if (string.Equals(toUri.Scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase))
        {
            relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        return relativePath;
    }

    private static string AppendDirectorySeparatorChar(string path)
    {
        // Append a slash only if the path is a directory and does not have a slash.
        if (!Path.HasExtension(path) && !path.EndsWith(Path.DirectorySeparatorChar.ToString()))
        {
            return path + Path.DirectorySeparatorChar;
        }

        return path;
    }
}