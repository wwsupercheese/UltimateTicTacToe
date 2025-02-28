using System;
using System.Drawing;
using System.Windows.Forms;

namespace UltimateTicTacToe
{
    public partial class MainForm : Form
    {
        private UltimateBoard board;         // �������� ������� �����
        private bool isPlayerX = true;       // ������� ����� (X ��������)
        private Point currentActiveBoard;    // �������� ����� (-1 - �����)
        private Panel mainPanel;             // ������ ��� ��������� �����
        private Label statusLabel;           // ����� ������� ����
        private Button btnNewGame;           // ������ ����� ����
        private Button btnHelp;              // ������ �������
        private NumericUpDown nudDepth;      
        private NumericUpDown nudAlpha;
        private Label lblDepth;
        private Label lblAlpha;

        private GameBot _bot;
        public bool _botMode;
        private Button _btnVsBot;
        public bool _botPlayerX;
        private CancellationTokenSource _botTokenSource;
        public MainForm()
        {
            InitializeComponents();
            InitializeGame();
            _bot = new GameBot();
        }

        // ������������� ����������� ����������
        private void InitializeComponents()
        {
            // ��������� �������� �����
            ClientSize = new Size(490, 570);
            Text = "Ultimate Tic-Tac-Toe";
            FormBorderStyle = FormBorderStyle.FixedSingle;

            // ������ �������� ����
            mainPanel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(470, 470),
                BackColor = Color.White
            };

            // ����� �������
            statusLabel = new Label
            {
                AutoSize = true,
                Location = new Point(10, 485),
                Font = new Font("Arial", 12)
            };

            // ������ ����� ����
            btnNewGame = new Button
            {
                Text = "New Game",
                Location = new Point(400, 530),
                Size = new Size(80, 30)
            };
            btnNewGame.Click += BtnNewGame_Click;

            // ������ ��������� ����
            _btnVsBot = new Button
            {
                Text = "VS Bot",
                Location = new Point(220, 530),
                Size = new Size(80, 30)
            };
            _btnVsBot.Click += BtnVsBot_Click;

            
            // ������ �������
            btnHelp = new Button
            {
                Text = "Help",
                Location = new Point(310, 530),
                Size = new Size(80, 30),
                BackColor = Color.LightGoldenrodYellow,
                FlatStyle = FlatStyle.Popup
            };
            btnHelp.Click += BtnHelp_Click;

            // �������� ���������� �����
            lblDepth = new Label
            {
                Text = "������� ������:",
                Location = new Point(10, 510),
                AutoSize = true
            };

            nudDepth = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 5,
                Value = 3,
                Location = new Point(160, 510),
                Size = new Size(50, 20)
            };

            lblAlpha = new Label
            {
                Text = "����������� ��������:",
                Location = new Point(10, 540),
                AutoSize = true
            };

            nudAlpha = new NumericUpDown
            {
                Minimum = 0.1M,
                Maximum = 1.0M,
                Increment = 0.1M,
                DecimalPlaces = 1,
                Value = 0.5M,
                Location = new Point(160, 540),
                Size = new Size(50, 20)
            };

            // ��������� �������� �� �����
      
            Controls.AddRange([lblDepth, nudDepth, lblAlpha, nudAlpha]);

