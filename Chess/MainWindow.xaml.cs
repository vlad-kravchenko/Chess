using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Chess
{
    public partial class MainWindow : Window
    {
        Game game = new Game();

        public MainWindow()
        {
            InitializeComponent();
            UpdateGrid();
        }

        private void UpdateGrid()
        {
            MainGrid.Children.Clear();
            MainGrid.RowDefinitions.Clear();
            MainGrid.ColumnDefinitions.Clear();
            for (int i = 0; i < 8; i++)
            {
                MainGrid.RowDefinitions.Add(new RowDefinition());
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var cell = game.Map[row, col];

                    var rect = new Rectangle();
                    rect.VerticalAlignment = VerticalAlignment.Stretch;
                    rect.HorizontalAlignment = HorizontalAlignment.Stretch;
                    rect.Fill = cell.Color;
                    if (cell.Available)
                        rect.Fill = Brushes.LightGreen;
                    if (cell.UnderHit)
                        rect.Fill = Brushes.Red;
                    MainGrid.Children.Add(rect);
                    Grid.SetRow(rect, row);
                    Grid.SetColumn(rect, col);

                    if (!cell.Empty)
                    {
                        var image = new Image();
                        image.VerticalAlignment = VerticalAlignment.Stretch;
                        image.HorizontalAlignment = HorizontalAlignment.Stretch;
                        image.Source = Imaging.CreateBitmapSourceFromHBitmap(GetResource(cell.Figure).GetHbitmap(), 
                            IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        MainGrid.Children.Add(image);
                        Grid.SetRow(image, row);
                        Grid.SetColumn(image, col);
                    }
                }
            }
        }

        private System.Drawing.Bitmap GetResource(Figure figure)
        {
            switch (figure)
            {
                case Figure.WhiteKing: return Properties.Resources.WhiteKing;
                case Figure.WhiteQueen: return Properties.Resources.WhiteQueen;
                case Figure.WhiteRook: return Properties.Resources.WhiteRook;
                case Figure.WhiteBishop: return Properties.Resources.WhiteBishop;
                case Figure.WhiteKnight: return Properties.Resources.WhiteKnight;
                case Figure.WhitePawn: return Properties.Resources.WhitePawn;
                case Figure.BlackKing: return Properties.Resources.BlackKing;
                case Figure.BlackQueen: return Properties.Resources.BlackQueen;
                case Figure.BlackRook: return Properties.Resources.BlackRook;
                case Figure.BlackBishop: return Properties.Resources.BlackBishop;
                case Figure.BlackKnight: return Properties.Resources.BlackKnight;
                case Figure.BlackPawn: return Properties.Resources.BlackPawn;
            }
            return null;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int row = 0;
            int col = 0;
            GetClickCoordinates(out row, out col);
            if (!game.Pick(row, col))
                game.Move(row, col);
            UpdateGrid();
        }

        private void GetClickCoordinates(out int row, out int col)
        {
            col = row = 0;
            var point = Mouse.GetPosition(MainGrid);
            double accumulatedHeight = 0.0;
            double accumulatedWidth = 0.0;
            foreach (var rowDefinition in MainGrid.RowDefinitions)
            {
                accumulatedHeight += rowDefinition.ActualHeight;
                if (accumulatedHeight >= point.Y)
                    break;
                row++;
            }
            foreach (var columnDefinition in MainGrid.ColumnDefinitions)
            {
                accumulatedWidth += columnDefinition.ActualWidth;
                if (accumulatedWidth >= point.X)
                    break;
                col++;
            }
        }
    }
}