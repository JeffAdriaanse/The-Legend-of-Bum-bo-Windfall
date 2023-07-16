using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    /// <summary>
    /// Displays a visible grid indicator on the floor of the enemy battlefield.
    /// </summary>
    public static class BattlefieldGridView
    {
        public static readonly float GRID_VIEW_VERTICAL_OFFSET = 0.01f;
        /// <summary>
        /// The cells of the battlefield grid. Indices follow normal reading order (left-to-right, top-to-bottom) regarding the positions the cells, with position (0, 0) at the top left.
        /// </summary>
        private static BattlefieldGridCellView[] gridCells = new BattlefieldGridCellView[9];
        /// <summary>
        /// The lines of the battlefield grid. Indices follow normal reading order (left-to-right, top-to-bottom) regarding the positions the lines.
        /// </summary>
        private static BattlefieldGridLineView[] gridLines = new BattlefieldGridLineView[8];

        /// <summary>
        /// Initializes the enemy grid. Creates grid cells and lines.
        /// </summary>
        public static void InitializeGrid()
        {
            int cellIndex = 0;
            for (int gridCellIteratorY = 0; gridCellIteratorY < 3; gridCellIteratorY++)
            {
                for (int gridCellIteratorX = 0; gridCellIteratorX < 3; gridCellIteratorX++)
                {
                    if (cellIndex >= gridCells.Length)
                    {
                        break;
                    }

                    gridCells[cellIndex] = new BattlefieldGridCellView(new Vector2Int(gridCellIteratorX, gridCellIteratorY));
                    cellIndex++;
                }
            }

            int lineIndex = 0;
            for (int axisIterator = 0; axisIterator < 2; axisIterator++)
            {
                bool vertical = axisIterator != 0;
                for (int lineIterator = 0; lineIterator < 4; lineIterator++)
                {
                    if (lineIndex >= gridLines.Length)
                    {
                        break;
                    }

                    gridLines[lineIndex] = new BattlefieldGridLineView(vertical, lineIterator);
                    lineIndex++;
                }
            }

            HideGrid();
        }

        /// <summary>
        /// Shows the given grid cells and their adjacent grid lines.
        /// </summary>
        /// <param name="battlefieldPositions">The grid cells to show.</param>
        public static void ShowGrid(List<Vector2Int> battlefieldPositions)
        {
            HideGrid();

            //Activate grid cells
            List<BattlefieldGridCellView> enemyGridCells = GetGridCells(battlefieldPositions);
            foreach (BattlefieldGridCellView enemyGridCellView in enemyGridCells)
            {
                enemyGridCellView.cell.SetActive(true);
            }

            //Grid lines
            foreach (BattlefieldGridCellView gridCellView in enemyGridCells)
            {
                List<BattlefieldGridLineView> gridLines = AdjacentGridLines(gridCellView);
                foreach (BattlefieldGridLineView gridLineView in gridLines)
                {
                    gridLineView.line.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Deactivates all grid cells and lines.
        /// </summary>
        public static void HideGrid()
        {
            foreach (BattlefieldGridCellView gridCellView in gridCells)
            {
                if (gridCellView != null && gridCellView.cell != null)
                {
                    gridCellView.cell.SetActive(false);
                }
            }

            foreach (BattlefieldGridLineView gridLineView in gridLines)
            {
                if (gridLineView != null && gridLineView.line != null)
                {
                    gridLineView.line.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Returns a list of grid cells at the given battlefield positions.
        /// </summary>
        /// <param name="battlefieldPositions">The battlefield positions to find the grid cells for.</param>
        /// <returns>The grid cells located at each of the battlefield positions.</returns>
        private static List<BattlefieldGridCellView> GetGridCells(List<Vector2Int> battlefieldPositions)
        {
            List<BattlefieldGridCellView> cells = new List<BattlefieldGridCellView>();
            for (int battlefieldPositionIterator = 0; battlefieldPositionIterator < battlefieldPositions.Count; battlefieldPositionIterator++)
            {
                for (int gridCellIterator = 0; gridCellIterator < gridCells.Length; gridCellIterator++)
                {
                    //Add grid cells that have the same position as any of the battlefield positions
                    if (battlefieldPositions[battlefieldPositionIterator].x == gridCells[gridCellIterator].battlefieldPosition.x &&
                        battlefieldPositions[battlefieldPositionIterator].y == gridCells[gridCellIterator].battlefieldPosition.y)
                    {
                        cells.Add(gridCells[gridCellIterator]);
                    }
                }
            }
            return cells;
        }

        /// <summary>
        /// Returns a list containing all grid lines touching the given grid cell.
        /// </summary>
        /// <param name="battlefieldGridCellView">The grid cell to find adjacent grid lines for.</param>
        /// <returns>The grid lines adjacent to the grid cell.</returns>
        private static List<BattlefieldGridLineView> AdjacentGridLines(BattlefieldGridCellView battlefieldGridCellView)
        {
            List<BattlefieldGridLineView> lines = new List<BattlefieldGridLineView>();

            for (int gridLineIterator = 0; gridLineIterator < gridLines.Length; gridLineIterator++)
            {
                BattlefieldGridLineView gridLine = gridLines[gridLineIterator];

                int battlefieldPositionOnAxis = gridLine.vertical ? battlefieldGridCellView.battlefieldPosition.x : battlefieldGridCellView.battlefieldPosition.y;
                if (gridLine.index == battlefieldPositionOnAxis || gridLine.index == battlefieldPositionOnAxis + 1)
                {
                    lines.Add(gridLine);
                }
            }

            return lines;
        }
    }

    class BattlefieldGridCellView
    {
        public GameObject cell;

        public Vector2Int battlefieldPosition;

        /// <summary>
        /// Creates a grid cell at the given position.
        /// </summary>
        /// <param name="battlefieldPosition">The cell battlefield position.</param>
        public BattlefieldGridCellView(Vector2Int battlefieldPosition)
        {
            this.battlefieldPosition = battlefieldPosition;
            cell = GameObject.Instantiate((GameObject)Windfall.assetBundle.LoadAsset("GridCellView"));

            //Position
            cell.transform.position = WindfallHelper.WorldSpaceBattlefieldPosition(new Vector2(battlefieldPosition.x, battlefieldPosition.y));
            //Vertical offset
            cell.transform.position += new Vector3(0f, BattlefieldGridView.GRID_VIEW_VERTICAL_OFFSET * 0.5f, 0f);

            //Scale
            cell.transform.localScale = new Vector3(1.85f, 1.85f, 1.85f);
        }
    }

    class BattlefieldGridLineView
    {
        public GameObject line;

        public bool vertical;
        public int index;

        /// <summary>
        /// Creates a grid line with the given axis and index.
        /// </summary>
        /// <param name="vertical">The axis of the grid line.</param>
        /// <param name="index">The index of the grid line within the given axis.</param>
        public BattlefieldGridLineView(bool vertical, int index)
        {
            this.vertical = vertical;
            this.index = index;

            line = GameObject.Instantiate((GameObject)Windfall.assetBundle.LoadAsset("GridLineView"));

            //Positions are based on the four grid cells adjacent to the center
            float lineBattlefieldPosition = (index == 0 || index == 1) ? 0f : 2f;
            //Position lines between grid cells
            float distanceFromCenter = (index == 1 || index == 3) ? 0.5f : -0.5f;

            lineBattlefieldPosition += distanceFromCenter;

            Vector3 worldSpacePosition;
            if (!vertical)
            {
                //Get position of nearby grid cell
                worldSpacePosition = WindfallHelper.WorldSpaceBattlefieldPosition(new Vector2(1, lineBattlefieldPosition));

                //Vertical offset
                worldSpacePosition += new Vector3(0f, BattlefieldGridView.GRID_VIEW_VERTICAL_OFFSET * 0.75f, 0f);

                //Rotate line
                line.transform.eulerAngles = line.transform.eulerAngles + new Vector3(0f, 90f, 0f);

                //Scale line
                line.transform.localScale = new Vector3(line.transform.localScale.x, line.transform.localScale.y, 2.4f);
            }
            else
            {
                //Get position of nearby grid cell
                worldSpacePosition = WindfallHelper.WorldSpaceBattlefieldPosition(new Vector2(lineBattlefieldPosition, 1));

                //Vertical offset
                worldSpacePosition += new Vector3(0f, BattlefieldGridView.GRID_VIEW_VERTICAL_OFFSET * 1f, 0f);

                //Scale line
                line.transform.localScale = new Vector3(line.transform.localScale.x, line.transform.localScale.y, 1.8f);
            }

            line.transform.position = worldSpacePosition;
        }
    }
}
