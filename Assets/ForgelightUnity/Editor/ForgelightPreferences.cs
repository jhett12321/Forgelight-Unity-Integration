namespace ForgelightUnity.Editor
{
    using UnityEditor;
    using UnityEngine;

    public class ForgelightPreferences : Editor
    {
        // Are prefs loaded?
        private static bool prefsLoaded = false;

        // Settings
        public static int CullingDistance = 1000;

        // Add preferences section named "My Preferences" to the Preferences Window
        [PreferenceItem("Forgelight")]

        public static void PreferencesGUI()
        {
            // Load the preferences
            if (!prefsLoaded)
            {
                CullingDistance = EditorPrefs.GetInt("ForgelightCullDistance", 1000);
                prefsLoaded = true;
            }

            // Preferences GUI
            CullingDistance = EditorGUILayout.IntField("Cull from position distance", CullingDistance);

            // Save the preferences
            if (GUI.changed)
                EditorPrefs.SetInt("ForgelightCullDistance", CullingDistance);
        }
    }
}
