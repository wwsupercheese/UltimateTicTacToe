using Microsoft.VisualBasic.Devices;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace UltimateTicTacToe
{
    public class GameBot
    {
        private int MaxDepth = 4; // Глубина поиска
        private double alpha = 0.7; // Коэффициент точности бота (0 - случайные ходы, 1 - лучший ход
        private char _botSymbol;
        private char _playerSymbol;
        private bool _botIsX;
        private Random rand = new Random();

        public void setGameBot(bool isBotX, int depth, double _alpha)
        {
            alpha = _alpha;
            MaxDepth = depth;
            _botIsX = isBotX;
            _botSymbol = isBotX ? 'X' : 'O';
            _playerSymbol = isBotX ? 'O' : 'X';
        }
        public (Point board, Point cell) GetBotMove(UltimateBoard board, Point activeBoard)
        {
            var validMoves = GetValidMoves(board, activeBoard);
            if (validMoves.Count == 0) throw new InvalidOperationException("No valid moves");

            List<((Point board, Point cell) move, double score)> moves = new List<((Point, Point), double)>();

            string msg = "";
            foreach (var move in validMoves)
            {
                UltimateBoard newBoard = CloneBoard(board);
                newBoard.MakeMove(move.board, move.cell, _botSymbol);
                Point nextActiveBoard = newBoard.IsBoardAvailable(move.cell) ? move.cell : new Point(-1);
                double score = GetScore(newBoard, nextActiveBoard, MaxDepth - 1, true);
                moves.Add((move, score));
                msg += move.ToString() + " " + score + '\n';
            }

            var bestMoves = moves.OrderByDescending(x => x.score).ToList();
            
            return bestMoves[rand.Next((int)((1 - alpha) * (bestMoves.Count - 1)))].move;
        }

        private double GetScore(UltimateBoard board, Point activeBoard, int depth, bool isOpponentTurn = false)
        {
            char globalWinner = board.CheckGlobalWinner();
            if (globalWinner == _botSymbol) return 1.0;
            if (globalWinner == _playerSymbol) return -1.0;
            if (board.IsFull() || depth == 0) return EvaluatePosition(board);

            var validMoves = GetValidMoves(board, activeBoard);
            if (validMoves.Count == 0) return 0.0;

            List<double> scores = new List<double>();
            foreach (var move in validMoves)
            {
                UltimateBoard newBoard = CloneBoard(board);
                char currentSymbol = isOpponentTurn ? _playerSymbol : _botSymbol;
                newBoard.MakeMove(move.board, move.cell, currentSymbol);
                Point nextActiveBoard = newBoard.IsBoardAvailable(move.cell) ? move.cell : new Point(-1);
                scores.Add(GetScore(newBoard, nextActiveBoard, depth - 1, !isOpponentTurn));
            }

            return isOpponentTurn ? scores.Min() : scores.Max();
        }

        private double EvaluatePosition(UltimateBoard board)
        {
            double score = 0.0;

            // Глобальные паттерны
            foreach (var pattern in GlobalWinPatterns)
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
                    if (sb.Winner == _botSymbol) score += 0.3;
                    else if (sb.Winner == _playerSymbol) score -= 0.3;
                    else score += 0.1 * (EvaluateSmallBoardPotential(sb, _botSymbol) -
                                       EvaluateSmallBoardPotential(sb, _playerSymbol));
                }
            }

            return Math.Clamp(score, -1.0, 1.0);
        }

        private double EvaluateSmallBoardPotential(SmallBoard board, char player)
        {
            if (board.Winner != '\0')
                return board.Winner == player ? 1.0 : -1.0;

            double maxPotential = 0.0;
            char opponent = player == 'X' ? 'O' : 'X';

            foreach (var pattern in WinPatterns)
            {
                int playerCount = 0;
                int opponentCount = 0;

                foreach (int idx in pattern)
                {
                    int x = idx / 3, y = idx % 3;
                    if (board.Cells[x, y] == player) playerCount++;
                    else if (board.Cells[x, y] == opponent) opponentCount++;
                }

                if (opponentCount == 0)
                    maxPotential = Math.Max(maxPotential, playerCount / 3.0);
                if (playerCount == 0)
                    maxPotential = Math.Max(maxPotential, -opponentCount / 3.0);
            }

            return maxPotential;
        }

       private static readonly int[][] WinPatterns = {
            new[] {0, 1, 2}, new[] {3, 4, 5}, new[] {6, 7, 8}, // Rows
            new[] {0, 3, 6}, new[] {1, 4, 7}, new[] {2, 5, 8}, // Columns
            new[] {0, 4, 8}, new[] {2, 4, 6} // Diagonals
        };

        private static readonly Point[][] GlobalWinPatterns = {
            new[] {new Point(0,0), new Point(0,1), new Point(0,2)},
            new[] {new Point(1,0), new Point(1,1), new Point(1,2)},
            new[] {new Point(2,0), new Point(2,1), new Point(2,2)},
            new[] {new Point(0,0), new Point(1,0), new Point(2,0)},
            new[] {new Point(0,1), new Point(1,1), new Point(2,1)},
            new[] {new Point(0,2), new Point(1,2), new Point(2,2)},
            new[] {new Point(0,0), new Point(1,1), new Point(2,2)},
            new[] {new Point(0,2), new Point(1,1), new Point(2,0)}
        };

        private List<(Point board, Point cell)> GetValidMoves(UltimateBoard board, Point activeBoard)
        {
            var moves = new List<(Point, Point)>();
            var targetBoards = activeBoard.X == -1 ? GetAllAvailableBoards(board) : new[] { activeBoard };

            foreach (var boardPos in targetBoards)
            {
                for (int x = 0; x < 3; x++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        var cellPos = new Point(x, y);
                        if (board.CanMakeMove(boardPos, cellPos))
                            moves.Add((boardPos, cellPos));
                    }
                }
            }
            return moves;
        }

        private IEnumerable<Point> GetAllAvailableBoards(UltimateBoard board)
        {
            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                    if (board.IsBoardAvailable(new Point(x, y)))
                        yield return new Point(x, y);
        }

        private UltimateBoard CloneBoard(UltimateBoard original)
        {
            var clone = new UltimateBoard();
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    Point boardPos = new Point(x, y);
                    var smallBoard = original.GetSmallBoard(boardPos);
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            if (smallBoard.Cells[i, j] != '\0')
                                clone.MakeMove(boardPos, new Point(i, j), smallBoard.Cells[i, j]);
                        }
                    }
                }
            }
            return clone;
        }
    }
}