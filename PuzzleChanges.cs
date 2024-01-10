using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    class PuzzleChanges
    {
        public static void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(PuzzleChanges));
        }

        /// <summary>
        /// Connects matches that share BlockGroups when the puzzle board is creating matches. Triggers <see cref="BlockGroupModel.CombineShapesThatShareBlockGroups"/> in <see cref="Puzzle.ClearMatches"/>.
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
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BlockGroupModel), nameof(BlockGroupModel.CombineShapesThatShareBlockGroups))),
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
            BlockGroupModel.RemoveBlockGroups();
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
            BlockGroupModel.RemoveBlockGroups();
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
                if (!spawnPaper)
                {
                    return false;
                }

                spawnPaper = false;
            }

            BlockGroup blockGroup = __instance.GetComponent<BlockGroup>();
            if (blockGroup != null) BlockGroupModel.RemoveBlockGroup(blockGroup);

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
            BlockGroup newBlockGroup = BlockGroupModel.FindGroupOfBlock(newHoverBlock);
            if (newBlockGroup != null)
            {

                //Always allow movement into the main Block
                if (BlockGroupModel.IsMainBlock(newHoverBlock)) return true;

                //By default, move the cursor to the main Block of the BlockGroup
                Position setCursorPosition = newBlockGroup.GetPosition();

                if (previousHoverBlock != null)
                {
                    BlockGroup previousBlockGroup = BlockGroupModel.FindGroupOfBlock(previousHoverBlock);

                    //The cursor is attempting to move around inside the BlockGroup; it must be moved further to the edge of the BlockGroup instead
                    if (previousBlockGroup != null && newBlockGroup == previousBlockGroup)
                    {
                        Vector2Int distanceMoved = PuzzleHelper.Distance(previousHoverBlock.position, newHoverBlock.position, true);

                        if (distanceMoved.x == 1) setCursorPosition.x += newBlockGroup.GetDimensions().x; //Move cursor just beyond the right edge of the BlockGroup
                        else if (distanceMoved.x == -1) setCursorPosition.x -= 1; //Move cursor just beyond the left edge of the BlockGroup (should not be necessary)

                        if (distanceMoved.y == 1) setCursorPosition.y += newBlockGroup.GetDimensions().y; //Move cursor just beyond the top edge of the BlockGroup
                        else if (distanceMoved.y == -1) setCursorPosition.y -= 1; //Move cursor just beyond the bottom edge of the BlockGroup (should not be necessary)

                        //Move within grid bounds
                        setCursorPosition = PuzzleHelper.MoveWithinGridBounds(setCursorPosition, true);

                        //Account for new position being inside another BlockGroup
                        BlockGroup blockGroupCollision = BlockGroupModel.FindGroupOfBlock(puzzle.blocks[setCursorPosition.x, setCursorPosition.y]?.GetComponent<Block>());
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
        /// The vanilla method be accessed by adding 1000 to the _x argument before calling the method.
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
        /// Adapts <see cref="ClearShapeEvent.Execute"/> for compatibility with BlockGroups. Ensures the entire BlockGroup is cleared in cases where only part of the BlockGroup is in the next shape to clear. This often occurs when a spell effect removes tiles from the puzzle board.
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(ClearShapeEvent), nameof(ClearShapeEvent.Execute))]
        static void ClearShapeEvent_Execute(ClearShapeEvent __instance)
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

                BlockGroup blockGroup = BlockGroupModel.FindGroupOfBlock(block);
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

            foreach(GameObject block in blocksToAdd)
            {
                //Add the Blocks to the puzzleShape
                puzzleShape.tiles.Add(block);

                //Set blocks space to null
                Block blockComponent = block.GetComponent<Block>();
                WindfallHelper.app.view.puzzle.blocks[blockComponent.position.x, blockComponent.position.y] = null;
            }
        }
    }
}