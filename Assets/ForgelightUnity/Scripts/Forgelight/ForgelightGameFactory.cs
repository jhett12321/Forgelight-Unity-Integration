namespace ForgelightUnity.Forgelight
{
    using System.IO;
    using Newtonsoft.Json.Linq;
    using UnityEngine;
    using Utils;

    public class ForgelightGameFactory
    {
        //Active Forgelight Game
        public ForgelightGame ActiveForgelightGame { get; private set; }

        public void OpenForgelightGameFolder()
        {
            string path = DialogUtils.OpenDirectory(
            "Select folder containing Forgelight game files.",
            "",
            "", CheckGivenAssetDirectory);

            if (path != null)
            {
                LoadNewForgelightGame(path);
            }
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

            string alias = directoryInfo.Name;

            string resourceDirectory = Application.dataPath + "/Resources/" + alias;

            ForgelightGame forgelightGame = new ForgelightGame(alias, path, resourceDirectory);

            forgelightGame.LoadPackFiles(0.0f, 0.25f);
            forgelightGame.InitializeMaterialDefinitionManager();
            forgelightGame.ImportModels(0.3f, 0.6f);
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
            JObject info = ForgelightExtension.Instance.Config.GetForgelightGameInfo(name);

            string packDirectory = (string) info["pack_directory"];
            string resourceDirectory = (string) info["resource_directory"];

            ForgelightGame forgelightGame = new ForgelightGame(name, packDirectory, resourceDirectory);

            if (!Directory.Exists(resourceDirectory))
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
            if (ActiveForgelightGame != null)
            {
                ForgelightExtension.Instance.Config.UpdateForgelightGame(ActiveForgelightGame, false);
            }

            ActiveForgelightGame = newGame;
            ForgelightExtension.Instance.Config.UpdateForgelightGame(newGame, true);
            ForgelightMonoBehaviour.Instance.ForgelightGame = newGame.Name;
        }

        private static ValidationResult CheckGivenAssetDirectory(string path)
        {
            ValidationResult validationResult = new ValidationResult();

            path += "/Resources/Assets";

            string[] files = Directory.GetFiles(path);

            foreach (string fileName in files)
            {
                if (fileName.EndsWith(".pack"))
                {
                    validationResult.result = true;
                    validationResult.path = path;
                    return validationResult;
                }
            }

            validationResult.result = false;
            validationResult.errorTitle = "Invalid Asset Directory";
            validationResult.errorDesc = "The directory provided is not a valid Forgelight game. Please make sure to select the root game directory (not the asset folder) and try again.";

            return validationResult;
        }
    }
}