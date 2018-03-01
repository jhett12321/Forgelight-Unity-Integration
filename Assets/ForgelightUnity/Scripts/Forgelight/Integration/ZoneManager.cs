namespace ForgelightUnity.Forgelight.Integration
{
    using System.IO;
    using Assets;
    using Assets.Areas;
    using Assets.Zone;
    using UnityEditor;
    using UnityEngine;

    public class ZoneManager
    {
        public Zone LoadedZone { get; private set; }

        //Factories
        public AreaObjectFactory AreaObjectFactory;
        public TerrainFactory TerrainFactory;
        public ZoneLightFactory ZoneLightFactory;
        public ZoneObjectFactory ZoneObjectFactory;

        public ZoneManager()
        {
            AreaObjectFactory = new AreaObjectFactory();
            TerrainFactory = new TerrainFactory();
            ZoneLightFactory = new ZoneLightFactory();
            ZoneObjectFactory = new ZoneObjectFactory();
        }

        public void ChangeZone(ForgelightGame forgelightGame, Zone zone)
        {
            //Destroy any remaining zone data.
            DestroyActiveZone();

            //Load the new zone data.
            LoadedZone = zone;
            string zoneName = Path.GetFileNameWithoutExtension(zone.Name);

            ZoneObjectFactory.LoadZoneObjects(forgelightGame, zoneName, zone.Objects, 0.0f, 0.50f);
            ZoneLightFactory.LoadZoneLights(zoneName, zone.Lights, 0.50f, 0.75f);

            //TODO ecos
            //TODO floras
            //TODO invisible walls

            TerrainFactory.LoadTerrain(forgelightGame, Path.GetFileNameWithoutExtension(zone.Name), 0.75f, 0.9f);

            //Attempt to guess this zone's area definitions file.
            string areasName = Path.GetFileNameWithoutExtension(zone.Name) + "Areas.xml";

            foreach (Asset areaDefinition in forgelightGame.AvailableAreaDefinitions)
            {
                if (areaDefinition.Name == areasName)
                {
                    AreaObjectFactory.LoadAreaDefinitions((Areas) areaDefinition, 0.9f, 1.0f);
                    break;
                }
            }

            //Unload any unused assets.
            Resources.UnloadUnusedAssets();

            EditorUtility.ClearProgressBar();
        }

        public void DestroyActiveZone()
        {
            //Destroy the current zone, and related data.
            ZoneObjectFactory.DestroyAllObjects();
            ZoneLightFactory.DestroyAllLights();

            TerrainFactory.DestroyTerrain();
            AreaObjectFactory.DestroyAreas();
        }

        /// <summary>
        /// Merges the current scene into the loaded Zone instance.
        /// </summary>
        public void ApplySceneChangesToZone()
        {
            EditorUtility.DisplayProgressBar("Exporting Zone", "Exporting Zone, please wait...", 0.0f);

            ZoneObjectFactory.WriteToZone(LoadedZone);

            EditorUtility.DisplayProgressBar("Exporting Zone", "Exporting Zone, please wait...", 0.5f);

            //TODO ecos
            //TODO floras
            //TODO invisible walls

            ZoneLightFactory.WriteToZone(LoadedZone);

            //TODO unknowns

            EditorUtility.ClearProgressBar();
        }
    }
}
