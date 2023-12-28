using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.StyleSheets;
using static Block;
using static UnityEngine.UIElements.UIR.BestFitAllocator;

namespace The_Legend_of_Bum_bo_Windfall
{
    /// <summary>
    /// Defines a rectangular group of Blocks that act and appear as one larger Block. The bottom left Block is designated the 'main Block' and is enlarged to visually represent the entire group. All other Blocks are designated 'sub-Blocks' and are hidden. All Blocks in the BlockGroup are moved, placed, or destroyed in unison.
    /// </summary>
    public class BlockGroup : MonoBehaviour
    {
        private Vector2Int dimensions;

        /// <summary>
        /// Creates a new BlockGroup.
        /// </summary>
        /// <param name="dimensions">The dimensions of the BlockGroup.</param>
        public void Init(Vector2Int dimensions)
        {
            this.dimensions = dimensions;
        }

        /// <summary>
        /// The main Block of the BlockGroup.
        /// </summary>
        public Block MainBlock
        {
            get
            {
                return GetComponent<Block>();
            }
        }

        /// <summary>
        /// Returns the position of this BlockGroup.
        /// </summary>
        /// <returns>The position of this BlockGroup.</returns>
        public Position GetPosition()
        {
            return MainBlock.position;
        }

        /// <summary>
        /// Returns the dimensions of this BlockGroup.
        /// </summary>
        /// <returns>The dimensions of this BlockGroup.</returns>
        public Vector2Int GetDimensions()
        {
            return dimensions;
        }

        /// <summary>
        /// Returns the area of this BlockGroup.
        /// </summary>
        /// <returns>The total number of individual Blocks in this BlockGroup, including the main Block and all sub-Blocks.</returns>
        public int Area()
        {
            return dimensions.x & dimensions.y;
        }

        /// <summary>
        /// Moves the BlockGroup by the given distance.
        /// </summary>
        /// <param name="movement">The distance to move.</param>
        //public void Move(Vector2Int movement)
        //{
        //    Puzzle puzzle = WindfallHelper.app.view.puzzle;

        //    position.x += movement.x;
        //    if (position.x > puzzle.width) position.x -= puzzle.width;
        //    if (position.x < 0) position.x += puzzle.width;

        //    position.y += movement.y;
        //    if (position.y > puzzle.width) position.y -= puzzle.height;
        //    if (position.y < 0) position.y += puzzle.height;
        //}

        /// <summary>
        /// Returns the Block at the given position relative to the bottom left of the BlockGroup.
        /// </summary>
        /// <param name="position">The Block position.</param>
        /// <returns>The Block at the given position.</returns>
        public GameObject GetBlock(Position position)
        {
            //Get puzzle board
            Puzzle puzzle = WindfallHelper.app.view.puzzle;

            //Return null if position is beyond group dimensions
            if (position.x > dimensions.x || position.y > dimensions.y) return null;

            //Locate block
            Position blockPosition = new Position(GetPosition().x + position.x, GetPosition().y + position.y);

            //Return null for positions beyond the puzzle board
            if (!PuzzleHelper.IsWithinGridBounds(blockPosition)) return null;

            //Return the block
            return puzzle.blocks[blockPosition.x, blockPosition.y];
        }

        /// <summary>
        /// Returns all the puzzle blocks in this group.
        /// </summary>
        /// <returns>All blocks in this group.</returns>
        public List<GameObject> GetBlocks()
        {
            List<GameObject> blocks = new List<GameObject>();

            //Loop through all positions
            for (int xIterator = 0; xIterator < dimensions.x; xIterator++)
            {
                for (int yIterator = 0; yIterator < dimensions.y; yIterator++)
                {
                    //Add the blocks
                    GameObject block = GetBlock(new Position(xIterator, yIterator));
                    if (block != null) blocks.Add(block);
                }
            }

            return blocks;
        }

