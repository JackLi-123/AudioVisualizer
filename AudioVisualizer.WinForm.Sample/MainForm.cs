using AudioVisualizer.Core.Enum;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Windows.Forms;

namespace AudioVisualizer.WinForm.Sample
{
    public partial class MainForm : Form
    {
        private WasapiCapture capture;
        private WaveInEvent speechWaveIn;
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
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

            audioVisualizer1.AudioSampleRate = capture.WaveFormat.SampleRate;
            audioVisualizer1.Scale = 5;
            audioVisualizer1.VisualEffect = VisualEffect.SpectrumBar;

            audioVisualizer1.Start();
            capture.StartRecording();
            speechWaveIn.StartRecording();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            capture.StopRecording();
            audioVisualizer1.Stop();
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
            audioVisualizer1.PushSampleData(result);
        }
    }
}
