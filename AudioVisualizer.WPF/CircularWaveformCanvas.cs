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

            // 计算画布的中心点
            double centerX = width / 2;
            double centerY = height / 2;

            // 半径决定条形的散开范围
            double radius = (Math.Min(centerX, centerY) - 10) * 0.62;

            // 动态条形绘制
            double barWidth = 5; // 每个条形的宽度

            // 角度间隔，确保条形均匀分布在圆周上
            double angleStep = 360.0 / audioData.Length;

            // 绘制每个条形
            for (int i = 0; i < audioData.Length; i++)
            {
                // 计算条形的高度（通过音频数据缩放）
                double barHeight = Math.Abs(audioData[i] * height) * 30; // 根据音频数据决定条形高度

                // 计算当前条形的角度
                double angle = i * angleStep;

                // 将角度转换为弧度
                double radians = Math.PI * angle / 180.0;

                // 根据角度和半径计算条形的起点（圆周位置）
                double startX = centerX + radius * Math.Cos(radians); // 起点的 x 坐标
                double startY = centerY + radius * Math.Sin(radians); // 起点的 y 坐标

                double endX = centerX + (radius + barHeight) * Math.Cos(radians); // 终点的 x 坐标
                double endY = centerY + (radius + barHeight) * Math.Sin(radians); // 终点的 y 坐标

                // 绘制条形
                Pen pen = new Pen(Brushes.Green, barWidth); // 设置条形颜色和宽度
                dc.DrawLine(pen, new Point(startX, startY), new Point(endX, endY)); // 绘制从起点到终点的线条
            }
        }



    }
}
