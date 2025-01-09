using AudioVisualizer.Core;
using AudioVisualizer.WPF.Utilities;
using FftSharp;
using Microsoft.VisualBasic;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
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

        DateTime _startTime;
        private DispatcherTimer dataTimer;
        private DispatcherTimer drawingTimer;

        Visualizer _visualizer;          // 可视化
        WasapiCapture _capture;
        double[]? _spectrumData;            // 频谱数据

        static readonly System.Windows.Media.Color[] allColors =
             ColorUtils.GetAllHsvColors();           // 渐变颜色

        private int colorIndex = 0;
        private double rotation = 0;

        public AudioVisualizerView()
        {
            InitializeComponent();
            _startTime = DateTime.Now;

            _visualizer = new Visualizer(512);               // 新建一个可视化器, 并使用 256 个采样进行傅里叶变换
        }

        public int AudioSampleRate { get; set; } = 8192;

        public float Scale { get; set; } = 1;

        public VisualEffect VisualEffict { get; set; }

        public void PushSampleData(double[] waveData)
        {
            _visualizer.PushSampleData(waveData);
        }

        public int SpectrumSize
        {
            get { return (int)GetValue(SpectrumSizeProperty); }
            set { SetValue(SpectrumSizeProperty, value); }
        }

        public int SpectrumSampleRate
        {
            get { return (int)GetValue(SpectrumSampleRateProperty); }
            set { SetValue(SpectrumSampleRateProperty, value); }
        }

        public int SpectrumBlurry
        {
            get { return (int)GetValue(SpectrumBlurryProperty); }
            set { SetValue(SpectrumBlurryProperty, value); }
        }

        public double SpectrumFactor
        {
            get { return (double)GetValue(SpectrumFactorProperty); }
            set { SetValue(SpectrumFactorProperty, value); }
        }


        public float ColorTransitionTime
        {
            get { return (float)GetValue(ColorTransitionTimeProperty); }
            set { SetValue(ColorTransitionTimeProperty, value); }
        }
        public float ColorGradientOffset
        {
            get { return (float)GetValue(ColorGradientOffsetProperty); }
            set { SetValue(ColorGradientOffsetProperty, value); }
        }
        public int StripCount
        {
            get { return (int)GetValue(StripCountProperty); }
            set { SetValue(StripCountProperty, value); }
        }
        public float StripSpacing
        {
            get { return (float)GetValue(StripSpacingProperty); }
            set { SetValue(StripSpacingProperty, value); }
        }

        public bool IsRendering
        {
            get { return (bool)GetValue(IsRenderingProperty.DependencyProperty); }
            private set { SetValue(IsRenderingProperty, value); }
        }
        public bool RenderEnabled
        {
            get { return (bool)GetValue(EnableRenderingProperty); }
            set { SetValue(EnableRenderingProperty, value); }
        }

        public int RenderInterval
        {
            get { return (int)GetValue(RenderIntervalProperty); }
            set { SetValue(RenderIntervalProperty, value); }
        }

        public int CircleStripCount
        {
            get { return (int)GetValue(CircleStripCountProperty); }
            set { SetValue(CircleStripCountProperty, value); }
        }
        public float CircleStripSpacing
        {
            get { return (float)GetValue(CircleStripSpacingProperty); }
            set { SetValue(CircleStripSpacingProperty, value); }
        }
        public double CircleStripRotationSpeed
        {
            get { return (double)GetValue(CircleStripRotationSpeedProperty); }
            set { SetValue(CircleStripRotationSpeedProperty, value); }
        }

        public bool EnableCurveRendering
        {
            get { return (bool)GetValue(EnableCurveProperty); }
            set { SetValue(EnableCurveProperty, value); }
        }
        public bool EnableStripsRendering
        {
            get { return (bool)GetValue(EnableStripsProperty); }
            set { SetValue(EnableStripsProperty, value); }
        }

        public bool EnableBorderRendering
        {
            get { return (bool)GetValue(EnableBorderDrawingProperty); }
            set { SetValue(EnableBorderDrawingProperty, value); }
        }

        public bool EnableCircleStripsRendering
        {
            get { return (bool)GetValue(EnableCircleStripsRenderingProperty); }
            set { SetValue(EnableCircleStripsRenderingProperty, value); }
        }

        public static readonly DependencyProperty EnableCurveProperty =
            DependencyProperty.Register(nameof(EnableCurveRendering), typeof(bool), typeof(AudioVisualizerView), new PropertyMetadata(true));
        public static readonly DependencyProperty EnableStripsProperty =
            DependencyProperty.Register(nameof(EnableStripsRendering), typeof(bool), typeof(AudioVisualizerView), new PropertyMetadata(true));
        public static readonly DependencyProperty EnableBorderDrawingProperty =
            DependencyProperty.Register(nameof(EnableBorderRendering), typeof(bool), typeof(AudioVisualizerView), new PropertyMetadata(true));
        public static readonly DependencyProperty EnableCircleStripsRenderingProperty =
            DependencyProperty.Register(nameof(EnableCircleStripsRendering), typeof(bool), typeof(AudioVisualizerView), new PropertyMetadata(true));





        public static readonly DependencyProperty SpectrumSizeProperty =
            DependencyProperty.Register(nameof(SpectrumSize), typeof(int), typeof(AudioVisualizerView), new PropertyMetadata(512, SpectrumSizeChanged));
        public static readonly DependencyProperty SpectrumSampleRateProperty =
            DependencyProperty.Register(nameof(SpectrumSampleRate), typeof(int), typeof(AudioVisualizerView), new PropertyMetadata(8192, SpectrumSampleRateChanged));
        public static readonly DependencyProperty SpectrumBlurryProperty =
            DependencyProperty.Register(nameof(SpectrumBlurry), typeof(int), typeof(AudioVisualizerView), new PropertyMetadata(0));
        public static readonly DependencyProperty SpectrumFactorProperty =
            DependencyProperty.Register(nameof(SpectrumFactor), typeof(double), typeof(AudioVisualizerView), new PropertyMetadata(1.0));
        public static readonly DependencyPropertyKey IsRenderingProperty =
            DependencyProperty.RegisterReadOnly(nameof(IsRendering), typeof(bool), typeof(AudioVisualizerView), new PropertyMetadata(false));
        public static readonly DependencyProperty EnableRenderingProperty =
            DependencyProperty.Register(nameof(RenderEnabled), typeof(bool), typeof(AudioVisualizerView), new PropertyMetadata(false, RenderEnableChanged));
        public static readonly DependencyProperty RenderIntervalProperty =
            DependencyProperty.Register(nameof(RenderInterval), typeof(int), typeof(AudioVisualizerView), new PropertyMetadata(10));
        public static readonly DependencyProperty ColorTransitionTimeProperty =
            DependencyProperty.Register(nameof(ColorTransitionTime), typeof(float), typeof(AudioVisualizerView), new PropertyMetadata(30f));
        public static readonly DependencyProperty ColorGradientOffsetProperty =
            DependencyProperty.Register(nameof(ColorGradientOffset), typeof(float), typeof(AudioVisualizerView), new PropertyMetadata(.1f));



        public static readonly DependencyProperty StripCountProperty =
            DependencyProperty.Register(nameof(StripCount), typeof(int), typeof(AudioVisualizerView), new PropertyMetadata(128));
        public static readonly DependencyProperty StripSpacingProperty =
            DependencyProperty.Register(nameof(StripSpacing), typeof(float), typeof(AudioVisualizerView), new PropertyMetadata(.2f));
        public static readonly DependencyProperty CircleStripCountProperty =
            DependencyProperty.Register(nameof(CircleStripCount), typeof(int), typeof(AudioVisualizerView), new PropertyMetadata(128));
        public static readonly DependencyProperty CircleStripSpacingProperty =
            DependencyProperty.Register(nameof(CircleStripSpacing), typeof(float), typeof(AudioVisualizerView), new PropertyMetadata(.2f));
        public static readonly DependencyProperty CircleStripRotationSpeedProperty =
            DependencyProperty.Register(nameof(CircleStripRotationSpeed), typeof(double), typeof(AudioVisualizerView), new PropertyMetadata(.5));

        private static void SpectrumSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not AudioVisualizerView audioVisualizerView ||
                e.NewValue is not int spectrumSize)
                return;

            if (audioVisualizerView.IsRendering)
                throw new InvalidOperationException($"{nameof(SpectrumSize)} on only be set while not rendering");

            audioVisualizerView._visualizer.Size = spectrumSize * 2;
        }

        private static void SpectrumSampleRateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not AudioVisualizerView audioVisualizerView ||
                e.NewValue is not int spectrumSampleRate)
                return;

            if (audioVisualizerView.IsRendering)
                throw new InvalidOperationException($"{nameof(SpectrumSampleRate)} on only be set while not rendering");

            audioVisualizerView._capture.WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(spectrumSampleRate, 1);
        }

        private static void RenderEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not AudioVisualizerView audioVisualizerView ||
                e.NewValue is not bool value)
                return;
