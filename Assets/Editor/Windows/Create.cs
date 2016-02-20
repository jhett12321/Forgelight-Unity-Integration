using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Forgelight.Editor.DraggableObjects;
using Forgelight;

public class Create : EditorWindow
{
    private const int indent = 1;
    private const float objectCreationDistance = 20.0f;

    private string searchString = "";
    private Dictionary<string, bool> openElements = new Dictionary<string, bool>();
    private Vector2 scroll;

    [MenuItem("Forgelight/Windows/Create")]
    public static void Init()
    {
        EditorWindow.GetWindow(typeof(Create));
    }

    private void OnFocus()
    {
        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        SceneView.onSceneGUIDelegate += this.OnSceneGUI;
    }

    private void OnDestroy()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
    }

    private void OnGUI()
    {
        //Events
        EventType eventType = Event.current.type;

        if (eventType == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();
        }

        //Actors
        Dictionary<string, List<string>> availableActors = ForgelightExtension.Instance.AssetLoader.availableActors;

        //Debug
        List<string> testActors = new List<string>();
        testActors.Add("anActor.adr");
        testActors.Add("constructionProp.adr");

        availableActors["Debug Test"] = testActors;

        //Search Box
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.FlexibleSpace();
        GUILayout.Label("Search: ", EditorStyles.toolbarButton);
        searchString = GUILayout.TextField(searchString, EditorStyles.toolbarTextField, GUILayout.MinWidth(200));

        GUILayout.EndHorizontal();

        //Actor List
        EditorGUILayout.BeginHorizontal();
        {
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Width(300), GUILayout.Height(800));
            {
                foreach (KeyValuePair<string, List<string>> actors in availableActors)
                {
                    if (!openElements.ContainsKey(actors.Key))
                    {
                        openElements[actors.Key] = false;
                    }

                    ShowAlias(actors.Key, actors.Value);
                }
            }
            EditorGUILayout.EndScrollView();
        }
        EditorGUILayout.EndHorizontal();

        //Preview Box
        //GameObject model = Resources.Load();
    }

    public void OnSceneGUI(SceneView sceneView)
    {
        //Events
        EventType eventType = Event.current.type;

        if (eventType == EventType.DragUpdated)
        {
            if (DragAndDrop.GetGenericData("ActorDefinition") != null)
            {
                ActorDefinition draggedObj = DragAndDrop.GetGenericData("ActorDefinition") as ActorDefinition;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy; // show a drag-add icon on the mouse cursor

                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

                //We have entered the scene. Create the forgelight object at our current position.
                if (draggedObj.instantiatedGameObject == null)
                {
                    draggedObj.instantiatedGameObject = ForgelightExtension.Instance.ZoneObjectFactory.CreateForgelightObject(draggedObj.gameAlias,
                        draggedObj.actorDefinition, ray.GetPoint(objectCreationDistance), Quaternion.identity);
                }

                else
                {
                    draggedObj.instantiatedGameObject.transform.position = ray.GetPoint(objectCreationDistance);
                }

                Selection.activeGameObject = draggedObj.instantiatedGameObject;
            }
        }

        if (eventType == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();
        }
    }

    private void ShowAlias(string alias, List<string> availableActors)
    {
        // Show entry for parent object
        openElements[alias] = EditorGUILayout.Foldout(openElements[alias], alias);

        if (openElements[alias])
        {
            ShowAvailableActors(alias, availableActors);
        }
    }

    private void ShowAvailableActors(string alias, List<string> availableActors)
    {
        foreach (string actor in availableActors)
        {
            if (searchString == null || actor.ToLower().Contains(searchString.ToLower()))
            {
                EditorGUI.indentLevel += indent;

                Rect position = GUILayoutUtility.GetRect(40f, 40f, 16f, 16f, EditorStyles.label);

                if (Event.current.type == EventType.MouseDown)
                {
                    if (position.Contains(Event.current.mousePosition))
                    {
                        BeginDrag(alias, actor);
                    }
                }

                EditorGUI.Foldout(position, false, actor, true, EditorStyles.label);

                EditorGUI.indentLevel -= indent;
            }
        }
    }

    private void BeginDrag(string alias, string actor)
    {
        ActorDefinition actorDefinition = new ActorDefinition
        {
            gameAlias = alias,
            actorDefinition = actor
        };

        DragAndDrop.objectReferences = new Object[0];
        DragAndDrop.PrepareStartDrag();
        DragAndDrop.SetGenericData("ActorDefinition", actorDefinition);
        DragAndDrop.StartDrag("ActorDrag");

        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
    }
}
