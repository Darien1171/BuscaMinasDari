namespace BuscaMinasDari.Models
{
    public class MinesweeperCell : BindableObject
    {
        private bool _isMine;
        private bool _isRevealed;
        private bool _isFlagged;
        private int _adjacentMines;

        public int Row { get; set; }
        public int Column { get; set; }

        public bool IsMine
        {
            get => _isMine;
            set
            {
                if (_isMine != value)
                {
                    _isMine = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsRevealed
        {
            get => _isRevealed;
            set
            {
                if (_isRevealed != value)
                {
                    _isRevealed = value;
                    OnPropertyChanged();
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

        public int AdjacentMines
        {
            get => _adjacentMines;
            set
            {
                if (_adjacentMines != value)
                {
                    _adjacentMines = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsEmpty => !IsMine && AdjacentMines == 0;
    }
}