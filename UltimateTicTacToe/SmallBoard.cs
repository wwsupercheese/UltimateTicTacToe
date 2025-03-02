namespace UltimateTicTacToe
{
    // Класс малой доски (3x3)
    public class SmallBoard
    {
        public char[,] Cells = new char[3, 3];
        public char Winner { get; private set; } = '\0';

        // Проверка доступности клетки
        public bool IsCellAvailable(int row, int col)
        {
            return Cells[row, col] == '\0' && Winner == '\0';
        }

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
                {
                    return;
                }
            }

            // Проверка диагоналей
            if (CheckLine(Cells[0, 0], Cells[1, 1], Cells[2, 2]) ||
                CheckLine(Cells[0, 2], Cells[1, 1], Cells[2, 0]))
            {
                return;
            }
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
            if (Winner != '\0')
            {
                return true;
            }
            foreach (char cell in Cells)
            {
                if (cell == '\0')
                {
                    return false;
                }
            }

            return true;
        }
    }

}
