using Forgelight.Terrain;
using Forgelight.Zone;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Forgelight
{
    public class ForgelightExtension : MonoBehaviour
    {
        //Singleton
        private static ForgelightExtension instance = null;
        public static ForgelightExtension Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = (ForgelightExtension)FindObjectOfType(typeof(ForgelightExtension));

                    if (instance == null)
                    {
                        instance = new GameObject("Forgelight Editor").AddComponent<ForgelightExtension>();
                    }

                    EditorApplication.update += instance.EditorUpdate;

                    instance.Initialize();
                }

                return instance;
            }
        }

        //Asset Cache/Loading
        public AssetLoader AssetLoader { get; private set; }

        //Terrain Importing
        public TerrainLoader TerrainLoader { get; private set; }

        //Zone Editing
        public ZoneObjectFactory ZoneObjectFactory { get; private set; }

        //Zone Importing
        public ZoneLoader ZoneLoader { get; private set; }

        //Zone Exporting
        public ZoneExporter ZoneExporter { get; private set; }

        //Editor
        public Vector3 lastCameraPos = new Vector3();

        //TODO Implement .zone Conversion

        private void EditorUpdate()
        {
            if (instance == null)
            {
                instance = this;
            }

            if (Camera.current != null)
            {
                lastCameraPos = Camera.current.transform.position;
            }
        }

        private void Initialize()
        {
            instance = this;

            //Create objects
            AssetLoader = new AssetLoader();
            TerrainLoader = new TerrainLoader();
            ZoneLoader = new ZoneLoader();
            ZoneExporter = new ZoneExporter();

            //Zone Object Factory
            GameObject parent = GameObject.FindWithTag("ZoneObjects");

            if (parent == null)
            {
                parent = new GameObject("Forgelight Objects");
                parent.tag = "ZoneObjects";

                ZoneObjectFactory = parent.AddComponent<ZoneObjectFactory>();
            }
            else
            {
                ZoneObjectFactory = parent.GetComponent<ZoneObjectFactory>();

                if (ZoneObjectFactory == null)
                {
                    ZoneObjectFactory = parent.AddComponent<ZoneObjectFactory>();
                }
            }

            //Load saved state information. Create a new state file if it currently does not exist.
            JObject state = new JObject();
        }
    }
}