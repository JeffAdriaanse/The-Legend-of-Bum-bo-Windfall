using DG.Tweening;
using HarmonyLib;
using Mono.Collections.Generic;
using PathologicalGames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using static Block;

namespace The_Legend_of_Bum_bo_Windfall
{
    public static class PuzzleHelper
    {
        //Block display size
        public static readonly float BLOCK_SIZE = 1f;

        /// <summary>
        /// Shuffles the puzzle board. Intended to replace vanilla implementation in the <see cref="Puzzle.Shuffle"/> method and in spell logic.
        /// </summary>
        /// <param name="avoidCreatingCombos">Whether to attempt to avoid creating tile combos. Note that combos will not always be avoided perfectly, especially when there are a lot of BlockGroups being shuffled.</param>
        /// <param name="animateBlocks">Whether to animate the Blocks.</param>
        /// <param name="wiggleBlocks">Whether to wiggle the Blocks.</param>
        public static void ShufflePuzzleBoard(bool avoidCreatingCombos, bool animateBlocks, bool wiggleBlocks)
        {
            Puzzle puzzle = WindfallHelper.app.view.puzzle;

            //Store all BlockTypes on the puzzle board
            Dictionary<Block.BlockType, int> blockTypes = BlockTypeCounts(true, false);

            //Store all BlockGroups on the puzzle board
            List<Tuple<Block.BlockType, BlockGroupData>> blockGroups = new List<Tuple<Block.BlockType, BlockGroupData>>();
            foreach (BlockGroup blockGroup in BlockGroupModel.blockGroups)
            {
                blockGroups.Add(new Tuple<Block.BlockType, BlockGroupData>(blockGroup.MainBlock.block_type, blockGroup.GetBlockGroupData()));
            }

            //Clear the puzzle board
            puzzle.Despawn();

            //Randomly place BlockGroups
            foreach (Tuple<Block.BlockType, BlockGroupData> blockGroup in blockGroups)
            {
                //Add all possible BlockGroup Positions
                List<Position> validGroupPositions = new List<Position>();
                for (int j = 0; j < puzzle.height; j++)
                {
                    for (int i = 0; i < puzzle.width; i++)
                    {
                        Position boardPosition = new Position(i, j);
                        if (BlockGroupModel.ValidGroupPosition(boardPosition, blockGroup.Item2.dimensions, false)) validGroupPositions.Add(boardPosition);
                    }
                }

                //Proceed if there is at least one valid Position
                if (validGroupPositions.Count > 0)
                {
                    //Randomly select a BlockGroup Position
                    Position randomPosition = validGroupPositions[UnityEngine.Random.Range(0, validGroupPositions.Count)];

                    //Place BlockGroup
                    if (BlockGroupModel.PlaceBlockGroup(randomPosition, blockGroup.Item1, blockGroup.Item2)) continue;
                }

                //If the BlockGroup could not be created, it is split up into individual Blocks
                int numberOfBlocks = blockGroup.Item2.dimensions.x * blockGroup.Item2.dimensions.y;
                if (blockTypes.ContainsKey(blockGroup.Item1)) blockTypes[blockGroup.Item1] += numberOfBlocks;
                else blockTypes.Add(blockGroup.Item1, numberOfBlocks);
                continue;
            }

            //Randomly place Blocks
            for (int j = 0; j < puzzle.height; j++)
            {
                for (int i = 0; i < puzzle.width; i++)
                {
                    //Avoid overriding Blocks
                    if (puzzle.blocks[i, j] != null) continue;

                    List<Block.BlockType> neighbouringBlockTypes = null;
                    if (avoidCreatingCombos)
                    {
                        //Avoid placing Blocks that match neighbouring BlockTypes
                        neighbouringBlockTypes = NeighbouringBlockTypes(new Position(i, j));

                        //Check whether removing neighbours will leave any BlockTypes remaining
                        bool blockTypeExists = false;
                        for (int blockTypeCounter = 0; blockTypeCounter < blockTypes.Count; blockTypeCounter++)
                        {
                            KeyValuePair<Block.BlockType, int> blockType = blockTypes.ElementAt(blockTypeCounter);
                            if (!neighbouringBlockTypes.Contains(blockType.Key))
                            {
                                blockTypeExists = true;
                                break;
                            }
                        }

                        //Abort if no BlockTypes will remain
                        if (!blockTypeExists) neighbouringBlockTypes = null;
                    }

                    //Choose the BlockTypes with the most Blocks
                    List<Block.BlockType> chosenBlockTypes = new List<Block.BlockType>();
                    int max = 0;
                    for (int blockTypeCounter = 0; blockTypeCounter < blockTypes.Count; blockTypeCounter++)
                    {
                        KeyValuePair<Block.BlockType, int> blockType = blockTypes.ElementAt(blockTypeCounter);

                        //Avoid neighbouring BlockTypes
                        if (neighbouringBlockTypes != null && neighbouringBlockTypes.Contains(blockType.Key)) continue;

                        //Select max BlockTypes
                        if (blockType.Value > max)
                        {
                            chosenBlockTypes.Clear();
                            max = blockType.Value;
                        }
                        if (blockType.Value == max) chosenBlockTypes.Add(blockType.Key);
                    }

                    //Randomly select a BlockType
                    Block.BlockType randomBlockType = chosenBlockTypes[UnityEngine.Random.Range(0, chosenBlockTypes.Count)];

                    //Place Block
                    PlaceBlock(new Position(i, j), randomBlockType, animateBlocks, wiggleBlocks);
                    //Reduce placed BlockType
                    blockTypes[randomBlockType]--;
                    //Remove empty BlockTypes
                    if (blockTypes[randomBlockType] <= 0) blockTypes.Remove(randomBlockType);
                }
            }
        }

        /// <summary>
        /// Rerolls the puzzle board. Intended to replace vanilla implementation in spell logic.
        /// </summary>
        /// <param name="animateBlocks">Whether to animate the Blocks.</param>
        /// <param name="wiggleBlocks">Whether to wiggle the Blocks.</param>
        public static void RerollPuzzleBoard(bool animateBlocks, bool wiggleBlocks)
        {
            Puzzle puzzle = WindfallHelper.app.view.puzzle;

            //Clear the puzzle board
            puzzle.Despawn();

            //Place random tiles
            for (int i = 0; i < puzzle.width; i++)
            {
                for (int j = 0; j < puzzle.height; j++)
                {
                    PlaceBlock(new Position(i, j), puzzle.nextBlock(), animateBlocks, wiggleBlocks);
                }
            }

            WindfallHelper.app.controller.PlayPuzzleParticles();
        }

