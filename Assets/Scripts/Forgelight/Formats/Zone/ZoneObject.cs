using UnityEngine;
using System.Collections.Generic;
using Forgelight.Attributes;
using UnityEngine.Rendering;

namespace Forgelight.Formats.Zone
{
    [ExecuteInEditMode]
    public class ZoneObject : MonoBehaviour
    {
        private string currentActorDef = null;

        public string actorDefinition;
        public float renderDistance;

        private float farDistance = 200.0f;

        //TODO HACK make this interval determined by the player's current position.
        private readonly float interval = 5.0f;
        private readonly float farInterval = 15.0f;
        private float target = 5.0f;

        /// <summary>
        /// Indicates whether an object should cast shadows. We mostly turn this on (on indicates don't cast shadows, oddly) when an object is indoors (being indoors, shadows don't really matter).
        /// </summary>
        private bool dontCastShadows;
        [ExposeProperty]
        public bool DontCastShadows
        {
            get
            {
                return dontCastShadows;
            }
            set
            {
                if (renderers == null || renderers.Length == 0)
                {
                    renderers = GetComponentsInChildren<Renderer>();
                }

                foreach (Renderer renderer in renderers)
                {
                    if (value)
                    {
                        renderer.shadowCastingMode = ShadowCastingMode.Off;
                    }
                    else
                    {
                        renderer.shadowCastingMode = ShadowCastingMode.On;
                    }
                }

                dontCastShadows = value;
            }
        }

        /// <summary>
        /// LOD multiplier. Basically allows the designers bias the LOD distance farther or closer on a per-object basis. We generally try to avoid using it and leave it at the default 1.
        /// </summary>
        public float lodMultiplier;

        public long ID { get; set; }

        [SerializeField]
        private bool visible = false;

        private Renderer[] renderers;
        private List<GameObject> objectsToDestroy = new List<GameObject>();

        private void OnValidate()
        {
            if (actorDefinition != currentActorDef)
            {
                if (currentActorDef != null)
                {
                    ForgelightGame activeForgelightGame = ForgelightExtension.Instance.ForgelightGameFactory.ActiveForgelightGame;

                    if (activeForgelightGame != null)
                    {
                        ForgelightExtension.Instance.ZoneObjectFactory.UpdateForgelightObject(activeForgelightGame, this, actorDefinition);
                    }
                }

                currentActorDef = actorDefinition;
            }
        }

        private void OnRenderObject()
        {
            if (Time.realtimeSinceStartup >= target)
            {
                Vector3 offset = transform.position - ForgelightExtension.Instance.LastCameraPos;

                float sqrMagnitude = offset.sqrMagnitude;

                bool isFar = false;

                if (sqrMagnitude <= renderDistance * renderDistance)
                {
                    Show();
                }

                else
                {
                    Hide();

                    if (sqrMagnitude > renderDistance * renderDistance + farDistance * farDistance)
                    {
                        isFar = true;
                    }
                }

                target = Time.realtimeSinceStartup + interval;

                if (isFar)
                {
                    target += farInterval;
                }
            }

            foreach (Transform child in transform)
            {
                //Check to see if the user has accidentally moved a child object and not the parent.
                if (child.transform.localPosition != Vector3.zero)
                {
                    child.transform.localPosition = Vector3.zero;
                }
            }

            if (objectsToDestroy.Count > 0)
            {
                foreach (GameObject o in objectsToDestroy)
                {
                    DestroyImmediate(o);
                }

                objectsToDestroy.Clear();

                Resources.UnloadUnusedAssets();
            }
        }

        public void Hide()
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }

            visible = false;
        }

        public void Show()
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true);
            }

            visible = true;
        }

        public void DestroyObject(GameObject objToDestroy)
        {
            objectsToDestroy.Add(objToDestroy);
        }
    }
}