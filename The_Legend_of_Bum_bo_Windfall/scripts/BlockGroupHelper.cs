using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    public static class BlockGroupHelper
    {
        /// <summary>
        /// Generates a list of offsets to attempt to place a BlockGroup in, relative to the grid placement location of the BlockGroup and given its dimensions.
        /// </summary>
        /// <param name="dimensions">The dimensions of the BlockGroup.</param>
        /// <returns>A list of offsets to attempt to place the BlockGroup in.</returns>
        private static List<Vector2Int> AttemptOffsets(Vector2Int dimensions)
        {
            List<Vector2Int> offsets = new List<Vector2Int>();

            for (int i = -dimensions.x + 1; i <= 0; i++)
            {
                for (int j = -dimensions.y + 1; j <= 0; j++)
                {
                    offsets.Add(new Vector2Int(i, j));
                }
            }
            offsets.Reverse();
            return offsets;
        }

        /// <summary>
        /// Finds a valid offset for the BlockGroup with the given position and dimensions to fit on the puzzle board.
        /// </summary>
        /// <param name="position">The position that the BlockGroup must occupy.</param>
        /// <param name="dimensions">The dimensions of the BlockGroup.</param>
        /// <param name="allowOverridingBlockGroups">Whether to allow offsets that will result in another BlockGroup being fully engulfed by the BlockGroup.</param>
        /// <param name="random">Whether to prioritize offsets randomly as opposed to prioritizing deterministically with hard coded offsets.</param>
        /// <returns>The offset Position where the BlockGroup will be able to fit on the puzzle board, or null if no valid offset is found.</returns>
        public static Position FindValidGroupOffset(Position position, Vector2Int dimensions, bool allowOverridingBlockGroups, bool random)
        {
            List<Vector2Int> groupOffsets = new List<Vector2Int>();

            //Offsets to attempt
            if (dimensions.x == 2 && dimensions.y == 2) groupOffsets.AddRange(GroupOffsetsTwoByTwo);
            else if (dimensions.x == 3 && dimensions.y == 3) groupOffsets.AddRange(GroupOffsetsThreeByThree);
            else groupOffsets.AddRange(AttemptOffsets(dimensions));

            while (groupOffsets.Count > 0)
            {
                //Choose a random position or the first position
                int index = random ? UnityEngine.Random.Range(0, groupOffsets.Count) : 0;
                Position newPosition = new Position(position.x + groupOffsets[index].x, position.y + groupOffsets[index].y);
                groupOffsets.RemoveAt(index);

                //Validate Position
                if (ValidGroupPosition(newPosition, dimensions, allowOverridingBlockGroups)) return newPosition;
            }

            return null;
        }

        /// <summary>
        /// Determines whether a BlockGroup of the given dimensions will fit at the given Position.
        /// </summary>
        /// <param name="position">The Position of the BlockGroup.</param>
        /// <param name="dimensions">The dimensions of the BlockGroup.</param>
        /// <param name="allowOverridingBlockGroups">Whether to consider positions valid if they would result in BlockGroups being entirely replaced by the new BlockGroup.</param>
        /// <returns>Whether a BlockGroup of the given dimensions will fit at the given Position.</returns>
        public static bool ValidGroupPosition(Position position, Vector2Int dimensions, bool allowOverridingBlockGroups)
        {
            Puzzle puzzle = WindfallHelper.app.view.puzzle;

            if (position == null || dimensions == null) return false;

            //Validate Position
            for (int i = position.x; i < position.x + dimensions.x; i++)
            {
                for (int j = position.y; j < position.y + dimensions.y; j++)
                {
                    Position blockPosition = new Position(i, j);

                    //Invalid: Out of bounds
                    if (!PuzzleHelper.IsWithinPuzzleBounds(blockPosition)) return false;

                    //Check for blockGroups
                    Block block = puzzle.blocks[i, j]?.GetComponent<Block>();
                    if (block != null)
                    {
                        BlockGroup blockGroup = WindfallHelper.BlockGroupController.FindGroupOfBlock(block);
                        if (blockGroup != null && (!allowOverridingBlockGroups || !PuzzleHelper.ContainsArea(position, dimensions, blockGroup.GetPosition(), blockGroup.GetDimensions())))
                        {
                            //Invalid: A BlockGroup is in the way
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Modifies shapes that have been created in <see cref="ClearPuzzleEvent"/> for compatibility with BlockGroups such that they follow BlockGroup matching logic.
        /// Intended to be triggered in <see cref="Puzzle.ClearMatches"/> just after the shapes have been created.
        /// </summary>
        public static void ModifyPuzzleShapes(List<List<GameObject>> shapes)
        {
            List<List<GameObject>> modifiedShapes = new List<List<GameObject>>();

            foreach (List<GameObject> shape in shapes)
            {
                modifiedShapes.AddRange(DisqualifyLines(shape, 4));
            }

            CombineShapesThatShareBlockGroups(modifiedShapes);

            foreach (List<GameObject> shape in modifiedShapes)
            {
                ResizeByComboContribution(shape);
            }

            //Replace vanilla shapes
            shapes.Clear();
            shapes.AddRange(modifiedShapes);
        }

        private static List<List<GameObject>> DisqualifyLines(List<GameObject> shape, int threshold)
        {
            List<List<GameObject>> lines = SeparateLines(shape);

            List<List<GameObject>> validLines = new List<List<GameObject>>();
            //Disqualify lines that are too small
            foreach (List<GameObject> line in lines)
            {
                if (ShapeValue(line) >= threshold) validLines.Add(line);
            }

            List<List<GameObject>> shapes = new List<List<GameObject>>();

            //Merge lines back into shapes
            foreach (List<GameObject> line in validLines)
            {
                List<List<GameObject>> connectedShapes = new List<List<GameObject>>();

                bool lineIsConnectedToExistingShapes = false;

                //Search previously encountered shapes to determine if this line overlaps existing shapes
                foreach (List<GameObject> returnedShape in shapes)
                {
                    foreach (GameObject block in line)
                    {
                        if (returnedShape.Contains(block))
                        {
                            lineIsConnectedToExistingShapes = true;

                            //Track which shapes the line is connected to
                            connectedShapes.Add(returnedShape);

                            //Prevent duplicate Blocks
                            line.Remove(block);

                            break;
                        }
                    }
                }

                //Merge connected shapes with the line
                if (lineIsConnectedToExistingShapes)
                {
                    //Remove connected shapes
                    shapes.RemoveAll((List<GameObject> x) => connectedShapes.Contains(x));

                    //Add new merged shape
                    List<GameObject> mergedShape = new List<GameObject>();

                    foreach (List<GameObject> connectedShape in connectedShapes) mergedShape.AddRange(connectedShape);
                    mergedShape.AddRange(line);

                    shapes.Add(mergedShape);
                }
                else
                {
                    //Line shape has not been added yet
                    shapes.Add(line);
                }
            }

            return shapes;
        }

        /// <summary>
        /// Breaks up the given shape into individual lines of Blocks. Indended for breaking up shapes that have overlapping horizontal and vertical lines of Blocks.
        /// </summary>
        /// <param name="shape">The shape to disassemble.</param>
        /// <returns>The newly formed shapes.</returns>
        private static List<List<GameObject>> SeparateLines(List<GameObject> shape)
        {
            List<List<GameObject>> lines = new List<List<GameObject>>();

            List<GameObject> encounteredJunctions = new List<GameObject>();

            foreach (GameObject block in shape)
            {
                Block blockComponent = block?.GetComponent<Block>();
                if (blockComponent == null) continue;

                List<List<GameObject>> linesAtJunction = LinesAtJunction(block, shape);

                //A junction must have lines in each direction
                if (linesAtJunction[0].Count > 1 && linesAtJunction[1].Count > 1)
                {
                    foreach (List<GameObject> lineAtJunction in linesAtJunction)
                    {
                        //Ensure that newly added lines contain no previously encountered junctions
                        bool newLine = true;
                        foreach (GameObject lineBlock in lineAtJunction)
                        {
                            if (encounteredJunctions.Contains(lineBlock))
                            {
                                newLine = false;
                                break;
                            }
                        }

                        if (newLine)
                        {
                            lines.Add(lineAtJunction);
                        }
                    }

                    encounteredJunctions.Add(block);
                }
            }

            if (lines.Count > 0) return lines;
            return new List<List<GameObject>>() { shape };
        }

        /// <summary>
        /// Returns consecutive lines of Blocks at the given junction within the given shape on the X and Y axes (slow).
        /// </summary>
        /// <param name="junction">The junction to find lines for.</param>
        /// <param name="shape">The shape of Blocks that the lines are contained within.</param>
        /// <returns>Consecutive lines of Blocks at the given junction.<returns>
        private static List<List<GameObject>> LinesAtJunction(GameObject junction, List<GameObject> shape)
        {
            Block junctionComponent = junction?.GetComponent<Block>();
            if (junctionComponent == null) return null;

            List<GameObject> lineX = new List<GameObject>() { junction };
            int offsetX;
            //Find all consecutive Blocks in a line beside the junction on the X axis
            for (int directionIterator = 0; directionIterator < 2; directionIterator++)
            {
                //Search in both directions
                offsetX = (directionIterator == 0) ? 1 : -1;

                while (true)
                {
                    bool continuedLine = false;

                    foreach (GameObject block in shape)
                    {
                        Block blockComponent = block?.GetComponent<Block>();
                        if (blockComponent == null) continue;

                        if (blockComponent.position.y == junctionComponent.position.y && blockComponent.position.x == junctionComponent.position.x + offsetX)
                        {
                            lineX.Add(block);
                            continuedLine = true;
                            break;
                        }
                    }

                    //Proceed down the line
                    offsetX += (directionIterator == 0) ? 1 : -1;

                    //Stop when the line goes no further
                    if (!continuedLine) break;
                }
            }

            List<GameObject> lineY = new List<GameObject>() { junction };
            int offsetY;
            //Find all consecutive Blocks in a line beside the junction on the Y axis
            for (int directionIterator = 0; directionIterator < 2; directionIterator++)
            {
                //Search in both directions
                offsetY = (directionIterator == 0) ? 1 : -1;

                while (true)
                {
                    bool continuedLine = false;

                    foreach (GameObject block in shape)
                    {
                        Block blockComponent = block?.GetComponent<Block>();
                        if (blockComponent == null) continue;

                        if (blockComponent.position.y == junctionComponent.position.y + offsetY && blockComponent.position.x == junctionComponent.position.x)
                        {
                            lineY.Add(block);
                            continuedLine = true;
                            break;
                        }
                    }

                    //Proceed down the line
                    offsetY += (directionIterator == 0) ? 1 : -1;

                    //Stop when the line goes no further
                    if (!continuedLine) break;
                }
            }

            return new List<List<GameObject>>()
            {
                lineX,
                lineY,
            };
        }

        /// <summary>
        /// Determines the shape value contribution of Blocks and BlockGroups in the shape, then returns the overall shape value.
        /// </summary>
        /// <param name="shape">The shape to determine the shape value contribution of.</param>
        /// <returns>The shape value of the given shape.</returns>
        private static int ShapeValue(List<GameObject> shape)
        {
            //The number of Blocks contributing to the size of this puzzleShape
            int shapeValue = 0;

            List<BlockGroup> blockGroups = new List<BlockGroup>();

            //Iterate over every Block
            foreach (GameObject block in shape)
            {
                Block blockComponent = block?.GetComponent<Block>();
                if (blockComponent == null) continue;

                BlockGroup blockGroup = WindfallHelper.BlockGroupController.FindGroupOfBlock(blockComponent);

                if (blockGroup != null)
                {
                    //Find all BlockGroups
                    if (!blockGroups.Contains(blockGroup)) blockGroups.Add(blockGroup);

                    //Individual Blocks in BlockGroups do not contribute towards shape value.
                    continue;
                }

                //Add shape value contribution of regular Blocks
                shapeValue++;
            }

            //Add shape value contribution of each BlockGroup
            foreach (BlockGroup blockGroup in blockGroups)
            {
                shapeValue += blockGroup.GetShapeValue();
            }

            return shapeValue;
        }

        /// <summary>
        /// Adds or removes Blocks from the shape such that each BlockGroup involved does not contribute more Blocks than its combo contribution.
        /// </summary>
        /// <param name="shape">The shape to resize.</param>
        private static void ResizeByComboContribution(List<GameObject> shape)
        {
            List<GameObject> allowedBlocks = new List<GameObject>();

            //Track the shape contriution of each BlockGroup
            Dictionary<BlockGroup, int> blockGroupContributions = new Dictionary<BlockGroup, int>();

            //Reduce the contribution of overcontributing BlockGroups
            foreach (GameObject block in shape)
            {
                Block blockComponent = block?.GetComponent<Block>();
                if (blockComponent == null) continue;

                BlockGroup blockGroup = WindfallHelper.BlockGroupController.FindGroupOfBlock(blockComponent);

                if (blockGroup != null)
                {
                    //Encountered a new BlockGroup
                    if (!blockGroupContributions.ContainsKey(blockGroup))
                    {
                        blockGroupContributions.Add(blockGroup, 0);
                    }

                    //Add the BlockGroup Block if the BlockGroup has not reached its combo contribution
                    if (blockGroupContributions[blockGroup] < blockGroup.GetComboContribution())
                    {
                        blockGroupContributions[blockGroup]++;
                        allowedBlocks.Add(block);
                    }
                }
                else
                {
                    allowedBlocks.Add(block);
                }
            }

            //Increase the contribution of undercontributing BlockGroups
            for (int blockGroupCounter = 0; blockGroupCounter < blockGroupContributions.Count; blockGroupCounter++)
            {
                KeyValuePair<BlockGroup, int> blockGroupContribution = blockGroupContributions.ElementAt(blockGroupCounter);

                BlockGroup blockGroup = blockGroupContribution.Key;
                int blockGroupContributionValue = blockGroupContribution.Value;
                int comboContribution = blockGroup.GetComboContribution();

                List<GameObject> blockGroupBlocks = blockGroup.GetBlocks();

                //Add Blocks from the BlockGroup
                foreach (GameObject block in blockGroupBlocks)
                {
                    //Exit once the combo contribution is reached
                    if (blockGroupContributionValue >= comboContribution) break;

                    if (!allowedBlocks.Contains(block))
                    {
                        blockGroupContributionValue++;
                        allowedBlocks.Add(block);
                    }
                }
            }

            shape.Clear();
            shape.AddRange(allowedBlocks);
        }

        /// <summary>
        /// Ensures all shapes in the given list are connected between one another if they share any BlockGroups.
        /// </summary>
        /// <param name="shapes">The shapes to combine.</param>
        /// <param name="allowMixingTileTypes">Whether to combine shapes that have tiles of different types.</param>
        private static void CombineShapesThatShareBlockGroups(List<List<GameObject>> shapes, bool allowMixingTileTypes = false)
        {
            //Loop through all groups
            for (int groupIterator = 0; groupIterator < WindfallHelper.BlockGroupController.BlockGroups.Count; groupIterator++)
            {
                BlockGroup group = WindfallHelper.BlockGroupController.BlockGroups[groupIterator];

                //Find all shapes in the group
                List<List<GameObject>> shapesConnectedToGroup = ShapesConnectedToGroup(group, shapes);

                while (shapesConnectedToGroup.Count > 0)
                {
                    List<List<GameObject>> shapesToCombine = new List<List<GameObject>>();

                    //Store the first shape
                    List<GameObject> firstShape = shapesConnectedToGroup[0];
                    Block.BlockType firstShapeBlockType = PuzzleHelper.BlockTypeOfShape(firstShape);

                    for (int shapeIterator = 1; shapeIterator < shapesConnectedToGroup.Count; shapeIterator++)
                    {
                        List<GameObject> shape = shapesConnectedToGroup[shapeIterator];

                        if (!allowMixingTileTypes && PuzzleHelper.BlockTypeOfShape(shape) != firstShapeBlockType) continue;

                        //Add blocks from all subsequent shapes to the first shape
                        foreach (GameObject block in shape)
                        {
                            if (!firstShape.Contains(block)) firstShape.Add(block);
                        }

                        //Delete subsequent shapes
                        shapes.Remove(shape);
                        shapesToCombine.Add(shape);
                    }

                    shapesConnectedToGroup.RemoveAll(x => shapesToCombine.Contains(x));

                    //Add remaining blocks from the group to the first shape
                    foreach (GameObject block in group.GetBlocks())
                    {
                        if (!firstShape.Contains(block)) firstShape.Add(block);
                    }

                    shapesConnectedToGroup.Remove(firstShape);
                }
            }
        }

        /// <summary>
        /// Given a list of shapes, returns all shapes connected to the given group.
        /// </summary>
        /// <param name="blockGroup">The blockGroup to find shapes for.</param>
        /// <param name="overallShapes">The shapes to search through.</param>
        /// <returns>All shapes connected to the group.</returns>
        private static List<List<GameObject>> ShapesConnectedToGroup(BlockGroup blockGroup, List<List<GameObject>> overallShapes)
        {
            List<List<GameObject>> shapes = new List<List<GameObject>>();

            //Loop through all shapes
            foreach (List<GameObject> shape in overallShapes)
            {
                //Loop through all blocks
                foreach (GameObject blockObject in shape)
                {
                    Block block = blockObject.GetComponent<Block>();
                    if (block == null) continue;

                    //If any block is in the blockGroup, add the shape
                    if (blockGroup.Contains(block))
                    {
                        shapes.Add(shape);
                        break;
                    }
                }
            }

            return shapes;
        }

        /// <summary>
        /// Returns all groups in the given shape. (Unused)
        /// </summary>
        /// <param name="shape">The shape to find groups for.</param>
        /// <returns>All groups in the shape.</returns>
        private static List<BlockGroup> FindBlockGroupsInShape(List<GameObject> shape)
        {
            List<BlockGroup> returnedBlockGroups = new List<BlockGroup>();

            //Find the group of each block
            for (int blockIterator = 0; blockIterator < shape.Count; blockIterator++)
            {
                Block block = shape[blockIterator]?.GetComponent<Block>();
                if (block == null) continue;

                BlockGroup blockGroup = WindfallHelper.BlockGroupController.FindGroupOfBlock(block);
                if (blockGroup != null && !returnedBlockGroups.Contains(blockGroup)/*Ignore duplicates*/)
                {
                    //Add the block group
                    returnedBlockGroups.Add(blockGroup);
                }
            }

            return returnedBlockGroups;
        }

        /// <summary>
        /// Finds all BlockGroups horizontally/vertically blocking the given position and dimensions on the puzzle board.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="dimensions"></param>
        /// <param name="horizontal"></param>
        /// <returns></returns>
        public static List<BlockGroup> BlockingGroupsAlongAxis(Position position, Vector2Int dimensions, bool horizontal)
        {
            List<BlockGroup> blockingGroups = new List<BlockGroup>();

            Puzzle puzzle = WindfallHelper.app.view.puzzle;

            int width = horizontal ? puzzle.width : dimensions.x;
            int height = !horizontal ? puzzle.height : dimensions.y;

            int xPosition = horizontal ? 0 : position.x;
            int yPosition = !horizontal ? 0 : position.y;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    //Get all positions along the given axis within the given dimensions
                    Position blockPosition = new Position(i + xPosition, j + yPosition);

                    if (!PuzzleHelper.IsWithinPuzzleBounds(blockPosition)) continue;

                    GameObject currentBlock = puzzle.blocks[blockPosition.x, blockPosition.y];
                    Block currentBlockComponent = currentBlock?.GetComponent<Block>();

                    BlockGroup currentBlockGroup = null;
                    if (currentBlockComponent != null) currentBlockGroup = WindfallHelper.BlockGroupController.FindGroupOfBlock(currentBlockComponent);

                    //Add each BlockGroup if it has not been added already
                    if (currentBlockGroup != null && !blockingGroups.Contains(currentBlockGroup))
                    {
                        blockingGroups.Add(currentBlockGroup);
                    }
                }
            }

            return blockingGroups;
        }

        /// <summary>
        /// From the given list of BlockGroups, finds a BlockGroup touching the horizontal/vertical edge of the puzzle board in the given direction.
        /// </summary>
        /// <param name="blockGroups">The BlockGroups to search.</param>
        /// <param name="horizontal">Whether to check the right/left side (true) versus the top/bottom side (false)</param>
        /// <param name="positiveDirection">Whether to check the top/left side (true) versus the bottom/right side (false).</param>
        /// <returns>A BlockGroup touching the edge of the puzzle board, or null if none is found.</returns>
        public static BlockGroup FindGroupAtEdge(List<BlockGroup> blockGroups, bool horizontal, bool positiveDirection)
        {
            if (blockGroups == null) return null;

            Puzzle puzzle = WindfallHelper.app.view.puzzle;

            foreach (BlockGroup blockGroup in blockGroups)
            {
                if (blockGroup == null) continue;

                //Find edge position of the puzzle board
                Position groupPosition = blockGroup.GetPosition();
                Vector2Int groupDimensions = blockGroup.GetDimensions();
                int groupEdgePosition = horizontal ? groupPosition.x : groupPosition.y;
                if (positiveDirection) groupEdgePosition += (horizontal ? groupDimensions.x - 1 : groupDimensions.y - 1);

                //Find edge position of the BlockGroup
                int positiveBoardEdgePosition = horizontal ? puzzle.width - 1 : puzzle.height - 1;
                int boardEdgePosition = positiveDirection ? positiveBoardEdgePosition : 0;

                //If they are the same, the BlockGroup is touching the board edge
                if (groupEdgePosition == boardEdgePosition) return blockGroup;
            }

            return null;
        }

        /// <summary>
        /// Returns whether the given groups are the same size and aligned along the given axis.
        /// </summary>
        /// <param name="first">The first BlockGroup.</param>
        /// <param name="second">The second BlockGroup.</param>
        /// <param name="horizontal">Whether to consider the horizontal or vertical axis.</param>
        /// <returns>Whether the given groups are the same size and aligned along the given axis.</returns>
        public static bool GroupsAreAlignedAlongAxis(BlockGroup first, BlockGroup second, bool horizontal)
        {
            if (first == null || second == null) return false;

            Position firstPosition = first.GetPosition();
            Position secondPosition = second.GetPosition();

            Vector2Int firstDimensions = first.GetDimensions();
            Vector2Int secondDimensions = second.GetDimensions();

            //BlockGroups must be at the same position relative to the axis
            bool samePosition;
            if (horizontal)
            {
                samePosition = firstPosition.y == secondPosition.y;
            }
            else
            {
                samePosition = firstPosition.x == secondPosition.x;
            }

            //BlockGroups must have the same dimension relative to the axis
            bool sameSize;
            if (horizontal)
            {
                sameSize = firstDimensions.y == secondDimensions.y;
            }
            else
            {
                sameSize = firstDimensions.x == secondDimensions.x;
            }

            return samePosition && sameSize;
        }

        /// <summary>
        /// Removes all groups aligned with the given BlockGroup from the given list of BlockGroups along the given axis.
        /// </summary>
        /// <param name="selectedGroup">The BlockGroup to be aligned to.</param>
        /// <param name="blockingGroups">The list of BlockGroups to remove from.</param>
        /// <param name="horizontal">The axis to align to.</param>
        /// <returns>A list of all removed BlockGroups.</returns>
        public static List<BlockGroup> RemoveAlignedGroups(BlockGroup selectedGroup, List<BlockGroup> blockingGroups, bool horizontal)
        {
            List<BlockGroup> alignedGroups = new List<BlockGroup>();

            foreach (BlockGroup blockGroup in blockingGroups)
            {
                if (GroupsAreAlignedAlongAxis(selectedGroup, blockGroup, horizontal))
                {
                    alignedGroups.Add(blockGroup);
                }
            }

            blockingGroups.RemoveAll((BlockGroup x) => alignedGroups.Contains(x));

            return alignedGroups;
        }

        private static readonly List<Vector2Int> GroupOffsetsTwoByTwo = new List<Vector2Int>
        {
            new Vector2Int(0, 0),//Origin
            new Vector2Int(0, -1),//Move down
            new Vector2Int(-1, 0),//Move left
            new Vector2Int(-1, -1),//Move down left
        };

        private static readonly List<Vector2Int> GroupOffsetsThreeByThree = new List<Vector2Int>
        {
            new Vector2Int(0, 0),//Origin
            new Vector2Int(0, -1),//Move 1 down
            new Vector2Int(-1, 0),//Move 1 left
            new Vector2Int(-1, -1),//Move 1 down 1 left
            new Vector2Int(0, -2),//Move 2 down
            new Vector2Int(-2, 0),//Move 2 left
            new Vector2Int(-1, -2),//Move 2 down 1 left
            new Vector2Int(-2, -1),//Move 1 down 2 left
            new Vector2Int(-2, -2),//Move 2 down 2 left
        };
    }
}
