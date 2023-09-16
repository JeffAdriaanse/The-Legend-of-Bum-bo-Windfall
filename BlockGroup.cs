using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.StyleSheets;
using static UnityEngine.UIElements.UIR.BestFitAllocator;

namespace The_Legend_of_Bum_bo_Windfall
{
    public class BlockGroup
    {
        private Vector2Int position;
        private Vector2Int dimensions;

        /// <summary>
        /// Creates a new BlockGroup.
        /// </summary>
        /// <param name="position">The position on the puzzle board.</param>
        /// <param name="dimensions">The dimensions of the BlockGroup.</param>
        public BlockGroup(Vector2Int position, Vector2Int dimensions)
        {
            this.position = position;
            this.dimensions = dimensions;
        }

        /// <summary>
        /// Returns the position of this BlockGroup.
        /// </summary>
        /// <returns>The position of this BlockGroup.</returns>
        public Vector2Int GetPosition()
        {
            return position;
        }

        /// <summary>
        /// Sets the position of this BlockGroup.
        /// </summary>
        /// <returns>The position to set.</returns>
        public void SetPosition(Vector2Int position)
        {
            this.position = position;
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
        /// Moves the BlockGroup by the given distance.
        /// </summary>
        /// <param name="movement">The distance to move.</param>
        public void Move(Vector2Int movement)
        {
            Puzzle puzzle = WindfallHelper.app.view.puzzle;

            position.x += movement.x;
            if (position.x > puzzle.width) position.x -= puzzle.width;
            if (position.x < 0) position.x += puzzle.width;

            position.y += movement.y;
            if (position.y > puzzle.width) position.y -= puzzle.height;
            if (position.y < 0) position.y += puzzle.height;
        }

        /// <summary>
        /// Returns the main Block of this BlockGroup.
        /// </summary>
        /// <returns>The main Block of this BlockGroup.</returns>
        public GameObject GetMainBlock()
        {
            return GetBlock(new Vector2Int(0, 0));
        }

        /// <summary>
        /// Returns the Block at the given position relative to the bottom left of the BlockGroup.
        /// </summary>
        /// <param name="position">The Block position.</param>
        /// <returns>The Block at the given position.</returns>
        public GameObject GetBlock(Vector2Int position)
        {
            //Get puzzle board
            Puzzle puzzle = WindfallHelper.app.view.puzzle;

            //Return null if position is beyond group dimensions
            if (position.x > this.dimensions.x || position.y > this.dimensions.y) return null;

            //Locate block
            int xPosition = this.position.x + position.x;
            int yPosition = this.position.y + position.y;

            //Return null for positions beyond the puzzle board
            if (xPosition >= puzzle.width || yPosition >= puzzle.height) return null;

            //Return the block
            return puzzle.blocks[xPosition, yPosition];
        }

        /// <summary>
        /// Returns all the puzzle blocks in this group.
        /// </summary>
        /// <returns>All blocks in this group.</returns>
        public List<GameObject> GetBlocks()
        {
            List<GameObject> blocks = new List<GameObject>();
            Puzzle puzzle = WindfallHelper.app.view.puzzle;

            //Loop through all positions
            for (int xIterator = 0; xIterator < dimensions.x; xIterator++)
            {
                for (int yIterator = 0; yIterator < dimensions.y; yIterator++)
                {
                    //Add the blocks
                    GameObject block = GetBlock(new Vector2Int(xIterator, yIterator));
                    if (block != null) blocks.Add(block);
                }
            }

            return blocks;
        }

        /// <summary>
        /// Returns whether a position is in this group.
        /// </summary>
        /// <returns>Whether a position is in this group.</returns>
        public bool Contains(Vector2Int position)
        {
            //Check that the position is within the group horizontally
            if (position.x >= this.position.x && position.x < this.position.x + dimensions.x)
            {
                //Check that the position is within the group vertically
                if (position.y >= this.position.y && position.y < this.position.y + dimensions.y)
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
            return Contains(new Vector2Int(block.position.x, block.position.y));
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
            blockGroups.Clear();
        }

        /// <summary>
        /// Removes a block group. Intended to be triggered when a main Block is destroyed.
        /// </summary>
        public static void RemoveBlockGroup(BlockGroup blockGroup)
        {
            if (blockGroup != null) blockGroups.Remove(blockGroup);
        }

        /// <summary>
        /// Creates a block group for the given block.
        /// </summary>
        /// <param name="_block">The block.</param>
        /// <param name="dimensions">The dimensions of the group.</param>
        public static void CreateBlockGroup(Block _block, Vector2Int dimensions)
        {
            Puzzle puzzle = WindfallHelper.app.view.puzzle;

            //Make sure new tile group is in bounds
            Vector2Int newBlockPosition = new Vector2Int(_block.position.x, _block.position.y);
            while (newBlockPosition.x >= puzzle.width) newBlockPosition.x--;
            while (newBlockPosition.y >= puzzle.height) newBlockPosition.y--;

            //Create block group
            BlockGroup group = new BlockGroup(newBlockPosition, dimensions);
            //Add blockGroup to list
            blockGroups.Add(group);

            //Replace backend tiles
            PuzzleHelper.PlaceBlock(newBlockPosition, _block.block_type);

            //List<GameObject> blocks = group.GetBlocks();
            //foreach (GameObject blockObject in blocks)
            //{
            //    Block block = blockObject?.GetComponent<Block>();
            //    if (block == null) continue;

            //    if (block != _block)
            //    {
            //        Vector2Int blockPosition = new Vector2Int(block.position.x, block.position.y);
            //        block.GetComponent<Block>().Despawn(false);
            //        puzzle.setBlock(_block.block_type, (short)blockPosition.x, (short)blockPosition.y, false, false);
            //    }
            //}
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
                if (blockGroups.Contains(group)) blockGroups.Remove(group);
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
        /// Returns all groups in the given shape.
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
            if (blockObject == null) return false;

            Block block = blockObject.GetComponent<Block>();

            return IsMainBlock(block, FindGroupOfBlock(block));
        }

        /// <summary>
        /// Returns whether the given Block is the main Block in its BlockGroup.
        /// </summary>
        /// <param name="block">The Block.</param>
        /// <returns>True if the Block is in a BlockGroup and is the main Block in its BlockGroup.</returns>
        public static bool IsMainBlock(Block block)
        {
            return IsMainBlock(block, FindGroupOfBlock(block));
        }

        /// <summary>
        /// Returns whether the given Block is the main Block in the given BlockGroup.
        /// </summary>
        /// <param name="block">The Block.</param>
        /// <param name="blockGroup">The BlockGroup.</param>
        /// <returns>True if the Block is the main Block in the BlockGroup.</returns>
        public static bool IsMainBlock(Block block, BlockGroup blockGroup)
        {
            if (block == null || blockGroup == null) return false;

            Block mainBlock = blockGroup.GetMainBlock()?.GetComponent<Block>();
            if (mainBlock == null) return false;

            return block.position.x == mainBlock.position.x && block.position.y == mainBlock.position.y;
        }

        /// <summary>
        /// Finds all BlockGroups horizontally/vertically blocking the given position and dimensions on the puzzle board.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="dimensions"></param>
        /// <param name="horizontal"></param>
        /// <returns></returns>
        public static List<BlockGroup> BlockingGroupsAlongAxis(Vector2Int position, Vector2Int dimensions, bool horizontal)
        {
            List<BlockGroup> blockingGroups = new List<BlockGroup>();

            Puzzle puzzle = WindfallHelper.app.view.puzzle;

            int width = horizontal ? puzzle.width : dimensions.x;
            int height = !horizontal ? puzzle.height : dimensions.y;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    //Get all positions along the given axis within the given dimensions
                    int x = i + position.x;
                    int y = j + position.y;

                    if (x < 0 || x >= puzzle.blocks.GetLength(0)) continue;
                    if (y < 0 || y >= puzzle.blocks.GetLength(1)) continue;

                    GameObject currentBlock = puzzle.blocks[x, y];
                    Block currentBlockComponent = currentBlock?.GetComponent<Block>();

                    BlockGroup currentBlockGroup = null;
                    if (currentBlockComponent != null) currentBlockGroup = BlockGroupModel.FindGroupOfBlock(currentBlockComponent);

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
                Vector2Int groupPosition = blockGroup.GetPosition();
                Vector2Int groupDimensions = blockGroup.GetDimensions();
                int groupEdgePosition = horizontal ? groupPosition.x : groupPosition.y;
                if (positiveDirection) groupEdgePosition += (horizontal ? groupDimensions.x : groupDimensions.y);

                //Find edge position of the BlockGroup
                int positiveBoardEdgePosition = horizontal ? puzzle.width : puzzle.height;
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

            Vector2Int firstPosition = first.GetPosition();
            Vector2Int secondPosition = second.GetPosition();

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
    }
}
