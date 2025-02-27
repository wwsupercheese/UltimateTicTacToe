using System;
using System.Drawing;
using System.Windows.Forms;

namespace UltimateTicTacToe
{
    public partial class MainForm : Form
    {
        private UltimateBoard board;         // Основная игровая доска
        private bool isPlayerX = true;       // Текущий игрок (X начинает)
        private Point currentActiveBoard;    // Активная доска (-1 - любая)
        private Panel mainPanel;             // Панель для отрисовки досок
        private Label statusLabel;           // Метка статуса игры
        private Button btnNewGame;           // Кнопка новой игры

        public MainForm()
        {
            InitializeComponents();
            InitializeGame();
        }

        // Инициализация компонентов интерфейса
        private void InitializeComponents()
        {
            // Настройка основной формы
            ClientSize = new Size(490, 540);
            Text = "Ultimate Tic-Tac-Toe";
            FormBorderStyle = FormBorderStyle.FixedSingle;

            // Панель игрового поля
            mainPanel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(470, 470),
                BackColor = Color.White
            };

            // Метка статуса
            statusLabel = new Label
            {
                AutoSize = true,
                Location = new Point(10, 500),
                Font = new Font("Arial", 12)
            };

            // Кнопка новой игры
            btnNewGame = new Button
            {
                Text = "New Game",
                Location = new Point(400, 500),
                Size = new Size(80, 30)
            };
            btnNewGame.Click += BtnNewGame_Click;

            Controls.AddRange([mainPanel, statusLabel, btnNewGame]);
        }

        // Инициализация новой игры
        private void InitializeGame()
        {
            board = new UltimateBoard();
            CreateBoardUI();
            currentActiveBoard = new Point(-1, -1);
            isPlayerX = true;
            UpdateStatusLabel();
            HighlightActiveBoard();
        }

        // Создание пользовательского интерфейса игрового поля
        private void CreateBoardUI()
        {
            mainPanel.Controls.Clear();
            for (int bigRow = 0; bigRow < 3; bigRow++)
            {
                for (int bigCol = 0; bigCol < 3; bigCol++)
                {
                    var smallBoard = new SmallBoardUC(bigRow, bigCol);
                    smallBoard.Location = new Point(bigCol * 160, bigRow * 160);
                    smallBoard.CellClicked += SmallBoard_CellClicked;
                    mainPanel.Controls.Add(smallBoard);
                }
            }
        }

        // Обработчик клика по клетке малой доски
        private void SmallBoard_CellClicked(object sender, CellClickedEventArgs e)
        {
            var smallBoard = (SmallBoardUC)sender;
            Point bigPosition = new Point(smallBoard.BoardRow, smallBoard.BoardCol);

            if (IsMoveValid(bigPosition, e.SmallPosition))
            {
                MakeMove(bigPosition, e.SmallPosition);
                UpdateGameState(bigPosition, e.SmallPosition);
                CheckGlobalWinner();
            }
        }

        // Проверка допустимости хода
        private bool IsMoveValid(Point bigPos, Point smallPos)
        {
            // Активная доска не выбрана или выбрана правильная доска и клетка доступна
            return (currentActiveBoard.X == -1) ||
                   (bigPos == currentActiveBoard && board.CanMakeMove(bigPos, smallPos));
        }

        // Совершение хода на доске
        private void MakeMove(Point bigPos, Point smallPos)
        {
            board.MakeMove(bigPos, smallPos, isPlayerX ? 'X' : 'O');
            UpdateCellUI(bigPos, smallPos);
        }

        // Обновление интерфейса клетки после хода
        private void UpdateCellUI(Point bigPos, Point smallPos)
        {
            foreach (Control control in mainPanel.Controls)
            {
                if (control is SmallBoardUC sb &&
                    sb.BoardRow == bigPos.X &&
                    sb.BoardCol == bigPos.Y)
                {
                    sb.UpdateCell(smallPos.X, smallPos.Y, isPlayerX ? "X" : "O");
                    break;
                }
            }
        }

        // Обновление состояния игры после хода
        private void UpdateGameState(Point bigPos, Point smallPos)
        {
            // Проверка победы в малой доске
            if (board.CheckSmallBoardWinner(bigPos))
                MarkBoardAsWon(bigPos);

            // Определение следующей активной доски
            currentActiveBoard = board.IsBoardAvailable(smallPos) ? smallPos : new Point(-1);

            HighlightActiveBoard(); // Обновление подсветки
            isPlayerX = !isPlayerX; // Смена игрока
            UpdateStatusLabel(); // Обновление статуса
        }

        // Пометка доски как выигранной
        private void MarkBoardAsWon(Point bigPos)
        {
            foreach (Control control in mainPanel.Controls)
            {
                if (control is SmallBoardUC sb && sb.BoardRow == bigPos.X && sb.BoardCol == bigPos.Y)
                {
                    sb.MarkAsWon(board.GetSmallBoardWinner(bigPos));
                    break;
                }
            }
        }

        // Подсветка активных досок
        private void HighlightActiveBoard()
        {
            foreach (Control control in mainPanel.Controls)
            {
                if (control is SmallBoardUC sb)
                {
                    bool isActive = currentActiveBoard.X == -1 ||
                                  (sb.BoardRow == currentActiveBoard.X && sb.BoardCol == currentActiveBoard.Y);

                    // Обновление доступности клеток
                    sb.UpdateCellsAccessibility(
                        isActive,
                        isActive ? (pos => board.CanMakeMove(new Point(sb.BoardRow, sb.BoardCol), pos)) : pos => false
                    );
                }
            }
        }

