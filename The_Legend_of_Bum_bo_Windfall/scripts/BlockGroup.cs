using System.Collections.Generic;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    /// <summary>
    /// Defines a rectangular group of Blocks that act and appear as one larger Block. The bottom left Block is designated the 'main Block' and is enlarged to visually represent the entire group. All other Blocks are designated 'sub-Blocks' and are hidden. All Blocks in the BlockGroup are moved, placed, and destroyed in unison.
    /// </summary>
    public class BlockGroup : MonoBehaviour
    {
        /// <summary>
        /// The properties of the BlockGroup.
        /// </summary>
        private BlockGroupData blockGroupData;

        /// <summary>
        /// Creates a new BlockGroup.
        /// </summary>
        /// <param name="blockGroupData">The properties of the BlockGroup.</param>
        public void Init(BlockGroupData blockGroupData)
        {
            this.blockGroupData = blockGroupData;
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

        public BlockGroupData GetBlockGroupData()
        {
            return new BlockGroupData(blockGroupData);
        }

        /// <summary>
        /// Returns the position of this BlockGroup.
        /// </summary>
        /// <returns>The position of this BlockGroup.</returns>
        public Position GetPosition()
        {
            return new Position(MainBlock.position.x, MainBlock.position.y);
        }

        /// <summary>
        /// Returns the dimensions of this BlockGroup.
        /// </summary>
        /// <returns>The dimensions of this BlockGroup.</returns>
        public Vector2Int GetDimensions()
        {
            return new Vector2Int(blockGroupData.dimensions.x, blockGroupData.dimensions.y);
        }

        /// <summary>
        /// Returns the area of this BlockGroup.
        /// </summary>
        /// <returns>The total number of individual Blocks in this BlockGroup, including the main Block and all sub-Blocks.</returns>
        public int Area()
        {
            Vector2Int dimensions = GetDimensions();
            return dimensions.x * dimensions.y;
        }

        /// <summary>
        /// Returns the shape value of this BlockGroup.
        /// </summary>
        /// <returns>The number of Blocks the BlockGroup contributes towards the matching length of puzzle lines.</returns>
        public int GetShapeValue()
        {
            return blockGroupData.shapeValue;
        }

        /// <summary>
        /// Returns the combo contribution of this BlockGroup.
        /// </summary>
        /// <returns>The number of Blocks the BlockGroup contributes towards the size of matched tile combos.</returns>
        public int GetComboContribution()
        {
            return blockGroupData.comboContribution;
        }

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
            Vector2Int dimensions = GetDimensions();
            if (position.x > dimensions.x || position.y > dimensions.y) return null;

            //Locate block
            Position blockPosition = new Position(GetPosition().x + position.x, GetPosition().y + position.y);

            //Return null for positions beyond the puzzle board
            if (!PuzzleHelper.IsWithinPuzzleBounds(blockPosition)) return null;

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
            Vector2Int dimensions = GetDimensions();
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

    public class BlockGroupData
    {
        public BlockGroupData(BlockGroupData blockGroupData)
        {
            this.dimensions = blockGroupData.dimensions;
            this.shapeValue = blockGroupData.shapeValue;
            this.comboContribution = blockGroupData.comboContribution;
        }

        public BlockGroupData(int size)
        {
            this.dimensions = new Vector2Int(size, size);
            this.shapeValue = size;
            this.comboContribution = size;
        }

        public BlockGroupData(Vector2Int dimensions, int shapeValue, int comboContribution)
        {
            this.dimensions = dimensions;
            this.shapeValue = shapeValue;
            this.comboContribution = comboContribution;
        }

        /// <summary>
        /// Increments or decrements the size of the BlockGroup by the given integer.
        /// </summary>
        /// <param name="addend">The size to change by.</param>
        public void ChangeSize(int addend)
        {
            dimensions = new Vector2Int(dimensions.x + addend, dimensions.y + addend);
            shapeValue += addend;
            comboContribution += addend;
        }

        /// <summary>
        /// The dimensions of the BlockGroup on the puzzle board.
        /// </summary>
        public Vector2Int dimensions;

        /// <summary>
        /// Determines the number of Blocks the BlockGroup contributes towards the matching length of puzzle lines.
        /// </summary>
        public int shapeValue;

        /// <summary>
        /// Determines the number of Blocks the BlockGroup contributes towards the size of matched tile combos.
        /// </summary>
        public int comboContribution;
    }
}
