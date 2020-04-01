using System;
using System.Windows.Media;

namespace Chess
{
    public class Cell
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public Figure Figure { get; set; }
        public Brush Color { get; set; }
        public bool Available { get; set; }
        public Team Team { get; set; }
        public bool Empty { get { return Figure == Figure.None; } }
        public bool UnderHit { get; set; }

        public Cell(int row, int col, Figure figure)
        {
            Row = row;
            Col = col;
            Figure = figure;
            if (figure.ToString()[0] == 'W') Team = Team.White;
            else if (figure.ToString()[0] == 'B') Team = Team.Black;
            else Team = Team.None;
            Color = GetCellColor(row, col);
            UnderHit = false;
        }


        public bool IsKing { get { return Figure == Figure.BlackKing || Figure == Figure.WhiteKing; } }
        public bool IsRook { get { return Figure == Figure.BlackRook || Figure == Figure.WhiteRook; } }

        private Brush GetCellColor(int row, int col)
        {
            if (row % 2 == 0 && col % 2 == 0) return Brushes.White;
            if (row % 2 != 0 && col % 2 == 0) return Brushes.Gray;
            if (row % 2 != 0 && col % 2 != 0) return Brushes.White;
            return Brushes.Gray;
        }
    }
}