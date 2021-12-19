using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Point = System.Drawing.Point;

namespace EasyScreenshot
{
    public partial class ScreenShotEditorWindow : Window
    {
        private readonly ScreenShot _screenShot = ScreenShot.GetScreen();

        private readonly int _toolsPanelWidth;
        private readonly int _textAreaSizeHeight;

        public ScreenShotEditorWindow()
        {
            InitializeComponent();
            ImageScreen.Source = _screenShot.Image;

            _toolsPanelWidth = (int)GridSelectedArea.ColumnDefinitions[0].Width.Value;
            _textAreaSizeHeight = (int)GridSelectedArea.RowDefinitions[0].Height.Value;
        }

        private Point _lastPosition;

        private bool _mouseDown;

        private void MainCanvas_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            SetTextSizeAreaPosition();
            _lastPosition = GetSelectedAreaPosition();
            _mouseDown = true;

            PanelTools.Visibility = Visibility.Collapsed;

            RectangleSelectedArea.Width = 0;
            RectangleSelectedArea.Height = 0;

            Canvas.SetLeft(GridSelectedArea, _lastPosition.X);
            Canvas.SetTop(GridSelectedArea, _lastPosition.Y);

            GridSelectedArea.Visibility = Visibility.Visible;
        }

        private void MainCanvas_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseDown == false)
            {
                return;
            }

            var currentMousePosition = GetSelectedAreaPosition();
            if (currentMousePosition.X <= _lastPosition.X)
                Canvas.SetLeft(GridSelectedArea, currentMousePosition.X);
            if (currentMousePosition.Y <= _lastPosition.Y)
                Canvas.SetTop(GridSelectedArea, currentMousePosition.Y);

            RectangleSelectedArea.Width = Math.Abs(_lastPosition.X - currentMousePosition.X);
            RectangleSelectedArea.Height = Math.Abs(_lastPosition.Y - currentMousePosition.Y);

        }

        private Point GetSelectedAreaPosition()
        {
            var mousePosition = Mouse.GetPosition(MainCanvas);

            return new Point((int)mousePosition.X - _toolsPanelWidth, (int)mousePosition.Y - _textAreaSizeHeight);
        }

        private Rectangle GetSelectedAreaRectangle()
        {
            var startX = (int)Canvas.GetLeft(GridSelectedArea) + _toolsPanelWidth;
            var startY = (int)Canvas.GetTop(GridSelectedArea) + _textAreaSizeHeight;
            var width = (int)RectangleSelectedArea.DesiredSize.Width;
            var height = (int)RectangleSelectedArea.DesiredSize.Height;

            return new Rectangle(startX, startY, width, height);
        }

        private void MainCanvas_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _mouseDown = false;

            if (RectangleSelectedArea.Width == 0 || RectangleSelectedArea.Height == 0) return;

            PanelTools.Visibility = Visibility.Visible;
            SetPanelToolsPosition();
        }

        private void SetPanelToolsPosition()
        {
            var left = Canvas.GetLeft(GridSelectedArea);
            var right = left + GridSelectedArea.ActualWidth;

            if (left < 0 && right > _screenShot.Width)
            {
                Grid.SetColumn(PanelTools, 1);
                PanelTools.HorizontalAlignment = HorizontalAlignment.Right;
                PanelTools.Margin = new Thickness(15);
            }
            else
            {
                Grid.SetColumn(PanelTools, right > _screenShot.Width ? 0 : 2);
                PanelTools.HorizontalAlignment = HorizontalAlignment.Center;
                PanelTools.Margin = new Thickness(5);
            }
        }

        private void SetTextSizeAreaPosition()
        {
            var top = Mouse.GetPosition(MainCanvas).Y - _textAreaSizeHeight;

            if (top < 0)
            {
                Grid.SetRow(TextBlockAreaSize, 1);
                TextBlockAreaSize.VerticalAlignment = VerticalAlignment.Top;
            }
            else
                Grid.SetRow(TextBlockAreaSize, 0);
        }

        private bool CanSaved()
        {
            var areaRectangle = GetSelectedAreaRectangle();
            if (areaRectangle.Width > 0 && areaRectangle.Height > 0 && GridSelectedArea.Visibility == Visibility.Visible)
                return true;

            SystemSounds.Hand.Play();
            return false;
        }

        #region CommandBindings
        private void CommandBindingClose_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void CommandBindingSave_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG (*.png)|*.png|JPEG (*.jpg)|*.jpg";
            saveFileDialog.FileName = "Screen";

            if (CanSaved() && saveFileDialog.ShowDialog() == true)
            {
                _screenShot.Cut(GetSelectedAreaRectangle());
                var filePath = saveFileDialog.FileName;
                _screenShot.Save(filePath);
                Close();
            }
        }

        private void CommandBindingCopy_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (CanSaved())
            {
                _screenShot.Cut(GetSelectedAreaRectangle());
                Clipboard.SetImage(_screenShot.Image);
                Close();
            }
        }
        #endregion
    }
}