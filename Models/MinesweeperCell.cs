using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BuscaMinasDari.Models
{
    public class MinesweeperCell : INotifyPropertyChanged
    {
        private bool _isRevealed;
        private bool _isFlagged;
        private bool _isMine;
        private int _adjacentMines;

        public int Row { get; set; }
        public int Column { get; set; }

        public bool IsRevealed
        {
            get => _isRevealed;
            set
            {
                if (_isRevealed != value)
                {
                    _isRevealed = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ShowNumber));
                    OnPropertyChanged(nameof(ShowMine));
                }
            }
        }

        public bool IsFlagged
        {
            get => _isFlagged;
            set
            {
                if (_isFlagged != value)
                {
                    _isFlagged = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsMine
        {
            get => _isMine;
            set
            {
                if (_isMine != value)
                {
                    _isMine = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ShowMine));
                }
            }
        }

        public int AdjacentMines
        {
            get => _adjacentMines;
            set
            {
                if (_adjacentMines != value)
                {
                    _adjacentMines = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ShowNumber));
                }
            }
        }

        // Propiedades calculadas para la UI
        public bool ShowNumber => IsRevealed && !IsMine && AdjacentMines > 0;
        public bool ShowMine => IsRevealed && IsMine;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}