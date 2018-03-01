namespace ForgelightUnity.Forgelight
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json.Linq;
    using UnityEngine;

    public class Config
    {
        public static string StatePath
        {
            get { return Application.dataPath + "/Forgelight/state.json"; }
        }

        private JObject extensionState;

        private void CheckExtensionState()
        {
            if (extensionState == null)
            {
                LoadSavedState();
            }

            if (extensionState == null)
            {
                extensionState = GetDefaultState();
            }
        }

        /// <summary>
        /// Attempts to find the JSON object node containing the specified game info.
        /// </summary>
        /// <param name="name"></param>
        /// <returns> null if it cannot find a game that matches name, otherwise the JObject node containing game info.</returns>
        public JObject GetForgelightGameInfo(string name)
        {
            CheckExtensionState();
            JObject info = (JObject)extensionState["forgelight_games"][name];

            return info;
        }

        /// <summary>
        /// Attempts to find the JSON object node for the forgelight game that was last being used in editor.
        /// </summary>
        /// <returns> null if it cannot find a game that matches name, or was marked active, otherwise the JObject node containing game info.</returns>
        public string GetLastActiveForgelightGame()
        {
            CheckExtensionState();
            foreach (KeyValuePair<string, JToken> forgelightGame in (JObject)extensionState["forgelight_games"])
            {
                if ((bool)forgelightGame.Value["is_active_game"])
                {
                    return forgelightGame.Key;
                }
            }

            return null;
        }

        public List<string> GetAvailableForgelightGames()
        {
            CheckExtensionState();
            return ((JObject) extensionState["forgelight_games"]).Properties().Select(p => p.Name).ToList();
        }

        public void SaveNewForgelightGame(ForgelightGame forgelightGame)
        {
            CheckExtensionState();
            JObject gameElement = new JObject();

            gameElement.Add("pack_directory", forgelightGame.PackDirectory);
            gameElement.Add("resource_directory", forgelightGame.ResourceDirectory);
            gameElement.Add("is_active_game", true);

            ((JObject)extensionState["forgelight_games"])[forgelightGame.Name] = gameElement;

            WriteStateToDisk();
        }

        public void DeleteForgelightGame(ForgelightGame forgelightGame)
        {
            CheckExtensionState();

            extensionState["forgelight_games"][forgelightGame.Name].Remove();

            WriteStateToDisk();
        }

        public void UpdateForgelightGame(ForgelightGame forgelightGame, bool isActive)
        {
            CheckExtensionState();
            JObject info = (JObject)extensionState["forgelight_games"][forgelightGame.Name];

            info["is_active_game"] = isActive;

            WriteStateToDisk();
        }

        public void LoadSavedState()
        {
            //Load saved state information. Create a new state file if it currently does not exist.
            extensionState = null;
            if (!File.Exists(StatePath))
            {
                extensionState = GetDefaultState();
                WriteStateToDisk();
                return; //No need to try finding an active game as we did not find a state file.
            }

            if (extensionState == null)
            {
                extensionState = JObject.Parse(File.ReadAllText(@StatePath));
            }
        }

        public void WriteStateToDisk()
        {
            CheckExtensionState();
            string directory = Path.GetDirectoryName(StatePath);

            if (directory == null)
            {
                return;
            }

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(@StatePath, extensionState.ToString());
        }

        private JObject GetDefaultState()
        {
            JObject retval = new JObject();

            JObject extension = new JObject();
            extension.Add("version", "1.0");
            extension.Add("update_url", "http://blackfeatherproductions.com/version.txt");

            retval.Add("extension", extension);

            JObject forgelight_games = new JObject();
            retval.Add("forgelight_games", forgelight_games);

            return retval;
        }
    }
}