        /// <summary>
        /// Returns a List containing the BlockTypes of all neighbouring Blocks on the puzzle board.
        /// </summary>
        /// <param name="position">The position to find neighbouring BlockTypes of.</param>
        /// <returns>A List containing the BlockTypes of all neighbouring Blocks on the puzzle board.</returns>
        public static List<Block.BlockType> NeighbouringBlockTypes(Position position)
        {
            Puzzle puzzle = WindfallHelper.app.view.puzzle;

            List<Block.BlockType> blockTypes = new List<Block.BlockType>();

            //Negative x
            if (position.x > 0)
            {
                Block block = puzzle.blocks[position.x - 1, position.y]?.GetComponent<Block>();
                if (block != null) blockTypes.Add(block.block_type);
            }

            //Positive x
            if (position.x < puzzle.width - 1)
            {
                Block block = puzzle.blocks[position.x + 1, position.y]?.GetComponent<Block>();
                if (block != null) blockTypes.Add(block.block_type);
            }

            //Negative y
            if (position.y > 0)
            {
                Block block = puzzle.blocks[position.x, position.y - 1]?.GetComponent<Block>();
                if (block != null) blockTypes.Add(block.block_type);
            }

            //Positive y
            if (position.y < puzzle.height - 1)
            {
                Block block = puzzle.blocks[position.x, position.y + 1]?.GetComponent<Block>();
                if (block != null) blockTypes.Add(block.block_type);
            }

            return blockTypes;
        }

        /// <summary>
        /// Returns a list containing all blocks on the puzzle board.
        /// </summary>
        /// <param name="includeRegularBlocks">Whether to include regular blocks (blocks that are not in a BlockGroup)</param>
        /// <param name="includeBlockGroups">Whether to include BlockGroup blocks. Note that only the main block of each BlockGroup is included. Sub-blocks are not included.</param>
        /// <returns>All blocks on the puzzle board.</returns>
        private static List<Block> GetBlocks(bool includeRegularBlocks, bool includeBlockGroups)
        {
            Puzzle puzzle = WindfallHelper.app.view.puzzle;

            //Store blocks
            List<Block> blocks = new List<Block>();

            for (int j = 0; j < puzzle.height; j++)
            {
                for (int i = 0; i < puzzle.width; i++)
                {
                    Block block = puzzle.blocks[i, j].GetComponent<Block>();

                    //Handle blockGroups
                    BlockGroup blockGroup = BlockGroupModel.FindGroupOfBlock(block);
                    if (blockGroup != null)
                    {
                        //Ignore sub-blocks
                        if (includeBlockGroups && !BlockGroupModel.IsMainBlock(block)) blocks.Add(block);
                        continue;
                    }

                    //Regular blocks
                    if (includeRegularBlocks) blocks.Add(block);
                }
            }
            return blocks;
        }

        /// <summary>
        /// Returns a Dictionary containing the total number of Blocks of each BlockType on the puzzle board.
        /// </summary>
        /// <param name="includeRegularBlocks">Whether to include regular Blocks (Blocks that are not in a BlockGroup).</param>
        /// <param name="includeBlockGroups">Whether to include BlockGroup Blocks. Note that only the main Block of each BlockGroup is included. Sub-Blocks are not included.</param>
        /// <returns>The total number of Blocks of each BlockType on the puzzle board.</returns>
        public static Dictionary<Block.BlockType, int> BlockTypeCounts(bool includeRegularBlocks, bool includeBlockGroups)
        {
            Puzzle puzzle = WindfallHelper.app.view.puzzle;

            //Count the total number of tiles of each tile type
            Dictionary<Block.BlockType, int> amountOfEachBlockType = new Dictionary<Block.BlockType, int>();

            for (int j = 0; j < puzzle.height; j++)
            {
                for (int i = 0; i < puzzle.width; i++)
                {
                    Block block = puzzle.blocks[i, j].GetComponent<Block>();

                    //Resolve argument conditions
                    if (BlockGroupModel.FindGroupOfBlock(block) != null)
                    {
                        //Argument condition
                        if (!includeBlockGroups) continue;
                        //Ignore sub-Blocks
                        if (!BlockGroupModel.IsMainBlock(block)) continue;
                    }
                    else
                    {
                        //Argument condition
                        if (!includeRegularBlocks) continue;
                    }

                    //Increment Block count
                    if (amountOfEachBlockType.ContainsKey(block.block_type)) amountOfEachBlockType[block.block_type]++;
                    else amountOfEachBlockType.Add(block.block_type, 1);
                }
            }
            return amountOfEachBlockType;
        }

        /// <summary>
        /// Randomly places Blocks of the given BlockType. Avoids overriding tiles at the given avoid positions or of the given avoid types. Intended to be triggered in spell effect logic.
        /// </summary>
        /// <param name="blockType">The BlockType of the placed Blocks.</param>
        /// <param name="blockCount">The number of Blocks to place.</param>
        /// <param name="avoidPositions">The positions to avoid overriding.</param>
        /// <param name="avoidTypes">The BlockTypes to avoid overriding.</param>
        /// <param name="avoidMainBlocks">Whether to avoid overriding main Blocks in BlockGoups.</param>
        /// <param name="avoidSubBlocks">Whether to avoid overriding sub-Blocks in BlockGoups.</param>
        /// <param name="avoidAllBlocks">Whether to avoid overriding any Blocks.</param>
        public static void RandomlyPlaceBlocks(Block.BlockType blockType, int blockCount, List<Vector2Int> avoidPositions, List<Block.BlockType> avoidTypes, bool avoidMainBlocks, bool avoidSubBlocks, bool avoidAllBlocks)
        {
            //Find all valid placements
            List<Position> positions = RandomPositions(blockCount, avoidPositions, avoidTypes, avoidMainBlocks, avoidSubBlocks, avoidAllBlocks);

            //Randomly place Blocks
            while (blockCount > 0 && positions.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, positions.Count);

                //Place the Block
                PlaceBlock(positions[index], blockType, false, true);

                positions.RemoveAt(index);
                blockCount--;
            }
        }

