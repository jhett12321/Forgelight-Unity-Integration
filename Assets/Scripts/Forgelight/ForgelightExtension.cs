using System.Collections.Generic;
using System.IO;
using Forgelight.Attributes;
using Forgelight.Formats.Cnk;
using Forgelight.Formats.Zone;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Forgelight
{
    [InitializeOnLoad]
    public class ForgelightExtension
    {
        private string lastScene;

        //Singleton
        public static ForgelightExtension Instance { get; private set; }

        //State/Configuration
        public Config Config { get; private set; }


        //Asset Cache/Loading
        public ForgelightGameFactory ForgelightGameFactory { get; private set; }

        //Terrain Importing
        public TerrainLoader TerrainLoader { get; private set; }

        //Zone Editing
        private ZoneObjectFactory zoneObjectFactory;

        public ZoneObjectFactory ZoneObjectFactory
        {
            get
            {
                if (zoneObjectFactory == null)
                {
                    zoneObjectFactory = (ZoneObjectFactory)Object.FindObjectOfType(typeof(ZoneObjectFactory));

                    if (zoneObjectFactory == null)
                    {
                        zoneObjectFactory = new GameObject("Forgelight Zone Objects").AddComponent<ZoneObjectFactory>();
                    }
                }

                return zoneObjectFactory;
            }
        }

        //Zone Exporting
        public ZoneExporter ZoneExporter { get; private set; }

        //Editor
        public Vector3 LastCameraPos { get; private set; }

        static ForgelightExtension()
        {
            if (Instance == null)
            {
                Instance = new ForgelightExtension();

                //Create objects
                Instance.ForgelightGameFactory = new ForgelightGameFactory();
                Instance.TerrainLoader = new TerrainLoader();
                Instance.ZoneExporter = new ZoneExporter();
                Instance.Config = new Config();

                EditorApplication.update += Instance.EditorUpdate;
            }

            EditorApplication.hierarchyWindowChanged += Instance.Initialize;
        }

        private void EditorUpdate()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            if (Camera.current != null)
            {
                LastCameraPos = Camera.current.transform.position;
            }
        }

        private void Initialize()
        {
            if (EditorSceneManager.loadedSceneCount > 0 && EditorSceneManager.GetActiveScene().name == lastScene)
            {
                return;
            }

            lastScene = EditorSceneManager.GetActiveScene().name;

            //Initializes any games we have loaded in the past.
            Config.LoadSavedState();

            //ChangeActiveForgelightGame the Active Game.
            string activeGame = null;
            JToken activeGameInfos = null;

            //The data saved to the current scene.
            if (ForgelightMonoBehaviour.Instance.ForgelightGame != null)
            {
                activeGame = ForgelightMonoBehaviour.Instance.ForgelightGame;
            }

            if (activeGame != null)
            {
                activeGameInfos = Config.GetForgelightGameInfo(activeGame);
            }

            if (activeGameInfos == null)
            {
                string lastActiveConfigGame = Config.GetLastActiveForgelightGame();

                if (lastActiveConfigGame != null)
                {
                    activeGame = lastActiveConfigGame;
                    activeGameInfos = Config.GetForgelightGameInfo(activeGame);
                }
            }

            if (activeGameInfos != null)
            {
                ForgelightGameFactory.ChangeActiveForgelightGame(activeGame);
            }
        }
    }

    public class ForgelightMonoBehaviour : MonoBehaviour
    {
        private static ForgelightMonoBehaviour instance;
        public static ForgelightMonoBehaviour Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = (ForgelightMonoBehaviour)FindObjectOfType(typeof(ForgelightMonoBehaviour));

                    if (instance == null)
                    {
                        instance = new GameObject("Forgelight Editor").AddComponent<ForgelightMonoBehaviour>();
                    }
                }

                return instance;
            }
        }

        //Forgelight Game. Saved with the scene.
        [ReadOnly]
        public string ForgelightGame;
    }
}