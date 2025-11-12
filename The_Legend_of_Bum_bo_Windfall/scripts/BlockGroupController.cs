using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    public class BlockGroupController : MonoBehaviour
    {
        private List<BlockGroup> blockGroups = new List<BlockGroup>();
        public List<BlockGroup> BlockGroups
        {
            get { return blockGroups; }
            set { blockGroups = value; }
        }

        /// <summary>
        /// Places a BlockGroup at the given Position. Overrides nearby Blocks that are not in BlockGroups. Does not verify that the BlockGroup is being placed in a valid Position.
        /// </summary>
        /// <param name="position">The Position to place the BlockGroup.</param>
        /// <param name="blockGroupData">The BlockGroupData of the BlockGroup.</param>
        /// <returns>The created BlockGroup.</returns>
        public BlockGroup PlaceBlockGroup(Position position, Block.BlockType blockType, BlockGroupData blockGroupData, bool animateBlock, bool wiggleBlock)
        {
            BlockGroup placedBlockGroup = null;

            Block mainBlock = null;
            List<Block> placedBlocks = new List<Block>();

            //Replace backend Blocks
            for (int i = position.x; i < position.x + blockGroupData.dimensions.x; i++)
            {
                for (int j = position.y; j < position.y + blockGroupData.dimensions.y; j++)
                {
                    //Place Blocks *Note: BlockGroups are overridden to never animate bacause it causes them to align improperly
                    Block placedBlock = PuzzleHelper.PlaceBlock(new Position(i, j), blockType, /*animateBlock*/false, wiggleBlock, false);
                    placedBlocks.Add(placedBlock);

                    //The bottom left Block is the main Block
                    if (i == position.x && j == position.y) mainBlock = placedBlock;
                }
            }

            //Add BlockGroup to the main Block
            if (mainBlock != null)
            {
                placedBlockGroup = mainBlock.gameObject.AddComponent<BlockGroup>();
                placedBlockGroup.Init(new BlockGroupData(blockGroupData));
                blockGroups.Add(placedBlockGroup);
            }

            //Update Block displays
            foreach (Block placedBlock in placedBlocks) PuzzleHelper.DisplayBlock(placedBlock);

            return placedBlockGroup;
        }

        /// <summary>
        /// Attempts to create a BlockGroup for the given Block. Overrides nearby Blocks that are not in BlockGroups. Fails if there is insufficient space nearby to form the BlockGroup.
        /// </summary>
        /// <param name="block">The Block.</param>
        /// <param name="blockGroupData">The BlockGroupData of the BlockGroup.</param>
        /// <param name="random">Whether to prioritize the offset of the BlockGroup randomly.</param>
        /// <returns>The created BlockGroup, or null if no BlockGroup was created.</returns>
        public BlockGroup PlaceBlockGroup(Block block, BlockGroupData blockGroupData, bool animateBlock, bool wiggleBlock, bool random)
        {
            //Make sure BlockGroup Position is valid
            Position blockGroupPosition = BlockGroupHelper.FindValidGroupOffset(block.position, blockGroupData.dimensions, true, random);
            if (blockGroupPosition == null) return null;

            return PlaceBlockGroup(blockGroupPosition, block.block_type, blockGroupData, animateBlock, wiggleBlock);
        }

        /// <summary>
        /// Removes all block groups. Intended to be triggered in <see cref="Puzzle.ClearMatches"/>.
        /// </summary>
        public void RemoveBlockGroups()
        {
            foreach (BlockGroup blockGroup in blockGroups)
            {
                //Due to Block pooling, the BlockGroup must be destroyed immediately. If the BlockGroup is not destroyed immediately, removing and placing BlockGroups on the same frame can cause newly spawned Blocks to erroneously appear to be main Blocks due to the lingering BlockGroup component.
                UnityEngine.Object.DestroyImmediate(blockGroup);
            }

            blockGroups.Clear();
        }

        /// <summary>
        /// Removes a block group. Intended to be triggered when a main Block is destroyed.
        /// </summary>
        public void RemoveBlockGroup(BlockGroup blockGroup)
        {
            if (blockGroup != null)
            {
                blockGroups.Remove(blockGroup);
                //Due to Block pooling, the BlockGroup must be destroyed immediately. If the BlockGroup is not destroyed immediately, removing and placing BlockGroups on the same frame can cause newly spawned Blocks to erroneously appear to be main Blocks due to the lingering BlockGroup component.
                UnityEngine.Object.DestroyImmediate(blockGroup);
            }
        }

        /// <summary>
        /// Returns the BlockGroup of the given Block, or null if the Block is not in a BlockGroup.
        /// </summary>
        /// <param name="block">The Block to find the BlockGroup for.</param>
        /// <returns>The BlockGroup of the given Block, or null if the Block is not in a BlockGroup.</returns>
        public BlockGroup FindGroupOfBlock(Block block)
        {
            if (block == null) return null;

            foreach (BlockGroup blockGroup in blockGroups)
            {
                if (blockGroup != null && blockGroup.Contains(block))
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
        public BlockGroup FindGroupOfBlock(GameObject blockObject)
        {
            Block block = blockObject?.GetComponent<Block>();
            return FindGroupOfBlock(block);
        }

        /// <summary>
        /// Returns whether the Block of the given Gameobject is the main Block in its BlockGroup.
        /// </summary>
        /// <param name="blockObject">The Block.</param>
        /// <returns>True if the Block of the given Gameobject is in a BlockGroup and is the main Block in its BlockGroup.</returns>
        public bool IsMainBlock(GameObject blockObject)
        {
            return IsMainBlock(blockObject?.GetComponent<Block>());
        }

        /// <summary>
        /// Returns whether the given Block is the main Block in its BlockGroup.
        /// </summary>
        /// <param name="block">The Block.</param>
        /// <returns>True if the Block is in a BlockGroup and is the main Block in its BlockGroup.</returns>
        public bool IsMainBlock(Block block)
        {
            BlockGroup blockGroup = block?.GetComponent<BlockGroup>();
            return blockGroup != null && blockGroups.Contains(blockGroup);
        }
    }
}
