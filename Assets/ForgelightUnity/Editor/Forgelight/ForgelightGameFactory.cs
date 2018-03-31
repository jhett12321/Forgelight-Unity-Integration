namespace ForgelightUnity.Editor.Forgelight
{
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using Utils;

    public class ForgelightGameFactory
    {
        private ForgelightGame activeForgelightGame;

        public ForgelightGame ActiveForgelightGame
        {
            get { return activeForgelightGame; }
            private set
            {
                activeForgelightGame = value;
                ForgelightExtension.Instance.Config.ForgelightEditorPrefs.ActiveForgelightGame = activeForgelightGame.GameInfo;
            }
        }

        public void OpenForgelightGameFolder()
        {
            string path = EditorUtility.OpenFolderPanel("Select Forgelight Game Folder", "", "");

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (path.EndsWith("Resources"))
            {
                path += "/Assets";
            }
            else if(!path.EndsWith("/Resources/Assets"))
            {
                path += "/Resources/Assets";
            }

            if (!IsAssetDirectory(path))
            {
                bool dialog = DialogUtils.DisplayCancelableDialog("Invalid Asset Directory", "The directory provided is not a valid Forgelight game. Please make sure to select the root game directory (not the asset folder) and try again.");
                if (dialog)
                {
                    OpenForgelightGameFolder();
                }

                return;
            }

            LoadNewForgelightGame(path);
        }

        /// <summary>
        /// Loads a new forgelight game that does not currently exist.
        /// </summary>
        /// <param name="path"></param>
        private void LoadNewForgelightGame(string path)
        {
            DirectoryInfo directoryInfo = Directory.GetParent(path).Parent;
            if (directoryInfo == null)
            {
                return;
            }

            string name = directoryInfo.Name;
            string resourceDirectory = "Resources/" + name;

            ForgelightGame forgelightGame = new ForgelightGame(new ForgelightGameInfo(name, path, resourceDirectory));

            forgelightGame.LoadPackFiles(0.0f, 0.05f);
            forgelightGame.InitializeMaterialDefinitionManager();
            forgelightGame.ImportModels(0.05f, 0.6f);
            forgelightGame.ImportTerrain(0.6f, 0.9f);
            forgelightGame.UpdateActors(0.9f, 0.93f);
            forgelightGame.UpdateZones(0.93f, 0.97f);
            forgelightGame.UpdateAreas(0.97f, 1.0f);

            forgelightGame.OnLoadComplete();
            ForgelightExtension.Instance.Config.SaveNewForgelightGame(forgelightGame);

            UpdateActiveForgelightGame(forgelightGame);
        }

        /// <summary>
        /// Deserializes and initializes the raw state data for the given forgelight game.
        /// Loads pack files into memory, and sets up references to required assets.
        /// </summary>
        public void ChangeActiveForgelightGame(string name)
        {
            ForgelightGameInfo info = ForgelightExtension.Instance.Config.GetForgelightGameInfo(name);

            ForgelightGame forgelightGame = new ForgelightGame(info);

            if (!Directory.Exists(info.FullResourceDirectory))
            {
                Debug.LogError("Could not find directory for game " + name + "!\n" +
                               "Please update Assets/Forgelight/state.json to the correct path, or remove the game from the file if it no-longer exists.");
                return;
            }

            forgelightGame.LoadPackFiles(0.0f, 0.7f);
            forgelightGame.InitializeMaterialDefinitionManager();
            forgelightGame.UpdateActors(0.7f, 0.8f);
            forgelightGame.UpdateZones(0.8f, 0.9f);
            forgelightGame.UpdateAreas(0.9f, 1.0f);

            forgelightGame.OnLoadComplete();

            UpdateActiveForgelightGame(forgelightGame);
        }

        private void UpdateActiveForgelightGame(ForgelightGame newGame)
        {
            ActiveForgelightGame = newGame;
            ForgelightMonoBehaviour.Instance.ForgelightGame = newGame.GameInfo.Name;
        }

        private bool IsAssetDirectory(string path) => Directory.Exists(path) && Directory.GetFiles(path).Any(fileName => fileName.EndsWith(".pack"));
    }
}