using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ShipTrapTestGame
{
    public class Way
    {
        public Point Target { get; }
        public Point Start { get; }
        public int StepCount => Steps.Count;
        public bool IsValid { get; private set; }
        public enum Step { PP, PM, MP, MM } // (+1;+1), (+1;-1), (-1;+1), (-1;-1)
        public List<Step> Steps { get; private set; }
        private readonly List<Cell> cells;

        public Way(Point start, Point target, List<Cell> cells)
        {
            Steps = new List<Step>();
            Target = target;
            Start = start;
            IsValid = true;
            this.cells = cells;
            var waves = new int[cells.Count];
            for(var i = 0; i < cells.Count; i++)
                waves[i] = cells[i].Wave;
            FindSteps();
            for (var i = 0; i < cells.Count; i++)
                cells[i].Wave = waves[i];
        }

        private bool CanStepTo(double x, double y)
        {
            return CanStepTo(new Point(x, y));
        }

        private bool CanStepTo(Point point)
        {
            return point.X >= 0 && point.X < Game.Rows &&
                point.Y >= 0 && point.Y < Game.Columns &&
                cells.Find(c => c.Coordinate == point).Contains != Cell.Element.Mine;
        }

        /// <summary>
        /// Волновой алгоритм поиска пути.
        /// </summary>
        private int DoWave()
        {
            int i = 0;
            while (cells.Any(c => c.Wave == int.MaxValue))
            {
                var aa = cells.FindAll(c => c.Wave == i);
                if (aa.Count == 0)
                    break;
                foreach (var a in aa)
                {
                    if (CanStepTo(a.Coordinate.X + 1, a.Coordinate.Y + 1))
                    {
                        var cell = cells.Find(c => c.Coordinate == new Point(a.Coordinate.X + 1, a.Coordinate.Y + 1));
                        cell.Wave = cell.Wave < i + 1 ? cell.Wave : i + 1;
                        if (cell.Coordinate == Target)
                            return 1;
                    }
                    if (CanStepTo(a.Coordinate.X + 1, a.Coordinate.Y - 1))
                    {
                        var cell = cells.Find(c => c.Coordinate == new Point(a.Coordinate.X + 1, a.Coordinate.Y - 1));
                        cell.Wave = cell.Wave < i + 1 ? cell.Wave : i + 1;
                        if (cell.Coordinate == Target)
                            return 1;
                    }
                    if (CanStepTo(a.Coordinate.X - 1, a.Coordinate.Y + 1))
                    {
                        var cell = cells.Find(c => c.Coordinate == new Point(a.Coordinate.X - 1, a.Coordinate.Y + 1));
                        cell.Wave = cell.Wave < i + 1 ? cell.Wave : i + 1;
                        if (cell.Coordinate == Target)
                            return 1;
                    }
                    if (CanStepTo(a.Coordinate.X - 1, a.Coordinate.Y - 1))
                    {
                        var cell = cells.Find(c => c.Coordinate == new Point(a.Coordinate.X - 1, a.Coordinate.Y - 1));
                        cell.Wave = cell.Wave < i + 1 ? cell.Wave : i + 1;
                        if (cell.Coordinate == Target)
                            return 1;
                    }
                }
                i++;
            }
            return 0;
        }

        /// <summary>
        /// Вычисление шагов для кратчайшего пути.
        /// </summary>
        private void FindSteps()
        {
            if (DoWave() == 0)
            {
                IsValid = false;
            }
            else
            {
                IsValid = true;
                var actualCell = cells.Find(c => c.Coordinate == Target);
                for (int i = actualCell.Wave - 1; i >= 0; i--)
                {
                    var ff = cells.FindAll(c => c.Wave == i);
                    foreach (var f in ff)
                    {
                        if (f.Coordinate.X + 1 == actualCell.Coordinate.X)
                        {
                            if (f.Coordinate.Y + 1 == actualCell.Coordinate.Y)
                                Steps.Insert(0, Step.PP);
                            else if (f.Coordinate.Y - 1 == actualCell.Coordinate.Y)
                                Steps.Insert(0, Step.PM);
                            else
                                continue;
                        }
                        else if (f.Coordinate.X - 1 == actualCell.Coordinate.X)
                        {
                            if (f.Coordinate.Y + 1 == actualCell.Coordinate.Y)
                                Steps.Insert(0, Step.MP);
                            else if (f.Coordinate.Y - 1 == actualCell.Coordinate.Y)
                                Steps.Insert(0, Step.MM);
                            else
                                continue;
                        }
                        else
                            continue;
                        actualCell = f;
                        break;
                    }
                }
            }
        }
    }
}
