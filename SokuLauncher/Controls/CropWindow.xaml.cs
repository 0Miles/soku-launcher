using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SokuLauncher.Controls
{
    public partial class CropWindow : Window
    {
        public CropWindow()
        {
            DataContext = this;
            InitializeComponent();
        }

        private Image CurrentImage;
        private Point LastMousePosition;
        private bool IsStartMove = false;

        private readonly string CoverDir = Path.Combine(Static.LocalDirPath, "Cover");

        public double CropWidth { get; set; } = 420;
        public double CropHeight { get; set; } = 150;

        private string _ImagePath;
        public string ImagePath
        {
            get
            {
                return _ImagePath;
            }
            set
            {
                if (_ImagePath != value)
                {
                    _ImagePath = value;
                    LoadImage(value);
                }
            }
        }

        public string FileName { get; set; }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CenterImage();
        }

        private void LoadImage(string imagePath)
        {
            try
            {
                if (CurrentImage != null)
                {
                    MainCanvas.Children.Remove(CurrentImage);
                    CurrentImage = null;
                }

                BitmapImage bitmap;
                using (FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                {
                    bitmap = new BitmapImage();

                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();

                    bitmap.Freeze();

                    stream.Close();
                }

                double scaleFactor = Math.Max(CropWidth / bitmap.Width, CropHeight / bitmap.Height);
                double newWidth = bitmap.Width * scaleFactor;
                double newHeight = bitmap.Height * scaleFactor;

                CurrentImage = new Image
                {
                    Source = bitmap,
                    Width = newWidth,
                    Height = newHeight
                };

                MainCanvas.Children.Add(CurrentImage);

                CenterImage();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CenterImage()
        {
            Canvas.SetLeft(CurrentImage, (MainCanvas.ActualWidth - CurrentImage.Width) / 2);
            Canvas.SetTop(CurrentImage, (MainCanvas.ActualHeight - CurrentImage.Height) / 2);
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && CurrentImage != null && IsStartMove)
            {
                Point currentMousePosition = e.GetPosition(MainCanvas);

                double deltaX = currentMousePosition.X - LastMousePosition.X;
                double deltaY = currentMousePosition.Y - LastMousePosition.Y;

                double newLeft = GetNewLeft(Canvas.GetLeft(CurrentImage) + deltaX);
                double newTop = GetNewTop(Canvas.GetTop(CurrentImage) + deltaY);

                Canvas.SetLeft(CurrentImage, newLeft);
                Canvas.SetTop(CurrentImage, newTop);

                LastMousePosition = currentMousePosition;
            }
        }

        private double GetNewLeft(double newLeft, double? newWidth = null)
        {
            if (newWidth == null)
            {
                newWidth = CurrentImage.ActualWidth;
            }

            double leftBorder = (MainCanvas.ActualWidth - CropWidth) / 2;
            double rightBorder = leftBorder + CropWidth;

            if (newLeft > leftBorder)
            {
                newLeft = leftBorder;
            }
            else if (newLeft + newWidth < rightBorder)
            {
                newLeft = rightBorder - newWidth.Value;
            }
            return newLeft;
        }

        private double GetNewTop(double newTop, double? newHeight = null)
        {
            if (newHeight == null)
            {
                newHeight = CurrentImage.ActualHeight;
            }

            double topBorder = (MainCanvas.ActualHeight - CropHeight) / 2;
            double bottomBorder = topBorder + CropHeight;

            if (newTop > topBorder)
            {
                newTop = topBorder;
            }
            else if (newTop + newHeight < bottomBorder)
            {
                newTop = bottomBorder - newHeight.Value;
            }
            return newTop;
        }

        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (CurrentImage != null)
            {
                Point mousePos = e.GetPosition(MainCanvas);

                double scale = e.Delta > 0 ? 1.1 : 0.9;
                double newWidth = CurrentImage.Width * scale;
                double newHeight = CurrentImage.Height * scale;
                double deltaX = (CurrentImage.Width - newWidth) * (mousePos.X / MainCanvas.ActualWidth);
                double deltaY = (CurrentImage.Height - newHeight) * (mousePos.Y / MainCanvas.ActualHeight);

                if (scale != 1.1 && (newWidth < CropWidth || newHeight < CropHeight))
                {
                    if (CurrentImage.Width == CropWidth || CurrentImage.Height == CropHeight)
                    {
                        return;
                    }
                    double scaleFactor = Math.Max(CropWidth / newWidth, CropHeight / newHeight);
                    newWidth *= scaleFactor;
                    newHeight *= scaleFactor;
                }

                CurrentImage.Width = newWidth;
                CurrentImage.Height = newHeight;

                double newLeft = GetNewLeft(Canvas.GetLeft(CurrentImage) + deltaX, newWidth);
                double newTop = GetNewTop(Canvas.GetTop(CurrentImage) + deltaY, newHeight);

                Canvas.SetLeft(CurrentImage, newLeft);
                Canvas.SetTop(CurrentImage, newTop);
            }
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (CurrentImage != null)
            {
                Point currentMousePosition = e.GetPosition(MainCanvas);
                LastMousePosition = currentMousePosition;
                IsStartMove = true;
            }
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (CurrentImage != null)
            {
                IsStartMove = false;
            }
        }

        private void CropButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentImage != null)
            {
                double centerX = (MainCanvas.ActualWidth - CropWidth) / 2;
                double centerY = (MainCanvas.ActualHeight - CropHeight) / 2;

                if (centerX < 0) centerX = 0;
                if (centerY < 0) centerY = 0;

                RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)Math.Max(MainCanvas.ActualWidth, CropWidth), (int)Math.Max(MainCanvas.ActualHeight, CropHeight), 96, 96, PixelFormats.Pbgra32);
                renderBitmap.Render(MainCanvas);

                CroppedBitmap croppedBitmap = new CroppedBitmap(
                    renderBitmap,
                    new Int32Rect((int)centerX, (int)centerY, (int)CropWidth, (int)CropHeight));

                SaveCroppedImage(croppedBitmap);
            }
            DialogResult = true;
        }

        private void SaveCroppedImage(BitmapSource croppedBitmap)
        {
            string fileName = Guid.NewGuid().ToString() + ".png";

            FileName = Path.Combine(CoverDir, fileName);
            Directory.CreateDirectory(CoverDir);
            using (var fileStream = new FileStream(FileName, FileMode.Create))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(croppedBitmap));
                encoder.Save(fileStream);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
