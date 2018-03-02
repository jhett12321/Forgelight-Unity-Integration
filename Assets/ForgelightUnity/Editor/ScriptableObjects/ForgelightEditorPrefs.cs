namespace ForgelightUnity.Editor.ScriptableObjects
{
    using System.Collections.Generic;
    using Forgelight;
    using UnityEngine;

    /// <summary>
    /// Holds persistent editor information about forgelight games.
    /// </summary>
    [GlobalUnique]
    [DefaultAssetPath("Assets/Forgelight")]
    public class ForgelightEditorPrefs : ScriptableObject
    {
        public ForgelightGameInfo ActiveForgelightGame;
        public List<ForgelightGameInfo> ForgelightGames = new List<ForgelightGameInfo>();
    }
}
