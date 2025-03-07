using BuscaMinasDari.Models;

namespace BuscaMinasDari.Views
{
    public partial class CellView : ContentView
    {
        private static readonly Dictionary<int, Color> AdjacentColorMap = new()
        {
            { 1, Colors.Blue },
            { 2, Colors.Green },
            { 3, Colors.Red },
            { 4, Colors.DarkBlue },
            { 5, Colors.Brown },
            { 6, Colors.Cyan },
            { 7, Colors.Black },
            { 8, Colors.DarkGray }
        };

        public static readonly BindableProperty CellProperty =
            BindableProperty.Create(nameof(Cell), typeof(MinesweeperCell), typeof(CellView), null, propertyChanged: OnCellChanged);

        public MinesweeperCell? Cell
        {
            get => (MinesweeperCell?)GetValue(CellProperty);
            set => SetValue(CellProperty, value);
        }

        public CellView()
        {
            InitializeComponent();
        }

        private static void OnCellChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is CellView view && newValue is MinesweeperCell cell)
            {
                view.UpdateCellVisual(cell);

                // Subscribe to property changes on the cell
                cell.PropertyChanged += (s, e) =>
                {
                    view.UpdateCellVisual(cell);
                };
            }
        }

        public void UpdateCellVisual(MinesweeperCell? cell)
        {
            if (cell == null)
                return;

            System.Diagnostics.Debug.WriteLine($"Actualizando celda visual: Fila {cell.Row}, Columna {cell.Column}, Revelada: {cell.IsRevealed}, Marcada: {cell.IsFlagged}, Mina: {cell.IsMine}");

            if (cell.IsRevealed)
            {
                // Cell is revealed
                CellFrame.BackgroundColor = Colors.LightGray;

                if (cell.IsMine)
                {
                    // Show mine
                    ContentLabel.IsVisible = false;
                    FlagImage.IsVisible = false;
                    MineImage.IsVisible = true;
                    CellFrame.BackgroundColor = Colors.Red;
                }
                else
                {
                    // Show number if there are adjacent mines
                    MineImage.IsVisible = false;
                    FlagImage.IsVisible = false;

                    if (cell.AdjacentMines > 0)
                    {
                        ContentLabel.Text = cell.AdjacentMines.ToString();

                        if (AdjacentColorMap.ContainsKey(cell.AdjacentMines))
                        {
                            ContentLabel.TextColor = AdjacentColorMap[cell.AdjacentMines];
                        }
                        else
                        {
                            ContentLabel.TextColor = Colors.Black;
                        }

                        ContentLabel.IsVisible = true;
                    }
                    else
                    {
                        ContentLabel.IsVisible = false;
                    }
                }
            }
            else if (cell.IsFlagged)
            {
                // Cell is flagged
                CellFrame.BackgroundColor = Color.FromArgb("#E0E0E0");

                ContentLabel.IsVisible = false;
                MineImage.IsVisible = false;
                FlagImage.IsVisible = true;
            }
            else
            {
                // Unrevealed cell
                CellFrame.BackgroundColor = Color.FromArgb("#CECECE");

                ContentLabel.IsVisible = false;
                MineImage.IsVisible = false;
                FlagImage.IsVisible = false;
            }
        }
    }
}