namespace ForgelightUnity.Editor.Forgelight
{
    using System.Linq;
    using Editor.Utils;
    using ScriptableObjects;
    using UnityEditor;

    public class Config
    {
        private ForgelightEditorPrefs forgelightEditorPrefs;

        public ForgelightEditorPrefs ForgelightEditorPrefs
        {
            get
            {
                if (forgelightEditorPrefs != null)
                {
                    return forgelightEditorPrefs;
                }

                forgelightEditorPrefs = ScriptableObjectUtils.GetGlobalScriptableObject<ForgelightEditorPrefs>();

                if (forgelightEditorPrefs == null)
                {
                    forgelightEditorPrefs = ScriptableObjectUtils.CreateScriptableObject<ForgelightEditorPrefs>().ScriptableObject;
                }

                return forgelightEditorPrefs;
            }
        }

        /// <summary>
        /// Attempts to find the JSON object node containing the specified game info.
        /// </summary>
        /// <param name="name"></param>
        /// <returns> null if it cannot find a game that matches name, otherwise the JObject node containing game info.</returns>
        public ForgelightGameInfo GetForgelightGameInfo(string name)
        {
            return ForgelightEditorPrefs.ForgelightGames.FirstOrDefault(game => game.Name == name);
        }

        public string[] GetAvailableForgelightGames()
        {
            return ForgelightEditorPrefs.ForgelightGames.Select(game => game.Name).ToArray();
        }

        public void SaveNewForgelightGame(ForgelightGame forgelightGame)
        {
            ForgelightEditorPrefs.ForgelightGames.Add(forgelightGame.GameInfo);
            EditorUtility.SetDirty(ForgelightEditorPrefs);
        }

        public void DeleteForgelightGame(ForgelightGame forgelightGame)
        {
            ForgelightEditorPrefs.ForgelightGames.Remove(forgelightGame.GameInfo);
            EditorUtility.SetDirty(ForgelightEditorPrefs);
        }
    }
}