        /// <summary>
        /// Returns a list of random Positions on the puzzle board according to the given conditions.
        /// </summary>
        /// <param name="count">The number of Positions to select.</param>
        /// <param name="avoidPositions">Specific Positions to avoid.</param>
        /// <param name="avoidTypes">BlockTypes to avoid Positions of.</param>
        /// <param name="avoidMainBlocks">Whether to avoid Positions of main Blocks in BlockGoups.</param>
        /// <param name="avoidSubBlocks">Whether to avoid Positions of sub-Blocks in BlockGoups.</param>
        /// <param name="avoidSubBlocks">Whether to avoid Positions any Blocks.</param>
        /// <returns>A list of random Positions on the puzzle board.</returns>
        public static List<Position> RandomPositions(int count, List<Vector2Int> avoidPositions, List<Block.BlockType> avoidTypes, bool avoidMainBlocks, bool avoidSubBlocks, bool avoidAllBlocks)
        {
            Puzzle puzzle = WindfallHelper.app.view.puzzle;

            List<Position> positions = new List<Position>();

            //Find all valid placements
            for (int i = 0; i < puzzle.width; i++)
            {
                for (int j = 0; j < puzzle.height; j++)
                {
                    //Get block
                    Block block = puzzle.blocks[i, j]?.GetComponent<Block>();

                    //Add Position manually if space is empty
                    if (block == null)
                    {
                        positions.Add(new Position(i, j));
                        continue;
                    }

                    if (avoidAllBlocks) continue;

                    bool addPosition = true;

                    //Avoid given BlockTypes
                    if (avoidTypes != null)
                    {
                        foreach (Block.BlockType avoidType in avoidTypes)
                        {
                            if (block.block_type == avoidType)
                            {
                                addPosition = false;
                                break;
                            }
                        }
                    }

                    if (!addPosition) continue;

                    //Avoid given Positions
                    if (avoidPositions != null)
                    {
                        foreach (Vector2Int position in avoidPositions)
                        {
                            if (position.x == i && position.y == j)
                            {
                                addPosition = false;
                                break;
                            }
                        }
                    }

                    if (!addPosition) continue;

                    //BlockGroup conditions
                    BlockGroup blockGroup = BlockGroupModel.FindGroupOfBlock(block);
                    if (blockGroup != null)
                    {
                        if (BlockGroupModel.IsMainBlock(block))
                        {
                            if (avoidMainBlocks)
                            {
                                //Block is a main Block
                                addPosition = false;
                            }
                        }
                        else
                        {
                            if (avoidSubBlocks)
                            {
                                //Block is a sub-Block
                                addPosition = false;
                            }
                        }
                    }

                    if (!addPosition) continue;

                    //Add the block
                    positions.Add(block.position);
                }
            }

            //Random Positions
            List<Position> randomPositions = new List<Position>();
            for (int positionCounter = 0; positionCounter < count; positionCounter++)
            {
                if (positions.Count < 1) break;

                //Add random Position
                int randomIndex = UnityEngine.Random.Range(0, positions.Count);
                randomPositions.Add(positions[randomIndex]);

                //Remove from overall Positions
                positions.RemoveAt(randomIndex);
            }

            return randomPositions;
        }

        /// <summary>
        /// Places a Block of the given BlockType at the given position. If a BlockGroup occupies the given Position, the entire BlockGroup will be replaced. Intended to be triggered in BlockGroup logic or by spell effects.
        /// </summary>
        /// <param name="position">The position of the placed Block.</param>
        /// <param name="blockType">The BlockType of the placed Block.</param>
        /// <param name="animateBlock">Whether to animate the Block.</param>
        /// <param name="wiggleBlock">Whether to wiggle the Block.</param>
        /// <param name="replaceBlockGroups">Whether to fully replace the BlockGroup if the existing Block is in a BlockGroup.</param>
        /// <returns>The placed Block, or null if no Block was placed.</returns>
        public static Block PlaceBlock(Position position, Block.BlockType blockType, bool animateBlock, bool wiggleBlock, bool replaceBlockGroups = true)
        {
            Puzzle puzzle = WindfallHelper.app.view.puzzle;

            //Abort if outside puzzle board
            if (!IsWithinGridBounds(position)) return null;

            Block block = puzzle.blocks[position.x, position.y]?.GetComponent<Block>();
            if (block != null)
            {
                BlockGroup blockGroup = BlockGroupModel.FindGroupOfBlock(block);
                if (blockGroup != null)
                {
                    BlockGroupModel.RemoveBlockGroup(blockGroup);

                    //If the existing Block is in a BlockGroup, replace the entire BlockGroup
                    if (replaceBlockGroups)
                    {
                        Position blockGroupPosition = blockGroup.GetPosition();
                        BlockGroupData blockGroupData = blockGroup.GetBlockGroupData();

                        BlockGroup newBlockGroup = BlockGroupModel.PlaceBlockGroup(blockGroupPosition, blockType, blockGroupData);
                        return newBlockGroup?.MainBlock;
                    }
                }
            }

            //Remove existing Block
            if (block != null && block.gameObject.activeSelf) block.Despawn(true);

            //Place new Block
            //Add 1000 to x position to use vanilla method (see setBlock prefix patch)
            puzzle.setBlock(blockType, (short)(position.x + 1000), (short)position.y, animateBlock, wiggleBlock);

            //Get placed Block
            Block placedBlock = puzzle.blocks[position.x, position.y]?.GetComponent<Block>();

            //Update Block display
            DisplayBlock(placedBlock); //Note that if a BlockGroup main Block is placed with animation true, its visual position will not display correctly.

            return placedBlock;
        }

        /// <summary>
        /// Removes the given Block. Intended to be triggered in spell effect logic.
        /// </summary>
        /// <param name="block">The Block to remove.</param>
        /// <param name="puzzleShape">The puzzle shape for marking the removal of the block.</param>
        /// <param name="playSound">Whether to play the removal sound.</param>
        /// <param name="manaGain">The amount of mana to gain from the Block.</param>
        /// <param name="showManaGain">Whether to show a mana gain notification.</param>
        /// <param name="fromBlockGroup">Whether this Block is being removed by the main Block of its BlockGroup.</param>
        public static void RemoveBlock(Block block, PuzzleShape puzzleShape, bool playSound, short manaGain, bool showManaGain = true, bool fromBlockGroup = false)
        {
            if (block == null) return;

            if (!fromBlockGroup)
            {
                //Special behaviour for BlockGroups
                BlockGroup blockGroup = BlockGroupModel.FindGroupOfBlock(block);
                if (blockGroup != null)
                {
                    //If the main block is targeted, remove itself and all sub-Blocks
                    if (BlockGroupModel.IsMainBlock(block))
                    {
                        List<GameObject> blocks = blockGroup.GetBlocks();
                        blocks.Remove(block.gameObject);

                        BlockGroupModel.RemoveBlockGroup(blockGroup);

                        foreach (GameObject subBlock in blocks)
                        {
                            RemoveBlock(subBlock.GetComponent<Block>(), puzzleShape, false, manaGain, false, true);
                        }
                    }
                    else //If a sub-Block is targeted, abort
                    {
                        return;
                    }
                }
            }

            //Remove block
            WindfallHelper.app.view.puzzle.blocks[block.position.x, block.position.y] = null;
            //Puzzle shape
            if (puzzleShape != null) puzzleShape.tiles.Add(block.gameObject);

            //Grant mana
            if (manaGain > 0 && block.block_type < Block.BlockType.Wild)
            {
                short[] array = new short[6];
                array[(int)block.block_type] += manaGain;
                WindfallHelper.app.controller.UpdateMana(array, true);

                if (showManaGain) WindfallHelper.app.controller.ShowManaGain();
            }

            //Play sound
            if (playSound) WindfallHelper.app.view.soundsView.PlaySound(SoundsView.eSound.TileDestroyed, block.transform.position, SoundsView.eAudioSlot.Default, false);
        }

