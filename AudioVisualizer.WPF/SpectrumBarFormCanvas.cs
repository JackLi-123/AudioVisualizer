using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AudioVisualizer.WPF
{
    internal class SpectrumBarFormCanvas : Canvas
    {
        private double[] audioData;

        public void UpdateAudioData(double[] data)
        {
            audioData = data;
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (audioData == null || audioData.Length == 0)
                return;

            double width = ActualWidth;
            double height = ActualHeight;

            // Calculate the middle point
            double centerY = height / 2;

            Pen pen = new Pen(Brushes.Blue, 1);
            Brush brush = Brushes.Black;

            // Dynamic bar drawing
            double barWidth = width / audioData.Length; // Each bar's width
            double barSpacing = 2; // Space between bars (optional)

            // Iterate over the audio data and draw bars
            for (int i = 0; i < audioData.Length; i++)
            {
                double barHeight = Math.Abs(audioData[i] * height) * 30;  // Scale the data to the height of the canvas
                double x = i * (barWidth + barSpacing);    // Calculate the x position for the bar

                // Adjust the Y position so that the bars start from the middle and only go upwards
                double y = centerY - barHeight; // The bars will only expand upwards from the middle

                // Draw the bar
                dc.DrawRectangle(Brushes.Green, null, new Rect(x, y, barWidth, barHeight));
            }
        }
    }
}
