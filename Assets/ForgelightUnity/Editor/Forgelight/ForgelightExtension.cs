namespace ForgelightUnity.Editor.Forgelight
{
    using Assets.Zone;
    using Integration;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;

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

        //Asset Cache/Loading
        public ForgelightGameFactory ForgelightGameFactory { get; private set; }

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

            //ChangeActiveForgelightGame the Active Game.
            string activeGame = null;
            ForgelightGameInfo activeGameInfo = null;

            //The data saved to the current scene.
            if (ForgelightMonoBehaviour.Instance.ForgelightGame != null)
            {
                activeGame = ForgelightMonoBehaviour.Instance.ForgelightGame;
            }

            if (!string.IsNullOrEmpty(activeGame))
            {
                activeGameInfo = Config.GetForgelightGameInfo(activeGame);
            }

            if (activeGameInfo == null)
            {
                activeGameInfo = Config.ForgelightEditorPrefs.ActiveForgelightGame;
            }

            if (activeGameInfo != null)
            {
                ForgelightGameFactory.ChangeActiveForgelightGame(activeGame);
            }
        }
    }
}