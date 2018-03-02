namespace ForgelightUnity.Editor.Forgelight
{
    using System;
    using Editor.Utils;

    [Serializable]
    public class ForgelightGameInfo
    {
        /// <summary>
        /// The name of the forgelight game.
        /// </summary>
        public string Name;
        /// <summary>
        /// The source pack directory (Game folder/Resources/Assets)
        /// </summary>
        public string PackDirectory;
        /// <summary>
        /// The converted asset directory (relative to the resources folder).
        /// </summary>
        public string RelativeResourceDirectory;

        public string FullResourceDirectory
        {
            get { return ProjectFileUtils.GetFullPathFromAssetRelativePath(RelativeResourceDirectory); }
        }

        public ForgelightGameInfo(string name, string packDirectory, string relativeResourceDirectory)
        {
            this.Name = name;
            this.PackDirectory = packDirectory;
            this.RelativeResourceDirectory = relativeResourceDirectory;
        }
    }
}