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
    internal class CircularWaveformCanvas : Canvas
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


        //protected override void OnRender(DrawingContext dc)
        //{
        //    int stripCount = _stripCount;
        //    double thickness = ActualWidth / _stripCount * (1 - _stripSpacing);

        //    if (thickness < 0)
        //        thickness = 1;

        //    double middleY = ActualHeight / 2;  // 中间位置
        //    double amplitudeFactor = 50;  // 控制幅度的因子
        //    double radius = thickness / 2;  // 圆形的半径

        //    for (int i = 0; i < stripCount; i++)
        //    {
        //        double value = _spectrumData[i * _spectrumData.Length / stripCount];
        //        double y = Math.Round(middleY * (1 - value * amplitudeFactor), 1);  // 控制圆形的高度
        //        double x = ((double)i / (stripCount - 1)) * ActualWidth;

        //        // 设置圆形的中心坐标
        //        Point center = new Point(x, middleY);

        //        // 计算圆形的半径
        //        double circleRadius = Math.Abs(middleY - y);  // 计算到中间位置的距离

        //        // 使用 EllipseGeometry 绘制圆形
        //        EllipseGeometry ellipse = new EllipseGeometry(center, circleRadius, circleRadius);

        //        // 使用渐变色填充圆形
        //        GradientBrush brush = new LinearGradientBrush(_color1, _color2, new Point(0, 1), new Point(0, 0));
        //        Pen pen = new Pen(brush, thickness);

        //        // 绘制圆形
        //        dc.DrawGeometry(null, pen, ellipse);
        //    }
        //}



        protected override void OnRender(DrawingContext dc)
        {
            if (_spectrumData == null || _spectrumData.Length == 0 || _stripCount <= 0)
                return;

            double canvasWidth = ActualWidth;
            double canvasHeight = ActualHeight;

            if (canvasWidth <= 0 || canvasHeight <= 0)
                return;

            double centerX = canvasWidth / 2; // 圆心 X
            double centerY = canvasHeight / 2; // 圆心 Y
            double baseRadius = Math.Min(canvasWidth, canvasHeight) / 3.5; // 圆形的基础半径
            double amplitudeFactor = 50; // 增大条形高度的因子
            double thickness = (Math.PI * 2 * baseRadius) / (_stripCount * (1 + _stripSpacing)); // 动态调整宽度
            double angleStep = 360.0 / _stripCount; // 确保完整覆盖360度

            // 绘制内圆
            Pen circlePen = new Pen(new SolidColorBrush(_color1), 3);
            dc.DrawEllipse(null, circlePen, new Point(centerX, centerY), baseRadius, baseRadius);

            // 创建渐变画笔
            LinearGradientBrush gradientBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0)
            };
            gradientBrush.GradientStops.Add(new GradientStop(_color1, 0.0));
            gradientBrush.GradientStops.Add(new GradientStop(_color2, 1.0));
            Pen barPen = new Pen(gradientBrush, thickness);

            // 检查音频数据是否有效（全为零或接近零）
            bool isSilent = _spectrumData.All(value => Math.Abs(value) < 0.001); // 设置一个小阈值，表示无声音状态

            // 绘制条形
            for (int i = 0; i < _stripCount; i++)
            {
                double angle = i * angleStep; // 计算条形的角度
                double angleInRadians = angle * Math.PI / 180.0; // 转换为弧度

                double xStart = centerX + baseRadius * Math.Cos(angleInRadians);
                double yStart = centerY + baseRadius * Math.Sin(angleInRadians);

                double barLength;
                if (isSilent)
                {
                    barLength = 0; // 如果是无声音状态，条形长度保持为 0
                }
                else
                {
                    double maxAmplitude = _spectrumData.Max();
                    double normalizedValue = maxAmplitude > 0 ? _spectrumData[i % _spectrumData.Length] / maxAmplitude : 0; // 归一化数据
                    barLength = Math.Max(amplitudeFactor * normalizedValue, 0); // 动态高度
                }

                double xEnd = centerX + (baseRadius + barLength) * Math.Cos(angleInRadians);
                double yEnd = centerY + (baseRadius + barLength) * Math.Sin(angleInRadians);

                dc.DrawLine(barPen, new Point(xStart, yStart), new Point(xEnd, yEnd));
            }
        }


    }
}
