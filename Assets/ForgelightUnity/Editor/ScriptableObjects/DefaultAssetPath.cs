namespace ForgelightUnity.Editor.ScriptableObjects
{
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class DefaultAssetPath : Attribute
    {
        public string Path { get; private set; }

        public DefaultAssetPath(string path)
        {
            this.Path = path;
        }
    }
}