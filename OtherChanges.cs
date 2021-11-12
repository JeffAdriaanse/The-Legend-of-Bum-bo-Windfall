using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;

namespace The_Legend_of_Bum_bo_Windfall
{
    class OtherChanges
    {
        public static void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(OtherChanges));
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Applying other changes");
        }

        ////Patch: Enables debug menu
        //[HarmonyPostfix, HarmonyPatch(typeof(DebugController), "Start")]
        //static void DebugController_Start(DebugController __instance)
        //{
        //    __instance.turnOnDebugKey = true;
        //    Console.WriteLine("[The Legend of Bum-bo: Windfall] Enabling debug menu");
        //}

        ////Patch: Enables title debug menu and grants 100% game completion
        //[HarmonyPostfix, HarmonyPatch(typeof(TitleController), "Start")]
        //static void TitleController_Start(TitleController __instance)
        //{
        //    __instance.turnOnDebugKey = true;

        //    //Progression progression = new Progression();
        //    //progression.braveMoneyWins = 1;
        //    //progression.braveWins = 2;
        //    //progression.chapter3NoDamageWins = 1;
        //    //progression.chapter4Wins = 4;
        //    //progression.completedTutorial = true;
        //    //for (int i = 0; i < progression.cutscenes.Length; i++)
        //    //{
        //    //    progression.cutscenes[i] = true;
        //    //}
        //    //progression.deadMoneyWins = 1;
        //    //progression.deadWins = 2;
        //    //progression.emptyMoneyWins = 1;
        //    //progression.emptyWins = 2;
        //    //progression.lostMoneyWins = 1;
        //    //progression.lostWins = 2;
        //    //progression.nimbleMoneyWins = 1;
        //    //progression.nimbleWins = 2;
        //    //progression.stoutMoneyWins = 1;
        //    //progression.stoutWins = 2;
        //    //for (int i = 0; i < 43; i++)
        //    //{
        //    //    progression.unlocks[i] = true;
        //    //}
        //    //progression.weirdMoneyWins = 1;
        //    //progression.weirdWins = 2;
        //    //progression.wins = 14;
        //    //ProgressionController.SaveProgression(progression);

        //    Console.WriteLine("[The Legend of Bum-bo: Windfall] Enabling debug menu and unlocking everything");
        //}

        //Patch: Heart tiles no longer appear naturally when playing as Bum-bo the Lost
        [HarmonyPrefix, HarmonyPatch(typeof(Puzzle), "nextBlock")]
        static bool Puzzle_nextBlock(Puzzle __instance, ref int ___heartCounter)
        {
            if (__instance.app.model.characterSheet.bumboType == CharacterSheet.BumboType.TheLost)
            {
                ___heartCounter = -1;
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Preventing heart tiles from spawning when playing as Bum-bo the Lost");
            }
            return true;
        }
        [HarmonyPostfix, HarmonyPatch(typeof(TrinketController), "StartingBlocks")]
        static void TrinketController_StartingBlocks(TrinketController __instance, ref int[] __result)
        {
            if (__instance.app.model.characterSheet.bumboType == CharacterSheet.BumboType.TheLost)
            {
                if (__result[1] > 0)
                {
                    __result[1]--;
                }
            }
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Preventing starting heart tile from spawning when playing as Bum-bo the Lost");
        }

        //Patch: Fixes a bug in CharacterSheet addSoulHearts logic that permitted granting soul health past the maximum of six total hearts
        [HarmonyPrefix, HarmonyPatch(typeof(CharacterSheet), "addSoulHearts")]
        static bool CharacterSheet_addSoulHearts(CharacterSheet __instance, float _amount)
        {
            _amount = Mathf.Clamp(_amount, 0f, 6f - __instance.bumboBaseInfo.hitPoints - __instance.soulHearts);
            __instance.soulHearts += _amount;
            if (__instance.soulHearts > 6f)
            {
                __instance.soulHearts = 6f;
            }
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Preventing addSoulHearts from granting soul health past the maximum of six total hearts");
            return false;
        }

        //Patch: Fixes a bug in CharacterSheet addHitPoints logic that permitted granting soul health past the maximum of six total hearts
        [HarmonyPostfix, HarmonyPatch(typeof(CharacterSheet), "addHitPoints")]
        static void CharacterSheet_addHitPoints(CharacterSheet __instance)
        {
            while (__instance.bumboBaseInfo.hitPoints + __instance.soulHearts > 6f && __instance.app.model.characterSheet.soulHearts > 0f)
            {
                __instance.soulHearts -= 0.5f;
            }
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Removing overflowing soul health after gaining a red heart container");
        }

        //Patch: Fixes a bug in the Mega Boner tile combo logic; it no longer incorrectly breaks out of the for statements that look for enemies
        [HarmonyPatch(typeof(BoneMegaAttackEvent), "AttackEnemy")]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var code = new List<CodeInstruction>(instructions);

            Label endLabel = il.DefineLabel();

            bool firstBreak = true;
            int firstBreakIndex = -1;
            for (int codeIndex = 0; codeIndex < code.Count - 1; codeIndex++)
            {
                if (code[codeIndex].opcode == OpCodes.Br && code[codeIndex - 1].opcode == OpCodes.Callvirt && code[codeIndex - 2].opcode == OpCodes.Callvirt)
                {
                    if (firstBreak)
                    {
                        firstBreakIndex = codeIndex;
                        firstBreak = false;
                    }
                    else
                    {
                        code[codeIndex].opcode = OpCodes.Nop;
                        code[codeIndex].labels.Add(endLabel);

                        code[firstBreakIndex] = new CodeInstruction(OpCodes.Br, endLabel);
                        return code;
                    }
                }
            }
            return code;
        }

        //Patch: Fixes Mega Chomper Tile Combo sometimes trying to damage enemies that are already dead, which can cause a softlock
        [HarmonyPrefix, HarmonyPatch(typeof(ToothMegaAttackEvent), "AttackEnemy")]
        static bool ToothMegaAttackEvent_AttackEnemy(ToothMegaAttackEvent __instance)
        {
            List<Enemy> list = new List<Enemy>();
            list.AddRange(__instance.app.model.enemies);
            short num = 0;
            while (num < list.Count)
            {
                if (list[num] != null && (list[num].GetComponent<Enemy>().alive || list[num].GetComponent<Enemy>().isPoop))
                {
                    list[num].GetComponent<Enemy>().Hurt(__instance.app.model.characterSheet.getPuzzleDamage() + 3, Enemy.AttackImmunity.ReducePuzzleDamage, null, -1);
                }
                num += 1;
            }
            Console.WriteLine("[The Legend of Bum-bo: Windfall] Reworking Mega Chomper attack to prevent it from targeting null enemies");
            return false;
        }

        //Patch: Changes floor room generation
        [HarmonyPostfix, HarmonyPatch(typeof(MapCreationController), "CreateMap")]
        static void MapCreationController_CreateMap(MapCreationController __instance)
        {
            //Don't generate new map if player is in the tutorial or if a saved map is loading
            if (__instance.app.model.characterSheet.currentFloor > 0 && !__instance.app.controller.savedStateController.IsLoading())
            {
                List<MapRoom.RoomType> rooms = new List<MapRoom.RoomType>()
                {
                    MapRoom.RoomType.Start,
                    MapRoom.RoomType.Treasure,
                    MapRoom.RoomType.EnemyEncounter,
                    MapRoom.RoomType.EnemyEncounter,
                    MapRoom.RoomType.Treasure,
                    MapRoom.RoomType.Boss
                };

                List<MapRoom.Direction> roomDirections = new List<MapRoom.Direction>();
                for (int roomCounter = 0; roomCounter < rooms.Count - 1; roomCounter++)
                {
                    //Player will never go south (backwards)
                    List<MapRoom.Direction> possibleDirections = new List<MapRoom.Direction>()
                    {
                        MapRoom.Direction.N,
                        MapRoom.Direction.E,
                        MapRoom.Direction.W
                    };

                    //Don't go back to previous room
                    if (roomCounter > 0)
                    {
                        if (roomDirections[roomCounter - 1] == MapRoom.Direction.E)
                        {
                            possibleDirections.Remove(MapRoom.Direction.W);
                        }
                        else if (roomDirections[roomCounter - 1] == MapRoom.Direction.W)
                        {
                            possibleDirections.Remove(MapRoom.Direction.E);
                        }
                    }

                    //Add random direction
                    roomDirections.Add(possibleDirections[UnityEngine.Random.Range(0, possibleDirections.Count)]);
                }

                //Map size will fit all possible maps
                Vector2Int mapSize = new Vector2Int(((rooms.Count - 1) * 2) + 1, rooms.Count);
                __instance.app.model.mapModel.rooms = new MapRoom[mapSize.x, mapSize.y];

                //Set map size
                __instance.app.model.mapModel.mapSize = mapSize;

                //Reset map
                AccessTools.Method(typeof(MapCreationController), "FillMapWithClosedRooms").Invoke(__instance, null);

                //Start at middle bottom of map
                Vector2Int startRoomPosition = new Vector2Int(rooms.Count - 1, 0);
                //Track current room position
                Vector2Int currentPosition = new Vector2Int(startRoomPosition.x, startRoomPosition.y);
                //Track number of treasure rooms
                int treasureRoomCount = 0;

                //Generate map
                for (int roomCounter = 0; roomCounter < rooms.Count; roomCounter++)
                {
                    MapRoom currentRoom = __instance.app.model.mapModel.rooms[currentPosition.x, currentPosition.y];
                    currentRoom.roomType = rooms[roomCounter];
                    currentRoom.cleared = false;

                    //Set start room
                    if (roomCounter == 0)
                    {
                        __instance.app.model.mapModel.currentRoom = currentRoom;
                    }

                    //Add room difficulty
                    if (currentRoom.roomType == MapRoom.RoomType.Start || currentRoom.roomType == MapRoom.RoomType.EnemyEncounter)
                    {
                        currentRoom.difficulty = roomCounter == 0 ? 1 : 2;
                    }

                    //Set treasure room number and type
                    if (currentRoom.roomType == MapRoom.RoomType.Treasure)
                    {
                        treasureRoomCount++;
                        currentRoom.treasureNo = treasureRoomCount;

                        if (__instance.app.model.characterSheet.currentFloor == 1)
                        {
                            currentRoom.treasureRoomType = MapRoom.TreasureRoomType.Spell;
                        }
                        else if(__instance.app.model.characterSheet.currentFloor == 2)
                        {
                            currentRoom.treasureRoomType = MapRoom.TreasureRoomType.Trinket;
                        }
                        else
                        {
                            currentRoom.treasureRoomType = MapRoom.TreasureRoomType.Default;
                        }
                    }

                    if (currentRoom.roomType != MapRoom.RoomType.Boss)
                    {
                        //Add room direction
                        currentRoom.AddDoor(roomDirections[roomCounter]);
                        currentRoom.exitDirection = roomDirections[roomCounter];

                        //Update current position
                        if (roomDirections[roomCounter] == MapRoom.Direction.N)
                        {
                            currentPosition.y++;
                        }
                        else
                        {
                            currentPosition.x += roomDirections[roomCounter] == MapRoom.Direction.E ? 1 : -1;
                        }
                    }
                }
                Console.WriteLine("[The Legend of Bum-bo: Windfall] Changing floor room generation");
            }
        }
    }
}