        /// <summary>
        /// Assigns correct scale and position to the given Block, accounting for its BlockGroup status.
        /// </summary>
        /// <param name="block"></param>
        public static void DisplayBlock(Block block)
        {
            if (block == null) return;

            BlockGroup blockGroup = BlockGroupModel.FindGroupOfBlock(block);

            //Scale
            Vector3 scale = new Vector3(BLOCK_SIZE, BLOCK_SIZE, BLOCK_SIZE);

            if (blockGroup != null)
            {
                //Get Dimensions
                Vector2Int dimensions = blockGroup.GetDimensions();

                //Is main block
                bool mainBlock = BlockGroupModel.IsMainBlock(block);

                //Set scale
                scale = mainBlock ? new Vector3(BLOCK_SIZE * dimensions.x, BLOCK_SIZE * dimensions.y, BLOCK_SIZE) : Vector3.zero;

                //Set position
                if (mainBlock)
                {
                    block.transform.localPosition = WorldSpaceBlockPosition(new Vector2Int(block.position.x, block.position.y), dimensions);
                }
            }

            //Get ButtonHoverAnimation
            ButtonHoverAnimation buttonHoverAnimation = block.GetComponent<ButtonHoverAnimation>();
            //Scale the Block
            WindfallHelper.SetHoverInitialScale(buttonHoverAnimation, scale);
            block.transform.localScale = scale;
        }

        /// <summary>
        /// The world space block position of the given Block or BlockGroup.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="dimensions"></param>
        /// <returns></returns>
        private static Vector3 WorldSpaceBlockPosition(Vector2Int position, Vector2Int dimensions)
        {
            Puzzle puzzle = WindfallHelper.app.view.puzzle;

            float xMin = (float)AccessTools.Method(typeof(Puzzle), "newXPosition").Invoke(puzzle, new object[] { (short)position.x });
            float zMin = (float)AccessTools.Method(typeof(Puzzle), "newZPosition").Invoke(puzzle, new object[] { (short)position.y });

            if (dimensions.x > 1 || dimensions.y > 1)
            {
                float xMax = (float)AccessTools.Method(typeof(Puzzle), "newXPosition").Invoke(puzzle, new object[] { (short)(position.x + (dimensions.x - 1)) });
                float zMax = (float)AccessTools.Method(typeof(Puzzle), "newZPosition").Invoke(puzzle, new object[] { (short)(position.y + (dimensions.y - 1)) });

                return new Vector3((xMax + xMin) / 2, 0.042f, (zMax + zMin) / 2);
            }

            return new Vector3(xMin, 0.042f, zMin);
        }

