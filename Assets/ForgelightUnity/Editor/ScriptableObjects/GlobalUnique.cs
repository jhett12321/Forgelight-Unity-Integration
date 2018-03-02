namespace ForgelightUnity.Editor.ScriptableObjects
{
    using System;

    /// <summary>
    /// An attribute applied to ScriptableObjects indicating only one can exist in the whole project.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class GlobalUnique : Attribute {}
}