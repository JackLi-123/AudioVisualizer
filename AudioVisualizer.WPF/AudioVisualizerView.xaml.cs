using AudioVisualizer.Core;
using FftSharp;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Formats.Asn1.AsnWriter;
using Color = System.Drawing.Color;
using Point = System.Windows.Point;

namespace AudioVisualizer.WPF
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class AudioVisualizerView : UserControl
    {
        private bool started = false;
        private List<float> _sampleData = new List<float>();  // 音频数据

        private DispatcherTimer dataTimer;
        private DispatcherTimer drawingTimer;

        Visualizer visualizer;          // 可视化
        double[]? spectrumData;            // 频谱数据

        Color[] allColors;                 // 渐变颜色

        private int colorIndex = 0;
        private double rotation = 0;

        public AudioVisualizerView()
        {
            InitializeComponent();

            if (!IsInDesignMode())
            {
                visualizer = new Visualizer(256);               // 新建一个可视化器, 并使用 256 个采样进行傅里叶变换
                allColors = GetAllHsvColors();                  // 获取所有的渐变颜色 (HSV 颜色)

            }
        }

        public int AudioSampleRate { get; set; } = 8192;

        public float Scale { get; set; } = 1;

        public VisualEffect VisualEffict { get; set; }

        public void PushSampleData(double[] waveData)
        {
            visualizer.PushSampleData(waveData);
        }

        public void Start()
        {
            if (!started)
            {
                started = true;
                // 初始化可视化器
                //visualizer = new Visualizer(256);
                //allColors = GetAllHsvColors();

                // 定时器更新数据
                dataTimer = new DispatcherTimer();
                dataTimer.Interval = TimeSpan.FromMilliseconds(50);  // 每50毫秒更新一次
                dataTimer.Tick += (sender, args) =>
                {
                    double[] newSpectrumData = visualizer.GetSpectrumData();         // 从可视化器中获取频谱数据
                    newSpectrumData = Visualizer.GetBlurry(newSpectrumData, 2);                // 平滑频谱数据

                    spectrumData = newSpectrumData;
                };
                dataTimer.Start();

                // 启动绘制定时器
                drawingTimer = new DispatcherTimer();
                drawingTimer.Interval = TimeSpan.FromMilliseconds(50);  // 每16毫秒绘制一次，约60fps
                drawingTimer.Tick += (sender, args) =>
                {
                    if (spectrumData == null)
                        return;

                    DrawSoundEffects();
                    //this.InvalidateVisual();  // 重新绘制


                };
                drawingTimer.Start();

            }
        }

        public void Stop()
        {
            if (!started)
            {
                started = false;

                // 停止定时器
                dataTimer?.Stop();
                drawingTimer?.Stop();
            }
        }
        public void DrawSoundEffects() 
        {
            var audioBuffer = spectrumData;
            switch (VisualEffict)
            {
                case Core.VisualEffect.SpectrumCycle:
                    spectrumBarFormCanvas.Visibility = Visibility.Collapsed;
                    Dispatcher.Invoke((Delegate)(() => cycleWaveformCanvas.UpdateAudioData(audioBuffer)));
                    break;

                case Core.VisualEffect.SpectrumBar:
                    cycleWaveformCanvas.Visibility = Visibility.Collapsed;
                    Dispatcher.Invoke((Delegate)(() => spectrumBarFormCanvas.UpdateAudioData(audioBuffer)));
                    break;
                default:
                    break;
            }
        }


        private static bool IsInDesignMode()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return true;
            }

            string processName = Process.GetCurrentProcess().ProcessName.ToLower();
            if (processName.Contains("devenv") || processName.Contains("blend"))
            {
                return true;
            }

            return false;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            DrawDefaultVisualEffect(VisualEffict);
        }

        private void DrawDefaultVisualEffect(VisualEffect effect)
        {
            cycleWaveformCanvas.Children.Clear();
            spectrumBarFormCanvas.Children.Clear();

            switch (effect)
            {
                case Core.VisualEffect.SpectrumCycle:
                    DrawCircle();
                    break;

                case Core.VisualEffect.SpectrumBar:
                    DrawLine();
                    break;
                default:
                    break;
            }
        }

        private void DrawCircle(double sizeFactor = 0.6)
        {
            double canvasWidth = cycleWaveformCanvas.ActualWidth;
            double canvasHeight = cycleWaveformCanvas.ActualHeight;
            if (canvasWidth == 0 || canvasHeight == 0)
                return;

            double radius = Math.Min(canvasWidth, canvasHeight) * sizeFactor / 2;
            double centerX = canvasWidth / 2;
            double centerY = canvasHeight / 2;

            Ellipse circle = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Stroke = Brushes.LimeGreen,
                StrokeThickness = 3
            };

            Canvas.SetLeft(circle, centerX - radius);
            Canvas.SetTop(circle, centerY - radius);

            cycleWaveformCanvas.Children.Clear();

            cycleWaveformCanvas.Children.Add(circle);
        }

        private void DrawLine()
        {
            double canvasWidth = spectrumBarFormCanvas.ActualWidth;
            double canvasHeight = spectrumBarFormCanvas.ActualHeight;

            if (canvasWidth == 0 || canvasHeight == 0)
                return;


            LinearGradientBrush gradientBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0)
            };
            gradientBrush.GradientStops.Add(new GradientStop(Colors.Purple, 0.0));
            gradientBrush.GradientStops.Add(new GradientStop(Colors.Orange, 0.5));
            gradientBrush.GradientStops.Add(new GradientStop(Colors.Yellow, 1.0));

            Line line = new Line
            {
                X1 = 10,
                Y1 = canvasHeight / 2,
                X2 = canvasWidth - 10,
                Y2 = canvasHeight / 2,
                Stroke = gradientBrush,
                StrokeThickness = 2
            };

            spectrumBarFormCanvas.Children.Add(line);
        }

        


        /// <summary>
        /// 获取 HSV 中所有的基础颜色 (饱和度和明度均为最大值)
        /// </summary>
        /// <returns>所有的 HSV 基础颜色(共 256 * 6 个, 并且随着索引增加, 颜色也会渐变)</returns>
        private Color[] GetAllHsvColors()
        {
            Color[] result = new Color[256 * 6];

            for (int i = 0; i < 256; i++)
            {
                result[i] = Color.FromArgb(255, i, 0);
            }

            for (int i = 0; i < 256; i++)
            {
                result[256 + i] = Color.FromArgb(255 - i, 255, 0);
            }

            for (int i = 0; i < 256; i++)
            {
                result[512 + i] = Color.FromArgb(0, 255, i);
            }

            for (int i = 0; i < 256; i++)
            {
                result[768 + i] = Color.FromArgb(0, 255 - i, 255);
            }

            for (int i = 0; i < 256; i++)
            {
                result[1024 + i] = Color.FromArgb(i, 0, 255);
            }

            for (int i = 0; i < 256; i++)
            {
                result[1280 + i] = Color.FromArgb(255, 0, 255 - i);
            }

            return result;
        }

        private void cycleWaveformCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawDefaultVisualEffect(VisualEffict);
        }

        private void spectrumBarFormCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawDefaultVisualEffect(VisualEffict);
        }
    }

}
