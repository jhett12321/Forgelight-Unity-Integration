namespace ForgelightUnity.Editor.Utils
{
    using System;
    using System.IO;
    using UnityEngine;

    public class ProjectFileUtils
    {
        /// <summary>
        /// Gets the root project directory.
        /// </summary>
        public static string GetProjectDirectory()
        {
            return Directory.GetParent(Application.dataPath).ToString();
        }

        /// <summary>
        /// Gets the Assets directory inside the project root.
        /// </summary>
        /// <returns></returns>
        public static string GetAssetsDirectory()
        {
            return Application.dataPath;
        }

        /// <summary>
        /// Gets a full system path for a path relative to the root {Project} directory.
        /// </summary>
        /// <param name="relativePath">A relative path to {Project}</param>
        /// <returns></returns>
        public static string GetFullPathFromProjectRelativePath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(GetProjectDirectory(), relativePath));
        }

        /// <summary>
        /// Gets a full system path for a path relative to the {Project}/Assets directory.
        /// </summary>
        /// <param name="relativePath">A relative path to {Project}/Assets</param>
        /// <returns>The full system path.</returns>
        public static string GetFullPathFromAssetRelativePath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(GetAssetsDirectory(), relativePath));
        }

        /// <summary>
        /// Gets a relative path to {Project}/Assets from a full path.
        /// </summary>
        /// <param name="fullPath">The full path.</param>
        /// <returns>A relative path to {Project}/Assets</returns>
        public static string GetAssetRelativePathFromFullPath(string fullPath)
        {
            return MakeRelativePath(GetAssetsDirectory(), fullPath);
        }

        /// <summary>
        /// Gets a relative path to the root {Project} path from a full path.
        /// </summary>
        /// <param name="fullPath">The full path.</param>
        /// <returns>A relative path to {Project}</returns>
        public static string GetProjectRelativePathFromFullPath(string fullPath)
        {
            return MakeRelativePath(GetProjectDirectory(), fullPath);
        }

        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path or <c>toPath</c> if the paths are not related.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="UriFormatException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static String MakeRelativePath(String fromPath, String toPath)
        {
            if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
            if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }
    }
}