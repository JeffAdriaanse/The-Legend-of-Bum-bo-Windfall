using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;
using DG.Tweening;
using System.Xml;
using System.IO;
using System.Text;
using System.Reflection;
using PathologicalGames;

namespace The_Legend_of_Bum_bo_Windfall
{
    class SaveChanges
    {
        public static void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(SaveChanges));
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Applying save changes");
        }

        //Patch: initializePuzzle log
        [HarmonyPostfix, HarmonyPatch(typeof(Puzzle), "initializePuzzle")]
        static void Puzzle_initializePuzzle(Puzzle __instance)
        {
            Console.WriteLine("[The Legend of Bum-bo: Windfall] initializePuzzle, loading: " + __instance.app.controller.savedStateController.IsLoading());
        }

        //Patch: FloorStartEvent log
        [HarmonyPrefix, HarmonyPatch(typeof(FloorStartEvent), "Execute")]
        static bool FloorStartEvent_Execute(FloorStartEvent __instance)
        {
            Console.WriteLine("[The Legend of Bum-bo: Windfall] FloorStartEvent, " + __instance.app.model.mapModel.currentRoom.roomType);
            return true;
        }

        //Patch: MoveIntoRoomEvent log
        [HarmonyPostfix, HarmonyPatch(typeof(MoveIntoRoomEvent), "Execute")]
        static void MoveIntoRoomEvent_Execute(MoveIntoRoomEvent __instance)
        {
            Console.WriteLine("[The Legend of Bum-bo: Windfall] MoveIntoRoomEvent, " + __instance.app.model.mapModel.currentRoom.roomType);
        }

        //Patch: MoveIntoRoomEvent ResetRoom log
        [HarmonyPostfix, HarmonyPatch(typeof(MoveIntoRoomEvent), "ResetRoom")]
        static void MoveIntoRoomEvent_ResetRoom(MoveIntoRoomEvent __instance)
        {
            Console.WriteLine("[The Legend of Bum-bo: Windfall] ResetRoom, " + __instance.app.model.mapModel.currentRoom.roomType);
        }

        public static bool LoadIntoWoodenNickel(BumboApplication instance)
        {
            return instance.controller.gamblingController == null && PlayerPrefs.GetInt("loadGambling", 0) == 1 && loadIntoWoodenNickel && TitleController.startMode == TitleController.StartMode.Continue;
        }

        //Patch: Prevent music from playing
        [HarmonyPrefix, HarmonyPatch(typeof(LevelMusicView), "PlayLevelMusic")]
        static bool LevelMusicView_PlayLevelMusic(LevelMusicView __instance)
        {
            if (LoadIntoWoodenNickel(__instance.app))
            {
                return false;
            }
            return true;
        }

        //Patch: Loads directly to the Wooden Nickel
        [HarmonyPrefix, HarmonyPatch(typeof(FloorStartEvent), "Execute")]
        static bool FloorStartEvent_Execute_Prefix(FloorStartEvent __instance)
        {
            if (LoadIntoWoodenNickel(__instance.app))
            {
                __instance.app.controller.Init();
                __instance.app.controller.FinishFloor();
                __instance.app.controller.loadingController = UnityEngine.Object.Instantiate<GameObject>(Resources.Load("Loading") as GameObject).GetComponent<LoadingController>();
                __instance.app.controller.loadingController.loadScene("gambling");
                return false;
            }
            return true;
        }

        public static bool loadIntoWoodenNickel = true;
        //Patch: Sets loadIntoWoodenNickel to false when leaving the Wooden Nickel
        [HarmonyPostfix, HarmonyPatch(typeof(GamblingController), "StartChapterIntro")]
        static void GamblingController_StartChapterIntro(GamblingController __instance)
        {
            loadIntoWoodenNickel = false;
        }

        //Patch: Saves the shop when GamblingEvent is executed
        [HarmonyPostfix, HarmonyPatch(typeof(BumboEvent), "Execute")]
        static void BumboEvent_Execute(BumboEvent __instance)
        {
            if (__instance.app.model.bumboEvent.GetType().ToString() == "GamblingEvent" && __instance.app.controller.gamblingController != null)
            {
                SaveShop(__instance.app.controller.gamblingController.shop);
            }
        }

        //Saves the Wooden Nickel shop and the player's character sheet
        //Floor counter is reduced by 1
        public static void SaveShop(Shop __instance)
        {
            //Set flag to load into the Wooden Nickel when reloading a save (true)
            PlayerPrefs.SetInt("loadGambling", 1);
            PlayerPrefs.Save();

            XmlDocument lDoc = new XmlDocument();
            lDoc.LoadXml((string)AccessTools.Method(typeof(SavedStateController), "ReadXml").Invoke(__instance, new object[] { }));

            XmlNode xmlNode = lDoc.SelectSingleNode("save");
            XmlNode xmlNode2 = xmlNode.SelectSingleNode("gambling");

            if (xmlNode2 != null)
            {
                xmlNode2.RemoveAll();
            }
            else
            {
                xmlNode2 = lDoc.CreateElement("gambling");
                xmlNode.AppendChild(xmlNode2);
            }

            //Save shop
            for (int pickupCounter = 0; pickupCounter < 4; pickupCounter++)
            {
                Transform transform = null;
                switch (pickupCounter)
                {
                    case 0:
                        transform = __instance.item1.transform;
                        break;
                    case 1:
                        transform = __instance.item2.transform;
                        break;
                    case 2:
                        transform = __instance.item3.transform;
                        break;
                    case 3:
                        transform = __instance.item4.transform;
                        break;
                }

                GameObject pickup = null;
                for (int i = 0; i < transform.childCount; i++)
                {
                    if (transform.GetChild(i).GetComponent<HeartPickupView>() || transform.GetChild(i).GetComponent<TrinketPickupView>())
                    {
                        pickup = transform.GetChild(i).gameObject;
                        break;
                    }
                }

                if (pickup != null && pickup.activeSelf)
                {
                    XmlElement xmlElement29 = lDoc.CreateElement("pickup");
                    xmlNode2.AppendChild(xmlElement29);

                    xmlElement29.SetAttribute("index", pickupCounter.ToString());
                    if (pickup.GetComponent<HeartPickupView>())
                    {
                        xmlElement29.SetAttribute("type", "heart");
                    }
                    else if (pickup.GetComponent<TrinketPickupView>())
                    {
                        xmlElement29.SetAttribute("type", "trinket");
                        xmlElement29.SetAttribute("trinketName", pickup.GetComponent<TrinketPickupView>().trinket.trinketName.ToString());
                    }
                }
                else
                {
                    Console.WriteLine("[The Legend of Bum-bo: Windfall] Null pickup at index " + pickupCounter);
                }
            }

            //Save character sheet
            XmlNode xmlNode3 = xmlNode.SelectSingleNode("character");
            if (xmlNode3 != null)
            {
                xmlNode3.RemoveAll();
            }
            else
            {
                xmlNode3 = lDoc.CreateElement("character");
                xmlNode.AppendChild(xmlNode3);
            }

            XmlElement xmlElement = (XmlElement)xmlNode3;

            string name = "bumboType";
            int bumboType = (int)__instance.app.model.characterSheet.bumboType;
            xmlElement.SetAttribute(name, bumboType.ToString());
            XmlElement xmlElement4 = lDoc.CreateElement("baseInfo");
            xmlNode3.AppendChild(xmlElement4);
            XmlElement xmlElement5 = xmlElement4;
            string name2 = "bumboType";
            int bumboType2 = (int)__instance.app.model.characterSheet.bumboType;
            xmlElement5.SetAttribute(name2, bumboType2.ToString());
            xmlElement4.SetAttribute("hitPoints", __instance.app.model.characterSheet.bumboBaseInfo.hitPoints.ToString());
            xmlElement4.SetAttribute("soulHearts", __instance.app.model.characterSheet.bumboBaseInfo.soulHearts.ToString());
            xmlElement4.SetAttribute("itemDamage", __instance.app.model.characterSheet.bumboBaseInfo.itemDamage.ToString());
            xmlElement4.SetAttribute("luck", __instance.app.model.characterSheet.bumboBaseInfo.luck.ToString());
            xmlElement4.SetAttribute("puzzleDamage", __instance.app.model.characterSheet.bumboBaseInfo.puzzleDamage.ToString());
            xmlElement4.SetAttribute("dexterity", __instance.app.model.characterSheet.bumboBaseInfo.dexterity.ToString());
            xmlElement4.SetAttribute("coins", __instance.app.model.characterSheet.bumboBaseInfo.coins.ToString());

            for (int i = 0; i < __instance.app.model.characterSheet.bumboBaseInfo.startingSpells.Length; i++)
            {
                StartingSpell startingSpell = __instance.app.model.characterSheet.bumboBaseInfo.startingSpells[i];
                XmlElement xmlElement6 = lDoc.CreateElement("startingSpell");
                xmlElement4.AppendChild(xmlElement6);
                xmlElement6.SetAttribute("name", startingSpell.spell.ToString());
                xmlElement6.SetAttribute("boneCost", startingSpell.boneCost.ToString());
                xmlElement6.SetAttribute("heartCost", startingSpell.heartCost.ToString());
                xmlElement6.SetAttribute("poopCost", startingSpell.poopCost.ToString());
                xmlElement6.SetAttribute("boogerCost", startingSpell.boogerCost.ToString());
                xmlElement6.SetAttribute("toothCost", startingSpell.toothCost.ToString());
                xmlElement6.SetAttribute("peeCost", startingSpell.peeCost.ToString());
            }
            for (int j = 0; j < __instance.app.model.characterSheet.bumboBaseInfo.startingTrinkets.Length; j++)
            {
                TrinketName trinketName = __instance.app.model.characterSheet.bumboBaseInfo.startingTrinkets[j];
                XmlElement xmlElement7 = lDoc.CreateElement("startingTrinket");
                xmlElement4.AppendChild(xmlElement7);
                xmlElement7.SetAttribute("name", trinketName.ToString());
            }
            XmlElement xmlElement8 = lDoc.CreateElement("hiddenTrinket");
            xmlElement4.AppendChild(xmlElement8);
            xmlElement8.SetAttribute("name", __instance.app.model.characterSheet.bumboBaseInfo.hiddenTrinket.ToString());
            XmlNodeList xmlNodeList = xmlNode3.SelectNodes("spell");
            XmlNodeList xmlNodeList2 = xmlNode3.SelectNodes("trinket");
            XmlNode xmlNode4 = xmlNode3.SelectSingleNode("hiddenTrinket");
            xmlElement.SetAttribute("coins", __instance.app.model.characterSheet.coins.ToString());

            for (int k = 0; k < __instance.app.model.characterSheet.spells.Count; k++)
            {
                SpellElement spellElement = __instance.app.model.characterSheet.spells[k];
                XmlElement xmlElement9 = lDoc.CreateElement("spell");
                xmlNode3.AppendChild(xmlElement9);
                xmlElement9.SetAttribute("name", spellElement.spellName.ToString());
                xmlElement9.SetAttribute("boneCost", spellElement.Cost[0].ToString());
                xmlElement9.SetAttribute("heartCost", spellElement.Cost[1].ToString());
                xmlElement9.SetAttribute("poopCost", spellElement.Cost[2].ToString());
                xmlElement9.SetAttribute("boogerCost", spellElement.Cost[3].ToString());
                xmlElement9.SetAttribute("toothCost", spellElement.Cost[4].ToString());
                xmlElement9.SetAttribute("peeCost", spellElement.Cost[5].ToString());
                xmlElement9.SetAttribute("boneCostModifier", spellElement.CostModifier[0].ToString());
                xmlElement9.SetAttribute("heartCostModifier", spellElement.CostModifier[1].ToString());
                xmlElement9.SetAttribute("poopCostModifier", spellElement.CostModifier[2].ToString());
                xmlElement9.SetAttribute("boogerCostModifier", spellElement.CostModifier[3].ToString());
                xmlElement9.SetAttribute("toothCostModifier", spellElement.CostModifier[4].ToString());
                xmlElement9.SetAttribute("peeCostModifier", spellElement.CostModifier[5].ToString());
                xmlElement9.SetAttribute("costOverride", spellElement.CostOverride.ToString());
                xmlElement9.SetAttribute("charge", spellElement.charge.ToString());
                xmlElement9.SetAttribute("requiredCharge", spellElement.requiredCharge.ToString());
                xmlElement9.SetAttribute("chargeEveryRound", spellElement.chargeEveryRound.ToString());
                xmlElement9.SetAttribute("usedInRound", spellElement.usedInRound.ToString());
                xmlElement9.SetAttribute("baseDamage", spellElement.baseDamage.ToString());
            }
            for (int l = 0; l < __instance.app.model.characterSheet.trinkets.Count; l++)
            {
                TrinketElement trinketElement = __instance.app.model.characterSheet.trinkets[l];
                XmlElement xmlElement10 = lDoc.CreateElement("trinket");
                xmlNode3.AppendChild(xmlElement10);
                xmlElement10.SetAttribute("name", trinketElement.trinketName.ToString());
                //Saving trinket uses
                xmlElement10.SetAttribute("uses", trinketElement.uses.ToString());
            }
            if (__instance.app.model.characterSheet.hiddenTrinket != null)
            {
                XmlElement xmlElement11 = lDoc.CreateElement("hiddenTrinket");
                xmlNode3.AppendChild(xmlElement11);
                xmlElement11.SetAttribute("name", __instance.app.model.characterSheet.hiddenTrinket.trinketName.ToString());
            }

            xmlElement.SetAttribute("hitPoints", __instance.app.model.characterSheet.hitPoints.ToString());
            xmlElement.SetAttribute("soulHearts", __instance.app.model.characterSheet.soulHearts.ToString());
            xmlElement.SetAttribute("timesincestart", __instance.app.model.characterSheet.timesincestart.ToString());
            //Save previous floor
            xmlElement.SetAttribute("currentFloor", (__instance.app.model.characterSheet.currentFloor > 1 ? __instance.app.model.characterSheet.currentFloor - 1: __instance.app.model.characterSheet.currentFloor).ToString());
            XmlElement xmlElement12 = lDoc.CreateElement("floors");
            xmlNode3.AppendChild(xmlElement12);
            for (int m = 0; m < 5; m++)
            {
                XmlElement xmlElement13 = lDoc.CreateElement("floor");
                xmlElement12.AppendChild(xmlElement13);
                xmlElement13.SetAttribute("id", m.ToString());
                xmlElement13.SetAttribute("damageTaken", ((m >= __instance.app.model.characterSheet.damageTakenInFloor.Length) ? 0f : __instance.app.model.characterSheet.damageTakenInFloor[m]).ToString());
            }

            StringWriter stringWriter = new StringWriter();
            lDoc.Save(stringWriter);
            AccessTools.Method(typeof(SavedStateController), "WriteXml").Invoke(__instance, new object[] { stringWriter.ToString() });
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Saving shop and characterSheet");
        }

        //Patch: Saves coins when spending money at the Wooden Nickel
        [HarmonyPostfix, HarmonyPatch(typeof(GamblingController), "ModifyCoins")]
        static void GamblingController_ModifyCoins(GamblingController __instance)
        {
            SaveCoins(__instance.app);
        }

        //Saves player coin count
        public static void SaveCoins(BumboApplication __instance)
        {
            XmlDocument lDoc = new XmlDocument();
            lDoc.LoadXml((string)AccessTools.Method(typeof(SavedStateController), "ReadXml").Invoke(__instance, new object[] { }));

            XmlNode xmlNode = lDoc.SelectSingleNode("save/character");
            if (xmlNode != null)
            {
                XmlElement xmlElement = (XmlElement)xmlNode;
                xmlElement.SetAttribute("coins", __instance.model.characterSheet.coins.ToString());
            }

            StringWriter stringWriter = new StringWriter();
            lDoc.Save(stringWriter);
            AccessTools.Method(typeof(SavedStateController), "WriteXml").Invoke(__instance, new object[] { stringWriter.ToString() });
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Saving coin count");
        }

        //Patch: Saves damage taken
        [HarmonyPostfix, HarmonyPatch(typeof(CharacterSheet), "RegisterDamage")]
        static void CharacterSheet_RegisterDamage(CharacterSheet __instance, float damage)
        {
            if (damage > 0f)
            {
                XmlDocument lDoc = new XmlDocument();

                lDoc.LoadXml((string)AccessTools.Method(typeof(SavedStateController), "ReadXml").Invoke(__instance, new object[] { }));

                XmlNode xmlNode = lDoc.SelectSingleNode("save");

                XmlNode xmlNode2 = xmlNode.SelectSingleNode("damageTaken");

                if (xmlNode2 == null)
                {
                    //Add damage taken
                    XmlElement xmlElement = lDoc.CreateElement("damageTaken");
                    xmlNode.AppendChild(xmlElement);
                    xmlElement.SetAttribute("damage", (-damage).ToString());
                }
                else
                {
                    //Update damage taken if damage is already present
                    float damageTaken = float.Parse(xmlNode2.Attributes["damage"].Value);
                    damageTaken -= damage;
                    xmlNode2.Attributes["damage"].Value = damageTaken.ToString();
                }

                StringWriter stringWriter = new StringWriter();
                lDoc.Save(stringWriter);
                AccessTools.Method(typeof(SavedStateController), "WriteXml").Invoke(__instance, new object[] { stringWriter.ToString() });
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Saving damage taken");
            }
        }

        //Patch: Loads damage taken
        [HarmonyPostfix, HarmonyPatch(typeof(SavedStateController), "LoadCharacterSheet")]
        static void SavedStateController_LoadCharacterSheet_DamageTaken(SavedStateController __instance, XmlDocument ___lDoc)
        {
            if (___lDoc == null)
            {
                return;
            }

            XmlNode xmlNode = ___lDoc.SelectSingleNode("/save/damageTaken");
            if (xmlNode == null)
            {
                Console.WriteLine("[The Legend of Bum-bo: Windfall] damageTaken null");
                return;
            }

            float damage = float.Parse(xmlNode.Attributes["damage"].Value);

            while (damage < 0f && (__instance.app.model.characterSheet.hitPoints + __instance.app.model.characterSheet.soulHearts > 0.5f))
            {
                //RegisterDamage
                if (!(__instance.app.model.characterSheet.currentFloor < 0 || __instance.app.model.characterSheet.currentFloor >= 5))
                {
                    __instance.app.model.characterSheet.damageTakenInFloor[__instance.app.model.characterSheet.currentFloor] -= 0.5f;
                }

                //Reduce player health
                if (__instance.app.model.characterSheet.soulHearts > 0f)
                {
                    __instance.app.model.characterSheet.soulHearts -= 0.5f;
                    if (__instance.app.model.characterSheet.soulHearts < 0.5f)
                    {
                        __instance.app.model.characterSheet.soulHearts = 0f;
                    }
                }
                else if (__instance.app.model.characterSheet.hitPoints > 0.5f)
                {
                    __instance.app.model.characterSheet.hitPoints -= 0.5f;
                }
                damage += 0.5f;
            }

            __instance.app.view.hearts.GetComponent<HealthController>().UpdateHearts(true);
        }

        //Patch: Removes navigation arrows and moves camera when loading directly into a treasure room (namely, when reloading a treasure room save)
        [HarmonyPostfix, HarmonyPatch(typeof(FloorStartEvent), "Execute")]
        static void FloorStartEvent_Execute_Treasure_Room(FloorStartEvent __instance)
        {
            if (__instance.app.model.mapModel.currentRoom.roomType == MapRoom.RoomType.Treasure)
            {
                __instance.app.view.navigation.arrowNorth.SetActive(false);
                __instance.app.view.navigation.arrowSouth.SetActive(false);
                __instance.app.view.navigation.arrowEast.SetActive(false);
                __instance.app.view.navigation.arrowWest.SetActive(false);

                //Move camera to normal position for treasure room
                //Transform values copied from MoveIntoRoomEvent
                __instance.app.view.mainCameraView.transform.position = new Vector3(0f, 1f, -4.29f);
                __instance.app.view.mainCameraView.transform.eulerAngles = new Vector3(8.2f, 1.33f, 0f);
            }
        }

        //Track MoveIntoRoomEvent sequence
        static Sequence moveIntoRoomEventSequence;
        //Patch: Tracks sequence for WrapAround
        [HarmonyPrefix, HarmonyPatch(typeof(MoveIntoRoomEvent), "WrapAround")]
        static bool MoveIntoRoomEvent_WrapAround(MoveIntoRoomEvent __instance, NavigationArrowView.Direction ___direction)
        {
            __instance.app.view.mainCameraView.transform.position = new Vector3(__instance.app.view.mainCamera.transform.position.x * -1f, __instance.app.view.mainCamera.transform.position.y, __instance.app.view.mainCamera.transform.position.z);
            if (__instance.app.model.mapModel.currentRoom.roomType != MapRoom.RoomType.Treasure)
            {
                Sequence sequence = DOTween.Sequence();
                TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.Join(TweenSettingsExtensions.Append(TweenSettingsExtensions.Join(TweenSettingsExtensions.Append(sequence, TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOMove(__instance.app.view.mainCameraView.transform, __instance.app.model.cameraNavigationPosition.position + new Vector3((___direction != NavigationArrowView.Direction.Left) ? 0.333f : -0.333f, 0f, 0f), 0.35f, false), Ease.OutQuad)), TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DORotate(__instance.app.view.mainCameraView.transform, __instance.app.model.cameraNavigationPosition.rotation.eulerAngles + new Vector3(0f, 0f, (___direction != NavigationArrowView.Direction.Left) ? -3f : 3f), 0.35f, 0), Ease.OutQuad)), TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOMove(__instance.app.view.mainCameraView.transform, __instance.app.model.cameraNavigationPosition.position, 0.15f, false), Ease.InOutQuad)), TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DORotate(__instance.app.view.mainCameraView.transform, __instance.app.model.cameraNavigationPosition.rotation.eulerAngles, 0.15f, 0), Ease.InOutQuad)), delegate ()
                {
                    __instance.ResetRoom();
                });

                //Track sequence
                moveIntoRoomEventSequence = sequence;
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Tracking WrapAround sequence");
            }
            else
            {
                __instance.ResetRoom();
            }
            return false;
        }
        //Patch: Tracks sequence for HopInFront
        [HarmonyPrefix, HarmonyPatch(typeof(MoveIntoRoomEvent), "HopInFront")]
        static bool MoveIntoRoomEvent_HopInFront(MoveIntoRoomEvent __instance, NavigationArrowView.Direction ___direction)
        {
            __instance.app.view.mainCameraView.transform.position = __instance.app.model.cameraNavigationPosition.position + new Vector3(0f, 4.5f, -4f);
            if (__instance.app.model.mapModel.currentRoom.roomType != MapRoom.RoomType.Treasure)
            {
                Sequence sequence = DOTween.Sequence();
                TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.InsertCallback(TweenSettingsExtensions.Join(TweenSettingsExtensions.Join(sequence, TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOMoveZ(__instance.app.view.mainCameraView.transform, __instance.app.model.cameraNavigationPosition.position.z, 0.5f, false), Ease.InOutQuad)), TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOMoveY(__instance.app.view.mainCameraView.transform, __instance.app.model.cameraNavigationPosition.position.y, 0.5f, false), Ease.InOutQuad)), 0.05f, delegate ()
                {
                    __instance.app.view.boxes.enemyRoom3x3.GetComponent<EnemyRoomView>().ChangeLight(EnemyRoomView.RoomLightScheme.Default, 0.1f, true);
                }), delegate ()
                {
                    __instance.ResetRoom();
                });
                //Track sequence
                moveIntoRoomEventSequence = sequence;
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Tracking HopInFront sequence");
            }
            else
            {
                __instance.ResetRoom();
            }
            return false;
        }
        //Patch: Tracks sequence for HopInFront
        [HarmonyPrefix, HarmonyPatch(typeof(MoveIntoRoomEvent), "HopInBack")]
        static bool MoveIntoRoomEvent_HopInBack(MoveIntoRoomEvent __instance, NavigationArrowView.Direction ___direction)
        {
            __instance.app.view.mainCameraView.transform.position = __instance.app.model.cameraNavigationPosition.position + new Vector3(0f, 4.5f, 6f);
            if (__instance.app.model.mapModel.currentRoom.roomType != MapRoom.RoomType.Treasure)
            {
                Sequence sequence = DOTween.Sequence();
                TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.InsertCallback(TweenSettingsExtensions.Join(TweenSettingsExtensions.Join(sequence, TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOMoveZ(__instance.app.view.mainCameraView.transform, __instance.app.model.cameraNavigationPosition.position.z, 0.5f, false), Ease.InOutQuad)), TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOMoveY(__instance.app.view.mainCameraView.transform, __instance.app.model.cameraNavigationPosition.position.y, 0.5f, false), Ease.InOutQuad)), 0.05f, delegate ()
                {
                    __instance.app.view.boxes.enemyRoom3x3.GetComponent<EnemyRoomView>().ChangeLight(EnemyRoomView.RoomLightScheme.Default, 0.1f, true);
                }), delegate ()
                {
                    __instance.ResetRoom();
                });
                //Track sequence
                moveIntoRoomEventSequence = sequence;
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Tracking HopInBack sequence");
            }
            else
            {
                __instance.ResetRoom();
            }
            return false;
        }
        //Patch: Overrides BumboController StartRoom
        //Saves the game when entering treasure rooms
        //Waits for the room to be initialized before saving, which ensures that the puzzle board is saved properly
        [HarmonyPrefix, HarmonyPatch(typeof(BumboController), "StartRoom")]
        static bool BumboController_StartRoom(BumboController __instance)
        {
            __instance.boxController.ChangeRoomBox();
            __instance.app.view.decorationView.ShowDecorations();
            if (__instance.app.model.mapModel.currentRoom.coins != null && __instance.app.model.mapModel.currentRoom.coins.Length > 0)
            {
                short num = 0;
                while ((int)num < __instance.app.view.boxes.enemyRoom3x3.GetComponent<EnemyRoomView>().coinDecoration.Length)
                {
                    __instance.app.view.boxes.enemyRoom3x3.GetComponent<EnemyRoomView>().coinDecoration[(int)num].gameObject.SetActive(__instance.app.model.mapModel.currentRoom.coins[(int)num]);
                    num += 1;
                }
            }
            else
            {
                __instance.app.model.mapModel.currentRoom.coins = new bool[__instance.app.view.boxes.enemyRoom3x3.GetComponent<EnemyRoomView>().coinDecoration.Length];
                short num2 = 0;
                while ((int)num2 < __instance.app.view.boxes.enemyRoom3x3.GetComponent<EnemyRoomView>().coinDecoration.Length)
                {
                    __instance.app.model.mapModel.currentRoom.coins[(int)num2] = false;
                    __instance.app.view.boxes.enemyRoom3x3.GetComponent<EnemyRoomView>().coinDecoration[(int)num2].gameObject.SetActive(false);
                    num2 += 1;
                }
            }
            __instance.app.view.reflectionProbe.RenderProbe();
            short num3 = (short)((__instance.app.view.shitView.shit.Count - 3) / 2);
            short num4 = (short)(__instance.app.view.shitView.shit.Count - (int)num3 - 1);
            __instance.app.view.shitView.availableShit.Clear();
            for (int i = 0; i < __instance.app.view.shitView.shit.Count; i++)
            {
                if (i >= (int)num3 && i <= (int)num4)
                {
                    __instance.app.view.shitView.shit[i].gameObject.SetActive(true);
                    __instance.app.view.shitView.shit[i].Reset();
                    __instance.app.view.shitView.availableShit.Add(__instance.app.view.shitView.shit[i]);
                }
                else
                {
                    __instance.app.view.shitView.shit[i].gameObject.SetActive(false);
                }
            }
            for (short num5 = 0; num5 < 6; num5 += 1)
            {
                __instance.app.model.mana[(int)num5] = 0;
            }
            __instance.SetActiveSpells(true, true);
            __instance.StartRoomWithTrinkets();
            __instance.StartRoundWithTrinkets();
            __instance.app.model.spellModel.spellQueued = false;
            if (__instance.app.model.mapModel.currentRoom.roomType == MapRoom.RoomType.Treasure && !__instance.app.model.mapModel.currentRoom.cleared)
            {
                __instance.app.view.boxes.treasureRoom.GetComponent<TreasureRoom>().InitiateRoom();
            }
            else if (__instance.app.model.mapModel.currentRoom.roomType == MapRoom.RoomType.Shop && !__instance.app.model.mapModel.currentRoom.cleared)
            {
                __instance.app.view.boxes.shopRoom.GetComponent<ShopRoomView>().shop.Init();
            }
            if (__instance.app.controller.savedStateController.IsLoading())
            {
                if (__instance.app.model.mapModel.currentRoom.roomType == MapRoom.RoomType.Boss)
                {
                    if (__instance.app.model.characterSheet.currentFloor == 4)
                    {
                        __instance.app.view.levelMusicView.PlayFinalBossMusic();
                    }
                    else
                    {
                        __instance.app.view.levelMusicView.PlayBossMusic();
                    }
                }
                __instance.app.controller.savedStateController.LoadEnd();
            }
            else if (__instance.app.model.characterSheet.currentFloor != 0 && (__instance.app.model.mapModel.currentRoom.roomType == MapRoom.RoomType.EnemyEncounter || __instance.app.model.mapModel.currentRoom.roomType == MapRoom.RoomType.Boss || __instance.app.model.mapModel.currentRoom.roomType == MapRoom.RoomType.Start || __instance.app.model.mapModel.currentRoom.roomType == MapRoom.RoomType.Treasure) && !__instance.app.model.mapModel.currentRoom.cleared)
            {
                if (moveIntoRoomEventSequence != null && moveIntoRoomEventSequence.IsPlaying())
                {
                    //Wait for room to be initialized
                    moveIntoRoomEventSequence.OnComplete(delegate
                    {
                        __instance.app.controller.savedStateController.Save();
                        Console.WriteLine("[The Legend of Bum-bo: Windfall] Waited");
                    });
                }
                else
                {
                    __instance.app.controller.savedStateController.Save();
                    Console.WriteLine("[The Legend of Bum-bo: Windfall] Didn't wait");
                }
            }
            return false;
        }

        static TrinketName[] bossRewards;
        //Patch: Overrides save method
        //Accounts for trinket uses and enemy champion statuses
        //Checks for null enemy layouts
        //Accounts for treasure room pickups if the current room is a treasure room
        //Predetermines and saves boss room pickups if the current room is a boss room
        //Tracks whether game was saved in the Wooden Nickel
        [HarmonyPrefix, HarmonyPatch(typeof(SavedStateController), "Save")]
        static bool SavedStateController_Save(SavedStateController __instance, byte[] ___Key)
        {
            //Access FilePath
            string FilePath;
            if (Debug.isDebugBuild)
            {
                FilePath = Application.persistentDataPath + "/state.sav.xml";
            }
            else
            {
                FilePath = Application.persistentDataPath + "/state.sav";
            }

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", string.Empty));
            XmlElement xmlElement = xmlDocument.CreateElement("save");
            xmlDocument.AppendChild(xmlElement);
            XmlElement xmlElement2 = xmlDocument.CreateElement("character");
            xmlElement.AppendChild(xmlElement2);
            XmlElement xmlElement3 = xmlElement2;
            string name = "bumboType";
            int bumboType = (int)__instance.app.model.characterSheet.bumboType;
            xmlElement3.SetAttribute(name, bumboType.ToString());
            XmlElement xmlElement4 = xmlDocument.CreateElement("baseInfo");
            xmlElement2.AppendChild(xmlElement4);
            XmlElement xmlElement5 = xmlElement4;
            string name2 = "bumboType";
            int bumboType2 = (int)__instance.app.model.characterSheet.bumboType;
            xmlElement5.SetAttribute(name2, bumboType2.ToString());
            xmlElement4.SetAttribute("hitPoints", __instance.app.model.characterSheet.bumboBaseInfo.hitPoints.ToString());
            xmlElement4.SetAttribute("soulHearts", __instance.app.model.characterSheet.bumboBaseInfo.soulHearts.ToString());
            xmlElement4.SetAttribute("itemDamage", __instance.app.model.characterSheet.bumboBaseInfo.itemDamage.ToString());
            xmlElement4.SetAttribute("luck", __instance.app.model.characterSheet.bumboBaseInfo.luck.ToString());
            xmlElement4.SetAttribute("puzzleDamage", __instance.app.model.characterSheet.bumboBaseInfo.puzzleDamage.ToString());
            xmlElement4.SetAttribute("dexterity", __instance.app.model.characterSheet.bumboBaseInfo.dexterity.ToString());
            xmlElement4.SetAttribute("coins", __instance.app.model.characterSheet.bumboBaseInfo.coins.ToString());

            for (int i = 0; i < __instance.app.model.characterSheet.bumboBaseInfo.startingSpells.Length; i++)
            {
                StartingSpell startingSpell = __instance.app.model.characterSheet.bumboBaseInfo.startingSpells[i];
                XmlElement xmlElement6 = xmlDocument.CreateElement("startingSpell");
                xmlElement4.AppendChild(xmlElement6);
                xmlElement6.SetAttribute("name", startingSpell.spell.ToString());
                xmlElement6.SetAttribute("boneCost", startingSpell.boneCost.ToString());
                xmlElement6.SetAttribute("heartCost", startingSpell.heartCost.ToString());
                xmlElement6.SetAttribute("poopCost", startingSpell.poopCost.ToString());
                xmlElement6.SetAttribute("boogerCost", startingSpell.boogerCost.ToString());
                xmlElement6.SetAttribute("toothCost", startingSpell.toothCost.ToString());
                xmlElement6.SetAttribute("peeCost", startingSpell.peeCost.ToString());
            }
            for (int j = 0; j < __instance.app.model.characterSheet.bumboBaseInfo.startingTrinkets.Length; j++)
            {
                TrinketName trinketName = __instance.app.model.characterSheet.bumboBaseInfo.startingTrinkets[j];
                XmlElement xmlElement7 = xmlDocument.CreateElement("startingTrinket");
                xmlElement4.AppendChild(xmlElement7);
                xmlElement7.SetAttribute("name", trinketName.ToString());
            }
            XmlElement xmlElement8 = xmlDocument.CreateElement("hiddenTrinket");
            xmlElement4.AppendChild(xmlElement8);
            xmlElement8.SetAttribute("name", __instance.app.model.characterSheet.bumboBaseInfo.hiddenTrinket.ToString());
            XmlNodeList xmlNodeList = xmlElement2.SelectNodes("spell");
            XmlNodeList xmlNodeList2 = xmlElement2.SelectNodes("trinket");
            XmlNode xmlNode = xmlElement2.SelectSingleNode("hiddenTrinket");
            xmlElement2.SetAttribute("coins", __instance.app.model.characterSheet.coins.ToString());

            for (int k = 0; k < __instance.app.model.characterSheet.spells.Count; k++)
            {
                SpellElement spellElement = __instance.app.model.characterSheet.spells[k];
                XmlElement xmlElement9 = xmlDocument.CreateElement("spell");
                xmlElement2.AppendChild(xmlElement9);
                xmlElement9.SetAttribute("name", spellElement.spellName.ToString());
                xmlElement9.SetAttribute("boneCost", spellElement.Cost[0].ToString());
                xmlElement9.SetAttribute("heartCost", spellElement.Cost[1].ToString());
                xmlElement9.SetAttribute("poopCost", spellElement.Cost[2].ToString());
                xmlElement9.SetAttribute("boogerCost", spellElement.Cost[3].ToString());
                xmlElement9.SetAttribute("toothCost", spellElement.Cost[4].ToString());
                xmlElement9.SetAttribute("peeCost", spellElement.Cost[5].ToString());
                xmlElement9.SetAttribute("boneCostModifier", spellElement.CostModifier[0].ToString());
                xmlElement9.SetAttribute("heartCostModifier", spellElement.CostModifier[1].ToString());
                xmlElement9.SetAttribute("poopCostModifier", spellElement.CostModifier[2].ToString());
                xmlElement9.SetAttribute("boogerCostModifier", spellElement.CostModifier[3].ToString());
                xmlElement9.SetAttribute("toothCostModifier", spellElement.CostModifier[4].ToString());
                xmlElement9.SetAttribute("peeCostModifier", spellElement.CostModifier[5].ToString());
                xmlElement9.SetAttribute("costOverride", spellElement.CostOverride.ToString());
                xmlElement9.SetAttribute("charge", spellElement.charge.ToString());
                xmlElement9.SetAttribute("requiredCharge", spellElement.requiredCharge.ToString());
                xmlElement9.SetAttribute("chargeEveryRound", spellElement.chargeEveryRound.ToString());
                xmlElement9.SetAttribute("usedInRound", spellElement.usedInRound.ToString());
                xmlElement9.SetAttribute("baseDamage", spellElement.baseDamage.ToString());
            }
            for (int l = 0; l < __instance.app.model.characterSheet.trinkets.Count; l++)
            {
                TrinketElement trinketElement = __instance.app.model.characterSheet.trinkets[l];
                XmlElement xmlElement10 = xmlDocument.CreateElement("trinket");
                xmlElement2.AppendChild(xmlElement10);
                xmlElement10.SetAttribute("name", trinketElement.trinketName.ToString());
                //Saving trinket uses
                xmlElement10.SetAttribute("uses", trinketElement.uses.ToString());
            }
            if (__instance.app.model.characterSheet.hiddenTrinket != null)
            {
                XmlElement xmlElement11 = xmlDocument.CreateElement("hiddenTrinket");
                xmlElement2.AppendChild(xmlElement11);
                xmlElement11.SetAttribute("name", __instance.app.model.characterSheet.hiddenTrinket.trinketName.ToString());
            }

            xmlElement2.SetAttribute("hitPoints", __instance.app.model.characterSheet.hitPoints.ToString());
            xmlElement2.SetAttribute("soulHearts", __instance.app.model.characterSheet.soulHearts.ToString());
            xmlElement2.SetAttribute("timesincestart", __instance.app.model.characterSheet.timesincestart.ToString());
            xmlElement2.SetAttribute("currentFloor", __instance.app.model.characterSheet.currentFloor.ToString());
            XmlElement xmlElement12 = xmlDocument.CreateElement("floors");
            xmlElement2.AppendChild(xmlElement12);
            for (int m = 0; m < 5; m++)
            {
                XmlElement xmlElement13 = xmlDocument.CreateElement("floor");
                xmlElement12.AppendChild(xmlElement13);
                xmlElement13.SetAttribute("id", m.ToString());
                xmlElement13.SetAttribute("damageTaken", ((m >= __instance.app.model.characterSheet.damageTakenInFloor.Length) ? 0f : __instance.app.model.characterSheet.damageTakenInFloor[m]).ToString());
            }

            XmlElement xmlElement14 = xmlDocument.CreateElement("map");
            xmlElement.AppendChild(xmlElement14);
            xmlElement14.SetAttribute("roomLayoutWidth", __instance.app.model.mapModel.rooms.GetLength(0).ToString());
            xmlElement14.SetAttribute("roomLayoutHeight", __instance.app.model.mapModel.rooms.GetLength(1).ToString());
            xmlElement14.SetAttribute("bossName", __instance.app.model.bossName.ToString());
            for (int n = 0; n < __instance.app.model.mapModel.rooms.GetLength(1); n++)
            {
                for (int num = 0; num < __instance.app.model.mapModel.rooms.GetLength(0); num++)
                {
                    MapRoom mapRoom = __instance.app.model.mapModel.rooms[num, n];
                    XmlElement xmlElement15 = xmlDocument.CreateElement("room");
                    xmlElement14.AppendChild(xmlElement15);
                    xmlElement15.SetAttribute("x", num.ToString());
                    xmlElement15.SetAttribute("y", n.ToString());
                    string text = string.Empty;
                    if (mapRoom.doors[MapRoom.Direction.N])
                    {
                        text += "N";
                    }
                    if (mapRoom.doors[MapRoom.Direction.E])
                    {
                        text += "E";
                    }
                    if (mapRoom.doors[MapRoom.Direction.S])
                    {
                        text += "S";
                    }
                    if (mapRoom.doors[MapRoom.Direction.W])
                    {
                        text += "W";
                    }
                    xmlElement15.SetAttribute("doors", text);
                    xmlElement15.SetAttribute("type", ((int)mapRoom.roomType).ToString());
                    xmlElement15.SetAttribute("difficulty", mapRoom.difficulty.ToString());
                    xmlElement15.SetAttribute("cleared", mapRoom.cleared.ToString());
                    xmlElement15.SetAttribute("exitDirection", mapRoom.exitDirection.ToString());
                }
                xmlElement14.SetAttribute("currentRoomX", __instance.app.model.mapModel.currentRoom.x.ToString());
                xmlElement14.SetAttribute("currentRoomY", __instance.app.model.mapModel.currentRoom.y.ToString());
            }

            XmlElement xmlElement16 = xmlDocument.CreateElement("enemies");
            xmlElement.AppendChild(xmlElement16);
            xmlElement16.SetAttribute("difficulty", __instance.app.model.mapModel.currentRoomEnemyLayout.difficulty.ToString());
            XmlElement xmlElement17 = xmlDocument.CreateElement("ground");
            xmlElement16.AppendChild(xmlElement17);
            XmlElement xmlElement18 = xmlDocument.CreateElement("air");
            xmlElement16.AppendChild(xmlElement18);


            //Null check for enemy layouts
            //Enemy layouts will be null when entering a boss room after reloading from the preceding treasure room

            //Ground enemies
            if (__instance.app.model.mapModel.currentRoomEnemyLayout.groundEnemies != null)
            {
                for (int num2 = 0; num2 < __instance.app.model.mapModel.currentRoomEnemyLayout.groundEnemies.Count; num2++)
                {
                    for (int num3 = 0; num3 < __instance.app.model.mapModel.currentRoomEnemyLayout.groundEnemies[num2].Count; num3++)
                    {
                        EnemyName enemyName = __instance.app.model.mapModel.currentRoomEnemyLayout.groundEnemies[num2][num3];
                        XmlElement xmlElement19 = xmlDocument.CreateElement("enemy");
                        xmlElement17.AppendChild(xmlElement19);
                        xmlElement19.SetAttribute("name", enemyName.ToString());
                        xmlElement19.SetAttribute("x", num3.ToString());
                        xmlElement19.SetAttribute("y", num2.ToString());

                        //Save ground enemy champion status
                        if (__instance.app.controller.GetGroundEnemy(num3, num2) != null)
                        {
                            xmlElement19.SetAttribute("championStatus", __instance.app.controller.GetGroundEnemy(num3, num2).championType.ToString());
                        }
                    }
                }
            }
            //Air enemies
            if (__instance.app.model.mapModel.currentRoomEnemyLayout.airEnemies != null)
            {
                for (int num4 = 0; num4 < __instance.app.model.mapModel.currentRoomEnemyLayout.airEnemies.Count; num4++)
                {
                    for (int num5 = 0; num5 < __instance.app.model.mapModel.currentRoomEnemyLayout.airEnemies[num4].Count; num5++)
                    {
                        EnemyName enemyName2 = __instance.app.model.mapModel.currentRoomEnemyLayout.airEnemies[num4][num5];
                        XmlElement xmlElement20 = xmlDocument.CreateElement("enemy");
                        xmlElement18.AppendChild(xmlElement20);
                        xmlElement20.SetAttribute("name", enemyName2.ToString());
                        xmlElement20.SetAttribute("x", num5.ToString());
                        xmlElement20.SetAttribute("y", num4.ToString());

                        //Save air enemy champion status
                        if (__instance.app.controller.GetAirEnemy(num5, num4) != null)
                        {
                            xmlElement20.SetAttribute("championStatus", __instance.app.controller.GetAirEnemy(num5, num4).championType.ToString());
                        }
                    }
                }
            }

            //Save treasure room pickups
            if (__instance.app.model.mapModel.currentRoom.roomType == MapRoom.RoomType.Treasure && __instance.app.view.boxes.treasureRoom.GetComponent<TreasureRoom>())
            {
                XmlElement xmlElement24 = xmlDocument.CreateElement("treasure");
                xmlElement.AppendChild(xmlElement24);

                List<GameObject> pickups = (List<GameObject>)typeof(TreasureRoom).GetField("pickups", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance.app.view.boxes.treasureRoom.GetComponent<TreasureRoom>());

                for (int pickupCounter = 0; pickupCounter < pickups.Count; pickupCounter++)
                {
                    XmlElement xmlElement25 = xmlDocument.CreateElement("pickup");
                    xmlElement24.AppendChild(xmlElement25);

                    SpellElement spell = pickups[pickupCounter].GetComponent<SpellPickup>() ? pickups[pickupCounter].GetComponent<SpellPickup>().spell : null;
                    TrinketElement trinket = pickups[pickupCounter].GetComponent<TrinketPickupView>() ? pickups[pickupCounter].GetComponent<TrinketPickupView>().trinket : null;

                    xmlElement25.SetAttribute("type", spell != null ? "spell" : "trinket");

                    xmlElement25.SetAttribute("name", spell != null ? spell.spellName.ToString() : trinket.trinketName.ToString());

                    if (spell != null)
                    {
                        XmlElement xmlElement26 = xmlDocument.CreateElement("cost");
                        xmlElement25.AppendChild(xmlElement26);
                        xmlElement26.SetAttribute("boneCost", spell.Cost[0].ToString());
                        xmlElement26.SetAttribute("heartCost", spell.Cost[1].ToString());
                        xmlElement26.SetAttribute("poopCost", spell.Cost[2].ToString());
                        xmlElement26.SetAttribute("boogerCost", spell.Cost[3].ToString());
                        xmlElement26.SetAttribute("toothCost", spell.Cost[4].ToString());
                        xmlElement26.SetAttribute("peeCost", spell.Cost[5].ToString());
                    }
                }
            }

            //Predetermine and save boss room pickups
            if (__instance.app.model.mapModel.currentRoom.roomType == MapRoom.RoomType.Boss)
            {
                bossRewards = new TrinketName[2];
                //Random trinket
                bossRewards[0] = __instance.app.model.trinketModel.validTrinkets[UnityEngine.Random.Range(0, __instance.app.model.trinketModel.validTrinkets.Count)];

                //Random trinket that is not the same as the first trinket
                List<TrinketName> list = new List<TrinketName>();
                for (int i = 1; i < __instance.app.model.trinketModel.validTrinkets.Count; i++)
                {
                    bool flag = false;
                    if (bossRewards[0] == __instance.app.model.trinketModel.validTrinkets[i])
                    {
                        flag = true;
                    }

                    if (!flag)
                    {
                        list.Add(__instance.app.model.trinketModel.validTrinkets[i]);
                    }
                }
                if (list.Count > 0)
                {
                    int index = UnityEngine.Random.Range(0, list.Count);
                    bossRewards[1] = list[index];
                }
                else
                {
                    bossRewards[1] = __instance.app.model.trinketModel.validTrinkets[UnityEngine.Random.Range(0, __instance.app.model.trinketModel.validTrinkets.Count)];
                }
                XmlElement xmlElement27 = xmlDocument.CreateElement("bossRewards");
                xmlElement.AppendChild(xmlElement27);
                xmlElement27.SetAttribute("reward0", bossRewards[0].ToString());
                xmlElement27.SetAttribute("reward1", bossRewards[1].ToString());
            }
            else
            {
                bossRewards = null;
            }

            XmlElement xmlElement21 = xmlDocument.CreateElement("puzzle");
            xmlElement.AppendChild(xmlElement21);
            xmlElement21.SetAttribute("width", __instance.app.view.puzzle.blocks.GetLength(0).ToString());
            xmlElement21.SetAttribute("height", __instance.app.view.puzzle.blocks.GetLength(1).ToString());
            for (int num6 = 0; num6 < __instance.app.view.puzzle.blocks.GetLength(1); num6++)
            {
                for (int num7 = 0; num7 < __instance.app.view.puzzle.blocks.GetLength(0); num7++)
                {
                    XmlElement xmlElement22 = xmlDocument.CreateElement("block");
                    xmlElement21.AppendChild(xmlElement22);
                    xmlElement22.SetAttribute("x", num7.ToString());
                    xmlElement22.SetAttribute("y", num6.ToString());
                    xmlElement22.SetAttribute("type", __instance.app.view.puzzle.blocks[num7, num6].GetComponent<Block>().block_type.ToString());
                }
            }
            for (int num8 = 0; num8 < __instance.app.view.puzzle.nextBlockViews.Count; num8++)
            {
                XmlElement xmlElement23 = xmlDocument.CreateElement("nextblock");
                xmlElement21.AppendChild(xmlElement23);
                xmlElement23.SetAttribute("x", num8.ToString());
                xmlElement23.SetAttribute("type", __instance.app.view.puzzle.nextBlockViews[num8].GetCurrentBlockType().ToString());
            }

            if (__instance.app.controller.gamblingController == null)
            {
                //Set flag to load into the Wooden Nickel when reloading a save (false)
                PlayerPrefs.SetInt("loadGambling", 0);
                PlayerPrefs.Save();
            }

            //Set loadIntoWoodenNickel to true
            loadIntoWoodenNickel = true;

            try
            {
                StringWriter stringWriter = new StringWriter();
                xmlDocument.Save(stringWriter);
                AccessTools.Method(typeof(SavedStateController), "WriteXml").Invoke(__instance, new object[] { stringWriter.ToString() });
                Debug.Log("Saved state to " + FilePath.ToString());
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Could not write saved state to " + FilePath.ToString() + ": " + ex.ToString());
            }

            Console.WriteLine("[The Legend of Bum-bo: Windfall] Overriding save method");
            return false;
        }

        //Patch: Loads saved pickups when reloading a save in a treasure room
        [HarmonyPostfix, HarmonyPatch(typeof(TreasureRoom), "InitiateRoom")]
        static void TreasureRoom_InitiateRoom(TreasureRoom __instance, ref List<GameObject> ___pickups)
        {
            if (!__instance.app.controller.savedStateController.IsLoading())
            {
                return;
            }

            foreach (GameObject pickup in ___pickups)
            {
                UnityEngine.Object.Destroy(pickup);
            }
            ___pickups.Clear();

            XmlDocument lDoc = (XmlDocument)typeof(SavedStateController).GetField("lDoc", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance.app.controller.savedStateController);

            if (lDoc != null)
            {
                XmlNode xmlNode = lDoc.SelectSingleNode("/save/treasure");

                XmlNodeList xmlNodeList = xmlNode.SelectNodes("pickup");
                for (int i = 0; i < xmlNodeList.Count; i++)
                {
                    XmlNode xmlNode2 = xmlNodeList[i];
                    string type = xmlNode2.Attributes["type"].Value;
                    
                    if (type == "spell")
                    {
                        AccessTools.Method(typeof(TreasureRoom), "SetSpell", new Type[] { typeof(int), typeof(List<SpellName>), typeof(SpellElement.SpellCategory) }).Invoke(__instance, new object[] { i + 1, new List<SpellName>(), SpellElement.SpellCategory.Attack });

                        XmlNode xmlNode3 = xmlNode2.SelectSingleNode("cost");
                        short[] newCost = new short[]
                        {
                            Convert.ToInt16(xmlNode3.Attributes["boneCost"].Value),
                            Convert.ToInt16(xmlNode3.Attributes["heartCost"].Value),
                            Convert.ToInt16(xmlNode3.Attributes["poopCost"].Value),
                            Convert.ToInt16(xmlNode3.Attributes["boogerCost"].Value),
                            Convert.ToInt16(xmlNode3.Attributes["toothCost"].Value),
                            Convert.ToInt16(xmlNode3.Attributes["peeCost"].Value),
                        };

                        ___pickups[i].GetComponent<SpellPickup>().SetSpell(CharacterSheet.BumboType.TheBrave, (SpellName)Enum.Parse(typeof(SpellName), xmlNode2.Attributes["name"].Value), newCost, __instance.app.model.spellModel);
                    }
                    else
                    {
                        AccessTools.Method(typeof(TreasureRoom), "SetTrinkets", new Type[] { typeof(int), typeof(List<TrinketName>) }).Invoke(__instance, new object[] { i + 1, new List<TrinketName>() });
                        ___pickups[i].GetComponent<TrinketPickupView>().SetTrinket((TrinketName)Enum.Parse(typeof(TrinketName), xmlNode2.Attributes["name"].Value));
                    }
                }
            }
        }

        //Patch: Loads boss room pickups
        [HarmonyPostfix, HarmonyPatch(typeof(SavedStateController), "LoadBoss")]
        static void SavedStateController_LoadBoss(SavedStateController __instance, XmlDocument ___lDoc)
        {
            if (___lDoc == null)
            {
                return;
            }

            XmlNode xmlNode = ___lDoc.SelectSingleNode("/save/bossRewards");
            if (xmlNode != null)
            {
                bossRewards = new TrinketName[]
                {
                    (TrinketName)Enum.Parse(typeof(TrinketName), xmlNode.Attributes["reward0"].Value),
                    (TrinketName)Enum.Parse(typeof(TrinketName), xmlNode.Attributes["reward1"].Value)
                };
            }
        }
        //Patch: Changes boss room pickups to loaded pickups
        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), "CreateBossRewards")]
        static void BumboController_CreateBossRewards(BumboController __instance)
        {
            if (bossRewards != null)
            {
                __instance.app.view.bossRewardParents[0].transform.GetChild(0).GetComponent<TrinketPickupView>().SetTrinket(bossRewards[0], 0);
                __instance.app.view.bossRewardParents[1].transform.GetChild(0).GetComponent<TrinketPickupView>().SetTrinket(bossRewards[1], 0);
            }
        }

        //Patch: Overrides LoadCharacterSheet method in order to include trinket uses when reloading a save
        [HarmonyPrefix, HarmonyPatch(typeof(SavedStateController), "LoadCharacterSheet")]
        static bool SavedStateController_LoadCharacterSheet(SavedStateController __instance, XmlDocument ___lDoc)
        {
            if (___lDoc == null)
            {
                return false;
            }
            XmlNode xmlNode = ___lDoc.SelectSingleNode("/save/character");
            XmlNodeList xmlNodeList = xmlNode.SelectNodes("floors/floor");
            XmlNodeList xmlNodeList2 = xmlNode.SelectNodes("spell");
            XmlNodeList xmlNodeList3 = xmlNode.SelectNodes("trinket");
            XmlNode xmlNode2 = xmlNode.SelectSingleNode("hiddenTrinket");
            XmlNode xmlNode3 = xmlNode.SelectSingleNode("baseInfo");
            XmlNodeList xmlNodeList4 = xmlNode3.SelectNodes("startingSpell");
            XmlNodeList xmlNodeList5 = xmlNode3.SelectNodes("startingTrinket");
            XmlNode xmlNode4 = xmlNode3.SelectSingleNode("hiddenTrinket");
            __instance.app.model.characterSheet.bumboType = (CharacterSheet.BumboType)Convert.ToInt32(xmlNode.Attributes["bumboType"].Value);
            __instance.app.model.characterSheet.bumboBaseInfo = new BumboObject
            {
                bumboType = __instance.app.model.characterSheet.bumboType,
                hitPoints = Convert.ToSingle(xmlNode3.Attributes["hitPoints"].Value),
                soulHearts = Convert.ToSingle(xmlNode3.Attributes["soulHearts"].Value),
                itemDamage = Convert.ToInt32(xmlNode3.Attributes["itemDamage"].Value),
                luck = Convert.ToInt32(xmlNode3.Attributes["luck"].Value),
                puzzleDamage = Convert.ToInt32(xmlNode3.Attributes["puzzleDamage"].Value),
                dexterity = Convert.ToInt32(xmlNode3.Attributes["dexterity"].Value),
                coins = Convert.ToInt32(xmlNode3.Attributes["coins"].Value),
                startingSpells = new StartingSpell[xmlNodeList4.Count],
                startingTrinkets = new TrinketName[xmlNodeList5.Count],
                hiddenTrinket = (TrinketName)Enum.Parse(typeof(TrinketName), xmlNode4.Attributes["name"].Value)
            };
            for (int i = 0; i < xmlNodeList4.Count; i++)
            {
                XmlNode xmlNode5 = xmlNodeList4.Item(i);
                __instance.app.model.characterSheet.bumboBaseInfo.startingSpells[i] = new StartingSpell
                {
                    spell = (SpellName)Enum.Parse(typeof(SpellName), xmlNode5.Attributes["name"].Value),
                    boneCost = Convert.ToInt32(xmlNode5.Attributes["boneCost"].Value),
                    heartCost = Convert.ToInt32(xmlNode5.Attributes["heartCost"].Value),
                    poopCost = Convert.ToInt32(xmlNode5.Attributes["poopCost"].Value),
                    boogerCost = Convert.ToInt32(xmlNode5.Attributes["boogerCost"].Value),
                    toothCost = Convert.ToInt32(xmlNode5.Attributes["toothCost"].Value),
                    peeCost = Convert.ToInt32(xmlNode5.Attributes["peeCost"].Value)
                };
            }
            for (int j = 0; j < xmlNodeList5.Count; j++)
            {
                __instance.app.model.characterSheet.bumboBaseInfo.startingTrinkets[j] = (TrinketName)Enum.Parse(typeof(TrinketName), xmlNodeList5.Item(j).Attributes["name"].Value);
            }
            __instance.app.model.characterSheet.coins = Convert.ToInt32(xmlNode.Attributes["coins"].Value);
            for (int k = 0; k < xmlNodeList2.Count; k++)
            {
                XmlNode xmlNode6 = xmlNodeList2.Item(k);
                try
                {
                    SpellName key = (SpellName)Enum.Parse(typeof(SpellName), xmlNode6.Attributes["name"].Value);
                    SpellElement spellElement = __instance.app.model.spellModel.spells[key];
                    spellElement.Cost = new short[]
                    {
                Convert.ToInt16(xmlNode6.Attributes["boneCost"].Value),
                Convert.ToInt16(xmlNode6.Attributes["heartCost"].Value),
                Convert.ToInt16(xmlNode6.Attributes["poopCost"].Value),
                Convert.ToInt16(xmlNode6.Attributes["boogerCost"].Value),
                Convert.ToInt16(xmlNode6.Attributes["toothCost"].Value),
                Convert.ToInt16(xmlNode6.Attributes["peeCost"].Value)
                    };
                    spellElement.CostModifier = new short[]
                    {
                Convert.ToInt16(xmlNode6.Attributes["boneCostModifier"].Value),
                Convert.ToInt16(xmlNode6.Attributes["heartCostModifier"].Value),
                Convert.ToInt16(xmlNode6.Attributes["poopCostModifier"].Value),
                Convert.ToInt16(xmlNode6.Attributes["boogerCostModifier"].Value),
                Convert.ToInt16(xmlNode6.Attributes["toothCostModifier"].Value),
                Convert.ToInt16(xmlNode6.Attributes["peeCostModifier"].Value)
                    };
                    spellElement.CostOverride = Convert.ToBoolean(xmlNode6.Attributes["costOverride"].Value);
                    spellElement.charge = (int)Convert.ToInt16(xmlNode6.Attributes["charge"].Value);
                    spellElement.requiredCharge = (int)Convert.ToInt16(xmlNode6.Attributes["requiredCharge"].Value);
                    if (xmlNode6.Attributes["chargeEveryRound"] != null)
                    {
                        spellElement.chargeEveryRound = Convert.ToBoolean(xmlNode6.Attributes["chargeEveryRound"].Value);
                    }
                    if (xmlNode6.Attributes["usedInRound"] != null)
                    {
                        spellElement.usedInRound = Convert.ToBoolean(xmlNode6.Attributes["usedInRound"].Value);
                    }
                    if (xmlNode6.Attributes["baseDamage"] != null)
                    {
                        spellElement.baseDamage = Convert.ToInt32(xmlNode6.Attributes["baseDamage"].Value);
                    }
                    __instance.app.model.characterSheet.spells.Add(spellElement);
                }
                catch (Exception)
                {
                    Debug.LogWarning(string.Format("LoadCharacterSheet: Could not load spell {0}!", xmlNode6.Attributes["name"].Value));
                }
            }
            for (int l = 0; l < xmlNodeList3.Count; l++)
            {
                XmlNode xmlNode7 = xmlNodeList3.Item(l);
                try
                {
                    TrinketName key2 = (TrinketName)Enum.Parse(typeof(TrinketName), xmlNode7.Attributes["name"].Value);
                    TrinketElement trinketElement = __instance.app.model.trinketModel.trinkets[key2];
                    //Load trinket uses
                    trinketElement.uses = (int)Convert.ToInt16(xmlNode7.Attributes["uses"].Value);
                    __instance.app.model.characterSheet.trinkets.Add(trinketElement);
                }
                catch (Exception)
                {
                    Debug.LogWarning(string.Format("LoadCharacterSheet: Could not load trinket {0}!", xmlNode7.Attributes["name"].Value));
                }
            }
            if (xmlNode2 != null)
            {
                try
                {
                    TrinketName key3 = (TrinketName)Enum.Parse(typeof(TrinketName), xmlNode2.Attributes["name"].Value);
                    __instance.app.model.characterSheet.hiddenTrinket = __instance.app.model.trinketModel.trinkets[key3];
                }
                catch (Exception)
                {
                    Debug.LogWarning(string.Format("LoadCharacterSheet: Could not load hidden trinket {0}!", xmlNode2.Attributes["name"].Value));
                }
            }
            __instance.app.model.characterSheet.hitPoints = Convert.ToSingle(xmlNode.Attributes["hitPoints"].Value);
            __instance.app.model.characterSheet.soulHearts = Convert.ToSingle(xmlNode.Attributes["soulHearts"].Value);
            __instance.app.model.characterSheet.timesincestart = Convert.ToSingle(xmlNode.Attributes["timesincestart"].Value);
            __instance.app.model.characterSheet.currentFloor = Convert.ToInt32(xmlNode.Attributes["currentFloor"].Value);
            __instance.app.model.characterSheet.damageTakenInFloor = new float[xmlNodeList.Count];
            for (int m = 0; m < xmlNodeList.Count; m++)
            {
                int num = Convert.ToInt32(xmlNodeList[m].Attributes["id"].Value);
                if (num < 0 || num >= xmlNodeList.Count)
                {
                    Debug.LogWarning("Invalid floor ID: " + num);
                }
                else
                {
                    __instance.app.model.characterSheet.damageTakenInFloor[num] = Convert.ToSingle(xmlNodeList[m].Attributes["damageTaken"].Value);
                }
            }

            Console.WriteLine("[The Legend of Bum-bo: Windfall] Overriding LoadCharacterSheet method");
            return false;
        }

        //Patch: Overrides MakeAChampion method to load champion enemies
        //Also changes champion generation such that each enemy has a chance to spawn as a champion instead of only one champion at most per room
        //Champions are now more common once the player has unlocked 'Everything Is Terrible!'
        [HarmonyPrefix, HarmonyPatch(typeof(BumboController), "MakeAChampion")]
        static bool BumboController_MakeAChampion(BumboController __instance)
        {
            if (__instance.app.controller.savedStateController.IsLoading())
            {
                //Loading Champions
                XmlDocument lDoc = (XmlDocument)typeof(SavedStateController).GetField("lDoc", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance.app.controller.savedStateController);
                if (lDoc == null)
                {
                    return false;
                }
                XmlNode xmlNode = lDoc.SelectSingleNode("/save/enemies");
                XmlNode xmlNode2 = xmlNode.SelectSingleNode("ground");
                XmlNode xmlNode3 = xmlNode.SelectSingleNode("air");
                XmlNodeList xmlNodeList = xmlNode2.SelectNodes("enemy");
                for (int i = 0; i < xmlNodeList.Count; i++)
                {
                    XmlNode xmlNode4 = xmlNodeList.Item(i);
                    int groundX = Convert.ToInt32(xmlNode4.Attributes["x"].Value);
                    int groundY = Convert.ToInt32(xmlNode4.Attributes["y"].Value);
                    if (xmlNode4.Attributes["championStatus"] != null)
                    {
                        Enemy.ChampionType champion = (Enemy.ChampionType)Enum.Parse(typeof(Enemy.ChampionType), xmlNode4.Attributes["championStatus"].Value);
                        for (int j = 0; j < __instance.app.model.enemies.Count; j++)
                        {
                            if (__instance.app.model.enemies[j] != null && __instance.app.model.enemies[j].alive && __instance.app.model.enemies[j].championType == Enemy.ChampionType.NotAChampion && __instance.app.model.enemies[j].battlefieldPosition.x == groundX && __instance.app.model.enemies[j].battlefieldPosition.y == groundY && __instance.app.model.enemies[j].enemyType != Enemy.EnemyType.Flying)
                            {
                                __instance.app.model.enemies[j].SetChampion(champion);
                                if (__instance.app.model.enemies[j].championType != Enemy.ChampionType.NotAChampion)
                                {
                                    __instance.app.model.enemies[j].Init();
                                }
                            }
                        }
                    }
                }
                XmlNodeList xmlNodeList2 = xmlNode3.SelectNodes("enemy");
                for (int k = 0; k < xmlNodeList2.Count; k++)
                {
                    XmlNode xmlNode5 = xmlNodeList2.Item(k);
                    int airX = Convert.ToInt32(xmlNode5.Attributes["x"].Value);
                    int airY = Convert.ToInt32(xmlNode5.Attributes["y"].Value);
                    if (xmlNode5.Attributes["championStatus"] != null)
                    {
                        Enemy.ChampionType champion2 = (Enemy.ChampionType)Enum.Parse(typeof(Enemy.ChampionType), xmlNode5.Attributes["championStatus"].Value);
                        for (int l = 0; l < __instance.app.model.enemies.Count; l++)
                        {
                            if (__instance.app.model.enemies[l] != null && __instance.app.model.enemies[l].alive && __instance.app.model.enemies[l].championType == Enemy.ChampionType.NotAChampion && __instance.app.model.enemies[l].battlefieldPosition.x == airX && __instance.app.model.enemies[l].battlefieldPosition.y == airY && __instance.app.model.enemies[l].enemyType == Enemy.EnemyType.Flying)
                            {
                                __instance.app.model.enemies[l].SetChampion(champion2);
                                if (__instance.app.model.enemies[l].championType != Enemy.ChampionType.NotAChampion)
                                {
                                    __instance.app.model.enemies[l].Init();
                                }
                            }
                        }
                    }
                }
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Loading champion enemies");
                return false;
            }

            Console.WriteLine("[The Legend of Bum-bo: Windfall] Changing enemy champion generation");
            float[] array = new float[]
            {
                0f,
                0.1f,
                0.125f,
                0.15f,
                0.175f
            };
            float NegaCrownMultiplier = 1f;
            float difficultyMultiplier = (!__instance.app.model.progression.unlocks[7]) ? 0.5f : 0.7f;
            for (int i = 0; i < __instance.app.model.characterSheet.trinkets.Count; i++)
            {
                NegaCrownMultiplier -= __instance.GetTrinket(i).ChampionChance();
            }
            if (__instance.app.model.mapModel.currentRoom.roomType != MapRoom.RoomType.Boss)
            {
                List<Enemy> list = new List<Enemy>();
                for (int j = 0; j < __instance.app.model.enemies.Count; j++)
                {
                    if (__instance.app.model.enemies[j].alive && __instance.app.model.enemies[j].championType == Enemy.ChampionType.NotAChampion)
                    {
                        list.Add(__instance.app.model.enemies[j]);
                    }
                }
                while (list.Count > 0)
                {
                    int index = UnityEngine.Random.Range(0, list.Count);
                    if (UnityEngine.Random.Range(0f, 1f) < array[__instance.app.model.characterSheet.currentFloor] * NegaCrownMultiplier * difficultyMultiplier)
                    {
                        Enemy.ChampionType champion = (Enemy.ChampionType)UnityEngine.Random.Range(1, 10);
                        list[index].SetChampion(champion);
                        list[index].Init();
                    }
                    list.RemoveAt(index);
                }
            }
            return false;
        }
    }
}
