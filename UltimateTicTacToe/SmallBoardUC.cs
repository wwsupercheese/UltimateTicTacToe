namespace UltimateTicTacToe
{
    // Пользовательский элемент управления для малой доски
    internal class SmallBoardUC : UserControl
    {
        public int BoardRow { get; }     // Позиция доски в основной сетке
        public int BoardCol { get; }
        private readonly Button[,] cells = new Button[3, 3];
        private char winner = '\0';      // Победитель в текущей доске

        public event EventHandler<CellClickedEventArgs> CellClicked;

        public SmallBoardUC(int boardRow, int boardCol)
        {
            BoardRow = boardRow;
            BoardCol = boardCol;
            InitializeBoard();
        }
        // Выключение и включение кнопок
        public void SetCellsEnabled(bool enabled)
        {
            foreach (Button btn in cells)
            {
                btn.Enabled = enabled;
            }
        }
        // Инициализация интерфейса малой доски
        private void InitializeBoard()
        {
            Size = new Size(150, 150);
            BackColor = Color.White;

            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    cells[row, col] = new Button
                    {
                        Size = new Size(50, 50),
                        Location = new Point(col * 50, row * 50),
                        Tag = new Point(row, col),
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Arial", 16),
                        //BackColor = Color.White,
                        TabStop = false,
                    };
                    cells[row, col].Click += Cell_Click;
                    Controls.Add(cells[row, col]);
                }
            }
        }

        // Обновление состояния доступности клеток
        public void UpdateCellsAccessibility(bool isActive, Func<Point, bool> isCellAvailable)
        {
            if (winner != '\0')
            {
                return; // Не обновляем выигранные доски
            }

            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    Button btn = cells[row, col];
                    Point pos = new(row, col);
                    bool available = isActive && isCellAvailable(pos);
                    btn.Enabled = available;
                    btn.BackColor = available ? Color.White : Color.LightGray;
                }
            }
        }

        // Обработчик клика по клетке
        private void Cell_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            Point pos = (Point)btn.Tag;
            CellClicked?.Invoke(this, new CellClickedEventArgs(
                new Point(BoardRow, BoardCol), pos));
        }

        // Обновление клетки после хода
        public void UpdateCell(int row, int col, string symbol)
        {
            cells[row, col].Text = symbol;
            cells[row, col].Enabled = false;
            cells[row, col].BackColor = Color.White;
        }

        // Пометка доски как выигранной
        public void MarkAsWon(char winner)
        {
            this.winner = winner;
            BackColor = winner == 'X' ? Color.LightBlue : Color.LightPink;
            foreach (Button btn in cells)
            {
                btn.Enabled = false;
                btn.BackColor = BackColor;
            }
        }
        public void PerformCellClick(Point cellPos)
        {
            if (cells[cellPos.X, cellPos.Y].Enabled)
            {
                cells[cellPos.X, cellPos.Y].PerformClick();
            }
        }
    }
}
