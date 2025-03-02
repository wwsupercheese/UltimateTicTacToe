namespace UltimateTicTacToe
{
    // Класс основной игровой доски (9x9)
    public class UltimateBoard
    {
        private readonly SmallBoard[,] boards = new SmallBoard[3, 3];

        public UltimateBoard()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    boards[i, j] = new SmallBoard();
                }
            }
        }

        // Получение малой доски
        public SmallBoard GetSmallBoard(Point point)
        {
            return boards[point.X, point.Y];
        }

        // Проверка доступности клетки
        public bool CanMakeMove(Point bigPos, Point smallPos)
        {
            return boards[bigPos.X, bigPos.Y].IsCellAvailable(smallPos.X, smallPos.Y);
        }

        // Совершение хода
        public void MakeMove(Point bigPos, Point smallPos, char player)
        {
            boards[bigPos.X, bigPos.Y].SetCell(smallPos.X, smallPos.Y, player);
        }

        // Проверка победы в малой доске
        public bool CheckSmallBoardWinner(Point bigPos)
        {
            return boards[bigPos.X, bigPos.Y].Winner != '\0';
        }

        // Получение победителя малой доски
        public char GetSmallBoardWinner(Point bigPos)
        {
            return boards[bigPos.X, bigPos.Y].Winner;
        }

        // Проверка доступности доски для ходов
        public bool IsBoardAvailable(Point boardPos)
        {
            return boards[boardPos.X, boardPos.Y].Winner == '\0' &&
            !boards[boardPos.X, boardPos.Y].IsFull();
        }

        // Проверка глобальной победы
        public char CheckGlobalWinner()
        {
            // Проверка строк
            for (int row = 0; row < 3; row++)
            {
                if (CheckTriplet(boards[row, 0].Winner, boards[row, 1].Winner, boards[row, 2].Winner))
                {
                    return boards[row, 0].Winner;
                }
            }

            // Проверка столбцов
            for (int col = 0; col < 3; col++)
            {
                if (CheckTriplet(boards[0, col].Winner, boards[1, col].Winner, boards[2, col].Winner))
                {
                    return boards[0, col].Winner;
                }
            }

            // Проверка диагоналей
            return CheckTriplet(boards[0, 0].Winner, boards[1, 1].Winner, boards[2, 2].Winner)
                ? boards[0, 0].Winner
                : CheckTriplet(boards[0, 2].Winner, boards[1, 1].Winner, boards[2, 0].Winner) ? boards[0, 2].Winner : '\0';
        }

        // Проверка трех одинаковых символов
        private static bool CheckTriplet(char a, char b, char c)
        {
            return a != '\0' && a == b && b == c;
        }

        // Проверка заполненности всех досок
        public bool IsFull()
        {
            foreach (SmallBoard board in boards)
            {
                if (!board.IsFull())
                {
                    return false;
                }
            }


            return true;
        }
    }
}
