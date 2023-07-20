using DG.Tweening;
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
                    gridCells[cellIndex] = GameObject.Instantiate((GameObject)Windfall.assetBundle.LoadAsset("GridCellView")).AddComponent<BattlefieldGridCellView>();
                    gridCells[cellIndex].Init(new Vector2Int(gridCellIteratorX, gridCellIteratorY));

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

                    gridLines[lineIndex] = GameObject.Instantiate((GameObject)Windfall.assetBundle.LoadAsset("GridLineView")).AddComponent<BattlefieldGridLineView>();
                    gridLines[lineIndex].Init(vertical, lineIterator);

                    lineIndex++;
                }
            }

            ShowGrid(null);
        }

        /// <summary>
        /// Shows the given grid cells and their adjacent grid lines. Hides all other grid cells and lines.
        /// </summary>
        /// <param name="battlefieldPositions">The list of enemy positions for which grid cells and lines will be shown. If the list is null, all cells and lines in the grid will be hidden.</param>
        public static void ShowGrid(List<Vector2Int> battlefieldPositions)
        {
            List<BattlefieldGridCellView> enemyGridCells = new List<BattlefieldGridCellView>();
            List<BattlefieldGridLineView> enemyGridLines = new List<BattlefieldGridLineView>();

            //If battlefield positions are null, abort and hide all cells and lines
            if (battlefieldPositions != null)
            {
                //Find all grid cells occupied by the enemy
                enemyGridCells = GetGridCells(battlefieldPositions);

                //Find all grid lines around the enemy grid cells
                foreach (BattlefieldGridCellView gridCellView in enemyGridCells)
                {
                    if (gridCellView == null) { continue; }

                    List<BattlefieldGridLineView> cellGridLines = AdjacentGridLines(gridCellView);

                    foreach (BattlefieldGridLineView cellGridLine in cellGridLines)
                    {
                        if (cellGridLine == null) { continue; }

                        if (!enemyGridLines.Contains(cellGridLine))
                        {
                            enemyGridLines.Add(cellGridLine);
                        }
                    }
                }
            }

            //Toggle grid cells
            foreach (BattlefieldGridCellView gridCellView in gridCells)
            {
                if (gridCellView == null) { continue; }
                gridCellView.DisplayElement(enemyGridCells.Contains(gridCellView));
            }

            //Toggle grid lines
            foreach (BattlefieldGridLineView gridLineView in gridLines)
            {
                if (gridLineView == null) { continue; }
                gridLineView.DisplayElement(enemyGridLines.Contains(gridLineView));
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

    abstract class BattlefieldGridElementView : MonoBehaviour
    {
        protected readonly float SHOWING_HEIGHT = 0f;
        protected readonly float HIDING_HEIGHT = -0.05f;

        private Sequence displaySequence;
        private readonly float tweenDuration = 0.2f;

        private bool showing = true;

        public void DisplayElement(bool show)
        {
            if (show == showing)
            {
                return;
            }
            showing = show;

            if (show)
            {
                ShowElement();
            }
            else
            {
                HideElement();
            }
        }

        protected void ShowElement()
        {
            gameObject.SetActive(true);
            TriggerDisplaySequence(ShowingHeight());
        }

        protected void HideElement()
        {
            TriggerDisplaySequence(HidingHeight());
            //displaySequence.PrependInterval(tweenDuration);
            displaySequence.AppendCallback(delegate { gameObject.SetActive(false); });
        }

        protected void TriggerDisplaySequence(float targetHeight)
        {
            if (displaySequence != null && displaySequence.IsPlaying())
            {
                displaySequence.Kill(false);
            }

            displaySequence = DOTween.Sequence();
            displaySequence.Append(transform.DOMove(new Vector3(transform.position.x, targetHeight, transform.position.z), tweenDuration).SetEase(Ease.InOutQuad));
        }

        protected float ShowingHeight()
        {
            return SHOWING_HEIGHT;
        }
        
        protected float HidingHeight()
        {
            return HIDING_HEIGHT;
        }
    }

    class BattlefieldGridCellView : BattlefieldGridElementView
    {
        public Vector2Int battlefieldPosition;

        /// <summary>
        /// Initializes a grid cell at the given position.
        /// </summary>
        /// <param name="battlefieldPosition">The cell battlefield position.</param>
        public void Init(Vector2Int battlefieldPosition)
        {
            this.battlefieldPosition = battlefieldPosition;

            //Position
            gameObject.transform.position = WindfallHelper.WorldSpaceBattlefieldPosition(new Vector2(battlefieldPosition.x, battlefieldPosition.y));
            //Default to hiding position
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, HIDING_HEIGHT, gameObject.transform.position.z);

            //Scale
            gameObject.transform.localScale = new Vector3(1.85f, 3f, 1.85f);
        }
    }

    class BattlefieldGridLineView : BattlefieldGridElementView
    {
        private float SHOWING_HEIGHT_MODIFIER = 0f;

        public bool vertical;
        public int index;

        /// <summary>
        /// Creates a grid line with the given axis and index.
        /// </summary>
        /// <param name="vertical">The axis of the grid line.</param>
        /// <param name="index">The index of the grid line within the given axis.</param>
        public void Init(bool vertical, int index)
        {
            this.vertical = vertical;
            this.index = index;

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

                SHOWING_HEIGHT_MODIFIER = 0f;

                //Default to hiding position
                worldSpacePosition = new Vector3(worldSpacePosition.x, HIDING_HEIGHT, worldSpacePosition.z);

                //Rotate line
                gameObject.transform.eulerAngles = gameObject.transform.eulerAngles + new Vector3(0f, 90f, 0f);

                //Scale line
                gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x, gameObject.transform.localScale.y, 2.4f);
            }
            else
            {
                //Get position of nearby grid cell
                worldSpacePosition = WindfallHelper.WorldSpaceBattlefieldPosition(new Vector2(lineBattlefieldPosition, 1));

                SHOWING_HEIGHT_MODIFIER = 0.01f;

                //Default to hiding position
                worldSpacePosition = new Vector3(worldSpacePosition.x, HIDING_HEIGHT, worldSpacePosition.z);

                //Scale line
                gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x, gameObject.transform.localScale.y, 1.8f);
            }

            gameObject.transform.position = worldSpacePosition;
        }

        private float ShowingHeight()
        {
            return base.ShowingHeight() + SHOWING_HEIGHT_MODIFIER;
        }
    }
}
