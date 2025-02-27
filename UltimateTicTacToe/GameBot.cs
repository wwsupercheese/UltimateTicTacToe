using System;
using System.Collections.Generic;
using System.Drawing;

namespace UltimateTicTacToe
{
    public class GameBot
    {
        private char botChar;
        private char playerChar;
        public void setGameBot(bool isX)
        {
            botChar = isX ? 'X' : 'O';
            playerChar = isX ? 'O' : 'X';
        }
        private class BotBoard
        {
            private readonly SmallBoard[,] _boards = new SmallBoard[3, 3];

            public BotBoard(UltimateBoard original)
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        _boards[i, j] = CloneSmallBoard(original, new Point(i, j));
                    }
                }
            }

            private SmallBoard CloneSmallBoard(UltimateBoard source, Point boardPos)
            {
                var clone = new SmallBoard();
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        clone.Cells[i,j] = source.getCell(boardPos.X, boardPos.Y, i, j);
                    }
                }
                return clone;
            }

            public void MakeMove(Point boardPos, Point cellPos, char player)
            {
                _boards[boardPos.X, boardPos.Y].SetCell(cellPos.X, cellPos.Y, player);
            }

            public bool CheckSmallBoardWin(Point boardPos)
            {
                return _boards[boardPos.X, boardPos.Y].Winner != '\0';
            }

            public char GetBoardWinner(Point boardPos)
            {
                return _boards[boardPos.X, boardPos.Y].Winner;
            }

            public bool IsBoardAvailable(Point boardPos)
            {
                return _boards[boardPos.X, boardPos.Y].Winner == '\0'
                    && !_boards[boardPos.X, boardPos.Y].IsFull();
            }
        }

        private readonly Random _random = new Random();

        public (Point, Point) GetBotMove(UltimateBoard board, Point activeBoard)
        {
            var validMoves = GetValidMoves(board, activeBoard);
            if (validMoves.Count == 0) throw new InvalidOperationException("No valid moves");

            // Приоритет 1: Немедленный выигрыш
            foreach (var move in validMoves)
            {
                if (CheckImmediateWin(board, move.board, move.cell))
                    return move;
            }

            // Приоритет 2: Блокировка игрока
            foreach (var move in validMoves)
            {
                if (BlockOpponentWin(board, move.board, move.cell))
                    return move;
            }

            // Приоритет 3: Стратегические позиции
            var strategic = GetStrategicMove(validMoves);
            if (strategic != null) return strategic.Value;

            // Приоритет 4: Случайный ход с весом
            return GetWeightedRandomMove(validMoves);
        }

        private List<(Point board, Point cell)> GetValidMoves(UltimateBoard board, Point activeBoard)
        {
            var moves = new List<(Point, Point)>();
            var targetBoards = GetTargetBoards(board, activeBoard);

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

        private IEnumerable<Point> GetTargetBoards(UltimateBoard board, Point activeBoard)
        {
            if (activeBoard.X != -1) return new[] { activeBoard };

            var boards = new List<Point>();
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    var pos = new Point(x, y);
                    if (board.IsBoardAvailable(pos)) boards.Add(pos);
                }
            }
            return boards;
        }

        private bool CheckImmediateWin(UltimateBoard board, Point boardPos, Point cellPos)
        {
            var botBoard = new BotBoard(board);
            botBoard.MakeMove(boardPos, cellPos, botChar);
            return botBoard.CheckSmallBoardWin(boardPos);
        }

        private bool BlockOpponentWin(UltimateBoard board, Point boardPos, Point cellPos)
        {
            var botBoard = new BotBoard(board);
            botBoard.MakeMove(boardPos, cellPos, playerChar);
            return botBoard.CheckSmallBoardWin(boardPos);
        }

        private (Point, Point)? GetStrategicMove(List<(Point board, Point cell)> moves)
        {
            var priorityMoves = new List<(Point, Point)>();

            // Центральные клетки имеют высший приоритет
            foreach (var move in moves)
            {
                if (move.cell.X == 1 && move.cell.Y == 1)
                    priorityMoves.Add(move);
            }

            if (priorityMoves.Count > 0)
                return priorityMoves[_random.Next(priorityMoves.Count)];

            // Угловые клетки
            foreach (var move in moves)
            {
                if ((move.cell.X + move.cell.Y) % 2 == 0 && move.cell.X != 1 && move.cell.Y != 1)
                    priorityMoves.Add(move);
            }

            return priorityMoves.Count > 0
                ? priorityMoves[_random.Next(priorityMoves.Count)]
                : null;
        }

        private (Point, Point) GetWeightedRandomMove(List<(Point board, Point cell)> moves)
        {
            var weights = new int[moves.Count];
            for (int i = 0; i < moves.Count; i++)
            {
                weights[i] = GetMoveWeight(moves[i]);
            }

            int totalWeight = 0;
            foreach (int w in weights) totalWeight += w;

            int random = _random.Next(totalWeight);
            for (int i = 0; i < moves.Count; i++)
            {
                if (random < weights[i]) return moves[i];
                random -= weights[i];
            }

            return moves[0];
        }

        private int GetMoveWeight((Point board, Point cell) move)
        {
            int weight = 10;

            // Центр доски
            if (move.cell.X == 1 && move.cell.Y == 1) weight += 50;

            // Центральная доска
            if (move.board.X == 1 && move.board.Y == 1) weight += 30;

            // Углы
            if ((move.cell.X + move.cell.Y) % 2 == 0 && move.cell.X != 1 && move.cell.Y != 1)
                weight += 20;

            return weight;
        }
    }
}