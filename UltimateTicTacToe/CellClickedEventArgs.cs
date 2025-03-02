namespace UltimateTicTacToe
{
    // Аргументы события клика по клетке
    internal class CellClickedEventArgs(Point bigPos, Point smallPos) : EventArgs
    {
        public Point BigPosition { get; } = bigPos;
        public Point SmallPosition { get; } = smallPos;
    }
}
