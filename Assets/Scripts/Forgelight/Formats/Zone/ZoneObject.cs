using System.Collections.Generic;
using Forgelight.Attributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Forgelight.Formats.Zone
{
    [ExecuteInEditMode]
    public class ZoneObject : MonoBehaviour
    {
        private string currentActorDef;

        public string actorDefinition;
        public float renderDistance;

        private const float gracePeriod = 3.0f;
        private float target = 3.0f;

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

        private bool visible;

        private Renderer[] renderers;
        private List<GameObject> objectsToDestroy = new List<GameObject>();

        private ForgelightExtension forgelightExtension;

        private void OnEnable()
        {
            forgelightExtension = ForgelightExtension.Instance;
        }

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

            else
            {
                CheckVisibility();
            }
        }

        private void OnRenderObject()
        {
            if (forgelightExtension.cameraPosChanged)
            {
                target = Time.realtimeSinceStartup + gracePeriod;
            }

            if (Time.realtimeSinceStartup >= target || Selection.activeGameObject == gameObject)
            {
                CheckVisibility();
            }

            foreach (Transform child in transform)
            {
                if (child.gameObject == Selection.activeGameObject)
                {
                    Selection.activeGameObject = gameObject;
                }

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

        private void CheckVisibility()
        {
            Vector3 offset = transform.position - ForgelightExtension.Instance.LastCameraPos;

            float sqrMagnitude = offset.sqrMagnitude;
            if (sqrMagnitude <= renderDistance * renderDistance)
            {
                Show();
            }

            else
            {
                Hide();
            }

            target = float.MaxValue; //We don't need to update until we move again.
        }

        public void Hide()
        {
            if (visible)
            {
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(false);
                }

                visible = false;
            }
        }

        public void Show()
        {
            if (!visible)
            {
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(true);
                }

                visible = true;
            }
        }

        public void DestroyObject(GameObject objToDestroy)
        {
            objectsToDestroy.Add(objToDestroy);
        }
    }
}