using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Forgelight.Editor.DraggableObjects;

namespace Forgelight.Editor.Windows
{
    public class Create : EditorWindow
    {
        private const float objectCreationDistance = 20.0f;

        private string searchString = "";
        private Vector2 scroll;

        [MenuItem("Forgelight/Windows/Create")]
        public static void Init()
        {
            GetWindow(typeof (Create), false, "Create");
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

            //Search Box
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.FlexibleSpace();
            GUILayout.Label("Search: ", EditorStyles.toolbarButton);
            searchString = GUILayout.TextField(searchString, EditorStyles.toolbarTextField, GUILayout.MinWidth(200));

            GUILayout.EndHorizontal();

            //Actor List
            EditorGUILayout.BeginHorizontal();
            {
                scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(500));
                {
                    ForgelightGame activeForgelightGame = ForgelightExtension.Instance.ForgelightGameFactory.ActiveForgelightGame;

                    if (activeForgelightGame != null)
                    {
                        ShowAvailableActors(activeForgelightGame, activeForgelightGame.AvailableActors);
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
                    ActorDefinition draggedObj = (ActorDefinition) DragAndDrop.GetGenericData("ActorDefinition");

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy; // show a drag-add icon on the mouse cursor

                    Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

                    //We have entered the scene. Create the forgelight object at our current position.
                    if (draggedObj.instantiatedGameObject == null)
                    {
                        draggedObj.instantiatedGameObject = ForgelightExtension.Instance.ZoneObjectFactory.CreateForgelightObject(draggedObj.forgelightGame,
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

        private void ShowAvailableActors(ForgelightGame forgelightGame, List<string> availableActors)
        {
            foreach (string actor in availableActors)
            {
                if (searchString == null || actor.ToLower().Contains(searchString.ToLower()))
                {
                    Rect position = GUILayoutUtility.GetRect(40f, 40f, 16f, 16f, EditorStyles.label);

                    if (Event.current.type == EventType.MouseDown)
                    {
                        if (position.Contains(Event.current.mousePosition))
                        {
                            BeginDrag(forgelightGame, actor);
                        }
                    }

                    GUIStyle style = EditorStyles.label;
                    style.fixedWidth = 0;
                    style.stretchWidth = true;
                    style.clipping = TextClipping.Overflow;

                    EditorGUI.Foldout(position, false, actor, true, style);
                }
            }
        }

        private void BeginDrag(ForgelightGame forgelightGame, string actor)
        {
            ActorDefinition actorDefinition = new ActorDefinition
            {
                forgelightGame = forgelightGame,
                actorDefinition = actor
            };

            DragAndDrop.objectReferences = new Object[0];
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.SetGenericData("ActorDefinition", actorDefinition);
            DragAndDrop.StartDrag("ActorDrag");

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        }
    }
}