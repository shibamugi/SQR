using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows.Interop;
using ZXing;
using ZXing.Windows.Compatibility;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;

namespace Screen_QRcode_Reader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void open_link_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ProcessStartInfo url_open = new ProcessStartInfo()
                {
                    FileName = this.url_out.Text,
                    UseShellExecute = true,

                };
                System.Diagnostics.Process.Start(url_open);
            }
            catch
            {
                this.status.Text = "このURLは開けません(e:001)";
            }
        }

        private void move_space_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ButtonState != MouseButtonState.Pressed) return;
                this.DragMove();
            }
            catch
            {
                this.status.Text = "予期せぬトラブルが発生しました(e:002)";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void small_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WindowState = WindowState.Minimized;
            }
            catch
            {
                this.status.Text = "予期せぬトラブルが発生しました(e:003)";
            }
        }

        private BitmapSource? screen_cap()
        {
            try
            {
                var search_point = screen_view_point.PointToScreen(new System.Windows.Point(0.0d, 0.0d));
                var search_area = new Rect(search_point.X, search_point.Y, screen_view_point.ActualWidth, screen_view_point.ActualHeight);
                var view_scale = Dpi_Scale();
                BitmapSource cap_out;

                View_Search_Area(null);
                using (var bmp = new Bitmap((int)(search_area.Width * view_scale.X), (int)(search_area.Height * view_scale.Y)))
                using (var graph = Graphics.FromImage(bmp))
                {
                    graph.CopyFromScreen((int)search_area.X, (int)search_area.Y, 0, 0, bmp.Size);
                    cap_out = Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }

                return cap_out;
            }
            catch
            {
                return null;
            }
        }

        private void QRcode_screen_view()
        {
            try
            {
                BitmapSource? cap_in;
                cap_in = screen_cap();

                if (cap_in != null)
                {
                    var reader = new BarcodeReader()
                    {
                        Options = {
                            PossibleFormats = new[] { BarcodeFormat.QR_CODE }.ToList() }
                    };


                    var result = reader.Decode(cap_in);
                    if (result != null)
                    {
                        this.url_out.Text = result.Text;
                        this.status.Text = "QRコードを検出しました";
                        
                        if (this.url_out.Text.Length >= 7)
                        {
                            if (this.url_out.Text.Length >= 8)
                            {
                                if (this.url_out.Text.Substring(0, 8) == "https://" || this.url_out.Text.Substring(0, 7) == "http://")
                                {
                                    this.open_link.IsEnabled = true;
                                }
                                else this.open_link.IsEnabled = false;
                            }
                            else
                            {
                                if (this.url_out.Text.Substring(0, 7) == "http://")
                                {
                                    this.open_link.IsEnabled = true;
                                }
                                else this.open_link.IsEnabled = false;
                            }
                        }
                        else this.open_link.IsEnabled = false;

                        ZXing.ResultPoint[]? points = result.ResultPoints;
                        View_Search_Area(points);
                    }
                    else
                    {
                        this.url_out.Text = null;
                        this.status.Text = "QRコードを検出できませんでした";
                        this.open_link.IsEnabled = false;
                    }
                }
                else
                {
                    this.status.Text = "スキャンを開始できませんでした(e:004)";
                }
            }
            catch
            {
                this.status.Text = "スキャンを開始できませんでした(e:005)";
            }
        }

        private void scan_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                QRcode_screen_view();
            }
            catch
            {
                this.status.Text = "スキャンを開始できませんでした(e:006)";
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    this.WindowState = WindowState.Normal;
                }
            }
            catch
            {
                this.status.Text = "予期せぬトラブルが発生しました(e:007)";
            }
        }

        private System.Windows.Point Dpi_Scale()
        {
            PresentationSource src = PresentationSource.FromVisual(this);
            if (src != null)
            {
                return new System.Windows.Point(src.CompositionTarget.TransformToDevice.M11, src.CompositionTarget.TransformToDevice.M22);
            }
            else
            {
                return new System.Windows.Point(1.0, 1.0);
            }
        }

        private void View_Search_Area(ZXing.ResultPoint[]? points)
        {
            QR_Area.Points.Clear();
            if (points != null)
            {
                QR_Area.Points.Clear();
                var view_scale = Dpi_Scale();
                for (int i = 0; i < 3; i++)
                {
                    QR_Area.Points.Add(new System.Windows.Point(points[i].X / view_scale.X, points[i].Y / view_scale.Y));
                }
                QR_Area.Points.Add(new System.Windows.Point((points[0].X + (points[2].X - points[1].X)) / view_scale.X, (points[2].Y + (points[0].Y - points[1].Y)) / view_scale.Y));
            }
        }
    }
}
