using System.Collections.Generic;
using System.IO;
using Forgelight.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Forgelight
{
    public class ForgelightGameFactory
    {
        public Dictionary<string, ForgelightGame> forgelightGames { get; private set; }

        public ForgelightGameFactory()
        {
            forgelightGames = new Dictionary<string, ForgelightGame>();
        }

        public ForgelightGame CreateForgelightGame(string alias, string packDirectory, string resourceDirectory)
        {
            if (!forgelightGames.ContainsKey(alias))
            {
                forgelightGames[alias] = new ForgelightGame(alias, packDirectory, resourceDirectory);
            }

            return forgelightGames[alias];
        }

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

        private void LoadNewForgelightGame(string path)
        {
            string alias = Directory.GetParent(path).Parent.Name;
            string resourceDirectory = Application.dataPath + "/Resources/" + alias;

            ForgelightGame forgelightGame = CreateForgelightGame(alias, path, resourceDirectory);

            forgelightGame.LoadPackFiles(0.0f, 0.25f);
            forgelightGame.InitializeMaterialDefinitionManager();
            forgelightGame.ExportModels(0.3f, 0.5f);
            forgelightGame.UpdateActors(0.5f, 0.7f);
            //TODO Export Terrain
            forgelightGame.ExportTerrain(0.7f, 0.9f);
            forgelightGame.UpdateZones(0.9f, 1.0f);

            forgelightGame.OnLoadComplete();
            ForgelightExtension.Instance.SaveNewForgelightGame(forgelightGame);
        }

        /// <summary>
        /// Called at initial startup of extension.
        /// Loads pack files into memory, and sets up references to required assets.
        /// </summary>
        public void Initialize(JArray forgelightGames)
        {
            foreach (JToken asset in forgelightGames)
            {
                string alias = (string) asset["alias"];
                bool load = (bool) asset["load"];
                string packDirectory = (string) asset["pack_directory"];
                string resourceDirectory = (string) asset["resource_directory"];

                if (load)
                {
                    ForgelightGame forgelightGame = CreateForgelightGame(alias, packDirectory, resourceDirectory);

                    forgelightGame.LoadPackFiles(0.0f, 0.7f);
                    forgelightGame.InitializeMaterialDefinitionManager();
                    forgelightGame.UpdateActors(0.7f, 0.9f);
                    forgelightGame.UpdateZones(0.9f, 1.0f);

                    forgelightGame.OnLoadComplete();
                }
            }
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