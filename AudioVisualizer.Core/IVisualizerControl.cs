using AudioVisualizer.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioVisualizer.Core
{
    public interface IVisualizerControl
    {
        void Start();

        void Stop();

        void PushSampleData(double[] audioData);

        int AudioSampleRate { get; set; }

        float Scale { get; set; }

        VisualEffect VisualEffect { get; set; }

    }
}
