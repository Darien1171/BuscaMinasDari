using Microsoft.Maui.Dispatching;

namespace BuscaMinasDari.Models
{
    public class GameBoard : BindableObject
    {
        private MinesweeperCell[,] _cells = new MinesweeperCell[0, 0]; // Inicializado para evitar null
        private GameState _gameState;
        private int _mineCount;
        private int _flaggedCount;
        private int _revealedCount;
        private int _seconds;
        private bool _isTimerRunning;
        private IDispatcherTimer? _timer; // Marcado como nullable

        public int Rows { get; private set; }
        public int Columns { get; private set; }
        public MinesweeperCell[,] Cells => _cells;

        public GameState GameState
        {
            get => _gameState;
            set
            {
                if (_gameState != value)
                {
                    _gameState = value;
                    OnPropertyChanged();

                    if (value == GameState.Lost || value == GameState.Won)
                    {
                        StopTimer();
                    }
                }
            }
        }

        public int MineCount
        {
            get => _mineCount;
            private set
            {
                if (_mineCount != value)
                {
                    _mineCount = value;
                    OnPropertyChanged();
                }
            }
        }

        public int RemainingMines
        {
            get => _mineCount - _flaggedCount;
        }

        public int Seconds
        {
            get => _seconds;
            private set
            {
                if (_seconds != value)
                {
                    _seconds = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FormattedTime));
                }
            }
        }

        public string FormattedTime => TimeSpan.FromSeconds(Seconds).ToString(@"mm\:ss");

        public GameBoard()
        {
            // Inicializamos con un tablero vacío para evitar excepciones
            Rows = 0;
            Columns = 0;
            _cells = new MinesweeperCell[0, 0];
            _gameState = GameState.NotStarted;
        }

        public void Initialize(int rows, int columns, Difficulty difficulty)
        {
            Rows = rows;
            Columns = columns;
            _cells = new MinesweeperCell[rows, columns];
            _revealedCount = 0;
            _flaggedCount = 0;
            Seconds = 0;

            // Create cells
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    _cells[r, c] = new MinesweeperCell { Row = r, Column = c };
                }
            }

            // Set mines based on difficulty
            int minePercentage = difficulty switch
            {
                Difficulty.Easy => 12,    // 12% of cells have mines
                Difficulty.Medium => 16,  // 16% of cells have mines
                Difficulty.Hard => 20,    // 20% of cells have mines
                _ => 15
            };

            int totalCells = rows * columns;
            MineCount = (int)Math.Ceiling(totalCells * minePercentage / 100.0);

            // Place mines randomly
            Random random = new Random();
            int minesPlaced = 0;

            while (minesPlaced < MineCount)
            {
                int r = random.Next(rows);
                int c = random.Next(columns);

                if (!_cells[r, c].IsMine)
                {
                    _cells[r, c].IsMine = true;
                    minesPlaced++;
                }
            }

            // Calculate adjacent mines for each cell
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    if (!_cells[r, c].IsMine)
                    {
                        _cells[r, c].AdjacentMines = CountAdjacentMines(r, c);
                    }
                }
            }

            GameState = GameState.NotStarted;
            OnPropertyChanged(nameof(RemainingMines));
        }

        private int CountAdjacentMines(int row, int column)
        {
            int count = 0;

            for (int r = Math.Max(0, row - 1); r <= Math.Min(Rows - 1, row + 1); r++)
            {
                for (int c = Math.Max(0, column - 1); c <= Math.Min(Columns - 1, column + 1); c++)
                {
                    if ((r != row || c != column) && _cells[r, c].IsMine)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        public void RevealCell(int row, int column)
        {
            if (GameState == GameState.Lost || GameState == GameState.Won)
                return;

            var cell = _cells[row, column];

            if (cell.IsRevealed || cell.IsFlagged)
                return;

            // Start timer on first reveal
            if (GameState == GameState.NotStarted)
            {
                GameState = GameState.Playing;
                StartTimer();
            }

            // Hit a mine
            if (cell.IsMine)
            {
                RevealAllMines();
                GameState = GameState.Lost;
                return;
            }

            // Reveal the cell
            cell.IsRevealed = true;
            _revealedCount++;

            // Auto-reveal adjacent cells if this is an empty cell
            if (cell.IsEmpty)
            {
                RevealAdjacentCells(row, column);
            }

            // Check for win
            CheckForWin();
        }

        private void RevealAdjacentCells(int row, int column)
        {
            // Reveal all 8 adjacent cells if they are not flagged and not revealed
            for (int r = Math.Max(0, row - 1); r <= Math.Min(Rows - 1, row + 1); r++)
            {
                for (int c = Math.Max(0, column - 1); c <= Math.Min(Columns - 1, column + 1); c++)
                {
                    if (r == row && c == column)
                        continue;

                    var adjacentCell = _cells[r, c];
                    if (!adjacentCell.IsRevealed && !adjacentCell.IsFlagged)
                    {
                        // Recursively reveal
                        RevealCell(r, c);
                    }
                }
            }
        }

        public void ToggleFlag(int row, int column)
        {
            if (GameState == GameState.Lost || GameState == GameState.Won)
                return;

            if (GameState == GameState.NotStarted)
            {
                GameState = GameState.Playing;
                StartTimer();
            }

            var cell = _cells[row, column];

            if (cell.IsRevealed)
                return;

            cell.IsFlagged = !cell.IsFlagged;
            _flaggedCount += cell.IsFlagged ? 1 : -1;
            OnPropertyChanged(nameof(RemainingMines));

            // Check for win
            CheckForWin();
        }

        private void RevealAllMines()
        {
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    if (_cells[r, c].IsMine)
                    {
                        _cells[r, c].IsRevealed = true;
                    }
                }
            }
        }

        private void CheckForWin()
        {
            // Win condition: all non-mine cells are revealed
            if (_revealedCount == (Rows * Columns) - MineCount)
            {
                // Mark all mines as flagged
                for (int r = 0; r < Rows; r++)
                {
                    for (int c = 0; c < Columns; c++)
                    {
                        if (_cells[r, c].IsMine && !_cells[r, c].IsFlagged)
                        {
                            _cells[r, c].IsFlagged = true;
                        }
                    }
                }

                _flaggedCount = MineCount;
                OnPropertyChanged(nameof(RemainingMines));
                GameState = GameState.Won;
            }
        }

        public void StartTimer()
        {
            if (_isTimerRunning)
                return;

            _isTimerRunning = true;
            _timer = Application.Current.Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) => {
                Seconds++;
            };
            _timer.Start();
        }

        public void StopTimer()
        {
            if (!_isTimerRunning || _timer == null)
                return;

            _timer.Stop();
            _isTimerRunning = false;
        }
    }
}