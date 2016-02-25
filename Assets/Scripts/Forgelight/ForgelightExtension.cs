using System.IO;
using Forgelight.Formats.Zone;
using Forgelight.Terrain;
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

                    instance.Initialize();
                }

                return instance;
            }
        }

        public static string StatePath
        {
            get { return Application.dataPath + "/Forgelight/state.json"; }
        }

        private JObject extensionState;

        //Asset Cache/Loading
        public ForgelightGameFactory ForgelightGameFactory { get; private set; }

        //Terrain Importing
        public TerrainLoader TerrainLoader { get; private set; }

        //Zone Editing
        public ZoneObjectFactory ZoneObjectFactory { get; private set; }

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
            EditorApplication.update += instance.EditorUpdate;
            instance = this;

            //Create objects
            ForgelightGameFactory = new ForgelightGameFactory();
            TerrainLoader = new TerrainLoader();
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

            extensionState = null;
            if (!File.Exists(StatePath))
            {
                extensionState = GetDefaultState();
                WriteStateToDisk();
            }

            if (extensionState == null)
            {
                extensionState = JObject.Parse(File.ReadAllText(@StatePath));
            }

            ForgelightGameFactory.Initialize((JArray)extensionState["forgelight_games"]);
        }

        private JObject GetDefaultState()
        {
            JObject retval = new JObject();

            JObject extension = new JObject();
            extension.Add("version", "1.0");
            extension.Add("update_url", "http://blackfeatherproductions.com/version.txt");

            retval.Add("extension", extension);

            JArray forgelight_games = new JArray();
            retval.Add("forgelight_games", forgelight_games);

            return retval;
        }

        public void SaveNewForgelightGame(ForgelightGame forgelightGame)
        {
            JObject gameElement = new JObject();

            gameElement.Add("alias", forgelightGame.Alias);
            gameElement.Add("pack_directory", forgelightGame.PackDirectory);
            gameElement.Add("resource_directory", forgelightGame.ResourceDirectory);
            gameElement.Add("load", true);

            ((JArray) extensionState["forgelight_games"]).Add(gameElement);

            WriteStateToDisk();
        }

        public void WriteStateToDisk()
        {
            File.WriteAllText(@StatePath, extensionState.ToString());
        }
    }
}