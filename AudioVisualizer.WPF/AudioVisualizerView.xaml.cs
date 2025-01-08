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
using System.Windows.Threading;
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
        Color[] allColors;                 // 渐变颜色
        public AudioVisualizerView()
        {
            InitializeComponent();
        }

        public int AudioSampleRate { get; set; } = 8192;

        public float Scale { get; set; } = 1;

        public VisualEffect VisualEffict { get; set; }

        public void PushSampleData(double[] waveData)
        {
            if (visualizer != null)
            {
                visualizer.PushSampleData(waveData);
                // 更新音频数据，并触发绘制
                _sampleData = waveData.Select(d => Math.Abs((float)d)).ToList();
                // 使用 Dispatcher 在 UI 线程中调用 InvalidateVisual
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    AudioVisualizerCanvas.InvalidateVisual();
                });
            }
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
                    // 模拟音频数据的实时推送
                    // 这里你可以通过从音频源获取数据，模拟推送数据到可视化器
                    // visualizer.PushSampleData(newData);
                    //_sampleData = visualizer.GetSampleData();
                    this.InvalidateVisual();
                };
                dataTimer.Start();

                // 启动绘制定时器
                drawingTimer = new DispatcherTimer();
                drawingTimer.Interval = TimeSpan.FromMilliseconds(16);  // 每16毫秒绘制一次，约60fps
                drawingTimer.Tick += (sender, args) =>
                {
                    this.InvalidateVisual();  // 重新绘制
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
            if (!IsInDesignMode())
            {
                visualizer = new Visualizer(256);               // 新建一个可视化器, 并使用 256 个采样进行傅里叶变换
                allColors = GetAllHsvColors();                  // 获取所有的渐变颜色 (HSV 颜色)

            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (_sampleData.Count == 0)
                return;

            var width = AudioVisualizerCanvas.ActualWidth;
            var height = AudioVisualizerCanvas.ActualHeight;
            var maxSampleValue = 1f;  // 假设最大频谱值为1

            // 创建渐变画刷
            var gradientBrush = new LinearGradientBrush(
                Colors.Blue, Colors.Cyan, 45);

            // 根据当前的视觉效果选择绘制方式
            switch (VisualEffict)
            {
                //case Core.VisualEffect.Oscilloscope:
                //    DrawOscilloscope(drawingContext, width, height, maxSampleValue, gradientBrush);
                //    break;

                case Core.VisualEffect.SpectrumBar:
                    DrawSpectrumBar(drawingContext, width, height, maxSampleValue, gradientBrush);
                    break;

                case Core.VisualEffect.SpectrumCycle:
                    DrawSpectrumCycle(drawingContext, width, height, maxSampleValue, gradientBrush);
                    break;
            }
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

        //private void DrawOscilloscope(DrawingContext drawingContext, double width, double height, float maxSampleValue, Brush brush)
        //{
        //    var halfHeight = height / 2;
        //    var step = width / _sampleData.Count;

        //    // 绘制波形图
        //    for (int i = 0; i < _sampleData.Count; i++)
        //    {
        //        float y = halfHeight - (_sampleData[i] * halfHeight);
        //        drawingContext.DrawLine(
        //            new Pen(brush, 1),
        //            new Point(i * step, halfHeight),
        //            new Point(i * step, y)
        //        );
        //    }
        //}

        private void DrawSpectrumBar(DrawingContext drawingContext, double width, double height, float maxSampleValue, Brush brush)
        {


            var barWidth = width / _sampleData.Count;// 每个条形的宽度
            var centerY = height / 2; // 以黑色区域的垂直中心为基准
            for (int i = 0; i < _sampleData.Count; i++)
            {
                if (_sampleData.Count != 0 && _sampleData[i] != 0)
                {
                    var barHeight = Math.Abs(_sampleData[i]) * height / 2; // 高度是总高度的一半
                    var x = i * barWidth; // 每个条形的 X 坐标
                    var y = centerY - barHeight; // 从中心向上绘制

                    // 绘制矩形条形
                    drawingContext.DrawRectangle(brush, null, new Rect(x, y, barWidth, barHeight));
                }
            }
        }

        private void DrawSpectrumCycle(DrawingContext drawingContext, double width, double height, float maxSampleValue, Brush brush)
        {
            var centerX = width / 2;
            var centerY = height / 2;
            var radius = Math.Min(width, height) / 3;

            // 绘制环形频谱
            for (int i = 0; i < _sampleData.Count; i++)
            {
                var angle = i * (360.0 / _sampleData.Count);
                var x = centerX + radius * Math.Cos(angle * Math.PI / 180);
                var y = centerY + radius * Math.Sin(angle * Math.PI / 180);
                var size = _sampleData[i] * 10;  // 影响波形的大小
                drawingContext.DrawEllipse(brush, null, new Point(x, y), size, size);
            }
        }
    }

}
