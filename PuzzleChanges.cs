using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
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
            PuzzleHelper.ShufflePuzzleBoard(true);
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
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(Block), nameof(Block.Despawn))]
        static void Block_Despawn(Block __instance)
        {
            BlockGroup blockGroup = __instance.GetComponent<BlockGroup>();
            if (blockGroup != null) BlockGroupModel.RemoveBlockGroup(blockGroup);
        }
    }
}
