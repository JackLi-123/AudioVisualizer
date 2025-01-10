using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Windows;

namespace AudioVisualizer.WPF.Sample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WasapiCapture capture;
        private WaveInEvent speechWaveIn;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Speaker voice capture, Specify capture wave format: mono, 32-bit depth, IeeeFloat encoding, 8192 sample rate.
            capture = new WasapiLoopbackCapture()
            {
                WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(8192, 1)
            };
            capture.DataAvailable += Capture_DataAvailable;

            // Mic speech capture 
            speechWaveIn = new WaveInEvent
            {
                WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(8192, 1)
            };
            speechWaveIn.DataAvailable += Capture_DataAvailable;


            audioVisualizerView.AudioSampleRate = capture.WaveFormat.SampleRate;
            audioVisualizerView.Scale = 5;
            audioVisualizerView.VisualEffect = Core.Enum.VisualEffect.SpectrumBar;

            audioVisualizerView.StartRenderAsync();
            capture.StartRecording();
            speechWaveIn.StartRecording();
            
        }

        /// <summary>
        /// Push audio micophone data into visualizer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Capture_DataAvailable(object? sender, WaveInEventArgs e)
        {
            int length = e.BytesRecorded / 4;           // Float data
            double[] result = new double[length];

            for (int i = 0; i < length; i++)
                result[i] = BitConverter.ToSingle(e.Buffer, i * 4);

            // Push into visualizer
            audioVisualizerView.PushSampleData(result);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            capture.StopRecording();
            audioVisualizerView.Stop();
            speechWaveIn.StopRecording();
        }
    }
}