#if DEBUG
            if (DesignerProperties.GetIsInDesignMode(d))
                return;
#endif

            if (value)
                audioVisualizerView.StartRenderAsync();
            else
                audioVisualizerView.StopRendering();
        }

        Task? renderTask;
        CancellationTokenSource? cancellation;

        private void StartRenderAsync()
        {
            if (renderTask != null)
                return;

            cancellation = new CancellationTokenSource();
            renderTask = RenderLoopAsync(cancellation.Token);
        }

        private async Task RenderLoopAsync(CancellationToken token)
        {
            IsRendering = true;
            _capture.StartRecording();

            while (true)
            {
                if (token.IsCancellationRequested)
                    break;

                _spectrumData = _visualizer.GetSpectrumData();

                if (SpectrumBlurry is int blurry and not 0)
                    _spectrumData = Visualizer.GetBlurry(_spectrumData, blurry);

                if (SpectrumFactor is double factor and not 1.0)
                    for (int i = 0; i < _spectrumData.Length; i++)
                        _spectrumData[i] *= factor;

                InvalidateVisual();

                await Task.Delay(RenderInterval);
            }

            _capture.StopRecording();
            IsRendering = false;
        }

        private void StopRendering()
        {
            cancellation?.Cancel();
            renderTask = null;
        }


        public void Start()
        {
            if (!started)
            {
                started = true;

                // 定时器更新数据
                dataTimer = new DispatcherTimer();
                dataTimer.Interval = TimeSpan.FromMilliseconds(0);  // 实时更新
                dataTimer.Tick += (sender, args) =>
                {
                    double[] newSpectrumData = _visualizer.GetSpectrumData();         // 从可视化器中获取频谱数据
                    newSpectrumData = Visualizer.GetBlurry(newSpectrumData, 2);                // 平滑频谱数据

                    _spectrumData = newSpectrumData;
                };
                dataTimer.Start();

                // 启动绘制定时器
                drawingTimer = new DispatcherTimer();
                drawingTimer.Interval = TimeSpan.FromMilliseconds(0);  // 试试绘制
                drawingTimer.Tick += (sender, args) =>
                {
                    if (_spectrumData == null)
                        return;

                    DrawSoundEffects();

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
            var audioBuffer = _spectrumData;
            GetCurrentColor(out Color color1, out Color color2);

            switch (VisualEffict)
            {
                case Core.VisualEffect.SpectrumCycle:
                    spectrumBarFormCanvas.Visibility = Visibility.Collapsed;
                    Dispatcher.Invoke((Delegate)(() => cycleWaveformCanvas.UpdateAudioData(_spectrumData, StripCount, StripSpacing, color1, color2)));
                    break;

                case Core.VisualEffect.SpectrumBar:
                    cycleWaveformCanvas.Visibility = Visibility.Collapsed;
                    Dispatcher.Invoke((Delegate)(() => spectrumBarFormCanvas.UpdateAudioData(_spectrumData, StripCount, StripSpacing, color1, color2)));
                    DrawLine(color1);
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
            GetCurrentColor(out Color color1, out Color color2);
            switch (effect)
            {
                case Core.VisualEffect.SpectrumCycle:
                    //DrawCircle();
                    break;

                case Core.VisualEffect.SpectrumBar:
                    DrawLine(color1);
                    break;
                default:
                    break;
            }
        }
        

        private void DrawLine(Color gradientColor)
        {
            double canvasWidth = spectrumBarFormCanvas.ActualWidth;
            double canvasHeight = spectrumBarFormCanvas.ActualHeight;

            if (canvasWidth == 0 || canvasHeight == 0)
                return;

            // 创建线性渐变刷子并传入单一颜色
            LinearGradientBrush gradientBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0)
            };
            gradientBrush.GradientStops.Add(new GradientStop(gradientColor, 0.0));  // 起始颜色
            gradientBrush.GradientStops.Add(new GradientStop(gradientColor, 1.0));  // 结束颜色

            // 创建线并设置位置、颜色等属性
            Line line = new Line
            {
                X1 = 10,
                Y1 = canvasHeight / 2,
                X2 = canvasWidth - 10,
                Y2 = canvasHeight / 2,
                Stroke = gradientBrush,
                StrokeThickness = 2
            };

            // 将线添加到画布中
            spectrumBarFormCanvas.Children.Add(line);
        }


        





        private void GetCurrentColor(out Color color1, out Color color2)
        {
            double time = (DateTime.Now - _startTime).TotalSeconds;
            double rate = time / ColorTransitionTime;

            color1 = GetColorFromRate(rate);
            color2 = GetColorFromRate(rate + ColorGradientOffset);
        }

        private Color GetColorFromRate(double rate)
        {
            if (rate < 0)
                rate = rate % 1 + 1;
            else
                rate = rate % 1;

            int maxIndex = allColors.Length - 1;
            return allColors[(int)(maxIndex * rate)];
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
