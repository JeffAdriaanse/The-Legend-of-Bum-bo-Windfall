using DG.Tweening;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.StyleSheets;
using static UnityEngine.ParticleSystem;

namespace The_Legend_of_Bum_bo_Windfall
{
    public static class PuzzleHelper
    {
        /// <summary>
        /// Randomly places Blocks of the given BlockType. Avoids overriding tiles at the given avoid positions or of the given avoid types. Intended to be triggered in spell effect logic.
        /// </summary>
        /// <param name="blockType">The BlockType of the placed Blocks.</param>
        /// <param name="blockCount">The number of Blocks to place.</param>
        /// <param name="avoidPositions">The positions to avoid overriding.</param>
        /// <param name="avoidTypes">The BlockTypes to avoid overriding.</param>
        /// <param name="avoidGroups">Whether to avoid overriding sub-Blocks in BlockGoups.</param>
        public static void RandomlyPlaceBlocks(Block.BlockType blockType, int blockCount, List<Vector2Int> avoidPositions, List<Block.BlockType> avoidTypes, bool avoidGroups = true)
        {
            List<Block> validBlocks = new List<Block>();
            Puzzle puzzle = WindfallHelper.app.view.puzzle;

            //Find all valid placements
            for (int i = 0; i < puzzle.width; i++)
            {
                for (int j = 0; j < puzzle.height; j++)
                {
                    //Get block
                    Block block = puzzle.blocks[i, j].GetComponent<Block>();
                    if (block == null) continue;

                    bool addBlock = true;

                    //Avoid given BlockTypes
                    if (avoidTypes != null)
                    {
                        foreach (Block.BlockType avoidType in avoidTypes)
                        {
                            if (block.block_type == avoidType)
                            {
                                addBlock = false;
                                break;
                            }
                        }
                    }

                    if (!addBlock) continue;

                    //Avoid given positions
                    if (avoidPositions != null)
                    {
                        foreach (Vector2Int position in avoidPositions)
                        {
                            if (position.x == i && position.y == j)
                            {
                                addBlock = false;
                                break;
                            }
                        }
                    }

                    if (!addBlock) continue;

                    //Avoid BlockGroup sub-Blocks
                    if (avoidGroups)
                    {
                        BlockGroup blockGroup = BlockGroupModel.FindGroupOfBlock(block);
                        if (blockGroup != null && !BlockGroupModel.IsMainBlock(block))
                        {
                            //Block is in a BlockGroup but is not the main block
                            addBlock = false;
                        }
                    }

                    if (!addBlock) continue;

                    //Add the block
                    validBlocks.Add(block);
                }
            }

            //Randomly place blocks
            while (blockCount > 0 && validBlocks.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, validBlocks.Count);

                //Place the block
                Block block = validBlocks[index];
                PlaceBlock(block.position, blockType);

                validBlocks.RemoveAt(index);
                blockCount--;
            }
        }

        /// <summary>
        /// Places a Block of the given BlockType at the given position. Intended to be triggered in BlockGroup logic or by spell effects.
        /// </summary>
        /// <param name="position">The position of the placed Block.</param>
        /// <param name="blockType">The BlockType of the placed Block.</param>
        public static void PlaceBlock(Position position, Block.BlockType blockType)
        {
            Puzzle puzzle = WindfallHelper.app.view.puzzle;

            //Abort if outside puzzle board
            if (position.x >= puzzle.width || position.y >= puzzle.height) return;

            //Remove existing block
            Block block = puzzle.blocks[position.x, position.y]?.GetComponent<Block>();
            if (block != null) block.Despawn(false);

            //Place new block
            puzzle.setBlock(blockType, (short)position.x, (short)position.y, false, true);
        }