        /// <summary>
        /// Returns whether a position is in this group.
        /// </summary>
        /// <param name="inputPosition">The input position.</param>
        /// <returns>Whether a position is in this group.</returns>
        public bool Contains(Position inputPosition, bool ignoreX = false, bool ignoreY = false)
        {
            Position groupPosition = GetPosition();
            Vector2Int dimensions = GetDimensions();

            //Check that the position is within the group horizontally
            if (ignoreX || (inputPosition.x >= groupPosition.x && inputPosition.x < groupPosition.x + dimensions.x))
            {
                //Check that the position is within the group vertically
                if (ignoreY || (inputPosition.y >= groupPosition.y && inputPosition.y < groupPosition.y + dimensions.y))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns whether a block is in this group.
        /// </summary>
        /// <returns>Whether a block is in this group.</returns>
        public bool Contains(Block block)
        {
            return Contains(block.position);
        }
    }

    public static class BlockGroupModel
    {
        public static List<BlockGroup> blockGroups = new List<BlockGroup>();

        /// <summary>
        /// Removes all block groups. Intended to be triggered in <see cref="Puzzle.ClearMatches"/>.
        /// </summary>
        public static void RemoveBlockGroups()
        {
            foreach (BlockGroup blockGroup in blockGroups)
            {
                UnityEngine.Object.Destroy(blockGroup);
            }

            blockGroups.Clear();
        }

        /// <summary>
        /// Removes a block group. Intended to be triggered when a main Block is destroyed.
        /// </summary>
        public static void RemoveBlockGroup(BlockGroup blockGroup)
        {
            if (blockGroup != null)
            {
                blockGroups.Remove(blockGroup);
                UnityEngine.Object.Destroy(blockGroup);
            }
        }

        private static List<Vector2Int> AttemptedPositions(Vector2Int dimensions)
        {
            List<Vector2Int> positions = new List<Vector2Int>();

            for (int i = -dimensions.x + 1; i <= 0; i++)
            {
                for (int j = -dimensions.y + 1; j <= 0; j++)
                {
                    positions.Add(new Vector2Int(i, j));
                }
            }

            positions.Reverse();

            return positions;
        }

        private static Position AvailableGroupPosition(Position position, Vector2Int dimensions)
        {
            Puzzle puzzle = WindfallHelper.app.view.puzzle;

            List<Vector2Int> groupOffsets = new List<Vector2Int>();

            //Offsets to attempt
            if (dimensions.x == 2 && dimensions.y == 2)
            {
                groupOffsets.AddRange(GroupOffsetsTwoByTwo);
            }
            else if (dimensions.x == 3 && dimensions.y == 3)
            {
                groupOffsets.AddRange(GroupOffsetsThreeByThree);
            }
            else
            {
                groupOffsets.AddRange(AttemptedPositions(dimensions));
            }

            foreach (Vector2Int offset in groupOffsets)
            {
                Position newPosition = new Position(position.x + offset.x, position.y + offset.y);

                if (!PuzzleHelper.IsWithinGridBounds(newPosition)) continue;

                bool valid = true;

                //Validate all positions
                for (int i = newPosition.x; i < newPosition.x + dimensions.x; i++)
                {
                    for (int j = newPosition.y; j < newPosition.y + dimensions.y; j++)
                    {
                        Position blockPosition = new Position(i, j);

                        //Invalid: Out of bounds
                        if (!PuzzleHelper.IsWithinGridBounds(blockPosition))
                        {
                            valid = false;
                            break;
                        }

                        //Invalid: A BlockGroup is in the way
                        Block block = puzzle.blocks[i, j]?.GetComponent<Block>();
                        if (block != null && FindGroupOfBlock(block) != null)
                        {
                            valid = false;
                            break;
                        }
                    }

                    if (!valid) break;
                }

                if (valid) return newPosition;
            }

            return null;
        }

        /// <summary>
        /// Attempts to create a BlockGroup for the given block at the given position. Overrides nearby blocks that are not in BlockGroups. Fails if there is insufficient space nearby to form the BlockGroup.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <param name="dimensions">The dimensions of the BlockGroup.</param>
        /// <returns>Whether the BlockGroup was successfully created.</returns>
        public static bool CreateBlockGroup(Block block, Vector2Int dimensions)
        {
            Puzzle puzzle = WindfallHelper.app.view.puzzle;

            //Make sure new tile group is valid
            Position newBlockPosition = AvailableGroupPosition(new Position(block.position.x, block.position.y), dimensions);
            if (newBlockPosition == null) return false;

            //Replace backend tiles
            for (int i = newBlockPosition.x; i < newBlockPosition.x + dimensions.x; i++)
            {
                for (int j = newBlockPosition.y; j < newBlockPosition.y + dimensions.y; j++)
                {
                    //Place blocks
                    PuzzleHelper.PlaceBlock(new Position(i, j), block.block_type);

                    //Add BlockGroup to the bottom left block
                    if (i == newBlockPosition.x && j == newBlockPosition.y)
                    {
                        BlockGroup blockGroup = puzzle.blocks[newBlockPosition.x, newBlockPosition.y].AddComponent<BlockGroup>();
                        blockGroup.Init(dimensions);
                        blockGroups.Add(blockGroup);

                        //Update main block display
                        PuzzleHelper.DisplayBlock(blockGroup.MainBlock);
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Ensures all tile shapes in the given list are connected between one another if they share any BlockGroups. Intended to be triggered in <see cref="Puzzle.ClearMatches"/>.
        /// </summary>
        /// <param name="shapes">The tile shapes to combine.</param>
        public static void CombineShapesThatShareBlockGroups(List<List<GameObject>> shapes)
        {
            //Remove BlockGroups that are in puzzle matches
            List<BlockGroup> consumedBlockGroups = new List<BlockGroup>();

            //Loop through all groups
            for (int groupIterator = 0; groupIterator < blockGroups.Count; groupIterator++)
            {
                BlockGroup group = blockGroups[groupIterator];

                //Find all shapes in the group
                List<List<GameObject>> shapesConnectedToGroup = ShapesConnectedToGroup(group, shapes);
                List<GameObject> firstShape = null;

                if (shapesConnectedToGroup.Count >= 1)
                {
                    //Store the first shape
                    firstShape = shapesConnectedToGroup[0];

                    for (int shapeIterator = 1; shapeIterator < shapesConnectedToGroup.Count; shapeIterator++)
                    {
                        List<GameObject> shape = shapesConnectedToGroup[shapeIterator];

                        //Add blocks from all subsequent shapes to the first shape
                        foreach (GameObject block in shape)
                        {
                            if (!firstShape.Contains(block)) firstShape.Add(block);
                        }

                        //Delete subsequent shapes
                        shapes.Remove(shape);
                    }

                    //BlockGroup is in a shape, so it must be removed
                    consumedBlockGroups.Add(group);
                }

                if (firstShape != null)
                {
                    //Add remaining blocks from the group to the first shape
                    foreach (GameObject block in group.GetBlocks())
                    {
                        if (!firstShape.Contains(block))
                        {
                            firstShape.Add(block);
                        }
                    }
                }
            }

            //Remove BlockGroups
            foreach (BlockGroup group in consumedBlockGroups)
            {
                RemoveBlockGroup(group);
            }
        }

        /// <summary>
        /// Returns all shapes connected to the given group.
        /// </summary>
        /// <param name="blockGroup">The blockGroup to find shapes for.</param>
        /// <param name="overallShapes">All shapes.</param>
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
        private static List<BlockGroup> FindGroupsInShape(List<GameObject> shape)
        {
            List<BlockGroup> returnedBlockGroups = new List<BlockGroup>();

            //Find the group of each block
            for (int blockIterator = 0; blockIterator < shape.Count; blockIterator++)
            {
                GameObject gameObject = shape[blockIterator];
                if (gameObject == null) continue;

                Block block = gameObject.GetComponent<Block>();
                if (block == null) continue;

                foreach (BlockGroup blockGroup in blockGroups)
                {
                    if (blockGroup.Contains(block) && !returnedBlockGroups.Contains(blockGroup)/*Ignore duplicates*/)
                    {
                        //Add the block group
                        returnedBlockGroups.Add(blockGroup);
                    }
                }
            }

            return returnedBlockGroups;
        }

        /// <summary>
        /// Returns the BlockGroup of the given Block, or null if the Block is not in a BlockGroup.
        /// </summary>
        /// <param name="block">The Block to find the BlockGroup for.</param>
        /// <returns>The BlockGroup of the given Block, or null if the Block is not in a BlockGroup.</returns>
        public static BlockGroup FindGroupOfBlock(Block block)
        {
            if (block == null) return null;

            foreach (BlockGroup blockGroup in blockGroups)
            {
                if (blockGroup.Contains(block))
                {
                    return blockGroup;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the BlockGroup of the Block associated with the given GameObject, or null if the Block is not in a BlockGroup.
        /// </summary>
        /// <param name="blockObject">The GameObject of the Block to find the BlockGroup for.</param>
        /// <returns>The BlockGroup of the Block, or null if the Block is not in a BlockGroup.</returns>
        public static BlockGroup FindGroupOfBlock(GameObject blockObject)
        {
            Block block = blockObject?.GetComponent<Block>();
            return FindGroupOfBlock(block);
        }

        /// <summary>
        /// Returns whether the Block of the given Gameobject is the main Block in its BlockGroup.
        /// </summary>
        /// <param name="blockObject">The Block.</param>
        /// <returns>True if the Block of the given Gameobject is in a BlockGroup and is the main Block in its BlockGroup.</returns>
        public static bool IsMainBlock(GameObject blockObject)
        {
            Block block = blockObject?.GetComponent<Block>();

            return IsMainBlock(block);
        }

        /// <summary>
        /// Returns whether the given Block is the main Block in its BlockGroup.
        /// </summary>
        /// <param name="block">The Block.</param>
        /// <returns>True if the Block is in a BlockGroup and is the main Block in its BlockGroup.</returns>
        public static bool IsMainBlock(Block block)
        {
            return block?.GetComponent<BlockGroup>() != null;
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

                    if (!PuzzleHelper.IsWithinGridBounds(blockPosition)) continue;

                    GameObject currentBlock = puzzle.blocks[blockPosition.x, blockPosition.y];
                    Block currentBlockComponent = currentBlock?.GetComponent<Block>();

                    BlockGroup currentBlockGroup = null;
                    if (currentBlockComponent != null) currentBlockGroup = FindGroupOfBlock(currentBlockComponent);

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