        /// <summary>
        /// Moves blocks that are being dragged by the player. Intended to replace <see cref="Puzzle.moveBlock"/>.
        /// </summary>
        public static void ReplaceMoveBlock(GameObject block, short _to_x, short _to_y, float _time)
        {
            //Current block
            Block blockComponent = block?.GetComponent<Block>();
            if (blockComponent == null) return;

            Puzzle puzzle = WindfallHelper.app.view.puzzle;

            //Selected block
            Block selectedBlockComponent = puzzle.selected_block?.GetComponent<Block>();
            if (selectedBlockComponent == null) return;

            //Only trigger logic on selected block
            if (blockComponent != selectedBlockComponent) return;

            BlockGroup selectedBlockGroup = null;
            if (selectedBlockComponent != null) selectedBlockGroup = BlockGroupModel.FindGroupOfBlock(selectedBlockComponent);

            bool horizontal = (Puzzle.MoveDirection)AccessTools.Field(typeof(Puzzle), "move_direction").GetValue(puzzle) == Puzzle.MoveDirection.Horizontal;

            //Original Block position
            Position originalBlockPosition = (Position)AccessTools.Field(typeof(Puzzle), "original_block_position").GetValue(puzzle);

            //Determine whether the tiles are moving in a positive direction
            bool positiveDirection;
            if (horizontal)
            {
                int delta_x = (int)AccessTools.Field(typeof(Puzzle), "delta_x").GetValue(puzzle);

                if (delta_x > 0)
                {
                    positiveDirection = true;
                }
                else if (delta_x < 0)
                {
                    positiveDirection = false;
                }
                else
                {
                    positiveDirection = _to_x - originalBlockPosition.x > 0;
                }
            }
            else
            {
                int delta_y = (int)AccessTools.Field(typeof(Puzzle), "delta_y").GetValue(puzzle);

                if (delta_y > 0)
                {
                    positiveDirection = true;
                }
                else if (delta_y < 0)
                {
                    positiveDirection = false;
                }
                else
                {
                    positiveDirection = _to_y - originalBlockPosition.y > 0;
                }
            }

            //Determine move distance
            int moveDistance;
            if (horizontal)
            {
                moveDistance = _to_x - originalBlockPosition.x;

                if (positiveDirection && moveDistance < 0) moveDistance = (_to_x + puzzle.width) - originalBlockPosition.x;
                if (!positiveDirection && moveDistance > 0) moveDistance = (_to_x - puzzle.width) - originalBlockPosition.x;
            }
            else
            {
                moveDistance = _to_y - originalBlockPosition.y;

                if (positiveDirection && moveDistance < 0) moveDistance = (_to_y + puzzle.height) - originalBlockPosition.y;
                if (!positiveDirection && moveDistance > 0) moveDistance = (_to_y - puzzle.height) - originalBlockPosition.y;
            }

            moveDistance = Math.Abs(moveDistance);
            moveDistance %= 9;

            //Position and dimensions
            Position selectedGroupPosition = new Position(originalBlockPosition.x, originalBlockPosition.y);
            Vector2Int selectedGroupDimensions;

            //Blocking groups and aligned groups
            List<BlockGroup> blockingGroups;

            if (selectedBlockGroup != null)
            {
                //Selected dimensions
                selectedGroupDimensions = selectedBlockGroup.GetDimensions();
                //Blocking groups
                blockingGroups = BlockGroupModel.BlockingGroupsAlongAxis(originalBlockPosition, selectedGroupDimensions, horizontal);
                //Aligned groups
                BlockGroupModel.RemoveAlignedGroups(selectedBlockGroup, blockingGroups, horizontal);
            }
            else
            {
                //Selected dimensions
                selectedGroupDimensions = new Vector2Int(1, 1);
                //Blocking groups
                blockingGroups = BlockGroupModel.BlockingGroupsAlongAxis(originalBlockPosition, selectedGroupDimensions, horizontal);
            }

            //Puzzle dimensions
            Vector2Int puzzleDimensions = new Vector2Int(puzzle.width, puzzle.height);

            //Invert axes when dragging vertically
            if (!horizontal)
            {
                selectedGroupPosition = new Position(selectedGroupPosition.y, selectedGroupPosition.x);
                selectedGroupDimensions = new Vector2Int(selectedGroupDimensions.y, selectedGroupDimensions.x);
                puzzleDimensions = new Vector2Int(puzzleDimensions.y, puzzleDimensions.x);
            }

            List<Tuple<Position, Position>> positions = new List<Tuple<Position, Position>>();

            //Iterate over the perpendicular dimension of the group
            for (int j = selectedGroupPosition.y; j < selectedGroupPosition.y + selectedGroupDimensions.y; j++)
            {
                int offset = 0;

                //Loop starting at the original block position
                for (int i = selectedGroupPosition.x; i < selectedGroupPosition.x + puzzleDimensions.x; i++)
                {
                    Position currentBlockPosition = horizontal ? new Position(i, j) : new Position(j, i);
                    currentBlockPosition = MoveWithinGridBounds(currentBlockPosition, true);

                    BlockGroup alignedBlockingGroup = AlignedWithBlockingGroup(currentBlockPosition, blockingGroups, horizontal);
                    if (alignedBlockingGroup != null)
                    {
                        //Ignore tiles aligned with blocking groups
                        offset++;
                        continue;
                    }

                    //Blocks are offset from the original block position
                    Position newPosition = horizontal ? new Position(originalBlockPosition.x + offset, currentBlockPosition.y) : new Position(currentBlockPosition.x, originalBlockPosition.y + offset);

                    int blockMoveDistance = moveDistance;
                    //Incrementally move block by movedistance
                    while (blockMoveDistance > 0)
                    {
                        if (horizontal)
                        {
                            newPosition.x += positiveDirection ? 1 : -1;
                        }
                        else
                        {
                            newPosition.y += positiveDirection ? 1 : -1;
                        }

                        int skipAttempt = 0;
                        //Skip over blocking groups
                        while (true)
                        {
                            skipAttempt++;
                            //Abort in the case of an infinite loop
                            if (skipAttempt >= 50)
                            {
                                Debug.LogError("Tile dragging error: Unable to skip blocking tiles");
                                return;
                            }

                            newPosition = MoveWithinGridBounds(newPosition, true);

                            //Skip past tile aligned with blocking groups
                            BlockGroup blockingGroup = AlignedWithBlockingGroup(newPosition, blockingGroups, horizontal);
                            if (blockingGroup != null)
                            {
                                if (horizontal)
                                {
                                    int farEdgePosition = blockingGroup.GetPosition().x;
                                    if (positiveDirection)
                                    {
                                        farEdgePosition += blockingGroup.GetDimensions().x - 1;
                                    }

                                    //Offset position to be one tile past the blocking group
                                    int blockingGroupOffset = Math.Abs(farEdgePosition - newPosition.x) + 1;
                                    newPosition.x += positiveDirection ? blockingGroupOffset : -blockingGroupOffset;
                                    //Repeat when a new position is reached
                                    continue;
                                }
                                else
                                {
                                    int farEdgePosition = blockingGroup.GetPosition().y;
                                    if (positiveDirection)
                                    {
                                        farEdgePosition += blockingGroup.GetDimensions().y - 1;
                                    }

                                    //Offset position to be one tile past the blocking group
                                    int blockingGroupOffset = Math.Abs(farEdgePosition - newPosition.y) + 1;
                                    newPosition.y += positiveDirection ? blockingGroupOffset : -blockingGroupOffset;
                                    //Repeat when a new position is reached
                                    continue;
                                }
                            }
                            break;
                        }
                        blockMoveDistance--;
                    }

                    newPosition = MoveWithinGridBounds(newPosition, true);

                    //Get current Block and BlockGroup
                    Block currentBlock = puzzle.blocks[currentBlockPosition.x, currentBlockPosition.y]?.GetComponent<Block>();
                    if (currentBlock == null) continue;
                    BlockGroup currentBlockGroup = BlockGroupModel.FindGroupOfBlock(currentBlock);

                    //Is the BlockGroup colliding with a blocking group or the edge of the puzzle board? If so, abort
                    if (currentBlockGroup != null && BlockGroupModel.IsMainBlock(currentBlock))
                    {
                        Vector2Int currentBlockGroupDimension = currentBlockGroup.GetDimensions();
                        for (int x = newPosition.x; x < newPosition.x + currentBlockGroupDimension.x; x++)
                        {
                            for (int y = newPosition.y; y < newPosition.y + currentBlockGroupDimension.y; y++)
                            {
                                Position nextBlockPosition = new Position(x, y);
                                //Abort
                                if (!IsWithinGridBounds(nextBlockPosition)) return;

                                Block nextBlock = puzzle.blocks[nextBlockPosition.x, nextBlockPosition.y]?.GetComponent<Block>();

                                BlockGroup nextBlockGroup = null;
                                if (nextBlock != null) nextBlockGroup = BlockGroupModel.FindGroupOfBlock(nextBlock);

                                //Abort
                                if (nextBlockGroup != null && blockingGroups.Contains(nextBlockGroup)) return;
                            }
                        }
                    }

                    positions.Add(new Tuple<Position, Position>(currentBlockPosition, newPosition));
                    offset++;
                }
            }

            //Move blocks
            foreach (Tuple<Position, Position> tuple in positions)
            {
                if (tuple == null) continue;

                Block moveBlock = puzzle.blocks[tuple.Item1.x, tuple.Item1.y]?.GetComponent<Block>();
                if (moveBlock == null) continue;

                MoveBlock(moveBlock.gameObject, tuple.Item2.x, tuple.Item2.y, _time);
            }
        }

