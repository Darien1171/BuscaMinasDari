using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BuscaMinasDari.Models
{
    public class GameBoard : INotifyPropertyChanged
    {
        private int _rows;
        private int _columns;
        private int _mineCount;
        private int _flagCount;
        private int _revealedCount;
        private GameState _gameState = GameState.Playing;
        private DateTime _startTime;
        private string _formattedTime = "00:00";
        private System.Timers.Timer _timer;

        public MinesweeperCell[,] Cells { get; private set; }

        public int Rows => _rows;
        public int Columns => _columns;
        public int MineCount => _mineCount;
        public int RemainingMines => _mineCount - _flagCount;

        public GameState GameState
        {
            get => _gameState;
            private set
            {
                if (_gameState != value)
                {
                    _gameState = value;
                    OnPropertyChanged();
                    if (_gameState != GameState.Playing)
                    {
                        StopTimer();
                    }
                }
            }
        }

        public string FormattedTime
        {
            get => _formattedTime;
            private set
            {
                if (_formattedTime != value)
                {
                    _formattedTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public GameBoard()
        {
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += (s, e) => {
                // Asegurarnos de que actualizamos el tiempo en el hilo de la UI
                MainThread.BeginInvokeOnMainThread(UpdateTime);
            };
        }

        public void Initialize(int rows, int columns, Difficulty difficulty)
        {
            _rows = rows;
            _columns = columns;

            // Establecer cantidad de minas según dificultad
            switch (difficulty)
            {
                case Difficulty.Easy:
                    _mineCount = (int)(rows * columns * 0.1); // 10% minas
                    break;
                case Difficulty.Medium:
                    _mineCount = (int)(rows * columns * 0.15); // 15% minas
                    break;
                case Difficulty.Hard:
                    _mineCount = (int)(rows * columns * 0.2); // 20% minas
                    break;
            }

            _flagCount = 0;
            _revealedCount = 0;
            _gameState = GameState.Playing;

            // Inicializar celdas
            Cells = new MinesweeperCell[rows, columns];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    Cells[r, c] = new MinesweeperCell
                    {
                        Row = r,
                        Column = c,
                        IsRevealed = false,
                        IsFlagged = false,
                        IsMine = false,
                        AdjacentMines = 0
                    };
                }
            }

            // Colocar minas aleatoriamente
            PlaceMines();

            // Calcular minas adyacentes para cada celda
            CalculateAdjacentMines();

            // Reiniciar temporizador
            _startTime = DateTime.Now;
            FormattedTime = "00:00";
            _timer.Start();

            OnPropertyChanged(nameof(RemainingMines));
        }

        private void PlaceMines()
        {
            Random random = new Random();
            int minesPlaced = 0;

            while (minesPlaced < _mineCount)
            {
                int r = random.Next(_rows);
                int c = random.Next(_columns);

                if (!Cells[r, c].IsMine)
                {
                    Cells[r, c].IsMine = true;
                    minesPlaced++;
                }
            }
        }

        private void CalculateAdjacentMines()
        {
            for (int r = 0; r < _rows; r++)
            {
                for (int c = 0; c < _columns; c++)
                {
                    if (Cells[r, c].IsMine)
                        continue;

                    int count = 0;

                    // Verificar las 8 celdas adyacentes
                    for (int dr = -1; dr <= 1; dr++)
                    {
                        for (int dc = -1; dc <= 1; dc++)
                        {
                            if (dr == 0 && dc == 0)
                                continue;

                            int nr = r + dr;
                            int nc = c + dc;

                            if (nr >= 0 && nr < _rows && nc >= 0 && nc < _columns && Cells[nr, nc].IsMine)
                            {
                                count++;
                            }
                        }
                    }

                    Cells[r, c].AdjacentMines = count;
                }
            }
        }

        public void RevealCell(int row, int column)
        {
            System.Diagnostics.Debug.WriteLine($"RevealCell llamado: Fila {row}, Columna {column}");

            // Ignorar si el juego terminó o la celda ya está revelada o marcada
            if (GameState != GameState.Playing ||
                Cells[row, column].IsRevealed ||
                Cells[row, column].IsFlagged)
            {
                System.Diagnostics.Debug.WriteLine("RevealCell: Acción ignorada (juego terminado o celda ya revelada/marcada)");
                return;
            }

            // Revelar la celda
            Cells[row, column].IsRevealed = true;
            _revealedCount++;
            System.Diagnostics.Debug.WriteLine($"Celda revelada. Es mina: {Cells[row, column].IsMine}, Minas adyacentes: {Cells[row, column].AdjacentMines}");

            // Verificar si es una mina
            if (Cells[row, column].IsMine)
            {
                System.Diagnostics.Debug.WriteLine("¡Es una mina! Fin del juego.");
                // Fin del juego - revelar todas las minas
                RevealAllMines();
                GameState = GameState.Lost;
                return;
            }

            // Si es una celda sin minas adyacentes, revelar celdas adyacentes recursivamente
            if (Cells[row, column].AdjacentMines == 0)
            {
                System.Diagnostics.Debug.WriteLine("Celda sin minas adyacentes. Revelando adyacentes...");
                RevealAdjacentCells(row, column);
            }

            // Verificar si el jugador ha ganado
            CheckWinCondition();
        }

        private void RevealAdjacentCells(int row, int column)
        {
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0)
                        continue;

                    int nr = row + dr;
                    int nc = column + dc;

                    if (nr >= 0 && nr < _rows && nc >= 0 && nc < _columns &&
                        !Cells[nr, nc].IsRevealed && !Cells[nr, nc].IsFlagged)
                    {
                        RevealCell(nr, nc);
                    }
                }
            }
        }

        public void ToggleFlag(int row, int column)
        {
            System.Diagnostics.Debug.WriteLine($"ToggleFlag llamado: Fila {row}, Columna {column}");

            // Ignorar si el juego terminó o la celda ya está revelada
            if (GameState != GameState.Playing || Cells[row, column].IsRevealed)
            {
                System.Diagnostics.Debug.WriteLine("ToggleFlag: Acción ignorada (juego terminado o celda ya revelada)");
                return;
            }

            // Alternar bandera
            Cells[row, column].IsFlagged = !Cells[row, column].IsFlagged;

            // Actualizar contador de banderas
            if (Cells[row, column].IsFlagged)
                _flagCount++;
            else
                _flagCount--;

            System.Diagnostics.Debug.WriteLine($"Bandera alternada. Estado actual: {Cells[row, column].IsFlagged}. Banderas colocadas: {_flagCount}");

            OnPropertyChanged(nameof(RemainingMines));

            // Verificar si el jugador ha ganado
            CheckWinCondition();
        }

        private void RevealAllMines()
        {
            for (int r = 0; r < _rows; r++)
            {
                for (int c = 0; c < _columns; c++)
                {
                    if (Cells[r, c].IsMine)
                    {
                        Cells[r, c].IsRevealed = true;
                    }
                }
            }
        }

        private void CheckWinCondition()
        {
            // Condiciones de victoria:
            // 1. Todas las celdas sin mina están reveladas, O
            // 2. Todas las celdas con mina están marcadas y ninguna celda sin mina está marcada

            // Verificar condición 1
            if (_revealedCount == (_rows * _columns) - _mineCount)
            {
                System.Diagnostics.Debug.WriteLine("¡Victoria! Todas las celdas sin mina reveladas.");
                GameState = GameState.Won;
                return;
            }

            // Verificar condición 2
            bool allMinesFlagged = true;
            bool anyNonMineFlagged = false;

            for (int r = 0; r < _rows; r++)
            {
                for (int c = 0; c < _columns; c++)
                {
                    if (Cells[r, c].IsMine && !Cells[r, c].IsFlagged)
                    {
                        allMinesFlagged = false;
                    }

                    if (!Cells[r, c].IsMine && Cells[r, c].IsFlagged)
                    {
                        anyNonMineFlagged = true;
                    }
                }
            }

            if (allMinesFlagged && !anyNonMineFlagged)
            {
                System.Diagnostics.Debug.WriteLine("¡Victoria! Todas las minas correctamente marcadas.");
                GameState = GameState.Won;
            }
        }

        private void UpdateTime()
        {
            if (GameState == GameState.Playing)
            {
                TimeSpan elapsed = DateTime.Now - _startTime;
                FormattedTime = $"{elapsed.Minutes:00}:{elapsed.Seconds:00}";
            }
        }

        private void StopTimer()
        {
            _timer.Stop();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}