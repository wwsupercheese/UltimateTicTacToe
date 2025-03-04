﻿using Microsoft.VisualBasic;
using static System.Formats.Asn1.AsnWriter;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace UltimateTicTacToe
{
    public class GameBot
    {
        private int MaxDepth = 4; // Глубина поиска
        private double alpha = 0.7; // Коэффициент точности бота (0 - случайные ходы, 1 - лучший ход
        private char _botSymbol;
        private char _playerSymbol;
        private bool _botIsX;
        private readonly Random rand = new();

        public void SetGameBot(bool isBotX, int depth, double _alpha)
        {
            alpha = _alpha;
            MaxDepth = depth;
            _botIsX = isBotX;
            _botSymbol = isBotX ? 'X' : 'O';
            _playerSymbol = isBotX ? 'O' : 'X';
        }
        public (Point board, Point cell) GetBotMove(UltimateBoard board, Point activeBoard)
        {
            List<(Point board, Point cell)> validMoves = GetValidMoves(board, activeBoard);
            if (validMoves.Count == 0)
            {
                throw new InvalidOperationException("No valid moves");
            }

            List<((Point board, Point cell) move, double score)> moves = [];
            var str = "";
            foreach ((Point board, Point cell) move in validMoves)
            {
                UltimateBoard newBoard = CloneBoard(board);
                newBoard.MakeMove(move.board, move.cell, _botSymbol);
                Point nextActiveBoard = newBoard.IsBoardAvailable(move.cell) ? move.cell : new Point(-1);
                double score = GetScore(newBoard, nextActiveBoard, MaxDepth - 1, true);
                str += move.cell.ToString() + "|" + score + "\r\n";
                moves.Add((move, score));
            }
            //MessageBox.Show(str);
            return GetMoveWhithAlpha(moves, alpha).Item1;
        }
        private ((Point, Point), double) GetMoveWhithAlpha(List<((Point, Point), double)> moves, double alpha)
        {
            var maxScore = moves.Max(x => x.Item2);
            
            if (alpha == 1) // Возвращаем один из лучших
            {
                var bestMoves = moves.Where(x => x.Item2 == maxScore).ToList();
                return bestMoves[rand.Next(bestMoves.Count)];
            }
            moves = [.. moves.OrderByDescending(x => x.Item2)];
            alpha = 1 - alpha;
            List<double> list = [];
            for (int i = 0; i < moves.Count; i++)
            {
                var score = ((1.0 - alpha) / Math.Pow(maxScore - moves[i].Item2 + 1, 5)) + (alpha / moves.Count);
                list.Add(score);
            }
            double s = list.Sum();

            list[0] = list[0] / s;
            //var str = moves[0].ToString() + "|" + list[0] + "\r\n";
            for (int i = 1; i < list.Count; i++)
            {
                list[i] = list[i - 1] + (list[i] / s);
                //str += moves[i].ToString() + "|" + (list[i] - list[i-1]) + "\r\n";
            }
            list[^1] = 1;
            //MessageBox.Show(str);

            int index = 0;
            double r = rand.NextDouble();
            while (list[index] < r)
            {
                index++;
            }
            //_ = MessageBox.Show((index + 1).ToString() + " / " + list.Count);
            return moves[index];
        }

        private double GetScore(UltimateBoard board, Point activeBoard, int depth, bool isOpponentTurn, double alpha = double.MinValue, double beta = double.MaxValue)
        {
            char globalWinner = board.CheckGlobalWinner();
            if (globalWinner == _botSymbol) return 100.0;
            if (globalWinner == _playerSymbol) return -100.0;

            if (board.IsFull() || depth == 0) return EvaluatePosition(board);

            List<(Point board, Point cell)> validMoves = GetValidMoves(board, activeBoard);
            if (validMoves.Count == 0) return 0.0;

            // Сортировка ходов для улучшения отсечения
            var orderedMoves = isOpponentTurn
                ? validMoves.OrderBy(m => EvaluateMovePotential(board, m))
                : validMoves.OrderByDescending(m => EvaluateMovePotential(board, m));

            double bestScore = isOpponentTurn ? double.MaxValue : double.MinValue;

            foreach (var move in orderedMoves)
            {
                UltimateBoard newBoard = CloneBoard(board);
                char currentSymbol = isOpponentTurn ? _playerSymbol : _botSymbol;
                newBoard.MakeMove(move.board, move.cell, currentSymbol);
                Point nextActiveBoard = newBoard.IsBoardAvailable(move.cell) ? move.cell : new Point(-1);

                double currentScore = GetScore(newBoard, nextActiveBoard, depth - 1, !isOpponentTurn, alpha, beta);

                if (isOpponentTurn)
                {
                    bestScore = Math.Min(bestScore, currentScore);
                    beta = Math.Min(beta, bestScore);
                }
                else
                {
                    bestScore = Math.Max(bestScore, currentScore);
                    alpha = Math.Max(alpha, bestScore);
                }

                if (beta <= alpha) break;
            }

            return bestScore;
        }

        // Вспомогательная функция для оценки потенциала хода
        private double EvaluateMovePotential(UltimateBoard board, (Point boardPos, Point cellPos) move)
        {
            double score = 0;

            // Приоритет центральной клетки
            if (move.cellPos.X == 1 && move.cellPos.Y == 1) score += 0.3;

            // Приоритет выигрыша малой доски
            var targetBoard = board.GetSmallBoard(move.boardPos);
            if (targetBoard.Winner == _botSymbol) score += 0.5;

            return score;
        }

        private double EvaluatePosition(UltimateBoard board)
        {
            double score = 0.0;

            // Глобальные паттерны
            foreach (Point[] pattern in GlobalWinPatterns)
            {
                double botPotential = 0.0;
                double playerPotential = 0.0;

                foreach (Point pos in pattern)
                {
                    SmallBoard sb = board.GetSmallBoard(pos);
                    botPotential += EvaluateSmallBoardPotential(sb, _botSymbol);
                    playerPotential += EvaluateSmallBoardPotential(sb, _playerSymbol);
                }

                score += Math.Tanh(botPotential - playerPotential);
            }

            // Локальный потенциал
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    SmallBoard sb = board.GetSmallBoard(new Point(x, y));
                    if (sb.Winner == _botSymbol)
                    {
                        score += 0.3;
                    }
                    else if (sb.Winner == _playerSymbol)
                    {
                        score -= 0.3;
                    }
                    else
                    {
                        score += 0.1 * (EvaluateSmallBoardPotential(sb, _botSymbol) -
                                       EvaluateSmallBoardPotential(sb, _playerSymbol));
                    }
                }
            }

            return score;
        }

        private static double EvaluateSmallBoardPotential(SmallBoard board, char player)
        {
            if (board.Winner != '\0')
            {
                return board.Winner == player ? 1.0 : -1.0;
            }

            double maxPotential = 0.0;
            char opponent = player == 'X' ? 'O' : 'X';

            foreach (int[] pattern in WinPatterns)
            {
                int playerCount = 0;
                int opponentCount = 0;

                foreach (int idx in pattern)
                {
                    int x = idx / 3, y = idx % 3;
                    if (board.Cells[x, y] == player)
                    {
                        playerCount++;
                    }
                    else if (board.Cells[x, y] == opponent)
                    {
                        opponentCount++;
                    }
                }

                if (opponentCount == 0)
                {
                    maxPotential = Math.Max(maxPotential, playerCount / 3.0);
                }

                if (playerCount == 0)
                {
                    maxPotential = Math.Max(maxPotential, -opponentCount / 3.0);
                }
            }

            return maxPotential;
        }

        private static readonly int[][] WinPatterns = [
            [0, 1, 2], [3, 4, 5], [6, 7, 8], // Rows
            [0, 3, 6], [1, 4, 7], [2, 5, 8], // Columns
            [0, 4, 8], [2, 4, 6] // Diagonals
        ];

        private static readonly Point[][] GlobalWinPatterns = [
            [new Point(0,0), new Point(0,1), new Point(0,2)],
            [new Point(1,0), new Point(1,1), new Point(1,2)],
            [new Point(2,0), new Point(2,1), new Point(2,2)],
            [new Point(0,0), new Point(1,0), new Point(2,0)],
            [new Point(0,1), new Point(1,1), new Point(2,1)],
            [new Point(0,2), new Point(1,2), new Point(2,2)],
            [new Point(0,0), new Point(1,1), new Point(2,2)],
            [new Point(0,2), new Point(1,1), new Point(2,0)]
        ];

        private static List<(Point board, Point cell)> GetValidMoves(UltimateBoard board, Point activeBoard)
        {
            List<(Point, Point)> moves = [];
            IEnumerable<Point> targetBoards = activeBoard.X == -1 ? GetAllAvailableBoards(board) : [activeBoard];

            foreach (Point boardPos in targetBoards)
            {
                for (int x = 0; x < 3; x++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        Point cellPos = new(x, y);
                        if (board.CanMakeMove(boardPos, cellPos))
                        {
                            moves.Add((boardPos, cellPos));
                        }
                    }
                }
            }
            return moves;
        }

        private static IEnumerable<Point> GetAllAvailableBoards(UltimateBoard board)
        {
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    if (board.IsBoardAvailable(new Point(x, y)))
                    {
                        yield return new Point(x, y);
                    }
                }
            }
        }

        private static UltimateBoard CloneBoard(UltimateBoard original)
        {
            UltimateBoard clone = new();
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    Point boardPos = new(x, y);
                    SmallBoard smallBoard = original.GetSmallBoard(boardPos);
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            if (smallBoard.Cells[i, j] != '\0')
                            {
                                clone.MakeMove(boardPos, new Point(i, j), smallBoard.Cells[i, j]);
                            }
                        }
                    }
                }
            }
            return clone;
        }
    }
}