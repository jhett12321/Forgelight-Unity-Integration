namespace ForgelightUnity.Editor.Utils
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using ScriptableObjects;
    using UnityEditor;
    using UnityEngine;

    public enum SortMode
    {
        None,
        CreationTime,
        ModifiedTime,
        Path
    }

    public struct ScriptableObjectInfo<T> where T : ScriptableObject
    {
        public readonly string Path;
        public readonly T ScriptableObject;
        public readonly DateTime CreationTime;
        public readonly DateTime ModifiedTime;

        public ScriptableObjectInfo(string path, T scriptableObject)
        {
            Path = path;
            ScriptableObject = scriptableObject;

            string fullPath = ProjectFileUtils.GetFullPathFromProjectRelativePath(path);
            CreationTime = File.GetCreationTime(fullPath);
            ModifiedTime = File.GetLastWriteTime(fullPath);
        }
    }

    public class ScriptableObjectUtils
    {
        /// <summary>
        /// Gets the global instance of the given ScriptableObject.
        /// The Object must have the "GlobalUnique" attribute.
        /// </summary>
        /// <returns>the global instance of the given ScriptableObject type.</returns>
        public static T GetGlobalScriptableObject<T>() where T : ScriptableObject
        {
            return GetGlobalScriptableObject<T>(null);
        }

        /// <summary>
        /// Gets the global instance of the given ScriptableObject.
        /// The Object must have the "GlobalUnique" attribute.
        /// </summary>
        /// <returns>the global instance of the given ScriptableObject type.</returns>
        public static ScriptableObject GetGlobalScriptableObject(Type type)
        {
            return GetGlobalScriptableObject<ScriptableObject>(type);
        }

        private static T GetGlobalScriptableObject<T>(Type type) where T : ScriptableObject
        {
            if (type == null)
            {
                type = typeof(T);
            }
            else if (typeof(T) != typeof(ScriptableObject))
            {
                throw new ArgumentException();
            }

            if (!type.HasAttribute<GlobalUnique>())
            {
                throw new ArgumentException("The scriptable object type provided is not globally unique!\n" +
                                            "Check the type specified, or add the GlobalUnique attribute to the scriptable object if it is globally unique.");
            }

            ScriptableObjectInfo<T>[] existingObjects = FindSavedScriptableObjects<T>(type);

            return existingObjects.Length == 0 ? null : existingObjects[0].ScriptableObject;
        }

        /// <summary>
        /// Generates/Updates a Path for a ScriptableObject relative to the project directory.
        /// </summary>
        private static string GenerateSavePath<T>(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = typeof(T).HasAttribute<DefaultAssetPath>() ? typeof(T).GetAttribute<DefaultAssetPath>().Path : "Assets";
            }

            if (string.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path = path + "/" + typeof(T).Name + ".asset";
            }

            // Make sure the parent directories exist.
            Directory.GetParent(ProjectFileUtils.GetFullPathFromProjectRelativePath(path)).Create();
            AssetDatabase.Refresh();

            return AssetDatabase.GenerateUniqueAssetPath(path);
        }

        /// <summary>
        /// Creates a scriptable object of the given type.
        /// </summary>
        /// <typeparam name="T">The ScriptableObject to create.</typeparam>
        /// <param name="path">The asset path to use. If not defined, uses the script's defined DefaultAssetPath, otherwise defaults to the Asset Directory.</param>
        /// <returns></returns>
        public static ScriptableObjectInfo<T> CreateScriptableObject<T>(string path = null) where T : ScriptableObject
        {
            path = GenerateSavePath<T>(path);
            T asset = ScriptableObject.CreateInstance<T>();

            AssetDatabase.CreateAsset(asset, path);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return new ScriptableObjectInfo<T>(path, asset);
        }

        /// <summary>
        /// Finds all scriptable objects in the project of the given type.
        /// </summary>
        /// <typeparam name="T">The scriptable object type.</typeparam>
        /// <param name="sortMode">An optional sort mode that can query creation, and modified time.</param>
        /// <param name="paths">Asset paths to search relative to the project directory (e.g. Assets/Soulbound)</param>
        /// <returns>A array containing all matching instances of the provided scriptable object.</returns>
        public static ScriptableObjectInfo<T>[] FindSavedScriptableObjects<T>(SortMode sortMode = SortMode.None, params string[] paths) where T : ScriptableObject
        {
            return FindSavedScriptableObjects<T>(null, sortMode, paths);
        }

        /// <summary>
        /// Finds all scriptable objects in the project of the given type.
        /// </summary>
        /// <param name="sortMode">An optional sort mode that can query creation, and modified time.</param>
        /// <param name="type">The scriptable object type.</param>
        /// <param name="paths">Asset paths to search relative to the project directory (e.g. Assets/Soulbound)</param>
        /// <returns>A array containing all matching instances of the provided scriptable object.</returns>
        public static ScriptableObjectInfo<ScriptableObject>[] FindSavedScriptableObjects(Type type, SortMode sortMode = SortMode.None, params string[] paths)
        {
            return FindSavedScriptableObjects<ScriptableObject>(type, sortMode, paths);
        }

        private static ScriptableObjectInfo<T>[] FindSavedScriptableObjects<T>(Type type, SortMode sortMode = SortMode.None, params string[] paths) where T : ScriptableObject
        {
            if (type == null || type == typeof(T))
            {
                type = typeof(T);
            }
            else if (typeof(T) != typeof(ScriptableObject))
            {
                throw new ArgumentException();
            }

            string[] guids = paths == null ? AssetDatabase.FindAssets("t:" + type) : AssetDatabase.FindAssets("t:" + type, paths);
            List<ScriptableObjectInfo<T>> scriptableObjects = new List<ScriptableObjectInfo<T>>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                T scriptableObject = (T)AssetDatabase.LoadAssetAtPath(path, type);

                if (scriptableObject != null)
                {
                    ScriptableObjectInfo<T> prefabInfo = new ScriptableObjectInfo<T>(path, scriptableObject);
                    scriptableObjects.Add(prefabInfo);
                }
            }

            switch (sortMode)
            {
                case SortMode.CreationTime:
                    scriptableObjects.Sort((prefabInfo1, prefabInfo2) => prefabInfo1.CreationTime.CompareTo(prefabInfo2.CreationTime));
                    break;
                case SortMode.ModifiedTime:
                    scriptableObjects.Sort((prefabInfo1, prefabInfo2) => prefabInfo1.ModifiedTime.CompareTo(prefabInfo2.ModifiedTime));
                    break;
                case SortMode.Path:
                    scriptableObjects.Sort((prefabInfo1, prefabInfo2) => string.Compare(prefabInfo1.Path, prefabInfo2.Path, StringComparison.Ordinal));
                    break;
            }

            if (scriptableObjects.Count > 1 && type.HasAttribute<GlobalUnique>())
            {
                Debug.LogError("The ScriptableObject " + type.Name + " is globally unique, but 2 instances were found in the project! This should be resolved, or you may experience undesired behaviour.");
                Debug.LogError("Paths: ");
                scriptableObjects.ForEach(so => Debug.Log(so.Path));
            }

            return scriptableObjects.ToArray();
        }
    }
}