namespace ForgelightUnity.Editor.Windows
{
    using System.Collections.Generic;
    using Forgelight;
    using Forgelight.Assets;
    using Forgelight.Assets.Zone;
    using Forgelight.Utils;
    using UnityEditor;
    using UnityEngine;

    public class ZoneLoader : EditorWindow
    {
        private string searchString = "";
        private Vector2 scroll;

        private Zone selectedZone;

        public static void Init()
        {
            GetWindow(typeof(ZoneLoader), false, "Zones");
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
                        List<Asset> zones = activeForgelightGame.AvailableZones;

                        ShowAvailableZones(activeForgelightGame, zones);
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ShowAvailableZones(ForgelightGame forgelightGame, List<Asset> availableZones)
        {
            foreach (Asset asset in availableZones)
            {
                if (searchString != null && !asset.DisplayName.ToLower().Contains(searchString.ToLower()))
                {
                    continue;
                }

                Zone zone = (Zone) asset;

                Rect rect = GUILayoutUtility.GetRect(40f, 40f, 16f, 16f, EditorStyles.label);

                if (Event.current.type == EventType.MouseDown)
                {
                    if (rect.Contains(Event.current.mousePosition))
                    {
                        if (selectedZone != null && selectedZone == zone)
                        {
                            OnZoneSelected(forgelightGame, zone);
                            selectedZone = null;
                        }

                        selectedZone = zone;
                    }
                }

                GUIStyle style = EditorStyles.label;
                style.fixedWidth = 0;
                style.stretchWidth = true;
                style.clipping = TextClipping.Overflow;

                EditorGUI.Foldout(rect, false, asset.DisplayName, true, style);
            }
        }

        private void OnZoneSelected(ForgelightGame forgelightGame, Zone zone)
        {
            if (DialogUtils.DisplayCancelableDialog("Changing Zone", "You have selected a new zone. Changing zones will DESTROY all objects and terrain in the current scene, and you will lose any unsaved changes. Are you sure you wish to continue?"))
            {
                ForgelightExtension.Instance.ZoneManager.ChangeZone(forgelightGame, zone);
            }
        }
    }
}