        /// <summary>
        /// Removes the given Block. Intended to be triggered in spell effect logic.
        /// </summary>
        /// <param name="block">The Block to remove.</param>
        /// <param name="puzzleShape">The puzzle shape for marking the removal of the block.</param>
        /// <param name="emptySpaces">The emptySpaces for marking the removal of the block.</param>
        /// <param name="playSound">Whether to play the removal sound.</param>
        /// <param name="manaGain">The amount of mana to gain from the Block.</param>
        /// <param name="showManaGain">Whether to show a mana gain notification.</param>
        /// <param name="fromBlockGroup">Whether this Block is being removed by the main Block of its BlockGroup.</param>
        public static void RemoveBlock(Block block, PuzzleShape puzzleShape, int[] emptySpaces, bool playSound, short manaGain, bool showManaGain = true, bool fromBlockGroup = false)
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
                            RemoveBlock(subBlock.GetComponent<Block>(), puzzleShape, emptySpaces, false, manaGain, false, true);
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
            //Empty spaces
            int xPosition = block.position.x;
            if (emptySpaces != null && xPosition < emptySpaces.Length) emptySpaces[xPosition]++;

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
        /// Initializes empty spaces to be marked on the puzzle board.
        /// </summary>
        public static void InitializeEmptySpaces()
        {
            ref List<int> emptySpaces = ref WindfallHelper.app.model.puzzleModel.emptySpaces;

            if (emptySpaces == null)
            {
                emptySpaces = new List<int>();
            }
            else
            {
                emptySpaces.Clear();
            }

            for (int i = 0; i < WindfallHelper.app.view.puzzle.width; i++)
            {
                emptySpaces.Add(0);
            }
        }

        /// <summary>
        /// Marks empty spaces on the puzzle board in the given column.
        /// </summary>
        /// <param name="column">The column to mark empty spaces in.</param>
        /// <param name="numberOfSpaces">The number of spaces to mark.</param>
        public static void MarkEmptySpaces(int column, int numberOfSpaces)
        {
            ref List<int> emptySpaces = ref WindfallHelper.app.model.puzzleModel.emptySpaces;
            if (emptySpaces == null) return;

            if (column < emptySpaces.Count)
            {
                emptySpaces[column] += numberOfSpaces;
            }
        }

        /// <summary>
        /// Marks empty spaces on the puzzle board in the given columns.
        /// </summary>
        /// <param name="markEmptySpaces">An array containing the number of empty spaces to add to each column, indexed according to column position.</param>
        public static void MarkEmptySpaces(int[] markEmptySpaces)
        {
            if (markEmptySpaces == null) return;

            for (int i = 0; i < markEmptySpaces.Length; i++)
            {
                MarkEmptySpaces(i, markEmptySpaces[i]);
            }
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
            Vector3 scale = new Vector3(InterfaceFixes.BLOCK_SIZE, InterfaceFixes.BLOCK_SIZE, InterfaceFixes.BLOCK_SIZE);

            if (blockGroup != null)
            {
                //Get Dimensions
                Vector2Int dimensions = blockGroup.GetDimensions();

                //Is main block
                bool mainBlock = BlockGroupModel.IsMainBlock(block);

                //Set scale
                scale = mainBlock ? new Vector3(InterfaceFixes.BLOCK_SIZE * dimensions.x, InterfaceFixes.BLOCK_SIZE * dimensions.y, InterfaceFixes.BLOCK_SIZE) : Vector3.zero;

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
            Position original_block_position = (Position)AccessTools.Field(typeof(Puzzle), "original_block_position").GetValue(puzzle);

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
                    positiveDirection = _to_x - original_block_position.x > 0;
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
                    positiveDirection = _to_y - original_block_position.y > 0;
                }
            }

            //Determine move distance
            int moveDistance;
            if (horizontal)
            {
                moveDistance = _to_x - original_block_position.x;

                if (positiveDirection && moveDistance < 0) moveDistance = (_to_x + puzzle.width) - original_block_position.x;
                if (!positiveDirection && moveDistance > 0) moveDistance = (_to_x - puzzle.width) - original_block_position.x;
            }
            else
            {
                moveDistance = _to_y - original_block_position.y;

                if (positiveDirection && moveDistance < 0) moveDistance = (_to_y + puzzle.height) - original_block_position.y;
                if (!positiveDirection && moveDistance > 0) moveDistance = (_to_y - puzzle.height) - original_block_position.y;
            }

