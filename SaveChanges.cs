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

namespace The_Legend_of_Bum_bo_Windfall
{
    class SaveChanges
    {
        public static void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(SaveChanges));
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Applying save changes");
        }

        //Patch: Overrides save method in order to include trinket uses when saving
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
            try
            {
                StringWriter stringWriter = new StringWriter();
                xmlDocument.Save(stringWriter);

                //Rewriting SavedStateController WriteXml method instead of using it because it is private
                string xml = stringWriter.ToString();

                if (Debug.isDebugBuild)
                {
                    File.WriteAllText(FilePath.ToString(), xml);
                }
                else
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(xml);
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        byte b = ___Key[i % ___Key.Length];
                        byte[] array = bytes;
                        int num = i;
                        array[num] ^= b;
                    }
                    File.WriteAllBytes(FilePath.ToString(), bytes);
                }
                Debug.Log("Saved state to " + FilePath.ToString());
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Could not write saved state to " + FilePath.ToString() + ": " + ex.ToString());
            }

            Console.WriteLine("[The Legend of Bum-bo: Windfall] Overriding save method");
            return false;
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
        [HarmonyPrefix, HarmonyPatch(typeof(BumboController), "MakeAChampion")]
        static bool BumboController_MakeAChampion(BumboController __instance)
        {
            if (__instance.app.controller.savedStateController.IsLoading())
            {
                //Use a gameObject to indicate whether hijacked SavedStateConroller method should be overridden with champion loading code
                GameObject loadChampionsBoolean;
                loadChampionsBoolean = GameObject.Find("Load Champions Bool");
                if (loadChampionsBoolean == null)
                {
                    loadChampionsBoolean = new GameObject("Load Champions Bool");
                }
                loadChampionsBoolean.SetActive(true);

                //Loading Champions by overriding LoadStart method
                __instance.app.controller.savedStateController.LoadStart();
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Loading champion enemies");

                loadChampionsBoolean.SetActive(false);
                return false;
            }
            return true;
        }

        //Patch: Hijacks LoadStart method to load champion enemies
        [HarmonyPrefix, HarmonyPatch(typeof(SavedStateController), "LoadStart")]
        static bool SavedStateController_LoadStart(SavedStateController __instance, XmlDocument ___lDoc)
        {
            //Checking whether method should be overridden with champion loading code
            if (GameObject.Find("Load Champions Bool") != null && GameObject.Find("Load Champions Bool").activeSelf)
            {
                //Loading Champions
                if (___lDoc == null)
                {
                    return false;
                }
                XmlNode xmlNode = ___lDoc.SelectSingleNode("/save/enemies");
                XmlNode xmlNode2 = xmlNode.SelectSingleNode("ground");
                XmlNode xmlNode3 = xmlNode.SelectSingleNode("air");
                XmlNodeList xmlNodeList = xmlNode2.SelectNodes("enemy");
                for (int i = 0; i < xmlNodeList.Count; i++)
                {
                    XmlNode xmlNode4 = xmlNodeList.Item(i);
                    int num = Convert.ToInt32(xmlNode4.Attributes["x"].Value);
                    int num2 = Convert.ToInt32(xmlNode4.Attributes["y"].Value);
                    if (xmlNode4.Attributes["championStatus"] != null)
                    {
                        Enemy.ChampionType champion = (Enemy.ChampionType)Enum.Parse(typeof(Enemy.ChampionType), xmlNode4.Attributes["championStatus"].Value);
                        for (int j = 0; j < __instance.app.model.enemies.Count; j++)
                        {
                            if (__instance.app.model.enemies[j] != null && __instance.app.model.enemies[j].alive && __instance.app.model.enemies[j].championType == Enemy.ChampionType.NotAChampion && __instance.app.model.enemies[j].battlefieldPosition.x == num && __instance.app.model.enemies[j].battlefieldPosition.y == num2 && __instance.app.model.enemies[j].enemyType != Enemy.EnemyType.Flying)
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
                    int num3 = Convert.ToInt32(xmlNode5.Attributes["x"].Value);
                    int num4 = Convert.ToInt32(xmlNode5.Attributes["y"].Value);
                    if (xmlNode5.Attributes["championStatus"] != null)
                    {
                        Enemy.ChampionType champion2 = (Enemy.ChampionType)Enum.Parse(typeof(Enemy.ChampionType), xmlNode5.Attributes["championStatus"].Value);
                        for (int l = 0; l < __instance.app.model.enemies.Count; l++)
                        {
                            if (__instance.app.model.enemies[l] != null && __instance.app.model.enemies[l].alive && __instance.app.model.enemies[l].championType == Enemy.ChampionType.NotAChampion && __instance.app.model.enemies[l].battlefieldPosition.x == num3 && __instance.app.model.enemies[l].battlefieldPosition.y == num4 && __instance.app.model.enemies[l].enemyType == Enemy.EnemyType.Flying)
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
                return false;
            }
            return true;
        }
    }
}