        /// <summary>
        /// Returns any blocking group that is axis aligned with the given Position, or null if none is found.
        /// </summary>
        /// <param name="position">The Position.</param>
        /// <param name="blockingGroups">A list of blocking groups to search from.</param>
        /// <param name="horizontal">Whether to check on the horizontal or vertical axis.</param>
        /// <returns>Any blocking group that is axis aligned with the given Position, or null if none is found.</returns>
        private static BlockGroup AlignedWithBlockingGroup(Position position, List<BlockGroup> blockingGroups, bool horizontal)
        {
            foreach (BlockGroup blockingGroup in blockingGroups)
            {
                if (horizontal)
                {
                    //Check whether newPosition is horizontally aligned with the blocking group
                    if (blockingGroup.Contains(position, false, true))
                    {
                        return blockingGroup;
                    }
                }
                else
                {
                    //Check whether newPosition is vertically aligned with the blocking group
                    if (blockingGroup.Contains(position, true, false))
                    {
                        return blockingGroup;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Drops Blocks down after matches. Intended to replace <see cref="Puzzle.consolidatePuzzle"/>.
        /// </summary>
        public static void ReplaceConsolidatePuzzle()
        {
            Puzzle puzzle = WindfallHelper.app.view.puzzle;

            int[] blockHeights = new int[puzzle.width];
            for (int yIterator = 0; yIterator < puzzle.height; yIterator++)
            {
                for (int xIterator = 0; xIterator < puzzle.width; xIterator++)
                {
                    GameObject block = puzzle.blocks[xIterator, yIterator];
                    if (block != null)
                    {
                        //Only drop down blocks that are not in BlockGroups
                        BlockGroup blockGroup = BlockGroupModel.FindGroupOfBlock(block);
                        if (blockGroup != null)
                        {
                            if (!BlockGroupModel.IsMainBlock(block)) continue;

                            //When encountering a BlockGroup, find the distance to the highest available space beneath it
                            int distanceToHighestSpace = -1;
                            for (int widthIterator = 0; widthIterator < blockGroup.GetDimensions().x; widthIterator++)
                            {
                                while (true)
                                {
                                    if (blockHeights[xIterator + widthIterator] >= puzzle.height) break;

                                    Block spaceBlock = puzzle.blocks[xIterator + widthIterator, blockHeights[xIterator + widthIterator]]?.GetComponent<Block>();
                                    if (spaceBlock != null)
                                    {
                                        BlockGroup spaceBlockGroup = BlockGroupModel.FindGroupOfBlock(puzzle.blocks[xIterator + widthIterator, blockHeights[xIterator + widthIterator]]);
                                        if (spaceBlockGroup != null)
                                        {
                                            blockHeights[xIterator + widthIterator] += spaceBlockGroup.GetDimensions().y;
                                            continue;
                                        }
                                    }
                                    break;
                                }

                                int distanceToSpace = blockGroup.GetPosition().y - blockHeights[xIterator + widthIterator];
                                if (distanceToHighestSpace == -1 || distanceToSpace < distanceToHighestSpace) distanceToHighestSpace = distanceToSpace;
                            }

                            //If there is space, move the BlockGroup down by the distance
                            if (distanceToHighestSpace > 0)
                            {
                                Position position = blockGroup.GetPosition();
                                for (int heightIterator = 0; heightIterator < blockGroup.GetDimensions().y; heightIterator++)
                                {
                                    for (int widthIterator = 0; widthIterator < blockGroup.GetDimensions().x; widthIterator++)
                                    {
                                        int xPosition = position.x + widthIterator;
                                        int yPosition = position.y + heightIterator;

                                        GameObject groupBlock = puzzle.blocks[xPosition, yPosition];

                                        MoveBlock(groupBlock, xPosition, yPosition - distanceToHighestSpace, 0.2f);
                                        puzzle.blocks[xPosition, yPosition - distanceToHighestSpace] = puzzle.blocks[xPosition, yPosition];
                                        puzzle.blocks[xPosition, yPosition] = null;

                                        blockHeights[xPosition]++;
                                    }
                                }
                            }
                            continue;
                        }

                        while (true)
                        {
                            if (blockHeights[xIterator] >= puzzle.height) break;

                            Block spaceBlock = puzzle.blocks[xIterator, blockHeights[xIterator]]?.GetComponent<Block>();
                            if (spaceBlock != null)
                            {
                                BlockGroup spaceBlockGroup = BlockGroupModel.FindGroupOfBlock(puzzle.blocks[xIterator, blockHeights[xIterator]]);
                                if (spaceBlockGroup != null)
                                {
                                    blockHeights[xIterator] += spaceBlockGroup.GetDimensions().y;
                                    continue;
                                }
                            }
                            break;
                        }

                        if (yIterator != blockHeights[xIterator])
                        {
                            if (blockHeights[xIterator] >= puzzle.height) continue;

                            MoveBlock(block, xIterator, blockHeights[xIterator], 0.2f);
                            puzzle.blocks[xIterator, blockHeights[xIterator]] = block;
                            puzzle.blocks[xIterator, yIterator] = null;
                        }

                        blockHeights[xIterator]++;
                    }
                }
            }
        }

        /// <summary>
        /// Sets positions of dragged Blocks. Intended to replace <see cref="Puzzle.setPositions"/>.
        /// </summary>
        public static void ReplaceSetPositions()
        {
            BumboView bumboView = WindfallHelper.app.view;
            bumboView.puzzlePlacement.EndDrag();

            Puzzle puzzle = bumboView.puzzle;

            int delta_x = (int)AccessTools.Field(typeof(Puzzle), "delta_x").GetValue(puzzle);
            int delta_y = (int)AccessTools.Field(typeof(Puzzle), "delta_y").GetValue(puzzle);

            if (delta_x != 0 || delta_y != 0)
            {
                //Set last board state
                short num = 0;
                while (num < puzzle.height)
                {
                    short num2 = 0;
                    while (num2 < puzzle.width)
                    {
                        WindfallHelper.app.model.puzzleModel.lastBoardState[num2, num] = puzzle.blocks[num2, num].GetComponent<Block>().block_type;
                        num2 += 1;
                    }
                    num += 1;
                }

                //Get selected block position and dimensions
                Block selectedBlock = puzzle.selected_block?.GetComponent<Block>();
                if (selectedBlock == null) return;

                Position selectedBlockPosition = selectedBlock.position;

                BlockGroup selectedBlockGroup = BlockGroupModel.FindGroupOfBlock(selectedBlock);
                Vector2Int selectedBlockDimensions = selectedBlockGroup != null ? selectedBlockGroup.GetDimensions() : new Vector2Int(1, 1);

                //Iterator start position and dimensions
                bool horizontal = delta_x != 0;

                int width = horizontal ? puzzle.width : selectedBlockDimensions.x;
                int height = !horizontal ? puzzle.height : selectedBlockDimensions.y;

                int xPosition = horizontal ? 0 : selectedBlockPosition.x;
                int yPosition = !horizontal ? 0 : selectedBlockPosition.y;

                //Determine new positions
                bool boardStateHasChanged = false;

                GameObject[,] newBlocks = new GameObject[width, height];

                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        int x = i + xPosition;
                        int y = j + yPosition;

                        Block block = puzzle.blocks[x, y]?.GetComponent<Block>();
                        if (block == null) continue;

                        Position blockInternalPosition = new Position(block.position.x, block.position.y);
                        Position newBlockIndexPosition = new Position(blockInternalPosition.x, blockInternalPosition.y);

                        if (horizontal) newBlockIndexPosition.y = j;
                        else newBlockIndexPosition.x = i;

                        newBlocks[newBlockIndexPosition.x, newBlockIndexPosition.y] = block.gameObject;

                        if (!boardStateHasChanged && (x != blockInternalPosition.x || y != blockInternalPosition.y))
                        {
                            boardStateHasChanged = true;
                        }
                    }
                }

                //Board state is unchanged, abort
                if (!boardStateHasChanged)
                {
                    puzzle.selected_block = null;
                    WindfallHelper.app.controller.eventsController.SetEvent(new IdleEvent());
                    return;
                }

                //Set positions
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        int x = i + xPosition;
                        int y = j + yPosition;

                        int newBlocksIndexX = x;
                        int newBlocksIndexY = y;
                        if (horizontal) newBlocksIndexY = j;
                        else newBlocksIndexX = i;

                        Block block = newBlocks[newBlocksIndexX, newBlocksIndexY]?.GetComponent<Block>();
                        if (block == null) continue;

                        puzzle.blocks[x, y] = block.gameObject;
                        puzzle.blocks[x, y]?.GetComponent<Block>()?.Goo(false, false);
                    }
                }

                puzzle.processing = true;
                WindfallHelper.app.controller.eventsController.SetEvent(new UpdatePuzzleEvent());
            }

            puzzle.selected_block = null;
        }

