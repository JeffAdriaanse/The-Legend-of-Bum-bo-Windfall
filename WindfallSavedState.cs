using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
	public static class WindfallSavedState
	{
        private static readonly string fileName = "windfallstate.sav";
        private static string FilePath { get { return WindfallHelper.FindFileInCurrentDirectory(fileName); } }

		private static XmlDocument windfallDoc;

		private static byte[] Key = Encoding.ASCII.GetBytes("[XXS^FTiLsL8!7_=GmCkj1pGP$g^gyqX!v");

		public static bool HasSavedState()
		{
			if (!File.Exists(FilePath))
			{
				return false;
			}
			bool result;
			try
			{

				new XmlDocument().LoadXml(ReadXml());
				result = true;
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Could not read windfall saved state from " + FilePath + ": " + ex.ToString());
				result = false;
			}
			return result;
		}

		public static void EraseSavedState()
		{
			File.Delete(FilePath);
		}

		public static bool IsLoading()
		{
			return windfallDoc != null;
		}

		private static string ReadXml()
		{
			byte[] array = File.ReadAllBytes(FilePath);
			for (int i = 0; i < array.Length; i++)
			{
				byte b = Key[i % Key.Length];
				byte[] array2 = array;
				int num = i;
				array2[num] ^= b;
			}
			return Encoding.UTF8.GetString(array);
		}

		private static void WriteXml(string xml)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(xml);
			for (int i = 0; i < bytes.Length; i++)
			{
				byte b = Key[i % Key.Length];
				byte[] array = bytes;
				int num = i;
				array[num] ^= b;
			}
			File.WriteAllBytes(FilePath, bytes);
		}

		public static void LoadStart(BumboApplication app)
		{
			windfallDoc = new XmlDocument();

			try
			{
				windfallDoc.LoadXml(ReadXml());
                Debug.Log("[The Legend of Bum-bo: Windfall] Loading windfall saved state from " + FilePath);
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[The Legend of Bum-bo: Windfall] Could not load windfall saved state from " + FilePath + ": " + ex.ToString());
				windfallDoc = null;
			}

			//Abort loading if current vanilla saved state is different from previous vanilla saved state
			if (windfallDoc != null)
			{
				if (windfallDoc.SelectSingleNode("/save/vanilla") == null || app.controller.savedStateController == null || !SavedStateController.HasSavedState())
				{
                    Debug.Log("[The Legend of Bum-bo: Windfall] No vanilla saved state detected; aborting loading of Windfall saved state");
					windfallDoc = null;
				}
				else if (windfallDoc.SelectSingleNode("/save/vanilla").Attributes["bytes"].Value != (string)AccessTools.Method(typeof(SavedStateController), "ReadXml").Invoke(app.controller.savedStateController, new object[] { }))
				{
                    Debug.Log("[The Legend of Bum-bo: Windfall] Incompatible vanilla saved state detected; aborting loading of Windfall saved state");
					windfallDoc = null;
				}
			}
		}

		public static void LoadEnd()
		{
			windfallDoc = null;
		}

		public static TrinketName[] bossRewards;
		public static void Save(BumboApplication app)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", string.Empty));

			//Create save element
			XmlElement saveElement = xmlDocument.CreateElement("save");
			xmlDocument.AppendChild(saveElement);

			//Track vanilla save data
			XmlElement vanillaElement = xmlDocument.CreateElement("vanilla");
			saveElement.AppendChild(vanillaElement);
			vanillaElement.SetAttribute("bytes", (string)AccessTools.Method(typeof(SavedStateController), "ReadXml").Invoke(app.controller.savedStateController, new object[] { }));

			//Trinket uses
			XmlElement trinketUsesElement = xmlDocument.CreateElement("trinketUses");
			saveElement.AppendChild(trinketUsesElement);
			for (int trinketCounter = 0; trinketCounter < app.model.characterSheet.trinkets.Count; trinketCounter++)
			{
				TrinketElement trinketElement = app.model.characterSheet.trinkets[trinketCounter];
				XmlElement trinketInstanceElement = xmlDocument.CreateElement("trinket");
				trinketUsesElement.AppendChild(trinketInstanceElement);
				trinketInstanceElement.SetAttribute("uses", trinketElement.uses.ToString());
			}

			//Glitched trinkets
			XmlElement glitchedTrinketsElement = xmlDocument.CreateElement("glitchedTrinkets");
			saveElement.AppendChild(glitchedTrinketsElement);
			for (int trinketCounter = 0; trinketCounter < 4; trinketCounter++)
			{
				if (app.model.trinketIsFake[trinketCounter] && app.model.fakeTrinkets[trinketCounter] != null)
				{
					TrinketElement trinketElement = app.model.fakeTrinkets[trinketCounter];
					XmlElement trinketInstanceElement = xmlDocument.CreateElement("trinket");
					glitchedTrinketsElement.AppendChild(trinketInstanceElement);
					trinketInstanceElement.SetAttribute("index", trinketCounter.ToString());
					trinketInstanceElement.SetAttribute("name", trinketElement.trinketName.ToString());
					trinketInstanceElement.SetAttribute("uses", trinketElement.uses.ToString());
				}
			}

			//Champion enemies
			XmlElement championsElement = xmlDocument.CreateElement("champions");
			saveElement.AppendChild(championsElement);
			XmlElement groundChampionsElement = xmlDocument.CreateElement("ground");
			championsElement.AppendChild(groundChampionsElement);
			XmlElement airChampionsElement = xmlDocument.CreateElement("air");
			championsElement.AppendChild(airChampionsElement);
			//Ground champions
			if (app.model.mapModel.currentRoomEnemyLayout.groundEnemies != null)
			{
				for (int num2 = 0; num2 < app.model.mapModel.currentRoomEnemyLayout.groundEnemies.Count; num2++)
				{
					for (int num3 = 0; num3 < app.model.mapModel.currentRoomEnemyLayout.groundEnemies[num2].Count; num3++)
					{
						XmlElement enemyElement = xmlDocument.CreateElement("enemy");
						groundChampionsElement.AppendChild(enemyElement);
						enemyElement.SetAttribute("x", num3.ToString());
						enemyElement.SetAttribute("y", num2.ToString());

						//Save ground enemy champion status
						if (app.controller.GetGroundEnemy(num3, num2) != null)
						{
							enemyElement.SetAttribute("championStatus", app.controller.GetGroundEnemy(num3, num2).championType.ToString());
						}
					}
				}
			}
			//Air champions
			if (app.model.mapModel.currentRoomEnemyLayout.airEnemies != null)
			{
				for (int num4 = 0; num4 < app.model.mapModel.currentRoomEnemyLayout.airEnemies.Count; num4++)
				{
					for (int num5 = 0; num5 < app.model.mapModel.currentRoomEnemyLayout.airEnemies[num4].Count; num5++)
					{
						XmlElement enemyElement = xmlDocument.CreateElement("enemy");
						airChampionsElement.AppendChild(enemyElement);
						enemyElement.SetAttribute("x", num5.ToString());
						enemyElement.SetAttribute("y", num4.ToString());

						//Save air enemy champion status
						if (app.controller.GetAirEnemy(num5, num4) != null)
						{
							enemyElement.SetAttribute("championStatus", app.controller.GetAirEnemy(num5, num4).championType.ToString());
						}
					}
				}
			}

			//Treasure room pickups
			if (app.model.mapModel.currentRoom.roomType == MapRoom.RoomType.Treasure && app.view.boxes.treasureRoom.GetComponent<TreasureRoom>())
			{
				XmlElement treasureElement = xmlDocument.CreateElement("treasure");
				saveElement.AppendChild(treasureElement);

				List<GameObject> pickups = (List<GameObject>)typeof(TreasureRoom).GetField("pickups", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(app.view.boxes.treasureRoom.GetComponent<TreasureRoom>());

				for (int pickupCounter = 0; pickupCounter < pickups.Count; pickupCounter++)
				{
					XmlElement pickupElement = xmlDocument.CreateElement("pickup");
					treasureElement.AppendChild(pickupElement);

					SpellElement spell = pickups[pickupCounter].GetComponent<SpellPickup>() ? pickups[pickupCounter].GetComponent<SpellPickup>().spell : null;
					TrinketElement trinket = pickups[pickupCounter].GetComponent<TrinketPickupView>() ? pickups[pickupCounter].GetComponent<TrinketPickupView>().trinket : null;

					pickupElement.SetAttribute("type", spell != null ? "spell" : "trinket");

					pickupElement.SetAttribute("name", spell != null ? spell.spellName.ToString() : trinket.trinketName.ToString());

					if (spell != null)
					{
						XmlElement costElement = xmlDocument.CreateElement("cost");
						pickupElement.AppendChild(costElement);
						costElement.SetAttribute("boneCost", spell.Cost[0].ToString());
						costElement.SetAttribute("heartCost", spell.Cost[1].ToString());
						costElement.SetAttribute("poopCost", spell.Cost[2].ToString());
						costElement.SetAttribute("boogerCost", spell.Cost[3].ToString());
						costElement.SetAttribute("toothCost", spell.Cost[4].ToString());
						costElement.SetAttribute("peeCost", spell.Cost[5].ToString());
					}
				}
			}

			//Predetermine and save boss room pickups
			if (app.model.mapModel.currentRoom.roomType == MapRoom.RoomType.Boss)
			{
				bossRewards = new TrinketName[2];
				//Random trinket
				bossRewards[0] = app.model.trinketModel.validTrinkets[UnityEngine.Random.Range(0, app.model.trinketModel.validTrinkets.Count)];

				//Random trinket that is not the same as the first trinket
				List<TrinketName> list = new List<TrinketName>();
				for (int i = 1; i < app.model.trinketModel.validTrinkets.Count; i++)
				{
					bool flag = false;
					if (bossRewards[0] == app.model.trinketModel.validTrinkets[i])
					{
						flag = true;
					}

					if (!flag)
					{
						list.Add(app.model.trinketModel.validTrinkets[i]);
					}
				}
				if (list.Count > 0)
				{
					int index = UnityEngine.Random.Range(0, list.Count);
					bossRewards[1] = list[index];
				}
				else
				{
					bossRewards[1] = app.model.trinketModel.validTrinkets[UnityEngine.Random.Range(0, app.model.trinketModel.validTrinkets.Count)];
				}
				XmlElement bossRewardsElement = xmlDocument.CreateElement("bossRewards");
				saveElement.AppendChild(bossRewardsElement);
				bossRewardsElement.SetAttribute("reward0", bossRewards[0].ToString());
				bossRewardsElement.SetAttribute("reward1", bossRewards[1].ToString());
			}
			else
			{
				bossRewards = null;
			}

			StringWriter stringWriter = new StringWriter();
			xmlDocument.Save(stringWriter);
			WriteXml(stringWriter.ToString());
			Console.WriteLine("[The Legend of Bum-bo: Windfall] Saving saved state");
		}

		public static void LoadTrinketUses(BumboApplication app)
		{
			if (windfallDoc == null)
			{
				return;
			}

			XmlNode trinketUsesNode = windfallDoc.SelectSingleNode("/save/trinketUses");

			XmlNodeList trinketUsesNodeList = trinketUsesNode.SelectNodes("trinket");

			for (int trinketCounter = 0; trinketCounter < trinketUsesNodeList.Count; trinketCounter++)
			{
				if (trinketCounter < app.model.characterSheet.trinkets.Count && app.model.characterSheet.trinkets[trinketCounter] != null)
				{
					XmlNode trinketNode = trinketUsesNodeList[trinketCounter];
					app.model.characterSheet.trinkets[trinketCounter].uses = Convert.ToInt16(trinketNode.Attributes["uses"].Value);
				}
			}
			Console.WriteLine("[The Legend of Bum-bo: Windfall] Loading trinket uses");
		}

		public static void LoadGlitchedTrinkets(BumboApplication app)
		{
			if (windfallDoc == null)
			{
				return;
			}

			XmlNode glitchedTrinketsNode = windfallDoc.SelectSingleNode("/save/glitchedTrinkets");

			XmlNodeList glitchedTrinketsNodeList = glitchedTrinketsNode.SelectNodes("trinket");

			for (int trinketCounter = 0; trinketCounter < glitchedTrinketsNodeList.Count; trinketCounter++)
			{
				XmlNode glitchedTrinketNode = glitchedTrinketsNodeList[trinketCounter];
				int index = Convert.ToInt16(glitchedTrinketNode.Attributes["index"].Value);
				if (app.model.characterSheet.trinkets[index] != null && app.model.characterSheet.trinkets[index].trinketName == TrinketName.Glitch)
				{
					app.model.trinketIsFake[index] = true;
					app.model.fakeTrinkets[index] = app.model.trinketModel.trinkets[(TrinketName)Enum.Parse(typeof(TrinketName), glitchedTrinketNode.Attributes["name"].Value)];
					app.model.fakeTrinkets[index].uses = Convert.ToInt16(glitchedTrinketNode.Attributes["uses"].Value);
				}
			}
			Console.WriteLine("[The Legend of Bum-bo: Windfall] Loading glitched trinkets");
		}


		public static void LoadChampionEnemies(BumboApplication app)
		{
			//Loading Champions
			if (windfallDoc == null)
			{
				return;
			}

			XmlNode championsNode = windfallDoc.SelectSingleNode("/save/champions");
			XmlNode groundNode = championsNode.SelectSingleNode("ground");
			XmlNode airNode = championsNode.SelectSingleNode("air");
			XmlNodeList groundEnemyNodeList = groundNode.SelectNodes("enemy");
			for (int i = 0; i < groundEnemyNodeList.Count; i++)
			{
				XmlNode groundEnemyNode = groundEnemyNodeList.Item(i);
				int groundX = Convert.ToInt32(groundEnemyNode.Attributes["x"].Value);
				int groundY = Convert.ToInt32(groundEnemyNode.Attributes["y"].Value);
				if (groundEnemyNode.Attributes["championStatus"] != null)
				{
					Enemy.ChampionType champion = (Enemy.ChampionType)Enum.Parse(typeof(Enemy.ChampionType), groundEnemyNode.Attributes["championStatus"].Value);
					for (int j = 0; j < app.model.enemies.Count; j++)
					{
						if (app.model.enemies[j] != null && app.model.enemies[j].alive && app.model.enemies[j].championType == Enemy.ChampionType.NotAChampion && app.model.enemies[j].battlefieldPosition.x == groundX && app.model.enemies[j].battlefieldPosition.y == groundY && app.model.enemies[j].enemyType != Enemy.EnemyType.Flying)
						{
							app.model.enemies[j].SetChampion(champion);
							if (app.model.enemies[j].championType != Enemy.ChampionType.NotAChampion)
							{
								app.model.enemies[j].Init();
							}
						}
					}
				}
			}
			XmlNodeList airEnemyNodeList = airNode.SelectNodes("enemy");
			for (int k = 0; k < airEnemyNodeList.Count; k++)
			{
				XmlNode airEnemyNode = airEnemyNodeList.Item(k);
				int airX = Convert.ToInt32(airEnemyNode.Attributes["x"].Value);
				int airY = Convert.ToInt32(airEnemyNode.Attributes["y"].Value);
				if (airEnemyNode.Attributes["championStatus"] != null)
				{
					Enemy.ChampionType champion = (Enemy.ChampionType)Enum.Parse(typeof(Enemy.ChampionType), airEnemyNode.Attributes["championStatus"].Value);
					for (int l = 0; l < app.model.enemies.Count; l++)
					{
						if (app.model.enemies[l] != null && app.model.enemies[l].alive && app.model.enemies[l].championType == Enemy.ChampionType.NotAChampion && app.model.enemies[l].battlefieldPosition.x == airX && app.model.enemies[l].battlefieldPosition.y == airY && app.model.enemies[l].enemyType == Enemy.EnemyType.Flying)
						{
							app.model.enemies[l].SetChampion(champion);
							if (app.model.enemies[l].championType != Enemy.ChampionType.NotAChampion)
							{
								app.model.enemies[l].Init();
							}
						}
					}
				}
			}
			Console.WriteLine("[The Legend of Bum-bo: Windfall] Loading champion enemies");
		}

		public static void LoadTreasure(TreasureRoom treasureRoom, List<GameObject> ___pickups)
		{
			if (windfallDoc == null)
			{
				return;
			}

			//Remove existing pickups
			foreach (GameObject pickup in ___pickups)
			{
				UnityEngine.Object.Destroy(pickup);
			}
			___pickups.Clear();

			XmlNode treasureNode = windfallDoc.SelectSingleNode("/save/treasure");

			XmlNodeList pickupNodeList = treasureNode.SelectNodes("pickup");
			for (int i = 0; i < pickupNodeList.Count; i++)
			{
				XmlNode pickupNode = pickupNodeList[i];
				string type = pickupNode.Attributes["type"].Value;

				if (type == "spell")
				{
					AccessTools.Method(typeof(TreasureRoom), "SetSpell", new Type[] { typeof(int), typeof(List<SpellName>), typeof(SpellElement.SpellCategory) }).Invoke(treasureRoom, new object[] { i + 1, new List<SpellName>(), SpellElement.SpellCategory.Attack });

					XmlNode xmlNode3 = pickupNode.SelectSingleNode("cost");
					short[] newCost = new short[] {
							Convert.ToInt16(xmlNode3.Attributes["boneCost"].Value),
							Convert.ToInt16(xmlNode3.Attributes["heartCost"].Value),
							Convert.ToInt16(xmlNode3.Attributes["poopCost"].Value),
							Convert.ToInt16(xmlNode3.Attributes["boogerCost"].Value),
							Convert.ToInt16(xmlNode3.Attributes["toothCost"].Value),
							Convert.ToInt16(xmlNode3.Attributes["peeCost"].Value)
					};

					___pickups[i].GetComponent<SpellPickup>().SetSpell(CharacterSheet.BumboType.TheBrave, (SpellName)Enum.Parse(typeof(SpellName), pickupNode.Attributes["name"].Value), newCost, treasureRoom.app.model.spellModel);
				}
				else
				{
					AccessTools.Method(typeof(TreasureRoom), "SetTrinkets", new Type[] { typeof(int), typeof(List<TrinketName>) }).Invoke(treasureRoom, new object[] { i + 1, new List<TrinketName>() });
					___pickups[i].GetComponent<TrinketPickupView>().SetTrinket((TrinketName)Enum.Parse(typeof(TrinketName), pickupNode.Attributes["name"].Value));
				}
			}
		}

		public static void LoadBoss()
		{
			if (windfallDoc == null)
			{
				return;
			}

			XmlNode bossRewardsNode = windfallDoc.SelectSingleNode("/save/bossRewards");
			if (bossRewardsNode != null)
			{
				bossRewards = new TrinketName[]
				{
					(TrinketName)Enum.Parse(typeof(TrinketName), bossRewardsNode.Attributes["reward0"].Value),
					(TrinketName)Enum.Parse(typeof(TrinketName), bossRewardsNode.Attributes["reward1"].Value)
				};
			}
		}

		public static void SaveDamageTaken(float damage)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(ReadXml());

			XmlNode saveNode = xmlDocument.SelectSingleNode("save");
			XmlNode damageNode = saveNode.SelectSingleNode("damageTaken");

			if (damageNode == null)
			{
				//Add damage taken
				XmlElement damageElement = xmlDocument.CreateElement("damageTaken");
				saveNode.AppendChild(damageElement);
				damageElement.SetAttribute("damage", (-damage).ToString());
			}
			else
			{
				//Update damage taken if damage is already present
				float damageTaken = float.Parse(damageNode.Attributes["damage"].Value);
				damageTaken -= damage;
				damageNode.Attributes["damage"].Value = damageTaken.ToString();
			}

			StringWriter stringWriter = new StringWriter();
			xmlDocument.Save(stringWriter);
			WriteXml(stringWriter.ToString());
			Console.WriteLine("[The Legend of Bum-bo: Windfall] Saving damage taken");
		}

		public static void LoadDamageTaken(BumboApplication app)
		{
			if (windfallDoc == null)
			{
				return;
			}

			if (!WindfallPersistentDataController.LoadData().implementBalanceChanges)
			{
                return;
            }

			XmlNode damageNode = windfallDoc.SelectSingleNode("/save/damageTaken");
			if (damageNode == null)
			{
				Console.WriteLine("[The Legend of Bum-bo: Windfall] damageTaken null");
				return;
			}

			float damage = float.Parse(damageNode.Attributes["damage"].Value);

			while (damage < 0f && (app.model.characterSheet.hitPoints + app.model.characterSheet.soulHearts > 0.5f))
			{
				//RegisterDamage
				if (!(app.model.characterSheet.currentFloor < 0 || app.model.characterSheet.currentFloor >= 5))
				{
					app.model.characterSheet.damageTakenInFloor[app.model.characterSheet.currentFloor] -= 0.5f;
				}

				//Reduce player health
				if (app.model.characterSheet.soulHearts > 0f)
				{
					app.model.characterSheet.soulHearts -= 0.5f;
					if (app.model.characterSheet.soulHearts < 0.5f)
					{
						app.model.characterSheet.soulHearts = 0f;
					}
				}
				else if (app.model.characterSheet.hitPoints > 0.5f)
				{
					app.model.characterSheet.hitPoints -= 0.5f;
				}
				damage += 0.5f;
			}

			app.view.hearts.GetComponent<HealthController>().UpdateHearts(true);
		}

		public static void SaveShop(Shop shop)
		{
			//Saves the Wooden Nickel shop and the player's character sheet
			//Floor counter is reduced by 1

			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(ReadXml());

			XmlNode saveNode = xmlDocument.SelectSingleNode("save");

			//Remove damage taken
			XmlNode damageNode = saveNode.SelectSingleNode("damageTaken");
			if (damageNode != null)
			{
				saveNode.RemoveChild(damageNode);
			}

			//Replace gambling node
			XmlNode gamblingNode = saveNode.SelectSingleNode("gambling");
			if (gamblingNode != null)
			{
				gamblingNode.RemoveAll();
			}
			else
			{
				gamblingNode = xmlDocument.CreateElement("gambling");
				saveNode.AppendChild(gamblingNode);
			}

			//Save shop
			for (int pickupCounter = 0; pickupCounter < 4; pickupCounter++)
			{
				Transform transform = null;
				switch (pickupCounter)
				{
					case 0:
						transform = shop.item1.transform;
						break;
					case 1:
						transform = shop.item2.transform;
						break;
					case 2:
						transform = shop.item3.transform;
						break;
					case 3:
						transform = shop.item4.transform;
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
					XmlElement pickupElement = xmlDocument.CreateElement("pickup");
					gamblingNode.AppendChild(pickupElement);

					pickupElement.SetAttribute("index", pickupCounter.ToString());
					if (pickup.GetComponent<HeartPickupView>())
					{
						pickupElement.SetAttribute("type", "heart");
					}
					else if (pickup.GetComponent<TrinketPickupView>())
					{
						pickupElement.SetAttribute("type", "trinket");
						pickupElement.SetAttribute("trinketName", pickup.GetComponent<TrinketPickupView>().trinket.trinketName.ToString());
					}
				}
			}

			//Save character sheet
			XmlNode characterNode = saveNode.SelectSingleNode("character");
			if (characterNode != null)
			{
				characterNode.RemoveAll();
			}
			else
			{
				characterNode = xmlDocument.CreateElement("character");
				saveNode.AppendChild(characterNode);
			}

			XmlElement xmlElement = (XmlElement)characterNode;

			string name = "bumboType";
			int bumboType = (int)shop.app.model.characterSheet.bumboType;
			xmlElement.SetAttribute(name, bumboType.ToString());
			XmlElement xmlElement4 = xmlDocument.CreateElement("baseInfo");
			characterNode.AppendChild(xmlElement4);
			XmlElement xmlElement5 = xmlElement4;
			string name2 = "bumboType";
			int bumboType2 = (int)shop.app.model.characterSheet.bumboType;
			xmlElement5.SetAttribute(name2, bumboType2.ToString());
			xmlElement4.SetAttribute("hitPoints", shop.app.model.characterSheet.bumboBaseInfo.hitPoints.ToString());
			xmlElement4.SetAttribute("soulHearts", shop.app.model.characterSheet.bumboBaseInfo.soulHearts.ToString());
			xmlElement4.SetAttribute("itemDamage", shop.app.model.characterSheet.bumboBaseInfo.itemDamage.ToString());
			xmlElement4.SetAttribute("luck", shop.app.model.characterSheet.bumboBaseInfo.luck.ToString());
			xmlElement4.SetAttribute("puzzleDamage", shop.app.model.characterSheet.bumboBaseInfo.puzzleDamage.ToString());
			xmlElement4.SetAttribute("dexterity", shop.app.model.characterSheet.bumboBaseInfo.dexterity.ToString());
			xmlElement4.SetAttribute("coins", shop.app.model.characterSheet.bumboBaseInfo.coins.ToString());

			for (int i = 0; i < shop.app.model.characterSheet.bumboBaseInfo.startingSpells.Length; i++)
			{
				StartingSpell startingSpell = shop.app.model.characterSheet.bumboBaseInfo.startingSpells[i];
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
			for (int j = 0; j < shop.app.model.characterSheet.bumboBaseInfo.startingTrinkets.Length; j++)
			{
				TrinketName trinketName = shop.app.model.characterSheet.bumboBaseInfo.startingTrinkets[j];
				XmlElement xmlElement7 = xmlDocument.CreateElement("startingTrinket");
				xmlElement4.AppendChild(xmlElement7);
				xmlElement7.SetAttribute("name", trinketName.ToString());
			}
			XmlElement xmlElement8 = xmlDocument.CreateElement("hiddenTrinket");
			xmlElement4.AppendChild(xmlElement8);
			xmlElement8.SetAttribute("name", shop.app.model.characterSheet.bumboBaseInfo.hiddenTrinket.ToString());
			XmlNodeList xmlNodeList = characterNode.SelectNodes("spell");
			XmlNodeList xmlNodeList2 = characterNode.SelectNodes("trinket");
			XmlNode xmlNode4 = characterNode.SelectSingleNode("hiddenTrinket");
			xmlElement.SetAttribute("coins", shop.app.model.characterSheet.coins.ToString());

			for (int k = 0; k < shop.app.model.characterSheet.spells.Count; k++)
			{
				SpellElement spellElement = shop.app.model.characterSheet.spells[k];
				XmlElement xmlElement9 = xmlDocument.CreateElement("spell");
				characterNode.AppendChild(xmlElement9);
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
			for (int l = 0; l < shop.app.model.characterSheet.trinkets.Count; l++)
			{
				TrinketElement trinketElement = shop.app.model.characterSheet.trinkets[l];
				XmlElement xmlElement10 = xmlDocument.CreateElement("trinket");
				characterNode.AppendChild(xmlElement10);
				xmlElement10.SetAttribute("name", trinketElement.trinketName.ToString());
				//Saving trinket uses
				xmlElement10.SetAttribute("uses", trinketElement.uses.ToString());
			}
			if (shop.app.model.characterSheet.hiddenTrinket != null)
			{
				XmlElement xmlElement11 = xmlDocument.CreateElement("hiddenTrinket");
				characterNode.AppendChild(xmlElement11);
				xmlElement11.SetAttribute("name", shop.app.model.characterSheet.hiddenTrinket.trinketName.ToString());
			}

			xmlElement.SetAttribute("hitPoints", shop.app.model.characterSheet.hitPoints.ToString());
			xmlElement.SetAttribute("soulHearts", shop.app.model.characterSheet.soulHearts.ToString());
			xmlElement.SetAttribute("timesincestart", shop.app.model.characterSheet.timesincestart.ToString());
			//Save previous floor
			xmlElement.SetAttribute("currentFloor", (shop.app.model.characterSheet.currentFloor > 1 ? shop.app.model.characterSheet.currentFloor - 1 : shop.app.model.characterSheet.currentFloor).ToString());
			XmlElement xmlElement12 = xmlDocument.CreateElement("floors");
			characterNode.AppendChild(xmlElement12);
			for (int m = 0; m < 5; m++)
			{
				XmlElement xmlElement13 = xmlDocument.CreateElement("floor");
				xmlElement12.AppendChild(xmlElement13);
				xmlElement13.SetAttribute("id", m.ToString());
				xmlElement13.SetAttribute("damageTaken", ((m >= shop.app.model.characterSheet.damageTakenInFloor.Length) ? 0f : shop.app.model.characterSheet.damageTakenInFloor[m]).ToString());
			}

			StringWriter stringWriter = new StringWriter();
			xmlDocument.Save(stringWriter);
			WriteXml(stringWriter.ToString());
			Console.WriteLine("[The Legend of Bum-bo: Windfall] Saving shop and characterSheet");
		}

		public static bool LoadShop(Shop __instance, TrinketModel ___trinketModel)
		{
			//Bug: Shop pickups not linked to shop when reloading a Wooden Nickel save

			//Load shop
			if (windfallDoc == null)
			{
				return false;
			}

			XmlNode gamblingNode = windfallDoc.SelectSingleNode("/save/gambling");

			if (gamblingNode != null)
			{
				XmlNodeList pickupNodeList = gamblingNode.SelectNodes("pickup");

				bool[] savedPickups = new bool[4];

				for (int i = 0; i < pickupNodeList.Count; i++)
				{
					XmlNode pickupNode = pickupNodeList.Item(i);

					int index = Convert.ToInt32(pickupNode.Attributes["index"].Value);
					savedPickups[i] = true;

					string type = pickupNode.Attributes["type"].Value;

					GameObject itemPickup = null;
					GameObject item = null;
					ItemPriceView itemPrice = null;
					switch (index)
					{
						case 0:
							item = __instance.item1;
							itemPrice = __instance.item1Price;
							break;
						case 1:
							item = __instance.item2;
							itemPrice = __instance.item2Price;
							break;
						case 2:
							item = __instance.item3;
							itemPrice = __instance.item3Price;
							break;
						case 3:
							item = __instance.item4;
							itemPrice = __instance.item4Price;
							break;
					}

					if (type == "heart")
					{
						itemPickup = (GameObject)AccessTools.Method(typeof(Shop), "AddHeart").Invoke(__instance, new object[] { item, itemPrice });
					}
					else if (type == "trinket")
					{
						TrinketName trinketName = (TrinketName)Enum.Parse(typeof(TrinketName), pickupNode.Attributes["trinketName"].Value);

						if (__instance.app.model.trinketModel.trinkets[trinketName].Category != TrinketElement.TrinketCategory.Prick)
						{
							if (___trinketModel == null)
							{
								___trinketModel = __instance.gameObject.AddComponent<TrinketModel>();
							}
						}

						itemPickup = (GameObject)AccessTools.Method(typeof(Shop), "AddTrinket").Invoke(__instance, new object[] { item, itemPrice });
						int price = 7;
						switch (trinketName)
						{
							case TrinketName.ManaPrick:
								price = 8;
								break;
							case TrinketName.DamagePrick:
								price = 5;
								break;
							case TrinketName.ChargePrick:
								price = 8;
								break;
							case TrinketName.ShufflePrick:
								price = 2;
								break;
							case TrinketName.RandomPrick:
								price = 5;
								break;
						}

						TrinketPickupView trinketPickupView = itemPickup.GetComponent<TrinketPickupView>();
						trinketPickupView.SetTrinket(trinketName, price);
						itemPrice.SetPrice(price);
						trinketPickupView.removePickup = trinketPickupView.trinket.Category == TrinketElement.TrinketCategory.Prick ? false : true;

						__instance.SetPickup(itemPickup, index);
					}
				}

				for (int index = 0; index < 3; index++)
				{
					if (__instance.GetPickup(index) != null)
					{
						__instance.GetPickup(index).GetComponent<TrinketPickupView>().shopIndex = index;
					}
				}
				return true;
			}

			return false;
		}

		public static bool LoadIntoWoodenNickel(BumboApplication app)
		{
			return app.controller.gamblingController == null && WoodenNickelSaveExists() && TitleController.startMode == TitleController.StartMode.Continue;
		}

		public static bool WoodenNickelSaveExists()
		{
			return windfallDoc == null ? false : windfallDoc.SelectSingleNode("/save/gambling") != null;
		}

		public static void LoadCharacterSheet(BumboApplication app)
		{
			if (windfallDoc == null)
			{
				return;
			}

			XmlNode xmlNode = windfallDoc.SelectSingleNode("/save/character");
			XmlNodeList xmlNodeList = xmlNode.SelectNodes("floors/floor");
			XmlNodeList xmlNodeList2 = xmlNode.SelectNodes("spell");
			XmlNodeList xmlNodeList3 = xmlNode.SelectNodes("trinket");
			XmlNode xmlNode2 = xmlNode.SelectSingleNode("hiddenTrinket");
			XmlNode xmlNode3 = xmlNode.SelectSingleNode("baseInfo");
			XmlNodeList xmlNodeList4 = xmlNode3.SelectNodes("startingSpell");
			XmlNodeList xmlNodeList5 = xmlNode3.SelectNodes("startingTrinket");
			XmlNode xmlNode4 = xmlNode3.SelectSingleNode("hiddenTrinket");
			app.model.characterSheet.bumboType = (CharacterSheet.BumboType)Convert.ToInt32(xmlNode.Attributes["bumboType"].Value);
			app.model.characterSheet.bumboBaseInfo = new BumboObject
			{
				bumboType = app.model.characterSheet.bumboType,
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
				app.model.characterSheet.bumboBaseInfo.startingSpells[i] = new StartingSpell
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
				app.model.characterSheet.bumboBaseInfo.startingTrinkets[j] = (TrinketName)Enum.Parse(typeof(TrinketName), xmlNodeList5.Item(j).Attributes["name"].Value);
			}
			app.model.characterSheet.coins = Convert.ToInt32(xmlNode.Attributes["coins"].Value);
			for (int k = 0; k < xmlNodeList2.Count; k++)
			{
				XmlNode xmlNode6 = xmlNodeList2.Item(k);
				try
				{
					SpellName key = (SpellName)Enum.Parse(typeof(SpellName), xmlNode6.Attributes["name"].Value);
					SpellElement spellElement = app.model.spellModel.spells[key];
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
					app.model.characterSheet.spells.Add(spellElement);
				}
				catch (Exception)
				{
					Debug.LogWarning(string.Format("Windfall LoadCharacterSheet: Could not load spell {0}!", xmlNode6.Attributes["name"].Value));
				}
			}
			for (int l = 0; l < xmlNodeList3.Count; l++)
			{
				XmlNode xmlNode7 = xmlNodeList3.Item(l);
				try
				{
					TrinketName key2 = (TrinketName)Enum.Parse(typeof(TrinketName), xmlNode7.Attributes["name"].Value);
					app.model.characterSheet.trinkets.Add(app.model.trinketModel.trinkets[key2]);
				}
				catch (Exception)
				{
					Debug.LogWarning(string.Format("Windfall LoadCharacterSheet: Could not load trinket {0}!", xmlNode7.Attributes["name"].Value));
				}
			}
			if (xmlNode2 != null)
			{
				try
				{
					TrinketName key3 = (TrinketName)Enum.Parse(typeof(TrinketName), xmlNode2.Attributes["name"].Value);
					app.model.characterSheet.hiddenTrinket = app.model.trinketModel.trinkets[key3];
				}
				catch (Exception)
				{
					Debug.LogWarning(string.Format("Windfall LoadCharacterSheet: Could not load hidden trinket {0}!", xmlNode2.Attributes["name"].Value));
				}
			}
			app.model.characterSheet.hitPoints = Convert.ToSingle(xmlNode.Attributes["hitPoints"].Value);
			app.model.characterSheet.soulHearts = Convert.ToSingle(xmlNode.Attributes["soulHearts"].Value);
			app.model.characterSheet.timesincestart = Convert.ToSingle(xmlNode.Attributes["timesincestart"].Value);
			app.model.characterSheet.currentFloor = Convert.ToInt32(xmlNode.Attributes["currentFloor"].Value);
			app.model.characterSheet.damageTakenInFloor = new float[xmlNodeList.Count];
			for (int m = 0; m < xmlNodeList.Count; m++)
			{
				int num = Convert.ToInt32(xmlNodeList[m].Attributes["id"].Value);
				if (num < 0 || num >= xmlNodeList.Count)
				{
					Debug.LogWarning("Invalid floor ID: " + num);
				}
				else
				{
					app.model.characterSheet.damageTakenInFloor[num] = Convert.ToSingle(xmlNodeList[m].Attributes["damageTaken"].Value);
				}
			}
		}

		public static void SaveCoins(BumboApplication app)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(ReadXml());

			XmlNode xmlNode = xmlDocument.SelectSingleNode("save/character");
			if (xmlNode != null)
			{
				XmlElement xmlElement = (XmlElement)xmlNode;
				xmlElement.SetAttribute("coins", app.model.characterSheet.coins.ToString());
			}

			StringWriter stringWriter = new StringWriter();
			xmlDocument.Save(stringWriter);
			WriteXml(stringWriter.ToString());
            Debug.Log("[The Legend of Bum-bo: Windfall] Saving coin count");
		}
	}
}