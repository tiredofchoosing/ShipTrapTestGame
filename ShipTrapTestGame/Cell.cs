using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;

namespace ShipTrapTestGame
{
    public class Cell
    {
        public Ellipse Ellipse { get; set; }
        public Point Coordinate { get; set; }
        public bool IsEmpty => Contains == Element.None;
        public bool IsFinish
        {
            get
            {
                if (Coordinate.X == 0 || Coordinate.X == Game.Rows - 1 ||
                    Coordinate.Y == 0 && Coordinate.X % 2 == 0 ||
                    Coordinate.Y == Game.Columns - 1 && Coordinate.X % 2 == 1)
                    return true;
                else
                    return false;
            }
        }
        public int Wave { get; set; } // Для волнового алгоритма поиска пути
        public enum Element { None, Mine, Ship }
        private Element contains;
        public Element Contains
        {
            get { return contains; }
            set
            {
                contains = value;
                switch (value)
                {
                    case Element.None:
                        Ellipse.Cursor = Cursors.Hand;
                        Wave = int.MaxValue;
                        break;
                    case Element.Mine:
                        Ellipse.Cursor = Cursors.Arrow;
                        Wave = -1;
                        break;
                    case Element.Ship:
                        Ellipse.Cursor = Cursors.Arrow;
                        Wave = 0;
                        break;
                }
            }
        }

        public Cell()
        {
            Ellipse = new Ellipse();
            Contains = Element.None;
            Coordinate = new Point(0, 0);
            Wave = int.MaxValue;
        }
    }
}
