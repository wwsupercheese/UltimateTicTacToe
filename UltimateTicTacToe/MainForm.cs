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

        private readonly GameBot _bot;
        public bool _botMode;
        private bool botCanStoped = false;
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
                Text = "�������:",
                Location = new Point(10, 510),
                AutoSize = true
            };

            nudDepth = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 8,
                Value = 7,
                Location = new Point(140, 510),
                Size = new Size(50, 20)
            };

            lblAlpha = new Label
            {
                Text = "��������:",
                Location = new Point(10, 540),
                AutoSize = true
            };

            nudAlpha = new NumericUpDown
            {
                Minimum = 0.0M,
                Maximum = 1.0M,
                Increment = 0.1M,
                DecimalPlaces = 1,
                Value = 0.9M,
                Location = new Point(140, 540),
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
                filetext = filetext.Replace("\t", new string(' ', 4)); // �������� ��������� �� 4 �������
                filetext = filetext.Replace("\n", "\r\n");


                Form infoForm = new()
                {
                    Text = "������� �� ����",
                    Size = new Size(450, 760),
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    StartPosition = FormStartPosition.CenterParent
                };

                TextBox textBox = new()
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
                _ = infoForm.ShowDialog(this);
            }
            catch (Exception)
            {
                _ = MessageBox.Show(filePath + " was not found!");
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
            nudAlpha.Enabled = !botmode;
            nudAlpha.BackColor = botmode ? Color.LightGray : Color.White;
            nudDepth.Enabled = !botmode;
            nudDepth.BackColor = botmode ? Color.LightGray : Color.White;
        }

        private void UpdateBtnVsBot(bool botmode = true)
        {
            _btnVsBot.Enabled = !botmode;
            _btnVsBot.BackColor = botmode ? Color.LightGray : Color.White;
        }

        private async void BtnVsBot_Click(object sender, EventArgs e)
        {
            _botMode = !_botMode;
            if (!botCanStoped && _botMode)
            {
                UpdateBtnVsBot();
            }
            UpdateBotTools(_botMode);

            if (_botMode)
            {
                _botTokenSource = new CancellationTokenSource();
                try
                {
                    int depth = (int)nudDepth.Value;
                    double alpha = (double)nudAlpha.Value;
                    _botPlayerX = isPlayerX;
                    _bot.SetGameBot(isPlayerX, depth, alpha);
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
            UpdateBtnVsBot(false);
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
                    SmallBoardUC smallBoard = new(bigRow, bigCol)
                    {
                        Location = new Point(bigCol * 160, bigRow * 160)
                    };
                    smallBoard.CellClicked += SmallBoard_CellClicked;
                    mainPanel.Controls.Add(smallBoard);
                }
            }
        }

        // ���������� ����� �� ������ ����� �����
        private async void SmallBoard_CellClicked(object sender, CellClickedEventArgs e)
        {
            SmallBoardUC smallBoard = (SmallBoardUC)sender;
            Point bigPosition = new(smallBoard.BoardRow, smallBoard.BoardCol);

            if (IsMoveValid(bigPosition, e.SmallPosition))
            {
                MakeMove(bigPosition, e.SmallPosition);
                UpdateGameState(bigPosition, e.SmallPosition);
                CheckGlobalWinner();
            }

            if (_botMode && (_botPlayerX == isPlayerX))
            {
                await MakeBotMoveAsync(_botTokenSource.Token);
                //CheckGlobalWinner();
            }
        }

        private async Task MakeBotMoveAsync(CancellationToken token)
        {
            if (!_botMode || (_botPlayerX != isPlayerX))
            {
                return;
            }

            try
            {
                // ��������� ��� ������, ����� ������������� ���� ������
                SetCellsEnabled(false);
                statusLabel.Text = "Bot is thinking...";

                // ����� ����� �������� �������� ��� �������� "�������" ����, ���� ����������
                await Task.Delay(1000, token); // ����������� �������� ��� ���������� UI

                (Point board, Point cell) move = await Task.Run(() =>
                {
                    token.ThrowIfCancellationRequested();
                    return _bot.GetBotMove(board, currentActiveBoard);
                }, token);

                // ��������� UI � �������� ������
                Invoke(() =>
                {
                    SetCellsEnabled(true);
                    SmallBoardUC targetBoard = FindBoardControl(move.board);
                    targetBoard?.PerformCellClick(move.cell);
                    string player = _botPlayerX ? "O" : "X";
                    statusLabel.Text = "You play " + player; // ���������� ������
                });
            }
            catch (Exception ex) when (ex is OperationCanceledException or InvalidOperationException)
            {
                _botMode = false;
                UpdateBotTools(false);
                Invoke((Action)(() => statusLabel.Text = ""));
            }
        }

        private SmallBoardUC? FindBoardControl(Point boardPos)
        {
            foreach (Control control in mainPanel.Controls)
            {
                if (control is SmallBoardUC sb &&
                    sb.BoardRow == boardPos.X &&
                    sb.BoardCol == boardPos.Y)
                {
                    return sb;
                }
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
            {
                MarkBoardAsWon(bigPos);
            }

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
                _ = MessageBox.Show($"Player {winner} wins!");
                InitializeGame();
            }
            else if (board.IsFull())
            {
                _ = MessageBox.Show("Game ended in a draw!");
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
        private void BtnNewGame_Click(object sender, EventArgs e)
        {
            InitializeGame();
        }
    }
}