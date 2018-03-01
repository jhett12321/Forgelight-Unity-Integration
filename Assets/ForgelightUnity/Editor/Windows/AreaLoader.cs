namespace ForgelightUnity.Editor.Windows
{
    using System.Collections.Generic;
    using Forgelight;
    using Forgelight.Assets;
    using Forgelight.Assets.Areas;
    using Forgelight.Utils;
    using UnityEditor;
    using UnityEngine;

    public class AreaLoader : EditorWindow
    {
        private string searchString = "";
        private Vector2 scroll;

        private Asset selectedAreas;

        public static void Init()
        {
            GetWindow(typeof(AreaLoader), false, "Areas");
        }

        private void OnGUI()
        {
            //Search Box
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.FlexibleSpace();
            GUILayout.Label("Search: ", EditorStyles.toolbarButton);
            searchString = GUILayout.TextField(searchString, EditorStyles.toolbarTextField, GUILayout.MinWidth(200));

            GUILayout.EndHorizontal();

            //Zone List
            EditorGUILayout.BeginHorizontal();
            {
                scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(500));
                {
                    ForgelightGame activeForgelightGame = ForgelightExtension.Instance.ForgelightGameFactory.ActiveForgelightGame;

                    if (activeForgelightGame != null)
                    {
                        List<Asset> areas = activeForgelightGame.AvailableAreaDefinitions;

                        ShowAvailableAreaDefs(activeForgelightGame, areas);
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ShowAvailableAreaDefs(ForgelightGame forgelightGame, List<Asset> availableAreas)
        {
            foreach (Asset asset in availableAreas)
            {
                if (searchString != null && !asset.DisplayName.ToLower().Contains(searchString.ToLower()))
                {
                    continue;
                }

                Areas areaDef = (Areas) asset;

                Rect rect = GUILayoutUtility.GetRect(40f, 40f, 16f, 16f, EditorStyles.label);

                if (Event.current.type == EventType.MouseDown)
                {
                    if (rect.Contains(Event.current.mousePosition))
                    {
                        if (selectedAreas != null && selectedAreas == areaDef)
                        {
                            OnAreasSelected(areaDef);
                            selectedAreas = null;
                        }

                        selectedAreas = areaDef;
                    }
                }

                GUIStyle style = EditorStyles.label;
                style.fixedWidth = 0;
                style.stretchWidth = true;
                style.clipping = TextClipping.Overflow;

                EditorGUI.Foldout(rect, false, asset.DisplayName, true, style);
            }
        }

        private void OnAreasSelected(Areas areas)
        {
            if (DialogUtils.DisplayCancelableDialog("Changing Area Definitions", "You have selected a new area definitions file. This will replace any areas currently loaded. Are you sure you wish to continue?"))
            {
                ForgelightExtension.Instance.ZoneManager.AreaObjectFactory.DestroyAreas();
                ForgelightExtension.Instance.ZoneManager.AreaObjectFactory.LoadAreaDefinitions(areas, 0.0f, 1.0f);
            }
        }
    }
}