        /// <summary>
        /// Moves the given Block to the given position.
        /// </summary>
        /// <param name="block"></param>
        /// <param name="_to_x"></param>
        /// <param name="_to_y"></param>
        /// <param name="_time"></param>
        public static void MoveBlock(GameObject block, int _to_x, int _to_y, float _time)
        {
            //Block
            Block blockComponent = block?.GetComponent<Block>();
            if (blockComponent == null) return;

            Vector3 worldSpaceBlockPosition = WorldSpaceBlockPosition(new Vector2Int(_to_x, _to_y), new Vector2Int(1, 1));

            //Move blockGroup and update movement position
            BlockGroup blockGroup = null;
            if (blockComponent != null) blockGroup = BlockGroupModel.FindGroupOfBlock(blockComponent);

            if (blockGroup != null && BlockGroupModel.IsMainBlock(blockComponent))
            {
                worldSpaceBlockPosition = WorldSpaceBlockPosition(new Vector2Int(_to_x, _to_y), blockGroup.GetDimensions());
                //blockGroup.SetPosition(new Vector2Int(_to_x, _to_y)); TODO: Fix blockGroup move positions!
            }

            //Move block
            ShortcutExtensions.DOLocalMove(block.transform, worldSpaceBlockPosition, _time, false).SetEase(Ease.InOutQuad).SetId("block_move");
            blockComponent.position = new Position(_to_x, _to_y);
        }

        /// <summary>
        /// Returns whether the given position is within grid bounds.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static bool IsWithinGridBounds(Position position)
        {
            Puzzle puzzle = WindfallHelper.app.view.puzzle;
            return (position.x >= 0 && position.x < puzzle.width) && (position.y >= 0 && position.y < puzzle.height);
        }

        /// <summary>
        /// Returns the distance between two Positions on the Puzzle Board.
        /// </summary>
        /// <param name="startPosition">The start Position.</param>
        /// <param name="endPosition">The end Position.</param>
        /// <param name="wraparound">Whether to allow wrapping around the edge of the puzzle board. In this case, the shortest distance is always chosen.</param>
        /// <returns>The distance between the two Positions.</returns>
        public static Vector2Int Distance(Position startPosition, Position endPosition, bool wraparound)
        {
            Puzzle puzzle = WindfallHelper.app.view.puzzle;
            Vector2Int distance = new Vector2Int(endPosition.x - startPosition.x, endPosition.y - startPosition.y);

            if (wraparound)
            {
                //Horizontal
                int horizontalDistance = Math.Abs(distance.x);
                int wraparoundhorizontalDistance = Math.Abs(horizontalDistance - puzzle.width);

                //If the wraparound distance is shorter than the initial distance, use the wraparound distance in the opposite direction of the inital distance
                if (wraparoundhorizontalDistance < horizontalDistance)
                {
                    distance.x = wraparoundhorizontalDistance * -Math.Sign(distance.x);
                }

                //Vertical
                int verticalDistance = Math.Abs(distance.y);
                int wraparoundVerticalDistance = Math.Abs(verticalDistance - puzzle.height);

                //If the wraparound distance is shorter than the initial distance, use the wraparound distance in the opposite direction of the inital distance
                if (wraparoundVerticalDistance < verticalDistance)
                {
                    distance.y = wraparoundVerticalDistance * -Math.Sign(distance.y);
                }
            }

            return distance;
        }

        /// <summary>
        /// Returns a new position moved within the grid bounds.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="wraparound"></param>
        /// <returns></returns>
        public static Position MoveWithinGridBounds(Position position, bool wraparound)
        {
            Puzzle puzzle = WindfallHelper.app.view.puzzle;

            Position newPosition = new Position(position.x, position.y);

            if (IsWithinGridBounds(newPosition)) return newPosition;

            if (wraparound)
            {
                newPosition.x %= puzzle.width;
                if (newPosition.x < 0) newPosition.x += puzzle.width;

                newPosition.y %= puzzle.height;
                if (newPosition.y < 0) newPosition.y += puzzle.height;

                return newPosition;
            }

            if (newPosition.x < 0) newPosition.x = 0;
            else if (newPosition.x >= puzzle.width) newPosition.x = puzzle.width - 1;

            if (newPosition.y < 0) newPosition.y = 0;
            else if (newPosition.y >= puzzle.height) newPosition.y = puzzle.height - 1;

            return newPosition;
        }

