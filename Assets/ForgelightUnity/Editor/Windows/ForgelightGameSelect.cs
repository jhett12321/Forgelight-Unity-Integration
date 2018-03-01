namespace ForgelightUnity.Editor.Windows
{
    using System.Collections.Generic;
    using Forgelight;
    using Forgelight.Utils;
    using UnityEditor;
    using UnityEngine;

    public class ForgelightGameSelect : EditorWindow
    {
        private string selectedGame;
        private string searchString = "";

        private Vector2 scroll;

        public static void Init()
        {
            GetWindow(typeof(ForgelightGameSelect), false, "Games");
        }

        private void OnGUI()
        {
            //Search Box
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.FlexibleSpace();
            GUILayout.Label("Search: ", EditorStyles.toolbarButton);
            searchString = GUILayout.TextField(searchString, EditorStyles.toolbarTextField, GUILayout.MinWidth(200));

            GUILayout.EndHorizontal();

            //Game List
            EditorGUILayout.BeginHorizontal();
            {
                scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(500));
                {
                    ShowAvailableForgelightGames(ForgelightExtension.Instance.Config.GetAvailableForgelightGames());
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ShowAvailableForgelightGames(List<string> availableGames)
        {
            foreach (string gameName in availableGames)
            {
                if (searchString == null || gameName.ToLower().Contains(searchString.ToLower()))
                {
                    Rect rect = GUILayoutUtility.GetRect(40f, 40f, 16f, 16f, EditorStyles.label);

                    if (Event.current.type == EventType.MouseDown)
                    {
                        if (rect.Contains(Event.current.mousePosition))
                        {
                            if (selectedGame != null && selectedGame == gameName)
                            {
                                OnGameSelected(gameName);
                                selectedGame = null;
                            }

                            selectedGame = gameName;
                        }
                    }

                    GUIStyle style = EditorStyles.label;
                    style.fixedWidth = 0;
                    style.stretchWidth = true;
                    style.clipping = TextClipping.Overflow;

                    EditorGUI.Foldout(rect, false, gameName, true, style);
                }
            }
        }

        private void OnGameSelected(string gameName)
        {
            if (!DialogUtils.DisplayCancelableDialog("Change Forgelight Game", "You have selected a new Forgelight game. Changing games will DESTROY all objects and terrain in the current scene, and you will lose any unsaved changes. Are you sure you wish to continue?"))
            {
                return;
            }

            ForgelightExtension.Instance.ZoneManager.DestroyActiveZone();
            ForgelightExtension.Instance.ForgelightGameFactory.ChangeActiveForgelightGame(gameName);
        }
    }
}