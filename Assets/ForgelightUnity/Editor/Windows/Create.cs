namespace ForgelightUnity.Editor.Windows
{
    using System.Collections.Generic;
    using DraggableObjects;
    using Forgelight;
    using Forgelight.Assets;
    using Forgelight.Assets.Adr;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// The Create Window. Shows all available actors, and can create Unity GameObjects through drag and drop.
    /// </summary>
    public class Create : EditorWindow
    {
        private const float objectCreationDistance = 20.0f;

        private string searchString = "";
        private Vector2 scrollTop;
        private Vector2 scrollBottom;

        private GameObject selectedActor;
        private Editor previewWindowEditor;

        //Splitter
        private float splitterPos;
        private Rect splitterRect;
        private bool dragging;
        private float splitterWidth = 5;

        public static void Init()
        {
            Create window = (Create) GetWindow(typeof (Create), false, "Create");
            window.splitterPos = 500.0f;
       }

        private void OnFocus()
        {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            SceneView.onSceneGUIDelegate += OnSceneGUI;
        }

        private void OnDestroy()
        {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
        }

        /// <summary>
        /// Called every update of this window.
        /// </summary>
        private void OnGUI()
        {
            DrawSearchBox();

            //The main content for this window.
            GUILayout.BeginVertical();

            DrawAvailableActors();
            DrawSplitter();
            DrawPreviewBox();

            GUILayout.EndVertical();

            //Events
            ProcessDragEvents(false);
            ProcessSplitterEvents();
        }

        /// <summary>
        /// Called every update of the scene view.
        /// </summary>
        /// <param name="sceneView"></param>
        public void OnSceneGUI(SceneView sceneView)
        {
            ProcessDragEvents(true);
        }

        #region Draws
        /// <summary>
        /// Draws a small toolbar with a search box to filter out actor definitions.
        /// </summary>
        private void DrawSearchBox()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.FlexibleSpace();
            GUILayout.Label("Search: ", EditorStyles.toolbarButton);
            searchString = GUILayout.TextField(searchString, EditorStyles.toolbarTextField, GUILayout.MinWidth(200));
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Retrieves the available actors from the game factory, creating a list of actors, and checks if they have been clicked this frame.
        /// Triggers the "BeginDrag" operation on a double click.
        /// </summary>
        private void DrawAvailableActors()
        {
            scrollTop = GUILayout.BeginScrollView(scrollTop, GUILayout.Height(splitterPos), GUILayout.MaxHeight(splitterPos), GUILayout.MinHeight(splitterPos));
            ForgelightGame forgelightGame = ForgelightExtension.Instance.ForgelightGameFactory.ActiveForgelightGame;

            if (forgelightGame != null)
            {
                List<Asset> availableActors = forgelightGame.AvailableActors;

                foreach (Asset asset in availableActors)
                {
                    if (searchString != null && !asset.DisplayName.ToLower().Contains(searchString.ToLower()))
                    {
                        continue;
                    }

                    Adr actor = (Adr) asset;

                    Rect rect = GUILayoutUtility.GetRect(40f, 40f, 16f, 16f, EditorStyles.label);

                    if (Event.current.type == EventType.MouseDown)
                    {
                        if (rect.Contains(Event.current.mousePosition))
                        {
                            BeginDrag(forgelightGame, actor);
                            DestroyImmediate(previewWindowEditor);
                            selectedActor = ForgelightExtension.Instance.ZoneManager.ZoneObjectFactory.GetForgelightObject(forgelightGame, actor.Base);
                        }
                    }

                    GUIStyle style = EditorStyles.label;
                    style.fixedWidth = 0;
                    style.stretchWidth = true;
                    style.clipping = TextClipping.Overflow;

                    EditorGUI.Foldout(rect, false, asset.DisplayName, true, style);
                }
            }

            GUILayout.EndScrollView();
        }

        /// <summary>
        /// Draws a small box shape that is used to resize the elements in this window.
        /// </summary>
        private void DrawSplitter()
        {
            GUILayout.Box("",GUILayout.Height(splitterWidth), GUILayout.MaxHeight(splitterWidth), GUILayout.MinHeight(splitterWidth), GUILayout.ExpandWidth(true));

            splitterRect = GUILayoutUtility.GetLastRect();

            EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeVertical);
        }

        /// <summary>
        /// Draws a 3D object preview of the selected forgelight object, if available.
        /// </summary>
        private void DrawPreviewBox()
        {
            scrollBottom = GUILayout.BeginScrollView(scrollBottom, GUILayout.ExpandHeight(true));

            if (selectedActor != null)
            {
                if (previewWindowEditor == null)
                {
                    previewWindowEditor = Editor.CreateEditor(selectedActor);
                }

                Rect rect = GUILayoutUtility.GetRect(1.0f, 1.0f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                previewWindowEditor.OnPreviewGUI(rect, GUIStyle.none);
            }

            GUILayout.EndScrollView();
        }
        #endregion

        #region Events

        /// <summary>
        /// Checks for mouse events in this frame, moving created objects and notifying "DragAndDrop" of state changes.
        /// </summary>
        /// <param name="inScene">true if being called from OnSceneGUI, otherwise false.</param>
        private void ProcessDragEvents(bool inScene)
        {
            EventType eventType = Event.current.type;

            //Don't try and make/move the object when we are not focused in the scene.
            if (inScene && eventType == EventType.DragUpdated)
            {
                if (DragAndDrop.GetGenericData("ActorDefinition") != null)
                {
                    ActorDefinition draggedObj = (ActorDefinition)DragAndDrop.GetGenericData("ActorDefinition");

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy; // show a drag-add icon on the mouse cursor

                    Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

                    //We have entered the scene. Create the forgelight object at our current position.
                    if (draggedObj.instantiatedGameObject == null)
                    {
                        draggedObj.instantiatedGameObject = ForgelightExtension.Instance.ZoneManager.ZoneObjectFactory.CreateForgelightObject(draggedObj.forgelightGame, draggedObj.actorDefinition, ray.GetPoint(objectCreationDistance), Quaternion.identity);
                    }

                    else
                    {
                        draggedObj.instantiatedGameObject.transform.position = ray.GetPoint(objectCreationDistance);
                    }

                    Selection.activeGameObject = draggedObj.instantiatedGameObject;
                }
            }

            else if (eventType == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
            }
        }

        /// <summary>
        /// Checks for mouse events in this frame, resizing the window elements as necessary.
        /// </summary>
        private void ProcessSplitterEvents()
        {
            if (Event.current != null)
            {
                switch (Event.current.type)
                {
                    case EventType.MouseDown:
                        if (splitterRect.Contains(Event.current.mousePosition))
                        {
                            dragging = true;
                        }

                        break;
                    case EventType.MouseDrag:
                        if (dragging)
                        {
                            Rect lastRect = GUILayoutUtility.GetLastRect();

                            if (splitterPos + (splitterWidth * 3) + Event.current.delta.y < lastRect.height && splitterPos + Event.current.delta.y > 0)
                            {
                                splitterPos += Event.current.delta.y;
                                Repaint();
                            }
                        }

                        break;
                    case EventType.MouseUp:
                        if (dragging)
                        {
                            dragging = false;
                        }

                        break;
                }
            }
        }
        #endregion

        /// <summary>
        /// Creates the DragAndDrop object's references, and begins the drag operation.
        /// </summary>
        /// <param name="forgelightGame">The forgelight game containing "actor"</param>
        /// <param name="actor">The selected actor</param>
        private void BeginDrag(ForgelightGame forgelightGame, Adr actor)
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