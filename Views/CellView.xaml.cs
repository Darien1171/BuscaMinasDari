using BuscaMinasDari.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BuscaMinasDari.Views
{
    public partial class CellView : ContentView, INotifyPropertyChanged
    {
        // La instancia de la celda
        private MinesweeperCell _cell;

        // Propiedad bindable para la celda
        public static readonly BindableProperty CellProperty =
            BindableProperty.Create(nameof(Cell), typeof(MinesweeperCell), typeof(CellView), null,
                propertyChanged: OnCellChanged);

        public MinesweeperCell Cell
        {
            get => _cell;
            set
            {
                if (_cell != value)
                {
                    // Desuscribirse del evento del objeto antiguo si existe
                    if (_cell != null)
                    {
                        _cell.PropertyChanged -= OnModelPropertyChanged;
                    }

                    _cell = value;

                    // Suscribirse al evento del nuevo objeto
                    if (_cell != null)
                    {
                        _cell.PropertyChanged += OnModelPropertyChanged;
                    }

                    OnPropertyChanged();
                    UpdateCellVisuals();
                }
            }
        }

        // Constructor
        public CellView()
        {
            InitializeComponent();
        }

        // Handler para cambios en la propiedad Cell
        private static void OnCellChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var cellView = (CellView)bindable;
            cellView.Cell = (MinesweeperCell)newValue;
        }

        // Handler para cambios en las propiedades del modelo
        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Actualizar el aspecto visual cuando cambia cualquier propiedad de la celda
            UpdateCellVisuals();
        }

        // Actualiza los elementos visuales según el estado de la celda
        private void UpdateCellVisuals()
        {
            if (_cell == null) return;

            MainThread.BeginInvokeOnMainThread(() => {
                // Actualizar el fondo del Border (ahora usando el nombre específico)
                CellBorder.BackgroundColor = _cell.IsRevealed
                    ? Application.Current.RequestedTheme == AppTheme.Dark
                        ? (Color)Application.Current.Resources["CellRevealedDark"]
                        : (Color)Application.Current.Resources["CellRevealed"]
                    : Application.Current.RequestedTheme == AppTheme.Dark
                        ? (Color)Application.Current.Resources["CellUnrevealedDark"]
                        : (Color)Application.Current.Resources["CellUnrevealed"];

                // Actualizar el número
                if (_cell.IsRevealed && !_cell.IsMine && _cell.AdjacentMines > 0)
                {
                    NumberLabel.Text = _cell.AdjacentMines.ToString();
                    NumberLabel.TextColor = GetNumberColor(_cell.AdjacentMines);
                    NumberLabel.IsVisible = true;
                }
                else
                {
                    NumberLabel.IsVisible = false;
                }

                // Actualizar la mina
                MineImage.IsVisible = _cell.IsRevealed && _cell.IsMine;

                // Actualizar la bandera
                FlagImage.IsVisible = _cell.IsFlagged && !_cell.IsRevealed;
            });
        }

        // Obtener el color según el número de minas adyacentes
        private Color GetNumberColor(int number)
        {
            return number switch
            {
                1 => (Color)Application.Current.Resources["Number1"],
                2 => (Color)Application.Current.Resources["Number2"],
                3 => (Color)Application.Current.Resources["Number3"],
                4 => (Color)Application.Current.Resources["Number4"],
                5 => (Color)Application.Current.Resources["Number5"],
                6 => (Color)Application.Current.Resources["Number6"],
                7 => (Color)Application.Current.Resources["Number7"],
                8 => (Color)Application.Current.Resources["Number8"],
                _ => Colors.Black
            };
        }

        // Implementación de INotifyPropertyChanged
        public new event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}