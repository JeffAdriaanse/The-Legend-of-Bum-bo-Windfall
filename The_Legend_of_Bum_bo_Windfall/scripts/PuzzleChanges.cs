using DG.Tweening;
using HarmonyLib;
using PathologicalGames;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    class PuzzleChanges
    {
        /// <summary>
        /// Fixes an odd texure offset issue
        /// </summary>
        [HarmonyPostfix, HarmonyPatch(typeof(Block), nameof(Block.SetBlockActive))]
        static void Block_SetBlockActive(Block __instance)
        {
            __instance.NormalizeColor();
        }

        private static readonly string[] tileTypesToReplaceTextures = new string[]
        {
            "Bone",
            "Heart",
            "Poop",
            "Booger",
            "Tooth",
            "Pee",
            "Wild",
        };

        /// <summary>
        /// Changes puzzle tile textures
        /// </summary>
        [HarmonyPostfix, HarmonyPatch(typeof(BumboController), nameof(BumboController.Init))]
        static void BumboController_Init()
        {
            PrefabsDict tilePrefabsDict = PoolManager.Pools["Blocks"].prefabs;
            string[] tileKeys = tilePrefabsDict.Keys.ToArray();

            //Change block active material texture
            if (tilePrefabsDict.TryGetValue("Block", out Transform block) && block != null && block.TryGetComponent(out Block blockComponent)) blockComponent.activeMaterial.mainTexture = Windfall.assetBundle.LoadAsset<Texture2D>("Tiles Active");

            //Change tile prefab textures
            List<string> replaceTileKeys = new List<string>();
            foreach (string key in tileKeys)
            {
                //If the key contains any target tile types and is not inactive, add it to the list
                if (tileTypesToReplaceTextures.Any(key.Contains) && !key.Contains("Inactive")) replaceTileKeys.Add(key);
            }

            foreach (string key in replaceTileKeys)
            {
                if (tilePrefabsDict.TryGetValue(key, out var tile) && tile != null)
                {
                    MeshRenderer[] meshRenderers = tile.GetComponentsInChildren<MeshRenderer>();
                    foreach (MeshRenderer renderer in meshRenderers) renderer.material.mainTexture = Windfall.assetBundle.LoadAsset<Texture2D>("Tiles Active");
                }
            }
        }

        /// <summary>
        /// Modifies created shapes for compatibility with BlockGroups such that they follow BlockGroup matching logic. Triggers <see cref="BlockGroupHelper.ModifyPuzzleShapes"/> in <see cref="Puzzle.ClearMatches"/>.
        /// </summary>
        /// <param name="instructions">The method instructions.</param>
        /// <returns>Transpiled instructions.</returns>
        [HarmonyPatch(typeof(Puzzle), nameof(Puzzle.ClearMatches))]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);
            
            int insertionIndex = -1;
            for (int i = 0; i < code.Count - 1; i++)
            {
                //Get insertion index
                if (insertionIndex < 0 && i >= 2 && code[i].opcode == OpCodes.Newarr && code[i - 1].opcode == OpCodes.Ldfld && code[i - 2].opcode == OpCodes.Ldarg_0)
                {
                    insertionIndex = i;
                    break;
                }
            }

            if (insertionIndex >= 0)
            {
                var instructionsToInsert = new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BlockGroupHelper), nameof(BlockGroupHelper.ModifyPuzzleShapes))),
                };

                code.InsertRange(insertionIndex, instructionsToInsert);
            }

            return code;
        }

        /// <summary>
        /// Removes block groups when the puzzle board is initializing.
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(Puzzle), nameof(Puzzle.initializePuzzle))]
        static void Puzzle_initializePuzzle()
        {
            WindfallHelper.BlockGroupController.RemoveBlockGroups();
        }

        /// <summary>
        /// Replaces vanilla <see cref="Puzzle.moveBlock"/> implementation.
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(Puzzle), "moveBlock")]
        static bool Puzzle_moveBlock(GameObject block, short _to_x, short _to_y, float _time)
        {
            PuzzleHelper.ReplaceMoveBlock(block, _to_x, _to_y, _time);
            return false;
        }

        /// <summary>
        /// Replaces vanilla <see cref="Puzzle.consolidatePuzzle"/> implementation.
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(Puzzle), nameof(Puzzle.consolidatePuzzle))]
        static bool Puzzle_consolidatePuzzle()
        {
            PuzzleHelper.ReplaceConsolidatePuzzle();
            return false;
        }

        /// <summary>
        /// Replaces vanilla <see cref="Puzzle.setPositions"/> implementation.
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(Puzzle), nameof(Puzzle.setPositions))]
        static bool Puzzle_setPositions()
        {
            PuzzleHelper.ReplaceSetPositions();
            return false;
        }

        /// <summary>
        /// Replaces vanilla <see cref="Puzzle.fillPuzzle"/> implementation.
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(Puzzle), nameof(Puzzle.fillPuzzle))]
        static bool Puzzle_fillPuzzle(ref List<int> _empty_spaces)
        {
            Puzzle puzzle = WindfallHelper.app.view.puzzle;

            //Override default empty space implementation
            if (_empty_spaces != null) _empty_spaces = PuzzleHelper.FindEmptySpaces();

            //Abort custom refill logic unless the puzzle board is refilling
            if (WindfallHelper.app.controller.savedStateController.IsLoading()) return true;
            if (WindfallHelper.app.model.characterSheet.currentFloor == 0) return true;
            if (_empty_spaces == null) return true;
            if ((bool)AccessTools.Field(typeof(Puzzle), "totally_fill").GetValue(puzzle) == true) return true;

            //Custom refill logic
            PuzzleHelper.ReplaceFillPuzzle(_empty_spaces);
            return false;
        }

        /// <summary>
        /// Replaces vanilla <see cref="Puzzle.Shuffle"/> implementation.
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(Puzzle), nameof(Puzzle.Shuffle))]
        static bool Puzzle_Shuffle()
        {
            PuzzleHelper.ShufflePuzzleBoard(true, false, false);
            return false;
        }

        /// <summary>
        /// Removes all BlockGroups before Blocks despawn in <see cref="Puzzle.Despawn"/> implementation.
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(Puzzle), nameof(Puzzle.Despawn))]
        static void Puzzle_Despawn()
        {
            WindfallHelper.BlockGroupController.RemoveBlockGroups();
        }

        /// <summary>
        /// Removes BlockGroup before the Block despawns in <see cref="Block.Despawn(bool)"/> implementation.
        /// The method is set up to abort when used in vanilla logic (except during ClearShapeEvent) by detecting the spawnPaper boolean value.
        /// This is because Block despawns are already handled by <see cref="PuzzleHelper.PlaceBlock"/> and need to be removed from vanilla spell logic.
        /// The vanilla method can still be used by calling the method with spawnPaper equal to true.
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(Block), nameof(Block.Despawn))]
        static bool Block_Despawn(Block __instance, ref bool spawnPaper)
        {
            if (!(WindfallHelper.app.model.bumboEvent.ToString() == "ClearShapeEvent"))
            {
                if (!spawnPaper) return false;
                spawnPaper = false;
            }

            BlockGroup blockGroup = __instance.GetComponent<BlockGroup>();
            if (blockGroup != null) WindfallHelper.BlockGroupController.RemoveBlockGroup(blockGroup);
            return true;
        }

        /// <summary>
        /// Modifies Gamepad puzzle logic to account for BlockGroups when moving the hover cursor.
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(GamepadPuzzleController), "attach_hover_block")]
        static bool GamepadPuzzleController_attach_hover_block(GamepadPuzzleController __instance)
        {
            //Only apply to gamepad input
            if (!InputManager.Instance.IsUsingGamepadInput()) return true;

            //Hover Block null check failsafe
            GameObject m_HoverBlock = (GameObject)AccessTools.Field(typeof(GamepadPuzzleController), "m_HoverBlock").GetValue(__instance);
            if (m_HoverBlock == null) return false;

            Puzzle puzzle = WindfallHelper.app.view.puzzle;

            //Get Position of intended new hover Block
            int m_HoverColumn = (int)AccessTools.Field(typeof(GamepadPuzzleController), "m_HoverColumn").GetValue(__instance);
            int m_HoverRow = (int)AccessTools.Field(typeof(GamepadPuzzleController), "m_HoverRow").GetValue(__instance);

            Block previousHoverBlock = m_HoverBlock.transform.parent?.gameObject?.GetComponent<Block>();
            Block newHoverBlock = puzzle.blocks[m_HoverColumn, m_HoverRow]?.GetComponent<Block>();
            //New hover block null check failsafe
            if (newHoverBlock == null) return false;

            //Handle BlockGroup collisions
            BlockGroup newBlockGroup = WindfallHelper.BlockGroupController.FindGroupOfBlock(newHoverBlock);
            if (newBlockGroup != null)
            {

                //Always allow movement into the main Block
                if (WindfallHelper.BlockGroupController.IsMainBlock(newHoverBlock)) return true;

                //By default, move the cursor to the main Block of the BlockGroup
                Position setCursorPosition = newBlockGroup.GetPosition();

                if (previousHoverBlock != null)
                {
                    BlockGroup previousBlockGroup = WindfallHelper.BlockGroupController.FindGroupOfBlock(previousHoverBlock);

                    //The cursor is attempting to move around inside the BlockGroup; it must be moved further to the edge of the BlockGroup instead
                    if (previousBlockGroup != null && newBlockGroup == previousBlockGroup)
                    {
                        Vector2Int distanceMoved = PuzzleHelper.Distance(previousHoverBlock.position, newHoverBlock.position, true);

                        if (distanceMoved.x == 1) setCursorPosition.x += newBlockGroup.GetDimensions().x; //Move cursor just beyond the right edge of the BlockGroup
                        else if (distanceMoved.x == -1) setCursorPosition.x -= 1; //Move cursor just beyond the left edge of the BlockGroup (should not be necessary)

                        if (distanceMoved.y == 1) setCursorPosition.y += newBlockGroup.GetDimensions().y; //Move cursor just beyond the top edge of the BlockGroup
                        else if (distanceMoved.y == -1) setCursorPosition.y -= 1; //Move cursor just beyond the bottom edge of the BlockGroup (should not be necessary)

                        //Move within grid bounds
                        setCursorPosition = PuzzleHelper.MoveWithinPuzzleBounds(setCursorPosition, true);

                        //Account for new position being inside another BlockGroup
                        BlockGroup blockGroupCollision = WindfallHelper.BlockGroupController.FindGroupOfBlock(puzzle.blocks[setCursorPosition.x, setCursorPosition.y]?.GetComponent<Block>());
                        if (blockGroupCollision != null) setCursorPosition = blockGroupCollision.GetPosition();
                    }
                }

                //Set row & column
                AccessTools.Field(typeof(GamepadPuzzleController), "m_HoverColumn").SetValue(__instance, setCursorPosition.x);
                AccessTools.Field(typeof(GamepadPuzzleController), "m_HoverRow").SetValue(__instance, setCursorPosition.y);
                return true;
            }

            return true;
        }

        /// <summary>
        /// Replaces vanilla <see cref="Puzzle.setBlock"/> implementation.
        /// A hacky solution is used to access the vanilla PlaceBlock method (otherwise an infinite loop would occur)
        /// Ideally this would be accomplished with a Harmony Reverse Patch to copy the vanilla method, but the Reverse Patch doesn't seem to work for this method
        /// The vanilla method can be accessed by adding 1000 to the _x argument before calling the method.
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(Puzzle), nameof(Puzzle.setBlock))]
        static bool Puzzle_setBlock(Block.BlockType _block_type, ref short _x, short _y, bool _animate, bool _wiggle)
        {
            if (_x >= 900)
            {
                _x = (short)(_x - 1000);
                return true;
            }

            PuzzleHelper.PlaceBlock(new Position(_x, _y), _block_type, _animate, _wiggle);
            return false;
        }

        /// <summary>
        /// Adapts <see cref="ClearShapeEvent.DespawnTiles"/> for compatibility with BlockGroups.
        /// Ensures the entire BlockGroup is cleared in cases where only part of the BlockGroup is in the next shape to clear. This often occurs when a spell effect removes tiles from the puzzle board.
        /// This must also trigger for conventional tile matches to clear properly due tile combo shape resizing in <see cref="BlockGroup.ResizeByComboContribution"/>.
        /// The Blocks are added to the puzzleShape in this method (after <see cref="ClearShapeEvent.Execute"/>, not before) because otherwise the tiles would contribute to tile combo size and mana gain.
        /// Also overrides vanilla wild tile logic to ensure wild tiles that are in BlockGroups have the correct number of uses as they are being triggered in tile combos.
        /// TODO: Fix BumboWoo triggering based on the larger combo size as a result of this patch.
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(ClearShapeEvent), nameof(ClearShapeEvent.DespawnTiles))]
        static void ClearShapeEvent_DespawnTiles(ClearShapeEvent __instance)
        {
            List<PuzzleShape> shapesToClear = WindfallHelper.app.model.puzzleModel.shapesToClear;
            if (shapesToClear.Count < 1) return;

            PuzzleShape puzzleShape = shapesToClear[0];
            if (puzzleShape == null) return;

            List<BlockGroup> blockGroups = new List<BlockGroup>();

            //Get all BlockGroups in the puzzleShape
            foreach (GameObject tile in puzzleShape.tiles)
            {
                Block block = tile.GetComponent<Block>();
                if (block == null) continue;

                BlockGroup blockGroup = WindfallHelper.BlockGroupController.FindGroupOfBlock(block);
                if (blockGroup != null && !blockGroups.Contains(blockGroup)) blockGroups.Add(blockGroup);
            }

            List<GameObject> blocksToAdd = new List<GameObject>();

            //Get all Blocks from the BlockGroups that are not already in the puzzleShape
            foreach (BlockGroup blockGroup in blockGroups)
            {
                List<GameObject> blockGroupBlocks = blockGroup.GetBlocks();

                foreach (GameObject blockGroupBlock in blockGroupBlocks)
                {
                    if (!puzzleShape.tiles.Contains(blockGroupBlock) && !blocksToAdd.Contains(blockGroupBlock)) blocksToAdd.Add(blockGroupBlock);
                }
            }

            foreach (GameObject block in blocksToAdd)
            {
                //Add the Blocks to the puzzleShape
                puzzleShape.tiles.Add(block);

                //Set blocks space to null
                Block blockComponent = block.GetComponent<Block>();
                WindfallHelper.app.view.puzzle.blocks[blockComponent.position.x, blockComponent.position.y] = null;
            }

            SetWildUses(shapesToClear);
        }

        /// <summary>
        /// Updates wild tiles in the first shape to have the correct number of uses remaining according to the given shapesToClear.
        /// </summary>
        private static void SetWildUses(List<PuzzleShape> shapesToClear)
        {
            PuzzleShape puzzleShape = shapesToClear[0];
            if (puzzleShape == null) return;

            foreach (GameObject tile in puzzleShape.tiles)
            {
                Block block = tile.GetComponent<Block>();
                if (block == null) continue;
                if (block.block_type != Block.BlockType.Wild) continue;

                int wildUses = 0;
                foreach (PuzzleShape shape in shapesToClear)
                {
                    if (shape.tiles.Contains(tile)) wildUses++;
                }

                block.wildUses = wildUses;
            }
        }

        /// <summary>
        /// Adjusts <see cref="PuzzlePlacementView.StartDrag"/> to correcly scale the tile placement indicator when a BlockGroup is being dragged.
        /// </summary>
        [HarmonyPostfix, HarmonyPatch(typeof(PuzzlePlacementView), nameof(PuzzlePlacementView.StartDrag))]
        static void PuzzlePlacementView_StartDrag(PuzzlePlacementView __instance)
        {
            Puzzle puzzle = WindfallHelper.app.view.puzzle;

            if (puzzle.selected_block != null)
            {
                //If a BlockGroup is being dragged, scale the placement object according to the BlockGroup dimensions
                BlockGroup blockGroup = WindfallHelper.BlockGroupController.FindGroupOfBlock(puzzle.selected_block);
                if (blockGroup != null)
                {
                    //Scale dimensions
                    Vector2Int dimensions = blockGroup.GetDimensions();
                    float firstPlacementScaleX = 1.1f * dimensions.x;
                    float secondPlacementScaleX = 1f * dimensions.x;
                    float placementScaleZ = 1.1f * dimensions.y;

                    //Replace the original tweening sequence
                    DOTween.Kill(__instance.name, false);
                    DOTween.Sequence().Append(ShortcutExtensions.DOScale(__instance.placementObject.transform, new Vector3(firstPlacementScaleX, 0.5f, placementScaleZ), 0.15f).SetEase(Ease.InOutQuad)).Join(ShortcutExtensions.DOScale(__instance.positionObject.transform, new Vector3(1.1f, 0.25f, 1.1f), 0.15f).SetEase(Ease.InOutQuad)).Append(ShortcutExtensions.DOScale(__instance.placementObject.transform, new Vector3(secondPlacementScaleX, 0.5f, placementScaleZ), 0.15f).SetEase(Ease.InOutQuad)).Join(ShortcutExtensions.DOScale(__instance.positionObject.transform, new Vector3(1f, 0.25f, 1.1f), 0.15f).SetEase(Ease.InOutQuad)).SetId(__instance.name);
                }
            }
        }

        /// <summary>
        /// Adjusts <see cref="Puzzle.highlightRowsAndColumns"/> to account for BlocKGroups when highlighting tiles.
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(Puzzle), nameof(Puzzle.highlightRowsAndColumns))]
        static bool Puzzle_highlightRowsAndColumns(Puzzle __instance)
        {
            Block selectedBlock = __instance.selected_block?.GetComponent<Block>();

            if (selectedBlock != null)
            {
                BlockGroup selectedBlockGroup = WindfallHelper.BlockGroupController.FindGroupOfBlock(selectedBlock);
                Position position = selectedBlock.position;

                Vector2Int dimensions = selectedBlockGroup != null ? selectedBlockGroup.GetDimensions() : new Vector2Int(1, 1);

                List<BlockGroup> blockingGroups = new List<BlockGroup>();
                blockingGroups.AddRange(BlockGroupHelper.BlockingGroupsAlongAxis(position, dimensions, true));
                blockingGroups.AddRange(BlockGroupHelper.BlockingGroupsAlongAxis(position, dimensions, false));

                if (selectedBlockGroup != null)
                {
                    BlockGroupHelper.RemoveAlignedGroups(selectedBlockGroup, blockingGroups, true);
                    BlockGroupHelper.RemoveAlignedGroups(selectedBlockGroup, blockingGroups, false);
                }

                for (int i = 0; i < __instance.width; i++)
                {
                    for (int j = 0; j < __instance.height; j++)
                    {
                        Block block = __instance.blocks[i, j].GetComponent<Block>();
                        if (block == null || block == selectedBlock) continue;

                        BlockGroup blockGroup = WindfallHelper.BlockGroupController.FindGroupOfBlock(block);

                        bool outsideRangeX = i < position.x || i > position.x + (dimensions.x - 1);
                        bool outsideRangeY = j < position.y || j > position.y + (dimensions.y - 1);

                        bool insideBlockingGroup = blockGroup != null && blockingGroups.Contains(blockGroup);

                        bool darken = (outsideRangeX && outsideRangeY) || insideBlockingGroup;

                        if (darken) __instance.blocks[i, j].SendMessage("DarkenColor");
                        else __instance.blocks[i, j].SendMessage("LightenColor");
                    }
                }
            }
            return false;
        }
    }
}