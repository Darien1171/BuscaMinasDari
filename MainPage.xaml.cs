using BuscaMinasDari.Models;
using BuscaMinasDari.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BuscaMinasDari
{
    public partial class MainPage : ContentPage
    {
        private GameBoard _gameBoard;
        private ObservableCollection<MinesweeperCell> _cells;
        private bool _isGameOver;
        private string _gameOverMessage = string.Empty;
        private Color _gameOverColor = Colors.Transparent;
        private DateTime _lastTapTime = DateTime.MinValue;
        private MinesweeperCell? _lastTappedCell = null;

        public GameBoard GameBoard => _gameBoard;
        public ObservableCollection<MinesweeperCell> Cells => _cells;

        public bool IsGameOver
        {
            get => _isGameOver;
            set
            {
                if (_isGameOver != value)
                {
                    _isGameOver = value;
                    OnPropertyChanged();
                }
            }
        }

        public string GameOverMessage
        {
            get => _gameOverMessage;
            set
            {
                if (_gameOverMessage != value)
                {
                    _gameOverMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public Color GameOverColor
        {
            get => _gameOverColor;
            set
            {
                if (_gameOverColor != value)
                {
                    _gameOverColor = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand NewGameCommand { get; private set; }

        public MainPage()
        {
            InitializeComponent();

            _gameBoard = new GameBoard();
            _cells = new ObservableCollection<MinesweeperCell>();

            // Create commands
            NewGameCommand = new Command<Difficulty>(StartNewGame);

            // Set up binding context
            BindingContext = this;

            // Subscribe to game state changes
            _gameBoard.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(GameBoard.GameState))
                {
                    UpdateGameState();
                }
                else if (e.PropertyName == nameof(GameBoard.RemainingMines) ||
                         e.PropertyName == nameof(GameBoard.FormattedTime))
                {
                    // Forzar actualización de la UI para estos valores
                    OnPropertyChanged(nameof(GameBoard));
                }
            };

            // Start a new game with medium difficulty
            StartNewGame(Difficulty.Medium);
        }

        // Manejadores de eventos para los gestos táctiles
        private void OnCellTapped(object sender, TappedEventArgs e)
        {
            if (sender is CellView cellView && cellView.Cell != null)
            {
                var currentTime = DateTime.Now;
                var cell = cellView.Cell;

                // Depuración
                System.Diagnostics.Debug.WriteLine($"Celda tocada: Fila {cell.Row}, Columna {cell.Column}");

                // Si este es el segundo toque en la misma celda en menos de 300ms, lo ignoramos
                // porque probablemente es parte de un doble toque que se manejará en OnCellDoubleTapped
                if (_lastTappedCell == cell && (currentTime - _lastTapTime).TotalMilliseconds < 300)
                {
                    return;
                }

                // Actualizamos la referencia para el próximo toque
                _lastTappedCell = cell;
                _lastTapTime = currentTime;

                // Asegurarse de que la UI muestra el cambio (retraso ligero para evitar colisión con doble toque)
                Dispatcher.Dispatch(async () =>
                {
                    // Pequeño retraso para permitir que OnCellDoubleTapped se dispare si es un doble toque
                    await Task.Delay(50);
                    RevealCell(cell);
                });
            }
        }

        private void OnCellDoubleTapped(object sender, TappedEventArgs e)
        {
            if (sender is CellView cellView && cellView.Cell != null)
            {
                var cell = cellView.Cell;

                // Depuración
                System.Diagnostics.Debug.WriteLine($"Celda doble tocada: Fila {cell.Row}, Columna {cell.Column}");

                // Marcar la celda
                FlagCell(cell);

                // Actualizamos para evitar que se dispare también el evento de toque simple
                _lastTappedCell = null;
            }
        }

        private void StartNewGame(Difficulty difficulty)
        {
            // Initialize board size based on difficulty
            int rows, columns;

            switch (difficulty)
            {
                case Difficulty.Easy:
                    rows = 9;
                    columns = 9;
                    break;
                case Difficulty.Medium:
                    rows = 12;
                    columns = 12;
                    break;
                case Difficulty.Hard:
                    rows = 16;
                    columns = 16;
                    break;
                default:
                    rows = 9;
                    columns = 9;
                    break;
            }

            // Initialize the board
            _gameBoard.Initialize(rows, columns, difficulty);

            // Configurar la vista de colección
            GameCollectionView.ItemsLayout = new GridItemsLayout(columns, ItemsLayoutOrientation.Vertical);

            // Update the cells collection
            _cells.Clear();

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    var cell = _gameBoard.Cells[r, c];
                    _cells.Add(cell);
                }
            }

            // Reset game over state
            IsGameOver = false;
        }

        private void RevealCell(MinesweeperCell? cell)
        {
            if (cell == null)
                return;

            _gameBoard.RevealCell(cell.Row, cell.Column);
        }

        private void FlagCell(MinesweeperCell? cell)
        {
            if (cell == null)
                return;

            _gameBoard.ToggleFlag(cell.Row, cell.Column);
        }

        private void UpdateGameState()
        {
            switch (_gameBoard.GameState)
            {
                case GameState.Won:
                    GameOverColor = Colors.Green;
                    if (Application.Current?.Resources != null &&
                        Application.Current.Resources.TryGetValue("WinGreen", out var winColor))
                    {
                        GameOverColor = (Color)winColor;
                    }

                    GameOverMessage = "¡Has ganado! ¡Buen trabajo!";
                    IsGameOver = true;
                    break;

                case GameState.Lost:
                    GameOverColor = Colors.Red;
                    if (Application.Current?.Resources != null &&
                        Application.Current.Resources.TryGetValue("MineRed", out var loseColor))
                    {
                        GameOverColor = (Color)loseColor;
                    }

                    GameOverMessage = "¡BOOM! Fin del juego.";
                    IsGameOver = true;
                    break;

                default:
                    IsGameOver = false;
                    break;
            }
        }
    }
}