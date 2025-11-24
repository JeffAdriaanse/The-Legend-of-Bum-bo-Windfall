using DG.Tweening;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace The_Legend_of_Bum_bo_Windfall
{
    /// <summary>
    /// Displays a visible grid indicator on the floor of the enemy battlefield.
    /// </summary>
    public class BattlefieldGridViewController : MonoBehaviour
    {
        public readonly float GRID_VIEW_VERTICAL_OFFSET = 0.01f;
        /// <summary>
        /// The cells of the battlefield grid. Indices follow normal reading order (left-to-right, top-to-bottom) regarding the positions the cells, with position (0, 0) at the top left.
        /// </summary>
        private BattlefieldGridCellView[] gridCells = new BattlefieldGridCellView[9];
        /// <summary>
        /// The lines of the battlefield grid. Indices follow normal reading order (left-to-right, top-to-bottom) regarding the positions the lines.
        /// </summary>
        private BattlefieldGridLineView[] gridLines = new BattlefieldGridLineView[8];

        /// <summary>
        /// Initializes the enemy grid. Creates grid cells and lines.
        /// </summary>
        public void InitializeGrid()
        {
            int cellIndex = 0;
            for (int gridCellIteratorY = 0; gridCellIteratorY < 3; gridCellIteratorY++)
            {
                for (int gridCellIteratorX = 0; gridCellIteratorX < 3; gridCellIteratorX++)
                {
                    if (cellIndex >= gridCells.Length) break;
                    gridCells[cellIndex] = GameObject.Instantiate((GameObject)Windfall.assetBundle.LoadAsset("GridCellView")).AddComponent<BattlefieldGridCellView>();
                    WindfallHelper.ResetShader(gridCells[cellIndex].transform, false);
                    gridCells[cellIndex].Init(new Vector2Int(gridCellIteratorX, gridCellIteratorY));
                    gridCells[cellIndex].gameObject.SetActive(false);

                    cellIndex++;
                }
            }

            int lineIndex = 0;
            for (int axisIterator = 0; axisIterator < 2; axisIterator++)
            {
                bool vertical = axisIterator != 0;
                for (int lineIterator = 0; lineIterator < 4; lineIterator++)
                {
                    if (lineIndex >= gridLines.Length) break;

                    gridLines[lineIndex] = GameObject.Instantiate((GameObject)Windfall.assetBundle.LoadAsset("GridLineView")).AddComponent<BattlefieldGridLineView>();
                    WindfallHelper.ResetShader(gridLines[lineIndex].transform, false);
                    gridLines[lineIndex].Init(vertical, lineIterator);
                    gridLines[lineIndex].gameObject.SetActive(false);

                    lineIndex++;
                }
            }
        }

        /// <summary>
        /// Shows the given grid cells and their adjacent grid lines. Hides all other grid cells and lines.
        /// </summary>
        /// <param name="battlefieldPositions">The list of enemy positions for which grid cells and lines will be shown. If the list is null, all cells and lines in the grid will be hidden.</param>
        public void ShowGrid(List<Vector2Int> battlefieldPositions)
        {
            List<BattlefieldGridCellView> enemyGridCells = new List<BattlefieldGridCellView>();
            List<BattlefieldGridLineView> enemyGridLines = new List<BattlefieldGridLineView>();

            //If battlefield positions are null, abort and hide all cells and lines
            if (battlefieldPositions != null && battlefieldPositions.Count > 0)
            {
                //Find all grid cells occupied by the enemy
                enemyGridCells = GetGridCells(battlefieldPositions);

                //Find all grid lines around the enemy grid cells
                foreach (BattlefieldGridCellView gridCellView in enemyGridCells)
                {
                    if (gridCellView == null) continue;

                    List<BattlefieldGridLineView> cellGridLines = AdjacentGridLines(gridCellView);
                    foreach (BattlefieldGridLineView cellGridLine in cellGridLines)
                    {
                        if (cellGridLine == null) continue;

                        if (!enemyGridLines.Contains(cellGridLine)) enemyGridLines.Add(cellGridLine);
                    }
                }
            }

            //Toggle grid cells
            foreach (BattlefieldGridCellView gridCellView in gridCells)
            {
                if (gridCellView == null) continue;
                gridCellView.DisplayElement(enemyGridCells.Contains(gridCellView));
            }

            //Toggle grid lines
            foreach (BattlefieldGridLineView gridLineView in gridLines)
            {
                if (gridLineView == null) continue;
                gridLineView.DisplayElement(enemyGridLines.Contains(gridLineView));
            }
        }

        /// <summary>
        /// Returns a list of grid cells at the given battlefield positions.
        /// </summary>
        /// <param name="battlefieldPositions">The battlefield positions to find the grid cells for.</param>
        /// <returns>The grid cells located at each of the battlefield positions.</returns>
        private List<BattlefieldGridCellView> GetGridCells(List<Vector2Int> battlefieldPositions)
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
        private List<BattlefieldGridLineView> AdjacentGridLines(BattlefieldGridCellView battlefieldGridCellView)
        {
            List<BattlefieldGridLineView> lines = new List<BattlefieldGridLineView>();

            for (int gridLineIterator = 0; gridLineIterator < gridLines.Length; gridLineIterator++)
            {
                BattlefieldGridLineView gridLine = gridLines[gridLineIterator];

                int battlefieldPositionOnAxis = gridLine.vertical ? battlefieldGridCellView.battlefieldPosition.x : battlefieldGridCellView.battlefieldPosition.y;
                if (gridLine.index == battlefieldPositionOnAxis || gridLine.index == battlefieldPositionOnAxis + 1) lines.Add(gridLine);
            }

            return lines;
        }
    }

    abstract class BattlefieldGridElementView : MonoBehaviour
    {
        private Sequence displaySequence;
        private readonly float TWEEN_DURATION = 0.2f;
        protected readonly Vector3 HIDING_SCALE = Vector3.zero;

        private bool showing = false;

        public void DisplayElement(bool show)
        {
            if (show == showing) return;
            showing = show;

            if (show) ShowElement();
            else HideElement();
        }

        protected void ShowElement()
        {
            gameObject.SetActive(true);
            TriggerDisplaySequence(ShowingScale());
        }

        protected void HideElement()
        {
            TriggerDisplaySequence(Vector3.zero);
            //displaySequence.PrependInterval(tweenDuration);
            displaySequence.AppendCallback(delegate { gameObject.SetActive(false); });
        }

        protected void TriggerDisplaySequence(Vector3 scale)
        {
            if (displaySequence != null && displaySequence.IsPlaying()) displaySequence.Kill(false);

            displaySequence = DOTween.Sequence();
            displaySequence.Append(transform.DOScale(scale, TWEEN_DURATION).SetEase(Ease.InOutQuad));
        }

        protected virtual Vector3 ShowingScale()
        {
            return Vector3.one;
        }
    }

    class BattlefieldGridCellView : BattlefieldGridElementView
    {
        public Vector2Int battlefieldPosition;

        private Vector3 SHOWING_SCALE = new Vector3(1.85f, 3f, 1.85f);

        /// <summary>
        /// Initializes a grid cell at the given position.
        /// </summary>
        /// <param name="battlefieldPosition">The cell battlefield position.</param>
        public void Init(Vector2Int battlefieldPosition)
        {
            this.battlefieldPosition = battlefieldPosition;

            //Initial position
            gameObject.transform.position = WindfallHelper.WorldSpaceBattlefieldPosition(new Vector2(battlefieldPosition.x, battlefieldPosition.y));

            //Initial scale
            gameObject.transform.localScale = HIDING_SCALE;
        }

        protected override Vector3 ShowingScale()
        {
            return SHOWING_SCALE;
        }
    }

    class BattlefieldGridLineView : BattlefieldGridElementView
    {
        private readonly float Y_AXIS_OFFSET = 0.02f;
        private Vector3 SHOWING_SCALE = Vector3.one;

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
                //Convert adjusted battlefield position to world space
                worldSpacePosition = WindfallHelper.WorldSpaceBattlefieldPosition(new Vector2(1, lineBattlefieldPosition));

                //Rotate horizontal lines
                gameObject.transform.eulerAngles = gameObject.transform.eulerAngles + new Vector3(0f, 90f, 0f);

                //Set showing scale
                SHOWING_SCALE = new Vector3(gameObject.transform.localScale.x, gameObject.transform.localScale.y, 2.4f);
            }
            else
            {
                //Convert adjusted battlefield position to world space
                worldSpacePosition = WindfallHelper.WorldSpaceBattlefieldPosition(new Vector2(lineBattlefieldPosition, 1));

                //Offset vertical lines on the y axis
                worldSpacePosition = new Vector3(worldSpacePosition.x, Y_AXIS_OFFSET, worldSpacePosition.z);

                //Set showing scale
                SHOWING_SCALE = new Vector3(gameObject.transform.localScale.x, gameObject.transform.localScale.y, 1.8f);
            }

            //Initial scale
            gameObject.transform.localScale = HIDING_SCALE;

            //Initial position
            gameObject.transform.position = worldSpacePosition;
        }

        protected override Vector3 ShowingScale()
        {
            return SHOWING_SCALE;
        }
    }
}
