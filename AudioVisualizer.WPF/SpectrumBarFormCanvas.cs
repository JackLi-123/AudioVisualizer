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
        private double[]? _spectrumData;
        private int _stripCount;
        private float _stripSpacing;
        private Color _color1;
        private Color _color2;


        public void UpdateAudioData(double[] spectrumData, int stripCount, float stripSpacing, Color color1, Color color2)
        {
            _spectrumData = spectrumData;
            _stripCount = stripCount;
            _stripSpacing = stripSpacing;
            _color1 = color1;
            _color2 = color2;
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            int stripCount = _stripCount;
            double thickness = ActualWidth / _stripCount * (1 - _stripSpacing);

            if (thickness < 0)
                thickness = 1;

            double middleY = ActualHeight / 2;  // 中间位置
            double amplitudeFactor = 50;  // 控制幅度的因子

            PathGeometry pathGeometry = new PathGeometry();

            int end = stripCount - 1;
            for (int i = 0; i < stripCount; i++)
            {
                double value = _spectrumData[i * _spectrumData.Length / stripCount];
                double y = Math.Round(ActualHeight / 2 * (1 - value * amplitudeFactor), 1);
                double x = ((double)i / end) * ActualWidth;

                if (y > middleY)
                {
                    y = middleY;
                }

                pathGeometry.Figures.Add(new PathFigure()
                {
                    StartPoint = new Point(x, ActualHeight / 2),
                    Segments =
                            {
                                new LineSegment()
                                {
                                    Point = new Point(x, y)
                                }
                            }
                });
            }


            // Create gradient brush for coloring the bars
            GradientBrush brush = new LinearGradientBrush(_color1, _color2, new Point(0, 1), new Point(0, 0));
            Pen pen = new Pen(brush, thickness);

            // Draw the geometry on the canvas
            dc.DrawGeometry(null, pen, pathGeometry);
        }

    }
}
