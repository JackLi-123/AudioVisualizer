## **Getting Started**

### **Step 1: Import the Core and WinForms SDK**

```c#
using AudioVisualizer;
using AudioVisualizer.WinForm;
```

### **Step 2: Add the RealtimeApiWinFormControl Control**

Drag and drop the `AudioVisualizerView` onto your form or add it programmatically:

```c#
AudioVisualizerView audioVisualizer = new AudioVisualizerView();
this.Controls.Add(audioVisualizer );
```

### **Step 3: Get Hook up microphone and speaker**

```c#
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
```

## **License**

Licensed under the [MIT](LICENSE) License.

 
