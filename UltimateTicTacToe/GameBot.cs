using System;
using System.Collections.Generic;
using System.Drawing;

namespace UltimateTicTacToe
{
    public class GameBot
    {
        private readonly Random _random = new Random();

        // Основная функция хода бота
        public (Point board, Point cell) GetBotMove (UltimateBoard board, Point activeBoard)
        {
            var validMoves = GetValidMove(board, activeBoard);
            
            // "Мозги" бота
            var move = GetRandomMove(validMoves);
            Thread.Sleep(1500); // Задержка
            return move;
        }

        // Случайный ход
        private (Point board, Point cell) GetRandomMove(List<(Point, Point)> validMoves)
        {
            var move = validMoves.Count > 0
                ? validMoves[_random.Next(validMoves.Count)]
                : throw new InvalidOperationException("No valid moves available");
            return move;
        }

        // Находит все доступные ходы на текущей доске
        private List<(Point, Point)> GetValidMove(UltimateBoard board, Point activeBoard)
        {
            var validMoves = new List<(Point, Point)>();
            var targetBoards = GetTargetBoards(board, activeBoard);

            foreach (var boardPos in targetBoards)
            {
                for (int row = 0; row < 3; row++)
                {
                    for (int col = 0; col < 3; col++)
                    {
                        var cellPos = new Point(row, col);
                        if (board.CanMakeMove(boardPos, cellPos))
                            validMoves.Add((boardPos, cellPos));
                    }
                }
            }
            return validMoves;
        }
       
        // Определяет список досок для анализа на основе текущей активной доски
        private IEnumerable<Point> GetTargetBoards(UltimateBoard board, Point activeBoard)
        {
            if (activeBoard.X != -1)
                yield return activeBoard;
            else
            {
                for (int row = 0; row < 3; row++)
                    for (int col = 0; col < 3; col++)
                        if (board.IsBoardAvailable(new Point(row, col)))
                            yield return new Point(row, col);
            }
        }
    }
}