        // Проверка глобальной победы
        private void CheckGlobalWinner()
        {
            char winner = board.CheckGlobalWinner();
            if (winner != '\0')
            {
                MessageBox.Show($"Player {winner} wins!");
                InitializeGame();
            }
            else if (board.IsFull())
            {
                MessageBox.Show("Game ended in a draw!");
                InitializeGame();
            }
        }

        // Обновление текста статуса
        private void UpdateStatusLabel()
        {
            statusLabel.Text = $"Player: {(isPlayerX ? "X" : "O")}";
            statusLabel.Text += currentActiveBoard.X == -1 ?
                " - Can play anywhere" :
                $" - Must play in board ({currentActiveBoard.X + 1}, {currentActiveBoard.Y + 1})";
        }

        // Обработчик новой игры
        private void BtnNewGame_Click(object sender, EventArgs e) => InitializeGame();

    }

    // Класс основной игровой доски (9x9)
    class UltimateBoard
    {
        private readonly SmallBoard[,] boards = new SmallBoard[3, 3];

        public UltimateBoard()
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    boards[i, j] = new SmallBoard();
        }

        // Проверка доступности клетки
        public bool CanMakeMove(Point bigPos, Point smallPos) =>
            boards[bigPos.X, bigPos.Y].IsCellAvailable(smallPos.X, smallPos.Y);

        // Совершение хода
        public void MakeMove(Point bigPos, Point smallPos, char player) =>
            boards[bigPos.X, bigPos.Y].SetCell(smallPos.X, smallPos.Y, player);

        // Проверка победы в малой доске
        public bool CheckSmallBoardWinner(Point bigPos) =>
            boards[bigPos.X, bigPos.Y].Winner != '\0';

        // Получение победителя малой доски
        public char GetSmallBoardWinner(Point bigPos) =>
            boards[bigPos.X, bigPos.Y].Winner;

        // Проверка доступности доски для ходов
        public bool IsBoardAvailable(Point boardPos) =>
            boards[boardPos.X, boardPos.Y].Winner == '\0' &&
            !boards[boardPos.X, boardPos.Y].IsFull();

        // Проверка глобальной победы
        public char CheckGlobalWinner()
        {
            // Проверка строк
            for (int row = 0; row < 3; row++)
                if (CheckTriplet(boards[row, 0].Winner, boards[row, 1].Winner, boards[row, 2].Winner))
                    return boards[row, 0].Winner;

            // Проверка столбцов
            for (int col = 0; col < 3; col++)
                if (CheckTriplet(boards[0, col].Winner, boards[1, col].Winner, boards[2, col].Winner))
                    return boards[0, col].Winner;

            // Проверка диагоналей
            if (CheckTriplet(boards[0, 0].Winner, boards[1, 1].Winner, boards[2, 2].Winner))
                return boards[0, 0].Winner;

            if (CheckTriplet(boards[0, 2].Winner, boards[1, 1].Winner, boards[2, 0].Winner))
                return boards[0, 2].Winner;

            return '\0';
        }

        // Проверка трех одинаковых символов
        private bool CheckTriplet(char a, char b, char c) =>
            a != '\0' && a == b && b == c;

        // Проверка заполненности всех досок
        public bool IsFull()
        {
            foreach (var board in boards)
                if (!board.IsFull()) return false;
            return true;
        }
    }

    // Класс малой доски (3x3)
    class SmallBoard
    {
        public char[,] Cells = new char[3, 3];
        public char Winner { get; private set; } = '\0';

        // Проверка доступности клетки
        public bool IsCellAvailable(int row, int col) =>
            Cells[row, col] == '\0' && Winner == '\0';

        // Установка символа в клетку
        public void SetCell(int row, int col, char player)
        {
            Cells[row, col] = player;
            CheckWinner();
        }

        // Проверка победы в доске
        private void CheckWinner()
        {
            // Проверка строк и столбцов
            for (int i = 0; i < 3; i++)
            {
                if (CheckLine(Cells[i, 0], Cells[i, 1], Cells[i, 2]) ||
                    CheckLine(Cells[0, i], Cells[1, i], Cells[2, i]))
                    return;
            }

            // Проверка диагоналей
            if (CheckLine(Cells[0, 0], Cells[1, 1], Cells[2, 2]) ||
                CheckLine(Cells[0, 2], Cells[1, 1], Cells[2, 0]))
                return;
        }

        // Проверка линии из трех одинаковых символов
        private bool CheckLine(char a, char b, char c)
        {
            if (a != '\0' && a == b && b == c)
            {
                Winner = a;
                return true;
            }
            return false;
        }

        // Проверка заполненности доски
        public bool IsFull()
        {
            foreach (char cell in Cells)
                if (cell == '\0') return false;
            return true;
        }
    }

    // Пользовательский элемент управления для малой доски
    class SmallBoardUC : UserControl
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
            if (winner != '\0') return; // Не обновляем выигранные доски

            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    var btn = cells[row, col];
                    var pos = new Point(row, col);
                    bool available = isActive && isCellAvailable(pos);
                    btn.Enabled = available;
                    btn.BackColor = available ? Color.White : Color.LightGray;
                }
            }
        }

        // Обработчик клика по клетке
        private void Cell_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            var pos = (Point)btn.Tag;
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
    }
    
    // Аргументы события клика по клетке
    class CellClickedEventArgs : EventArgs
    {
        public Point BigPosition { get; } // Позиция малой доски в основной
        public Point SmallPosition { get; } // Позиция клетки в малой доске

        public CellClickedEventArgs(Point bigPos, Point smallPos)
        {
            BigPosition = bigPos;
            SmallPosition = smallPos;
        }
    }
}