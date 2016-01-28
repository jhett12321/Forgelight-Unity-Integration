using UnityEditor;
using UnityEngine;

public class Forgelight : MonoBehaviour
{
    //Singleton
    private static Forgelight instance = null;
    public static Forgelight Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (Forgelight)FindObjectOfType(typeof(Forgelight));
                EditorApplication.update += instance.EditorUpdate;
            }

            return instance;
        }
    }

    //Terrain Importing
    private TerrainLoader terrainLoader;
    public TerrainLoader TerrainLoader
    {
        get
        {
            if (terrainLoader == null)
            {
                terrainLoader = new TerrainLoader();
            }

            return terrainLoader;
        }
    }

    //Zone Editing
    private ZoneObjectFactory zoneObjectFactory;
    public ZoneObjectFactory ZoneObjectFactory
    {
        get
        {
            if (zoneObjectFactory != null)
            {
                return zoneObjectFactory;
            }

            GameObject parent = GameObject.FindWithTag("ZoneObjects");

            if (parent == null)
            {
                parent = new GameObject("Forgelight Objects");
                parent.tag = "ZoneObjects";

                zoneObjectFactory = parent.AddComponent<ZoneObjectFactory>();
            }
            else
            {
                zoneObjectFactory = parent.GetComponent<ZoneObjectFactory>();

                if (zoneObjectFactory == null)
                {
                    zoneObjectFactory = parent.AddComponent<ZoneObjectFactory>();
                }
            }

            return zoneObjectFactory;
        }
    }

    //Zone Importing
    private ZoneLoader zoneLoader;
    public ZoneLoader ZoneLoader
    {
        get
        {
            if (zoneLoader == null)
            {
                zoneLoader = new ZoneLoader();
            }

            return zoneLoader;
        }
    }

    //Zone Exporting
    private ZoneExporter zoneExporter;
    public ZoneExporter ZoneExporter
    {
        get
        {
            if (zoneExporter == null)
            {
                zoneExporter = new ZoneExporter();
            }

            return zoneExporter;
        }
    }

    //Editor
    public Vector3 lastCameraPos = new Vector3();


    //TODO Implement .zone Conversion
    //TODO Implement asset pack streaming.

    private void EditorUpdate()
    {
        if (instance == null)
        {
            instance = this;
        }

        if (Camera.current != null)
        {
            lastCameraPos = Camera.current.transform.position;
        }
    }
}