            moveDistance = Math.Abs(moveDistance);
            moveDistance %= 9;

            //Find all BlockGroups blocking the way
            List<BlockGroup> blockingGroups;
            List<BlockGroup> alignedGroups = new List<BlockGroup>();

            if (selectedBlockGroup != null)
            {
                blockingGroups = BlockGroupModel.BlockingGroupsAlongAxis(selectedBlockGroup.GetPosition(), selectedBlockGroup.GetDimensions(), horizontal);

                //Aligned groups
                alignedGroups = BlockGroupModel.RemoveAlignedGroups(selectedBlockGroup, blockingGroups, horizontal);

                //If there is an aligned BlockGroup being moved at the edge of the puzzle board, the tiles must be moved further according to the group dimension (block wraparound)
                //TODO: Vertical
                if (horizontal)
                {
                    int movementOffset = 0;
                    for (int i = 0; i < puzzle.width; i++)
                    {
                        Block targetBlock = puzzle.blocks[i, original_block_position.y]?.GetComponent<Block>();
                        if (targetBlock != null)
                        {
                            BlockGroup targetBlockGroup = BlockGroupModel.FindGroupOfBlock(targetBlock);
                            if (targetBlockGroup != null && BlockGroupModel.IsMainBlock(targetBlock) && alignedGroups.Contains(targetBlockGroup))
                            {
                                int newPositionLeft = i + (positiveDirection ? moveDistance : -moveDistance);
                                int newPositionRight = newPositionLeft + targetBlockGroup.GetDimensions().x - 1;

                                if (newPositionLeft < 0 && newPositionRight >= 0)
                                {
                                    movementOffset = Math.Abs(newPositionLeft);
                                }
                                else if (newPositionRight >= puzzle.width && newPositionLeft < puzzle.width)
                                {
                                    movementOffset = puzzle.width - newPositionRight - 1;
                                }
                            }
                        }
                    }
                    moveDistance -= Math.Abs(movementOffset);
                }

                //If an aligned group will collide with a blocking group, abort movement
                if (horizontal)
                {
                    for (int i = 0; i < puzzle.width; i++)
                    {
                        Block targetBlock = puzzle.blocks[i, original_block_position.y]?.GetComponent<Block>();
                        if (targetBlock != null)
                        {
                            BlockGroup targetBlockGroup = BlockGroupModel.FindGroupOfBlock(targetBlock);
                            if (targetBlockGroup != null && BlockGroupModel.IsMainBlock(targetBlock) && alignedGroups.Contains(targetBlockGroup))
                            {
                                for (int offset = 0; offset <= moveDistance; offset++)
                                {
                                    int newPositionX = i + (positiveDirection ? offset : -offset);

                                    Position alignedGroupPosition = new Position(newPositionX, targetBlockGroup.GetPosition().y);
                                    Vector2Int alignedGroupDimensions = targetBlockGroup.GetDimensions();

                                    //Test
                                    Console.WriteLine(alignedGroupPosition.x.ToString() + ", " + alignedGroupPosition.y.ToString());

                                    for (int j = alignedGroupPosition.x; j < alignedGroupPosition.x + alignedGroupDimensions.x; j++)
                                    {
                                        for (int k = alignedGroupPosition.y; k < alignedGroupPosition.y + alignedGroupDimensions.y; k++)
                                        {
                                            Position nextBlockPosition = new Position(j, k);
                                            nextBlockPosition = MoveWithinGridBounds(nextBlockPosition, true);

                                            //Test
                                            Console.WriteLine("Block: " + nextBlockPosition.x.ToString() + ", " + nextBlockPosition.y.ToString());

                                            Block nextBlock = puzzle.blocks[nextBlockPosition.x, nextBlockPosition.y]?.GetComponent<Block>();

                                            BlockGroup nextBlockGroup = null;
                                            if (nextBlock != null) nextBlockGroup = BlockGroupModel.FindGroupOfBlock(nextBlock);

                                            if (nextBlockGroup != null && blockingGroups.Contains(nextBlockGroup))
                                            {
                                                //Abort
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                blockingGroups = BlockGroupModel.BlockingGroupsAlongAxis(original_block_position, new Vector2Int(1, 1), horizontal);
            }

            int startPosX;
            int startPosY;

            int width;
            int height;

            if (horizontal)
            {
                startPosX = 0;
                startPosY = original_block_position.y;

                width = puzzle.width;
                height = selectedBlockGroup != null ? selectedBlockGroup.GetDimensions().y : 1;
            }
            else
            {
                startPosX = original_block_position.x;
                startPosY = 0;

                height = puzzle.height;
                width = selectedBlockGroup != null ? selectedBlockGroup.GetDimensions().x : 1;
            }

            for (int i = startPosX; i < startPosX + width; i++)
            {
                for (int j = startPosY; j < startPosY + height; j++)
                {
                    int newX = i;
                    int newY = j;

                    int blockMoveDistance = moveDistance;

                    //Find destination distance
                    while (blockMoveDistance > 0)
                    {
                        //Move 
                        if (horizontal)
                        {
                            newX += positiveDirection ? 1 : -1;

                            if (newX >= puzzle.width) newX -= puzzle.width;
                            if (newX < 0) newX += puzzle.width;
                        }
                        else
                        {
                            newY += positiveDirection ? 1 : -1;

                            if (newY >= puzzle.height) newY -= puzzle.height;
                            if (newY < 0) newY += puzzle.height;
                        }

                        //Consider blocking groups
                        while (true)
                        {
                            Block targetBlock = puzzle.blocks[newX, newY]?.GetComponent<Block>();
                            if (targetBlock != null)
                            {
                                BlockGroup targetBlockGroup = BlockGroupModel.FindGroupOfBlock(targetBlock);
                                if (targetBlockGroup != null && blockingGroups.Contains(targetBlockGroup))
                                {
                                    //Skip blocking groups
                                    if (horizontal)
                                    {
                                        int offset = targetBlockGroup.GetDimensions().x;
                                        newX += positiveDirection ? offset : -offset;

                                        if (newX >= puzzle.width) newX -= puzzle.width;
                                        if (newX < 0) newX += puzzle.width;

                                        //Repeat when a new position is reached
                                        continue;
                                    }
                                    else
                                    {
                                        int offset = targetBlockGroup.GetDimensions().y;
                                        newY += positiveDirection ? offset : -offset;

                                        if (newY >= puzzle.height) newY -= puzzle.height;
                                        if (newY < 0) newY += puzzle.height;

                                        //Repeat when a new position is reached
                                        continue;
                                    }
                                }
                            }

                            break;
                        }

                        blockMoveDistance--;
                    }

                    Block moveBlock = puzzle.blocks[i, j]?.GetComponent<Block>();

                    if (moveBlock != null)
                    {
                        BlockGroup moveBlockGroup = BlockGroupModel.FindGroupOfBlock(moveBlock);
                        if (moveBlockGroup != null)
                        {
                            //Do not move blocks in blocking groups
                            if (blockingGroups.Contains(moveBlockGroup)) continue;
                        }

                        //Move blocks
                        MoveBlock(moveBlock.gameObject, newX, newY, _time);
                    }
                }
            }
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
                                while(true)
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
                while ((int)num < puzzle.height)
                {
                    short num2 = 0;
                    while ((int)num2 < puzzle.width)
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

                //Board state is unchanged, abort (broken!)
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
    }
}
