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
        private List<float> _sampleData = new List<float>();  // ��Ƶ����

        private DispatcherTimer dataTimer;
        private DispatcherTimer drawingTimer;

        Visualizer visualizer;          // ���ӻ�
        Color[] allColors;                 // ������ɫ
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
                // ������Ƶ���ݣ�����������
                _sampleData = waveData.Select(d => Math.Abs((float)d)).ToList();
                // ʹ�� Dispatcher �� UI �߳��е��� InvalidateVisual
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
                // ��ʼ�����ӻ���
                //visualizer = new Visualizer(256);
                //allColors = GetAllHsvColors();

                // ��ʱ����������
                dataTimer = new DispatcherTimer();
                dataTimer.Interval = TimeSpan.FromMilliseconds(50);  // ÿ50�������һ��
                dataTimer.Tick += (sender, args) =>
                {
                    // ģ����Ƶ���ݵ�ʵʱ����
                    // ���������ͨ������ƵԴ��ȡ���ݣ�ģ���������ݵ����ӻ���
                    // visualizer.PushSampleData(newData);
                    //_sampleData = visualizer.GetSampleData();
                    this.InvalidateVisual();
                };
                dataTimer.Start();

                // �������ƶ�ʱ��
                drawingTimer = new DispatcherTimer();
                drawingTimer.Interval = TimeSpan.FromMilliseconds(16);  // ÿ16�������һ�Σ�Լ60fps
                drawingTimer.Tick += (sender, args) =>
                {
                    this.InvalidateVisual();  // ���»���
                };
                drawingTimer.Start();

            }
        }

        public void Stop()
        {
            if (!started)
            {
                started = false;

                // ֹͣ��ʱ��
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
                visualizer = new Visualizer(256);               // �½�һ�����ӻ���, ��ʹ�� 256 ���������и���Ҷ�任
                allColors = GetAllHsvColors();                  // ��ȡ���еĽ�����ɫ (HSV ��ɫ)

            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (_sampleData.Count == 0)
                return;

            var width = AudioVisualizerCanvas.ActualWidth;
            var height = AudioVisualizerCanvas.ActualHeight;
            var maxSampleValue = 1f;  // �������Ƶ��ֵΪ1

            // �������仭ˢ
            var gradientBrush = new LinearGradientBrush(
                Colors.Blue, Colors.Cyan, 45);

            // ���ݵ�ǰ���Ӿ�Ч��ѡ����Ʒ�ʽ
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
        /// ��ȡ HSV �����еĻ�����ɫ (���ͶȺ����Ⱦ�Ϊ���ֵ)
        /// </summary>
        /// <returns>���е� HSV ������ɫ(�� 256 * 6 ��, ����������������, ��ɫҲ�ὥ��)</returns>
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

        //    // ���Ʋ���ͼ
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


            var barWidth = width / _sampleData.Count;// ÿ�����εĿ��
            var centerY = height / 2; // �Ժ�ɫ����Ĵ�ֱ����Ϊ��׼
            for (int i = 0; i < _sampleData.Count; i++)
            {
                if (_sampleData.Count != 0 && _sampleData[i] != 0)
                {
                    var barHeight = Math.Abs(_sampleData[i]) * height / 2; // �߶����ܸ߶ȵ�һ��
                    var x = i * barWidth; // ÿ�����ε� X ����
                    var y = centerY - barHeight; // ���������ϻ���

                    // ���ƾ�������
                    drawingContext.DrawRectangle(brush, null, new Rect(x, y, barWidth, barHeight));
                }
            }
        }

        private void DrawSpectrumCycle(DrawingContext drawingContext, double width, double height, float maxSampleValue, Brush brush)
        {
            var centerX = width / 2;
            var centerY = height / 2;
            var radius = Math.Min(width, height) / 3;

            // ���ƻ���Ƶ��
            for (int i = 0; i < _sampleData.Count; i++)
            {
                var angle = i * (360.0 / _sampleData.Count);
                var x = centerX + radius * Math.Cos(angle * Math.PI / 180);
                var y = centerY + radius * Math.Sin(angle * Math.PI / 180);
                var size = _sampleData[i] * 10;  // Ӱ�첨�εĴ�С
                drawingContext.DrawEllipse(brush, null, new Point(x, y), size, size);
            }
        }
    }

}
