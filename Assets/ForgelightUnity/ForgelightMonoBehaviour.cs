namespace ForgelightUnity
{
    using Attributes;
    using UnityEngine;

    /// <summary>
    /// Adds a small single gameobject to the scene for tracking the source forgelight game.
    /// TODO Handle multiple scenes.
    /// </summary>
    public class ForgelightMonoBehaviour : MonoBehaviour
    {
        private static ForgelightMonoBehaviour instance;
        public static ForgelightMonoBehaviour Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = (ForgelightMonoBehaviour)FindObjectOfType(typeof(ForgelightMonoBehaviour));

                    if (instance == null)
                    {
                        instance = new GameObject("Forgelight Editor").AddComponent<ForgelightMonoBehaviour>();
                    }
                }

                return instance;
            }
        }

        //Forgelight Game. Saved with the scene.
        [ReadOnly]
        public string ForgelightGame;
    }
}