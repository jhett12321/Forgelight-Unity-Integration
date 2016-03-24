using Forgelight.Attributes;
using Forgelight.Formats.Cnk;
using Forgelight.Formats.Zone;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
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

        //Zone Manager
        public ZoneManager ZoneManager { get; private set; }

        //Chunk Loading
        public ChunkLoader ChunkLoader { get; private set; }

        //Asset Cache/Loading
        public ForgelightGameFactory ForgelightGameFactory { get; private set; }

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
        public bool cameraPosChanged { get; private set; }

        static ForgelightExtension()
        {
            if (Instance == null)
            {
                Instance = new ForgelightExtension
                {
                    ForgelightGameFactory = new ForgelightGameFactory(),
                    ChunkLoader = new ChunkLoader(),
                    ZoneExporter = new ZoneExporter(),
                    Config = new Config(),
                    ZoneManager = new ZoneManager()
                };

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
                if (LastCameraPos != Camera.current.transform.position)
                {
                    LastCameraPos = Camera.current.transform.position;
                    cameraPosChanged = true;
                }
                else
                {
                    cameraPosChanged = false;
                }
            }
        }

        private void Initialize()
        {
            if (EditorSceneManager.loadedSceneCount > 0 && SceneManager.GetActiveScene().name == lastScene)
            {
                return;
            }

            lastScene = SceneManager.GetActiveScene().name;

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