            Controls.AddRange([mainPanel, statusLabel, btnNewGame, _btnVsBot, btnHelp]);

        }
        private void BtnHelp_Click(object sender, EventArgs e)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "help.txt");

            try
            {
                  string filetext = File.ReadAllText(filePath);

 
                var infoForm = new Form
                {
                    Text = "������� �� ����",
                    Size = new Size(450, 500),
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    StartPosition = FormStartPosition.CenterParent
                };

                var textBox = new TextBox
                {
                    Text = filetext,
                    Multiline = true,
                    ReadOnly = true,
                    Dock = DockStyle.Fill,
                    Font = new Font("Arial", 10),
                    BackColor = Color.AliceBlue,
                    ScrollBars = ScrollBars.Vertical
                };

                infoForm.Controls.Add(textBox);
                infoForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(filePath + " was not found!");
            }                
        }

        // ���������� ������
        private void SetCellsEnabled(bool enabled)
        {
            foreach (Control control in mainPanel.Controls)
            {
                if (control is SmallBoardUC smallBoard)
                {
                    smallBoard.SetCellsEnabled(enabled);
                }
            }
        }

        private void UpdateBotTools(bool botmode)
        {
            nudAlpha.Enabled = !_botMode;
            nudAlpha.BackColor = _botMode ? Color.LightGray : Color.White;
            nudDepth.Enabled = !_botMode;
            nudDepth.BackColor = _botMode ? Color.LightGray : Color.White;
        }
        private async void BtnVsBot_Click(object sender, EventArgs e)
        {
            _botMode = !_botMode;
            UpdateBotTools(_botMode);

            if (_botMode)
            {
                _botTokenSource = new CancellationTokenSource();
                try
                {
                    int depth = (int)nudDepth.Value;
                    double alpha = (double)nudAlpha.Value;
                    _botPlayerX = isPlayerX;
                    _bot.setGameBot(isPlayerX, depth, alpha);
                    await MakeBotMoveAsync(_botTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    // ���������� ������
                }
            }
            else
            {
                _botTokenSource?.Cancel();
            }
        }
        // ������������� ����� ����
        private void InitializeGame()
        {
            _botTokenSource?.Cancel();
            board = new UltimateBoard();
            CreateBoardUI();
            currentActiveBoard = new Point(-1, -1);
            isPlayerX = true;
            _botMode = false;
            UpdateBotTools(false);
            UpdateStatusLabel();
            HighlightActiveBoard();
        }

        // �������� ����������������� ���������� �������� ����
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

        // ���������� ����� �� ������ ����� �����
        private async void SmallBoard_CellClicked(object sender, CellClickedEventArgs e)
        {
            var smallBoard = (SmallBoardUC)sender;
            Point bigPosition = new Point(smallBoard.BoardRow, smallBoard.BoardCol);

            if (IsMoveValid(bigPosition, e.SmallPosition))
            {
                MakeMove(bigPosition, e.SmallPosition);
                UpdateGameState(bigPosition, e.SmallPosition);
                CheckGlobalWinner();
            }

            if (_botMode && (_botPlayerX == isPlayerX))
            {
                await MakeBotMoveAsync(_botTokenSource.Token);
            }
        }

        private async Task MakeBotMoveAsync(CancellationToken token)
        {
            if (!_botMode || (_botPlayerX != isPlayerX)) return;

            try
            {
                // ��������� ��� ������, ����� ������������� ���� ������
                SetCellsEnabled(false);
                statusLabel.Text = "Bot is thinking...";

                // ����� ����� �������� �������� ��� �������� "�������" ����, ���� ����������
                await Task.Delay(1000); // ����������� �������� ��� ���������� UI

                var move = await Task.Run(() =>
                {
                    token.ThrowIfCancellationRequested();
                    return _bot.GetBotMove(board, currentActiveBoard);
                }, token);

                // ��������� UI � �������� ������
                this.Invoke((Action)(() =>
                {
                    SetCellsEnabled(true);
                    var targetBoard = FindBoardControl(move.board);
                    targetBoard?.PerformCellClick(move.cell);
                    string player = _botPlayerX ? "O" : "X";
                    statusLabel.Text = "You play " + player; // ���������� ������
                }));
            }
            catch (Exception ex) when (ex is OperationCanceledException || ex is InvalidOperationException)
            {
                _botMode = false;
                UpdateBotTools(false);
                this.Invoke((Action)(() => statusLabel.Text = ""));
            }
        }

        private SmallBoardUC FindBoardControl(Point boardPos)
        {
            foreach (Control control in mainPanel.Controls)
            {
                if (control is SmallBoardUC sb &&
                    sb.BoardRow == boardPos.X &&
                    sb.BoardCol == boardPos.Y)
                    return sb;
            }
            return null;
        }
        // �������� ������������ ����
        private bool IsMoveValid(Point bigPos, Point smallPos)
        {
            // �������� ����� �� ������� ��� ������� ���������� ����� � ������ ��������
            return (currentActiveBoard.X == -1) ||
                   (bigPos == currentActiveBoard && board.CanMakeMove(bigPos, smallPos));
        }

        // ���������� ���� �� �����
        private void MakeMove(Point bigPos, Point smallPos)
        {
            board.MakeMove(bigPos, smallPos, isPlayerX ? 'X' : 'O');
            UpdateCellUI(bigPos, smallPos);
        }

        // ���������� ���������� ������ ����� ����
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

        // ���������� ��������� ���� ����� ����
        private void UpdateGameState(Point bigPos, Point smallPos)
        {
            // �������� ������ � ����� �����
            if (board.CheckSmallBoardWinner(bigPos))
                MarkBoardAsWon(bigPos);

            // ����������� ��������� �������� �����
            currentActiveBoard = board.IsBoardAvailable(smallPos) ? smallPos : new Point(-1);

            HighlightActiveBoard(); // ���������� ���������
            isPlayerX = !isPlayerX; // ����� ������
            UpdateStatusLabel(); // ���������� �������
        }

        // ������� ����� ��� ����������
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

        // ��������� �������� �����
        private void HighlightActiveBoard()
        {
            foreach (Control control in mainPanel.Controls)
            {
                if (control is SmallBoardUC sb)
                {
                    bool isActive = currentActiveBoard.X == -1 ||
                                  (sb.BoardRow == currentActiveBoard.X && sb.BoardCol == currentActiveBoard.Y);

                    // ���������� ����������� ������
                    sb.UpdateCellsAccessibility(
                        isActive,
                        isActive ? (pos => board.CanMakeMove(new Point(sb.BoardRow, sb.BoardCol), pos)) : pos => false
                    );
                }
            }
        }

        // �������� ���������� ������
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

        // ���������� ������ �������
        private void UpdateStatusLabel()
        {
            statusLabel.Text = $"Player: {(isPlayerX ? "X" : "O")}";
            statusLabel.Text += currentActiveBoard.X == -1 ?
                " - Can play anywhere" :
                $" - Must play in board ({currentActiveBoard.X + 1}, {currentActiveBoard.Y + 1})";
        }

        // ���������� ����� ����
        private void BtnNewGame_Click(object sender, EventArgs e) => InitializeGame();
    }

    // ����� �������� ������� ����� (9x9)
    public class UltimateBoard
    {
        readonly SmallBoard[,] boards = new SmallBoard[3, 3];

        public UltimateBoard()
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    boards[i, j] = new SmallBoard();
        }

        // ��������� ����� �����
        public SmallBoard GetSmallBoard(Point point)
        {
            return boards[point.X, point.Y];
        }

        // �������� ����������� ������
        public bool CanMakeMove(Point bigPos, Point smallPos) =>
            boards[bigPos.X, bigPos.Y].IsCellAvailable(smallPos.X, smallPos.Y);

        // ���������� ����
        public void MakeMove(Point bigPos, Point smallPos, char player) =>
            boards[bigPos.X, bigPos.Y].SetCell(smallPos.X, smallPos.Y, player);

        // �������� ������ � ����� �����
        public bool CheckSmallBoardWinner(Point bigPos) =>
            boards[bigPos.X, bigPos.Y].Winner != '\0';

        // ��������� ���������� ����� �����
        public char GetSmallBoardWinner(Point bigPos) =>
            boards[bigPos.X, bigPos.Y].Winner;

        // �������� ����������� ����� ��� �����
        public bool IsBoardAvailable(Point boardPos) =>
            boards[boardPos.X, boardPos.Y].Winner == '\0' &&
            !boards[boardPos.X, boardPos.Y].IsFull();

        // �������� ���������� ������
        public char CheckGlobalWinner()
        {
            // �������� �����
            for (int row = 0; row < 3; row++)
                if (CheckTriplet(boards[row, 0].Winner, boards[row, 1].Winner, boards[row, 2].Winner))
                    return boards[row, 0].Winner;

            // �������� ��������
            for (int col = 0; col < 3; col++)
                if (CheckTriplet(boards[0, col].Winner, boards[1, col].Winner, boards[2, col].Winner))
                    return boards[0, col].Winner;

            // �������� ����������
            if (CheckTriplet(boards[0, 0].Winner, boards[1, 1].Winner, boards[2, 2].Winner))
                return boards[0, 0].Winner;

            if (CheckTriplet(boards[0, 2].Winner, boards[1, 1].Winner, boards[2, 0].Winner))
                return boards[0, 2].Winner;

            return '\0';
        }

        // �������� ���� ���������� ��������
        private bool CheckTriplet(char a, char b, char c) =>
            a != '\0' && a == b && b == c;

        // �������� ������������� ���� �����
        public bool IsFull()
        {
            foreach (var board in boards)
                if (!board.IsFull()) return false;
            return true;
        }
    }

    // ����� ����� ����� (3x3)
    public class SmallBoard
    {
        public char[,] Cells = new char[3, 3];
        public char Winner { get; private set; } = '\0';

        // �������� ����������� ������
        public bool IsCellAvailable(int row, int col) =>
            Cells[row, col] == '\0' && Winner == '\0';

        // ��������� ������� � ������
        public void SetCell(int row, int col, char player)
        {
            Cells[row, col] = player;
            CheckWinner();
        }

        // �������� ������ � �����
        private void CheckWinner()
        {
            // �������� ����� � ��������
            for (int i = 0; i < 3; i++)
            {
                if (CheckLine(Cells[i, 0], Cells[i, 1], Cells[i, 2]) ||
                    CheckLine(Cells[0, i], Cells[1, i], Cells[2, i]))
                    return;
            }

            // �������� ����������
            if (CheckLine(Cells[0, 0], Cells[1, 1], Cells[2, 2]) ||
                CheckLine(Cells[0, 2], Cells[1, 1], Cells[2, 0]))
                return;
        }

        // �������� ����� �� ���� ���������� ��������
        private bool CheckLine(char a, char b, char c)
        {
            if (a != '\0' && a == b && b == c)
            {
                Winner = a;
                return true;
            }
            return false;
        }

        // �������� ������������� �����
        public bool IsFull()
        {
            foreach (char cell in Cells)
                if (cell == '\0') return false;
            return true;
        }
    }

    // ���������������� ������� ���������� ��� ����� �����
    class SmallBoardUC : UserControl
    {
        public int BoardRow { get; }     // ������� ����� � �������� �����
        public int BoardCol { get; }
        private readonly Button[,] cells = new Button[3, 3];
        private char winner = '\0';      // ���������� � ������� �����

        public event EventHandler<CellClickedEventArgs> CellClicked;

        public SmallBoardUC(int boardRow, int boardCol)
        {
            BoardRow = boardRow;
            BoardCol = boardCol;
            InitializeBoard();
        }
        // ���������� � ��������� ������
        public void SetCellsEnabled(bool enabled)
        {
            foreach (var btn in cells)
            {
                btn.Enabled = enabled;
            }
        }
        // ������������� ���������� ����� �����
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

        // ���������� ��������� ����������� ������
        public void UpdateCellsAccessibility(bool isActive, Func<Point, bool> isCellAvailable)
        {
            if (winner != '\0') return; // �� ��������� ���������� �����

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

        // ���������� ����� �� ������
        private void Cell_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            var pos = (Point)btn.Tag;
            CellClicked?.Invoke(this, new CellClickedEventArgs(
                new Point(BoardRow, BoardCol), pos));
        }

        // ���������� ������ ����� ����
        public void UpdateCell(int row, int col, string symbol)
        {
            cells[row, col].Text = symbol;
            cells[row, col].Enabled = false;
            cells[row, col].BackColor = Color.White;
        }

        // ������� ����� ��� ����������
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
                cells[cellPos.X, cellPos.Y].PerformClick();
        }
    }
    
    // ��������� ������� ����� �� ������
    class CellClickedEventArgs : EventArgs
    {
        public Point BigPosition { get; } // ������� ����� ����� � ��������
        public Point SmallPosition { get; } // ������� ������ � ����� �����

        public CellClickedEventArgs(Point bigPos, Point smallPos)
        {
            BigPosition = bigPos;
            SmallPosition = smallPos;
        }
    }
}