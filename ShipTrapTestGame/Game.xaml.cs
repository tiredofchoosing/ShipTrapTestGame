using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ShipTrapTestGame
{
    public partial class Game : Window
    {
        private List<Cell> cells;
        private Image ship;

        public static int Rows => 16;
        public static int Columns => 22;
        private Cell ShipCell => cells.Find(c => c.Contains == Cell.Element.Ship);

        public Game()
        {
            InitializeComponent();

            EventManager.RegisterClassHandler(typeof(Ellipse), MouseLeftButtonUpEvent, 
                new RoutedEventHandler(OnCellClick));
        }

        #region Methods

        /// <summary>
        /// Сброс состояния игрового поля до начального.
        /// </summary>
        private void Reset()
        {
            if (gameField.Children.Count > 2) 
                gameField.Children.RemoveRange(2, gameField.Children.Count);
            if (cells != null)
                cells.Clear();
            cells = null;
            InitCells();
            InitShip();
        }

        /// <summary>
        /// Создание и размещение кликабельных ячеек на игровом поле.
        /// </summary>
        private void InitCells()
        {
            cells = new List<Cell>();

            for (var i = 0; i < Rows; i++)
            {
                for (var j = 0; j < Columns / 2; j++)
                {
                    cells.Add(new Cell()
                    {
                        Ellipse = new Ellipse()
                        {
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Height = 44,
                            Width = 44,
                            Margin = new Thickness(i % 2 == 0 ? 0 + 61.8 * j : 31 + 61.8 * j,
                                i % 2 == 0 ? 0 + 63.1 * Math.Truncate((double)(i / 2)) :
                                31 + 63.1 * Math.Truncate((double)(i / 2)), 0, 0),
                            Fill = Brushes.Transparent
                        },
                        Contains = Cell.Element.None,
                        Coordinate = new Point(i, i % 2 == 0 ? j * 2 : j * 2 + 1)
                    });

                    gameField.Children.Add(cells.Last().Ellipse);
                }
            }
        }

        /// <summary>
        /// Размещение корабля по центру поля.
        /// </summary>
        private void InitShip()
        {
            var cell = cells[(int)(cells.Count / 2 + Columns / 4)];
            var ellipse = cell.Ellipse;
            ship = new Image()
            {
                HorizontalAlignment = shipImage.HorizontalAlignment,
                VerticalAlignment = shipImage.VerticalAlignment,
                Source = shipImage.Source,
                Width = shipImage.Width,
                Height = shipImage.Height,
                Margin = new Thickness(ellipse.Margin.Left + ellipse.Width / 2 - shipImage.Width / 2,
                    ellipse.Margin.Top + ellipse.Height / 2 - shipImage.Height / 2, 0, 0)
            };

            gameField.Children.Insert(2, ship);
            cell.Contains = Cell.Element.Ship;
        }

        /// <summary>
        /// Основная логика игры: расчет движения корабля.
        /// </summary>
        private void ShipMove()
        {
            var finishCells = cells.FindAll(c => c.IsFinish == true && c.IsEmpty == true);
            var ways = new List<Way>();

            foreach (var finish in finishCells)
                ways.Add(new Way(ShipCell.Coordinate, finish.Coordinate, cells));

            var way = ways.FindAll(w => w.IsValid).OrderBy(w => w.StepCount).FirstOrDefault();
            if (way == null)
            {
                GameEnd(true);
                return;
            }
            switch (way.Steps.First())
            {
                case Way.Step.PP:
                    ShipAnimatedMoveTo(cells.Find(c => c.Coordinate == 
                        new Point(ShipCell.Coordinate.X + 1, ShipCell.Coordinate.Y + 1)));
                    break;
                case Way.Step.PM:
                    ShipAnimatedMoveTo(cells.Find(c => c.Coordinate == 
                        new Point(ShipCell.Coordinate.X + 1, ShipCell.Coordinate.Y - 1)));
                    break;
                case Way.Step.MP:
                    ShipAnimatedMoveTo(cells.Find(c => c.Coordinate == 
                        new Point(ShipCell.Coordinate.X - 1, ShipCell.Coordinate.Y + 1)));
                    break;
                case Way.Step.MM:
                    ShipAnimatedMoveTo(cells.Find(c => c.Coordinate == 
                        new Point(ShipCell.Coordinate.X - 1, ShipCell.Coordinate.Y - 1)));
                    break;
                default:
                    break;
            }
            if (ShipCell.IsFinish)
                GameEnd(false);
        }

        /// <summary>
        /// Обновление UI после завершения игры.
        /// </summary>
        /// <param name="isPlayerWon">Флаг победы игрока.</param>
        private void GameEnd(bool isPlayerWon)
        {
            continueBtn.Visibility = Visibility.Hidden;
            result.Visibility = Visibility.Visible;
            result.Content = isPlayerWon ? "    Победа!" : "Поражение!";
            overlay.Visibility = Visibility.Visible;
            blur.Radius = 15;
        }

        /// <summary>
        /// Добавление мины в ячейку на игровое поле.
        /// </summary>
        /// <param name="cell">Ячейка, в которую добавляется мина.</param>
        private void AddMineTo(Cell cell)
        {
            var ellipse = cell.Ellipse;
            var mine = new Image()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Source = mineImage.Source,
                Width = mineImage.Width,
                Height = mineImage.Height,
                Margin = new Thickness(ellipse.Margin.Left + ellipse.Width / 2 - mineImage.Width / 2,
                    ellipse.Margin.Top + ellipse.Height / 2 - mineImage.Height / 2, 0, 0)
            };

            gameField.Children.Add(mine);
            cell.Contains = Cell.Element.Mine;
        }

        /// <summary>
        /// Анимированное перемещение корабля в указанную ячейку.
        /// </summary>
        /// <param name="cell">Ячейка, в которую перемещается корабль.</param>
        private void ShipAnimatedMoveTo(Cell cell)
        {
            var oldLocationCell = ShipCell;
            cell.Contains = Cell.Element.Ship;

            ShipAnimatedMoveTo(new Point(cell.Ellipse.Margin.Left + cell.Ellipse.Width / 2,
                cell.Ellipse.Margin.Top + cell.Ellipse.Height / 2));

            oldLocationCell.Contains = Cell.Element.None;
        }

        /// <summary>
        /// Анимированное перемещение корабля в указанную точку.
        /// </summary>
        /// <param name="point">Точка, в которую перемещается корабль.</param>
        private void ShipAnimatedMoveTo(Point point)
        {
            var shipAnimation = new ThicknessAnimation
            {
                From = ship.Margin,
                To = new Thickness(point.X - ship.Width / 2, point.Y - ship.Height / 2, 0, 0),
                Duration = TimeSpan.FromSeconds(1),
                FillBehavior = FillBehavior.HoldEnd
            };

            ship.BeginAnimation(Image.MarginProperty, shipAnimation);
        }
        #endregion

        #region Events

        /// <summary>
        /// Обработчик нажатия по ячейке левой кнопкой мыши.
        /// </summary>
        private void OnCellClick(object sender, RoutedEventArgs e)
        {
            var cell = cells.Find(c => c.Ellipse == (sender as Ellipse));
            if (cell.Contains != Cell.Element.None) return;

            AddMineTo(cell);
            ShipMove();
        }

        private void newGameBtn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Reset();
            overlay.Visibility = Visibility.Hidden;
            result.Visibility = Visibility.Hidden;
            blur.Radius = 0;
        }

        private void exitBtn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void menuBtn_MouseUp(object sender, RoutedEventArgs e)
        {
            overlay.Visibility = Visibility.Visible;
            blur.Radius = 15;
            continueBtn.Visibility = Visibility.Visible;
        }

        private void continueBtn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            overlay.Visibility = Visibility.Hidden;
            blur.Radius = 0;
        }
        #endregion
    }
}
