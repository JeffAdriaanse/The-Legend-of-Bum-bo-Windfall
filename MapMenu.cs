using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace The_Legend_of_Bum_bo_Windfall
{
    static class MapMenu
    {
        private static BumboApplication app;

        private static GameObject mapMenuButton;
        private static GameObject gamblingMapMenuButton;

        public static GameObject mapMenuCanvas;
        private static GameObject mapCanvasBackground;

        private static GameObject mapCanvasHeader;
        private static Vector2 headerStartPos;
        private static RectTransform headerRectTransform;
        private static float headerOffsetY = 230;

        private static GameObject mapCanvasRoomContainer;
        private static Vector2 roomContainerStartPos;
        private static RectTransform roomContainerRectTransform;
        private static float roomContainerOffsetX = -800;
        private static float roomContainerOffsetY = 350;

        private static GameObject mapCanvasExit;
        private static Vector2 exitStartPos;
        private static RectTransform exitRectTransform;
        private static float exitOffsetY = -100;

        private static GameObject mapCanvasMouse;

        private static float opacityValue = 0.5f;

        private static Ease mapTweeningEase = Ease.OutQuad;

        private static Sprite bumboHead;

        private static bool Gambling { get { return app.view.gamblingView != null; } }

        public static void CreateMapMenu(BumboElement bumboElement)
        {
            FindApp(bumboElement);

            if (app.model.characterSheet.currentFloor == 0) return;

            GrabAssets();

            if (mapMenuButton == null) CreateMapMenuButton();
            else mapMenuButton = app.view.GUICamera.transform.Find("HUD").Find("Map Menu Button").gameObject;

            if (mapMenuCanvas == null) CreateMapMenuCanvas();
            else mapMenuCanvas = app.view.transform.Find("Map Menu Canvas").gameObject;
        }

        private static void FindApp(BumboElement bumboElement)
        {
            app = bumboElement.app;
        }

        private static void GrabAssets()
        {
            AssetBundle assets = Windfall.assetBundle;
            if (assets == null)
            {
                Debug.Log("Failed to load AssetBundle!");
                return;
            }

            //Grab Bum-bo head sprite
            switch (app.model.characterSheet.bumboType)
            {
                case CharacterSheet.BumboType.TheBrave:
                    bumboHead = assets.LoadAsset<Sprite>("Brave Head");
                    break;
                case CharacterSheet.BumboType.TheNimble:
                    bumboHead = assets.LoadAsset<Sprite>("Nimble Head");
                    break;
                case CharacterSheet.BumboType.TheStout:
                    bumboHead = assets.LoadAsset<Sprite>("Stout Head");
                    break;
                case CharacterSheet.BumboType.TheWeird:
                    bumboHead = assets.LoadAsset<Sprite>("Weird Head");
                    break;
                case CharacterSheet.BumboType.TheDead:
                    bumboHead = assets.LoadAsset<Sprite>("Dead Head");
                    break;
                case CharacterSheet.BumboType.Eden:
                    bumboHead = assets.LoadAsset<Sprite>("Empty Head");
                    break;
                case CharacterSheet.BumboType.TheLost:
                    bumboHead = assets.LoadAsset<Sprite>("Lost Head");
                    break;
            }
        }

        private static void CreateMapMenuButton()
        {
            //Duplicate existing menu button
            GameObject menuButton = app.view.GUICamera.transform.Find("HUD").Find("menu button").gameObject;

            mapMenuButton = UnityEngine.Object.Instantiate(menuButton, new Vector3(1.21f, 2.09f, -4.71f), menuButton.transform.rotation, menuButton.transform.parent);

            ButtonHoverAnimation mapHover = mapMenuButton.GetComponent<ButtonHoverAnimation>();
            if (mapHover)
            {
                UnityEngine.Object.Destroy(mapHover);
                mapMenuButton.transform.localScale = Vector3.Scale(mapMenuButton.transform.localScale, new Vector3(0.7f, 1f, 0.7f));
                mapMenuButton.AddComponent<ButtonHoverAnimation>();
            }
            mapMenuButton.name = "Map Menu Button";

            //Change button texture
            AssetBundle assets = Windfall.assetBundle;
            if (assets == null)
            {
                Debug.Log("Failed to load AssetBundle!");
                return;
            }
            Material mapButtonMaterial = mapMenuButton.GetComponent<MeshRenderer>().material;
            mapButtonMaterial.mainTexture = assets.LoadAsset<Texture>("Map Button");
            mapButtonMaterial.mainTextureOffset = new Vector2(0, -0.08f);
            mapButtonMaterial.mainTextureScale = new Vector2(1.10f, 1.10f);
        }

        public static void CreateGamblingMapMenuButton()
        {
            if (!Gambling) return;

            GameObject menuButton = app.view.GUICamera.transform.Find("HUD").Find("menu button").gameObject;

            gamblingMapMenuButton = UnityEngine.Object.Instantiate(menuButton, new Vector3(1.21f, 2.09f, -4.71f), menuButton.transform.rotation, menuButton.transform.parent);

            ButtonHoverAnimation mapHover = gamblingMapMenuButton.GetComponent<ButtonHoverAnimation>();
            if (mapHover)
            {
                UnityEngine.Object.Destroy(mapHover);
                gamblingMapMenuButton.transform.localScale = Vector3.Scale(gamblingMapMenuButton.transform.localScale, new Vector3(0.7f, 1f, 0.7f));
                gamblingMapMenuButton.AddComponent<ButtonHoverAnimation>();
            }
            gamblingMapMenuButton.name = "Gambling Map Menu Button";

            //Change button texture
            AssetBundle assets = Windfall.assetBundle;
            if (assets == null)
            {
                Debug.Log("Failed to load AssetBundle!");
                return;
            }
            Material mapButtonMaterial = gamblingMapMenuButton.GetComponent<MeshRenderer>().material;
            mapButtonMaterial.mainTexture = assets.LoadAsset<Texture>("Map Button");
            mapButtonMaterial.mainTextureOffset = new Vector2(0, -0.08f);
            mapButtonMaterial.mainTextureScale = new Vector2(1.10f, 1.10f);
        }

        private static void CreateMapMenuCanvas()
        {
            AssetBundle assets = Windfall.assetBundle;
            if (assets == null)
            {
                Debug.Log("Failed to load AssetBundle!");
                return;
            }

            //Canvas
            GameObject mapMenuCanvasPrefab = assets.LoadAsset<GameObject>("Map Menu Canvas");
            mapMenuCanvas = UnityEngine.Object.Instantiate(mapMenuCanvasPrefab);
            mapMenuCanvas.transform.SetParent(app.view.transform);
            mapMenuCanvas.transform.SetAsFirstSibling();
            mapMenuCanvas.SetActive(false);

            //Mouse
            mapCanvasMouse = GameObject.Instantiate(app.GetComponentInChildren<GUIMouseView>(true).gameObject);
            mapCanvasMouse.layer = 5;
            mapCanvasMouse.transform.SetParent(mapMenuCanvas.transform);
            mapCanvasMouse.transform.localScale = new Vector3(2.4f, 2.4f, 2.4f);

            //Canvas children
            mapCanvasBackground = mapMenuCanvas.transform.Find("Background").gameObject;

            mapCanvasHeader = mapMenuCanvas.transform.Find("Header").gameObject;
            headerRectTransform = mapCanvasHeader.GetComponent<RectTransform>();
            headerStartPos = headerRectTransform.anchoredPosition;
            headerRectTransform.anchoredPosition = new Vector2(headerRectTransform.anchoredPosition.x, headerRectTransform.anchoredPosition.y + headerOffsetY);

            mapCanvasRoomContainer = mapMenuCanvas.transform.Find("Room Container").gameObject;
            roomContainerRectTransform = mapCanvasRoomContainer.GetComponent<RectTransform>();
            roomContainerStartPos = roomContainerRectTransform.anchoredPosition;
            roomContainerRectTransform.anchoredPosition = new Vector2(roomContainerRectTransform.anchoredPosition.x, roomContainerRectTransform.anchoredPosition.y + roomContainerOffsetY);

            mapCanvasExit = mapMenuCanvas.transform.Find("Exit").gameObject;
            exitRectTransform = mapCanvasExit.GetComponent<RectTransform>();
            exitStartPos = exitRectTransform.anchoredPosition;
            exitRectTransform.anchoredPosition = new Vector2(exitRectTransform.anchoredPosition.x, exitRectTransform.anchoredPosition.y + exitOffsetY);

            //Set up canvas elements
            SetUpHeader();
            SetUpRooms();
            SetupExit();
        }

        private static void SetUpHeader()
        {
            int lastActiveChapterIndex = (WindfallHelper.ChaptersUnlocked(app.model.progression) * 4) - 3;

            //Set active chapters
            for (int childCounter = 0; childCounter < mapCanvasHeader.transform.childCount; childCounter++)
            {
                GameObject gameObject = mapCanvasHeader.transform.GetChild(childCounter).gameObject;

                if (childCounter > lastActiveChapterIndex)
                {
                    gameObject.SetActive(false);
                }
                else
                {
                    gameObject.SetActive(true);
                }
            }

            //Add button functionality
            foreach (Button button in mapCanvasHeader.transform.GetComponentsInChildren<Button>(true))
            {
                UnityEngine.Events.UnityAction action = null;
                bool isWoodenNickelButton = false;

                int childIndex = button.transform.GetSiblingIndex();
                switch (childIndex)
                {
                    default:
                        isWoodenNickelButton = true;
                        break;
                    case 0:
                        action = SwitchToChapter1;
                        break;
                    case 4:
                        action = SwitchToChapter2;
                        break;
                    case 8:
                        action = SwitchToChapter3;
                        break;
                    case 12:
                        action = SwitchToChapter4;
                        break;
                }

                if (!isWoodenNickelButton)
                {
                    button.onClick.AddListener(action);
                    button.gameObject.AddComponent<ButtonHoverAnimation>();
                }

                //Add button hover functionality

                //Event triggers
                EventTrigger chapterButtonEventTrigger = button.gameObject.GetComponent<EventTrigger>();
                //Mouse enter trigger
                EventTrigger.Entry mouseEnter = new EventTrigger.Entry();
                mouseEnter.eventID = EventTriggerType.PointerEnter;
                mouseEnter.callback.AddListener((data) => { MouseEnterChapterButton((PointerEventData)data, button.gameObject); });
                chapterButtonEventTrigger.triggers.Add(mouseEnter);
                //Mouse exit trigger
                EventTrigger.Entry mouseExit = new EventTrigger.Entry();
                mouseExit.eventID = EventTriggerType.PointerExit;
                mouseExit.callback.AddListener((data) => { MouseExitChapterButton((PointerEventData)data, button.gameObject); });
                chapterButtonEventTrigger.triggers.Add(mouseExit);
            }

            //Set Bum-bo sprites
            for (int childCounter = 0; childCounter < mapCanvasHeader.transform.childCount; childCounter++)
            {
                GameObject chapterMarker = mapCanvasHeader.transform.GetChild(childCounter).Find("Bum-bo Chapter Marker")?.gameObject;
                if (chapterMarker)
                {
                    chapterMarker.GetComponent<Image>().sprite = bumboHead;
                }
            }
        }

        private static void UpdateHeader()
        {
            //Move Bum-bo chapter marker
            int currentChapterIndex = (app.model.characterSheet.currentFloor * 4) - (Gambling ? 5 : 3);

            for (int chapterCounter = 0; chapterCounter < mapCanvasHeader.transform.childCount; chapterCounter++)
            {
                GameObject chapterMarker = mapCanvasHeader.transform.GetChild(chapterCounter).Find("Bum-bo Chapter Marker")?.gameObject;
                if (chapterMarker)
                {
                    chapterMarker.SetActive(chapterCounter == currentChapterIndex - 1);
                }
            }
        }

        private static void MouseEnterChapterButton(PointerEventData data, GameObject button)
        {
            button.transform.GetComponentInChildren<Text>(true).transform.parent.gameObject.SetActive(true);
        }
        private static void MouseExitChapterButton(PointerEventData data, GameObject button)
        {
            button.transform.GetComponentInChildren<Text>(true).transform.parent.gameObject.SetActive(false);
        }

        private static void SetUpRooms()
        {
            //Set Bum-bo sprites
            for (int childCounter = 0; childCounter < mapCanvasRoomContainer.transform.childCount; childCounter++)
            {
                GameObject roomMarker = mapCanvasRoomContainer.transform.GetChild(childCounter).Find("Bum-bo Room Marker")?.gameObject;
                if (roomMarker)
                {
                    roomMarker.GetComponent<Image>().sprite = bumboHead;
                }
            }
        }

        private static int currentSelectedChapter = 0;

        private static Sequence changeChapterSequence;

        private static float MapCanvasRoomContainerX { get { return roomContainerRectTransform.anchoredPosition.x; } set { roomContainerRectTransform.anchoredPosition = new Vector2(value, roomContainerRectTransform.anchoredPosition.y); } }

        private static void SwitchToChapter1() { UpdateSelectedChapter(1, true); }
        private static void SwitchToChapter2() { UpdateSelectedChapter(2, true); }
        private static void SwitchToChapter3() { UpdateSelectedChapter(3, true); }
        private static void SwitchToChapter4() { UpdateSelectedChapter(4, true); }
        private static void UpdateSelectedChapter(int chapter, bool animate)
        {
            if (chapter == currentSelectedChapter)
            {
                return;
            }

            float modifiedRoomContainerOffsetX = roomContainerOffsetX;

            //Reverse direction if selecting a lower chapter
            if (chapter < currentSelectedChapter)
            {
                modifiedRoomContainerOffsetX = -roomContainerOffsetX;
            }

            //Halt current sequence if it hasn't completed
            if (changeChapterSequence != null) frostedGlassSequence.Kill(false);

            changeChapterSequence = DOTween.Sequence();

            if (animate)
            {
                //Move room container to the side
                changeChapterSequence.Append(DOTween.To(() => MapCanvasRoomContainerX, x => MapCanvasRoomContainerX = x, roomContainerStartPos.x + modifiedRoomContainerOffsetX, 0.4f));
            }

            //Update room visuals
            changeChapterSequence.AppendCallback(delegate
            {

                MapRoom[] mapRooms = SearchMap.FindMapRooms(app.model.mapModel);

                //Move Bum-bo room marker and change opacity of rooms
                for (int roomCounter = 1; roomCounter < 7; roomCounter++)
                {
                    Transform roomTransform = mapCanvasRoomContainer.transform.Find("Room " + roomCounter.ToString());
                    Transform arrowTransform = mapCanvasRoomContainer.transform.Find("Arrow " + roomCounter.ToString());

                    Color color = new Color(1, 1, 1, (chapter == app.model.characterSheet.currentFloor ? ((mapRooms[roomCounter - 1].visited) && !Gambling) : chapter < app.model.characterSheet.currentFloor) ? 1f : opacityValue);

                    if (roomTransform != null)
                    {
                        roomTransform.Find("Bum-bo Room Marker").gameObject.SetActive(chapter == app.model.characterSheet.currentFloor ? (mapRooms[roomCounter - 1] == app.model.mapModel.currentRoom && !Gambling) : false);

                        if (roomTransform.GetComponent<Image>().color != null)
                        {
                            roomTransform.GetComponent<Image>().color = color;
                        }
                    }
                }
            });

            if (animate)
            {
                changeChapterSequence.AppendCallback(delegate
                {
                    //Move room container to the opposite side
                    MapCanvasRoomContainerX = roomContainerStartPos.x - modifiedRoomContainerOffsetX;
                });
            }

            if (animate)
            {
                changeChapterSequence.AppendInterval(0.05f);

                //Move room container back to the center
                changeChapterSequence.Append(DOTween.To(() => MapCanvasRoomContainerX, x => MapCanvasRoomContainerX = x, roomContainerStartPos.x, 0.4f).SetEase(mapTweeningEase));
            }

            //Update selected chapter
            currentSelectedChapter = chapter;
        }

        private static void SetupExit()
        {
            Button button = mapCanvasExit.GetComponent<Button>();
            button.onClick.AddListener(CloseMapMenu);
            button.gameObject.AddComponent<ButtonHoverAnimation>();
        }

        private static Sequence frostedGlassSequence;
        private static int FrostedGlassRadius { get { return mapCanvasBackground.GetComponent<Image>().material.GetInt("_Radius"); } set { mapCanvasBackground.GetComponent<Image>().material.SetInt("_Radius", value); } }
        private static float MapCanvasHeaderY { get { return headerRectTransform.anchoredPosition.y; } set { headerRectTransform.anchoredPosition = new Vector2(headerRectTransform.anchoredPosition.x, value); } }
        private static float MapCanvasRoomContainerY { get { return roomContainerRectTransform.anchoredPosition.y; } set { roomContainerRectTransform.anchoredPosition = new Vector2(roomContainerRectTransform.anchoredPosition.x, value); } }
        private static float MapCanvasExitY { get { return exitRectTransform.anchoredPosition.y; } set { exitRectTransform.anchoredPosition = new Vector2(exitRectTransform.anchoredPosition.x, value); } }

        public static void OpenMapMenu()
        {
            //Hide labels
            foreach (Text labelText in mapCanvasHeader.GetComponentsInChildren<Text>(true))
            {
                labelText.transform.parent.gameObject.SetActive(false);
            }

            //Open menu
            UpdateHeader();
            UpdateSelectedChapter(app.model.characterSheet.currentFloor, false);
            mapMenuCanvas.SetActive(true);
            app.model.paused = true;

            //Initialize sequence
            if (frostedGlassSequence != null) frostedGlassSequence.Kill(false);
            frostedGlassSequence = DOTween.Sequence();

            //Add blur
            frostedGlassSequence.Append(DOTween.To(() => FrostedGlassRadius, x => FrostedGlassRadius = x, 7, 0.4f));

            //Reveal foreground canvas elements
            frostedGlassSequence.Insert(0, DOTween.To(() => MapCanvasHeaderY, x => MapCanvasHeaderY = x, headerStartPos.y, 0.4f)).SetEase(mapTweeningEase);
            frostedGlassSequence.Insert(0, DOTween.To(() => MapCanvasRoomContainerY, x => MapCanvasRoomContainerY = x, roomContainerStartPos.y, 0.4f)).SetEase(mapTweeningEase);
            frostedGlassSequence.Insert(0, DOTween.To(() => MapCanvasExitY, x => MapCanvasExitY = x, exitStartPos.y, 0.4f)).SetEase(mapTweeningEase);
        }

        private static void CloseMapMenu()
        {
            //Initialize sequence
            if (frostedGlassSequence != null) frostedGlassSequence.Kill(false);
            frostedGlassSequence = DOTween.Sequence();

            //Remove blur
            frostedGlassSequence.Append(DOTween.To(() => FrostedGlassRadius, x => FrostedGlassRadius = x, 0, 0.4f));

            //Hide foreground canvas elements
            frostedGlassSequence.Insert(0, DOTween.To(() => MapCanvasHeaderY, x => MapCanvasHeaderY = x, headerStartPos.y + headerOffsetY, 0.4f)).SetEase(mapTweeningEase);
            frostedGlassSequence.Insert(0, DOTween.To(() => MapCanvasRoomContainerY, x => MapCanvasRoomContainerY = x, roomContainerStartPos.y + roomContainerOffsetY, 0.4f)).SetEase(mapTweeningEase);
            frostedGlassSequence.Insert(0, DOTween.To(() => MapCanvasExitY, x => MapCanvasExitY = x, exitStartPos.y + exitOffsetY, 0.4f)).SetEase(mapTweeningEase);

            frostedGlassSequence.AppendCallback(delegate
            {
                //Close menu
                mapMenuCanvas.SetActive(false);
                app.model.paused = false;
            });
        }
    }

    static class SearchMap
    {
        public static MapRoom[] FindMapRooms(MapModel mapModel)
        {
            MapRoom[] mapRooms = new MapRoom[6];
            MapRoom currentRoom = mapModel.rooms[5, 0];
            for (int roomNumber = 0; roomNumber < 6; roomNumber++)
            {
                mapRooms[roomNumber] = currentRoom;
                switch (currentRoom.exitDirection)
                {
                    default:
                        currentRoom = mapModel.rooms[currentRoom.x, currentRoom.y + 1];
                        break;
                    case MapRoom.Direction.E:
                        currentRoom = mapModel.rooms[currentRoom.x + 1, currentRoom.y];
                        break;
                    case MapRoom.Direction.W:
                        currentRoom = mapModel.rooms[currentRoom.x - 1, currentRoom.y];
                        break;
                }
            }
            return mapRooms;
        }
    }
}