        /// <summary>
        /// Refills the puzzle board with tiles. Intended to replace <see cref="Puzzle.fillPuzzle(List{int})"/> only in cases when the puzzle board is refilling.
        /// </summary>
        public static void ReplaceFillPuzzle(List<int> _empty_spaces)
        {
            Puzzle puzzle = WindfallHelper.app.view.puzzle;

            //Manually count empty spaces instead of using PuzzleModel.emptySpaces
            _empty_spaces = FindEmptySpaces();

            int totalEmptySpacesToGenerate = 0;

            for (int columnIterator = 0; columnIterator < _empty_spaces.Count; columnIterator++)
            {
                int emptySpacesInColumn = _empty_spaces[columnIterator];
                if (puzzle.nextBlockViews[columnIterator].HasBlockType() && emptySpacesInColumn > 0)
                {
                    //Random tiles do not need to be generated for 'next' blocks
                    emptySpacesInColumn--;
                }
                totalEmptySpacesToGenerate += emptySpacesInColumn;
            }

            //Refill puzzle board
            List<Block.BlockType> blocksToPlace = new List<Block.BlockType>();
            int[] soulTilesToPlace = new int[_empty_spaces.Count];
            if (WindfallHelper.app.model.soulTiles > 0)
            {
                int totalEmptySpaceCount = 0;
                for (int emptySpacesIterator = 0; emptySpacesIterator < _empty_spaces.Count; emptySpacesIterator++)
                {
                    totalEmptySpaceCount += _empty_spaces[emptySpacesIterator];
                }

                //Add soul tiles
                int numberOfSoulTilesToPlace = Mathf.Min(WindfallHelper.app.model.soulTiles, totalEmptySpaceCount);
                WindfallHelper.app.model.soulTiles -= numberOfSoulTilesToPlace;

                //Randomly find an index where there is an empty space available to put the soul tile
                List<int> emptySpaceIndices = new List<int>(_empty_spaces.Count);
                for (int emptySpacesIndex = 0; emptySpacesIndex < _empty_spaces.Count; emptySpacesIndex++)
                {
                    emptySpaceIndices.Add(emptySpacesIndex);
                }
                while (numberOfSoulTilesToPlace > 0)
                {
                    int index = UnityEngine.Random.Range(0, emptySpaceIndices.Count);
                    int soulTilesIndex = emptySpaceIndices[index];
                    if (_empty_spaces[soulTilesIndex] == 0 || soulTilesToPlace[soulTilesIndex] >= _empty_spaces[soulTilesIndex])
                    {
                        emptySpaceIndices.RemoveAt(index);
                    }
                    else
                    {
                        soulTilesToPlace[soulTilesIndex]++;
                        numberOfSoulTilesToPlace--;
                    }
                }

                totalEmptySpacesToGenerate = 0;
                for (int emptySpacesIterator = 0; emptySpacesIterator < _empty_spaces.Count; emptySpacesIterator++)
                {
                    int numberOfTilesToRandomlyFillInColumn = _empty_spaces[emptySpacesIterator] - soulTilesToPlace[emptySpacesIterator];
                    if (puzzle.nextBlockViews[emptySpacesIterator].HasBlockType() && numberOfTilesToRandomlyFillInColumn > 0)
                    {
                        numberOfTilesToRandomlyFillInColumn--;
                    }
                    totalEmptySpacesToGenerate += numberOfTilesToRandomlyFillInColumn;
                }
            }

            //Fill with wild tiles
            short wildTileCounter = 0;
            while (wildTileCounter < WindfallHelper.app.model.wildTiles)
            {
                if (wildTileCounter < totalEmptySpacesToGenerate)
                {
                    blocksToPlace.Add(Block.BlockType.Wild);
                }
                wildTileCounter += 1;
            }
            WindfallHelper.app.model.wildTiles -= blocksToPlace.Count;

            //Fill with random blocks
            short nextBlocksCounter = 50;
            while (nextBlocksCounter > 0 && blocksToPlace.Count < totalEmptySpacesToGenerate)
            {
                Block.BlockType nextBlock = puzzle.nextBlock();
                blocksToPlace.Add(nextBlock);
                nextBlocksCounter -= 1;
            }

            //Refill puzzle board
            short widthIterator = 0;
            while (widthIterator < puzzle.width)
            {
                bool placeNextBlock = true;
                short emptySpaceIterator = 0;

                //Fill all empty spaces in each column
                while (emptySpaceIterator < _empty_spaces[widthIterator])
                {
                    Block.BlockType blockTypeToPlace = Block.BlockType.Bone;
                    bool placeBlock = true;

                    if (soulTilesToPlace[widthIterator] > 0)
                    {
                        blockTypeToPlace = Block.BlockType.Soul;
                        soulTilesToPlace[widthIterator]--;
                    }
                    else if (placeNextBlock && puzzle.nextBlockViews[widthIterator].HasBlockType())
                    {
                        blockTypeToPlace = puzzle.nextBlockViews[widthIterator].GetBlockType();
                        placeNextBlock = false;
                    }
                    else if (blocksToPlace.Count > 0)
                    {
                        int index2 = UnityEngine.Random.Range(0, blocksToPlace.Count);
                        blockTypeToPlace = blocksToPlace[index2];
                        blocksToPlace.RemoveAt(index2);
                    }
                    else
                    {
                        placeBlock = false;
                    }

                    if (placeBlock)
                    {
                        //Place block
                        short height = (short)HeightOfLowestEmptySpaceInColumn(widthIterator);
                        if (height >= 0) PlaceBlock(new Position(widthIterator, height), blockTypeToPlace, true, false);
                    }

                    emptySpaceIterator += 1;
                }
                widthIterator += 1;
            }

            AccessTools.Field(typeof(Puzzle), "totally_fill").SetValue(puzzle, false);
        }

        /// <summary>
        /// Returns the height of the lowest empty space in the given column of the puzzle board. The purpose of this method is to account for cases where an empty space is below a BlockGroup. 
        /// </summary>
        /// <param name="column">The column of the puzzle board.</param>
        /// <returns>The height of the lowest empty space in the given column of the puzzle board, or -1 if no empty space is found.</returns>
        private static int HeightOfLowestEmptySpaceInColumn(int column)
        {
            Puzzle puzzle = WindfallHelper.app.view.puzzle;

            //Iterate upwards from the bottom of the puzzle board
            for (int heightIterator = 0; heightIterator < puzzle.height; heightIterator++)
            {
                //Return first empty space
                if (puzzle.blocks[column, heightIterator]?.GetComponent<Block>() == null) return heightIterator;
            }

            return -1;
        }

        /// <summary>
        /// Manually finds all empty spaces in the puzzle board. Alleviates the need to manually input empty spaces to <see cref="PuzzleModel.emptySpaces"/> when removing tiles in spell logic or other situations.
        /// </summary>
        /// <returns>A list containing the number of empty spaces in each column of the puzzle board.</returns>
        public static List<int> FindEmptySpaces()
        {
            Puzzle puzzle = WindfallHelper.app.view.puzzle;

            List<int> emptySpaces = new List<int>();
            emptySpaces.AddRange(new int[puzzle.width]);

            for (int widthIterator = 0; widthIterator < puzzle.width; widthIterator++)
            {
                //Each space is empty by default
                emptySpaces[widthIterator] = puzzle.height;

                for (int heightIterator = 0; heightIterator < puzzle.height; heightIterator++)
                {
                    Block block = puzzle.blocks[widthIterator, heightIterator]?.GetComponent<Block>();
                    if (block == null) continue;

                    //If there is a block, mark the space as not empty
                    emptySpaces[widthIterator]--;
                }
            }

            return emptySpaces;
        }

        public static BlockType BlockTypeOfShape(List<GameObject> shape)
        {
            foreach (GameObject blockObject in shape)
            {
                Block block = blockObject.GetComponent<Block>();
                if (block == null) continue;

                if (block.block_type != BlockType.Wild) return block.block_type;
            }

            return BlockType.Wild;
        }
    }
}