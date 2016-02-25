using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Forgelight.Formats.Zone;
using UnityEditor;
using Object = Forgelight.Formats.Zone.Object;

namespace Forgelight.Editor.Windows
{
    public class ZoneLoader : EditorWindow
    {
        private bool running = false;

        private const int indent = 1;

        private string searchString = "";
        private Dictionary<ForgelightGame, bool> openElements = new Dictionary<ForgelightGame, bool>();
        private Vector2 scroll;

        private Formats.Zone.Zone selectedZone;

        [MenuItem("Forgelight/Windows/ZoneLoader")]
        public static void Init()
        {
            EditorWindow.GetWindow(typeof(ZoneLoader));
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
                    foreach (ForgelightGame forgelightGame in ForgelightExtension.Instance.ForgelightGameFactory.forgelightGames.Values)
                    {
                        if (!openElements.ContainsKey(forgelightGame))
                        {
                            openElements[forgelightGame] = false;
                        }

                        ShowGameZones(forgelightGame, forgelightGame.AvailableZones.Values.ToList());
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndHorizontal();

            //Preview Box
            //GameObject model = Resources.Load();
        }

        private void ShowGameZones(ForgelightGame forgelightGame, List<Formats.Zone.Zone> availableZones)
        {
            // Show entry for parent object
            openElements[forgelightGame] = EditorGUILayout.Foldout(openElements[forgelightGame], forgelightGame.Alias);

            if (openElements[forgelightGame])
            {
                ShowAvailableActors(forgelightGame, availableZones);
            }
        }

        private void ShowAvailableActors(ForgelightGame forgelightGame, List<Formats.Zone.Zone> availableZones)
        {
            foreach (Formats.Zone.Zone zone in availableZones)
            {
                if (searchString == null || zone.Name.ToLower().Contains(searchString.ToLower()))
                {
                    EditorGUI.indentLevel += indent;

                    Rect position = GUILayoutUtility.GetRect(40f, 40f, 16f, 16f, EditorStyles.label);

                    if (Event.current.type == EventType.MouseDown)
                    {
                        if (position.Contains(Event.current.mousePosition))
                        {
                            if (selectedZone != null && selectedZone == zone)
                            {
                                LoadZone(forgelightGame, zone);
                                selectedZone = null;
                            }

                            selectedZone = zone;
                        }
                    }

                    GUIStyle style = EditorStyles.label;
                    style.fixedWidth = 0;
                    style.stretchWidth = true;
                    style.clipping = TextClipping.Overflow;

                    EditorGUI.Foldout(position, false, zone.Name, true, style);

                    EditorGUI.indentLevel -= indent;
                }
            }
        }

        public void LoadZone(ForgelightGame forgelightGame, Formats.Zone.Zone zone)
        {
            running = true;

            int totalObjects = 0;
            int objectsProcessed = 0;

            //Calculate the total objects we need to process.
            foreach (Object zoneObject in zone.Objects)
            {
                totalObjects += zoneObject.Instances.Count();
            }

            ZoneObjectFactory ZoneObjectFactory = ForgelightExtension.Instance.ZoneObjectFactory;

            //Begin processing the file
            foreach (Object zoneObject in zone.Objects)
            {
                if (running && zoneObject.Instances.Count() > 0)
                {
                    foreach (Object.Instance instance in zoneObject.Instances)
                    {
                        ZoneObjectFactory.CreateForgelightObject(forgelightGame, zoneObject.ActorDefinition, ConvertForgelightPosition(instance.Position), ConvertForgelightRotation(instance.Rotation), ConvertForgelightScale(instance.Scale), zoneObject.RenderDistance, instance.LODMultiplier, instance.DontCastShadows, instance.ID);
                        objectsProcessed++;
                    }
                }

                else if(!running)
                {
                    OnLoadComplete();
                    return;
                }

                ProgressBar((float)totalObjects / (float)objectsProcessed, zoneObject.ActorDefinition);
            }

            ZoneObjectFactory.transform.localScale = new Vector3(-1, 1, 1);

            //Unload any unused assets.
            OnLoadComplete();
        }

        public void OnLoadComplete()
        {
            running = false;

            //Unload any unused assets.
            Resources.UnloadUnusedAssets();

            EditorUtility.ClearProgressBar();
        }

        private void ProgressBar(float progress, string currentTask)
        {
            if (EditorUtility.DisplayCancelableProgressBar("Loading Zone", currentTask, progress))
            {
                OnLoadComplete();
            }
        }

        private Vector3 ConvertForgelightPosition(Vector4 fPos)
        {
            return new Vector3(fPos.x, fPos.y, fPos.z);
        }

        private Quaternion ConvertForgelightRotation(Vector4 fRot)
        {
            return Quaternion.Euler(fRot.y * Mathf.Rad2Deg, fRot.x * Mathf.Rad2Deg, fRot.z * Mathf.Rad2Deg);
        }

        private Vector3 ConvertForgelightScale(Vector4 fSca)
        {
            return new Vector3(-fSca.x, fSca.y, fSca.z);
        }
    }
}
