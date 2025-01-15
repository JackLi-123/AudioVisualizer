using AudioVisualizer.Core;
using AudioVisualizer.Core.Enum;

//using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AudioVisualizer.WinForm
{
    public partial class AudioVisualizerView : UserControl
    {
        private bool started = false;
        private Timer dataTimer = new Timer();
        private Timer drawingTimer = new Timer();

        private int colorIndex = 0;
        private double rotation = 0;
        private BufferedGraphics? oldBuffer;


        Visualizer visualizer;             // Visualization
        double[]? spectrumData;            // Spectrum data

        Color[] allColors;                 // Gradient Color

        public AudioVisualizerView()
        {
            InitializeComponent();
        }
        public int AudioSampleRate { get; set; } = 8192;

        public float Scale { get; set; } = 5;

        public int RenderInterval
        {

            get { return dataTimer.Interval; }
            set
            {
                dataTimer.Interval = value;
                drawingTimer.Interval = value;
            }
        }

        public VisualEffect VisualEffect { get; set; }

        public void PushSampleData(double[] waveData)
        {
            visualizer.PushSampleData(waveData);
        }

        private void AudioVisualizer_Load(object sender, EventArgs e)
        {
            this.BackColor = Color.Black;
            if (!IsInDesignMode())
            {
                visualizer = new Visualizer(256);               // Create a new visualizer and perform Fourier transform using 256 samples

                allColors = GetAllHsvColors();                  // Get all gradient colors (HSV colors)


                dataTimer.Interval = 50;
                dataTimer.Tick += new System.EventHandler(this.DataTimer_Tick);

                drawingTimer.Interval = 50;
                drawingTimer.Tick += new System.EventHandler(this.DrawingTimer_Tick);
            }
        }

        public void Start()
        {
            if (!started)
            {
                dataTimer.Start();
                drawingTimer.Start();
            }
        }

        public void Stop()
        {
            if (!started)
            {
                dataTimer.Start();
                drawingTimer.Start();
            }
        }


        /// <summary>
        /// Retrieve all base colors in HSV (with maximum saturation and brightness)
        /// </summary>
        /// <returns>All HSV base colors (256 * 6 in total, and colors will gradually change as the index increases)</returns>
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



        /// <summary>
        /// Used to refresh spectrum data and implement spectrum data buffering
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataTimer_Tick(object? sender, EventArgs e)
        {
            double[] newSpectrumData = visualizer.GetSpectrumData();         // Retrieve spectrum data from the visualizer
            newSpectrumData = Visualizer.GetBlurry(newSpectrumData, 2);                // Smooth spectrum data

            spectrumData = newSpectrumData;
        }

        private void DrawingTimer_Tick(object? sender, EventArgs e)
        {
            if (spectrumData == null)
                return;

            rotation += .1;
            colorIndex++;

            Color color1 = allColors[colorIndex % allColors.Length];
            Color color2 = allColors[(colorIndex + 200) % allColors.Length];

            double[] bassArea = Visualizer.TakeSpectrumOfFrequency(spectrumData, AudioSampleRate, 250);       // Low frequency region
            double bassScale = bassArea.Average() * 100;                                                                    // Scaling caused by bass (scale)
            double extraScale = Math.Min(this.Width, this.Height) / 6;                                      // Scaling caused by bass (multiplied by window size)

            Rectangle border = new Rectangle(Point.Empty, this.Size);

            BufferedGraphics buffer = BufferedGraphicsManager.Current.Allocate(this.CreateGraphics(), this.ClientRectangle);
            Graphics g = buffer.Graphics;

            if (oldBuffer != null)
            {
                //oldBuffer.Render(buffer.Graphics);      // If you want to achieve the "ghosting" effect, uncomment this code and change g.Clear to g.FillRectange (xxx, semi transparent black)
                oldBuffer.Dispose();
            }

            using Pen pen = new Pen(Color.Pink);                  // Pen used for drawing audio sampling waveforms

            g.SmoothingMode = SmoothingMode.HighQuality;          // High quality drawing
            g.Clear(this.BackColor);

            switch (this.VisualEffect)
            {
                case VisualEffect.Oscilloscope:
                    DrawCurve(g, pen, visualizer.SampleData, visualizer.SampleData.Length, this.Width, 0, this.Height / 2, MathF.Min(this.Height / 10, 100) * Scale);
                    break;
                case VisualEffect.SpectrumBar:
                    DrawGradientBar(g, color1, color2, spectrumData, spectrumData.Length, this.Width, 0, this.Height / 2, 3, -this.Height * 10 * this.Scale);
                    break;
                case VisualEffect.SpectrumCycle:
                    DrawGradientCircle(g, color1, color2, spectrumData, spectrumData.Length, this.Width / 2, this.Height / 2, MathF.Min(this.Width, this.Height) / 4 + extraScale * bassScale, 1, rotation, this.Width / 2 * 10 * Scale);
                    break;
                case VisualEffect.Border:
                    DrawGradientBorder(g, Color.FromArgb(0, color1), color2, border, bassScale * this.Scale, this.Width / 10);
                    break;
                default:
                    DrawGradientBar(g, color1, color2, spectrumData, spectrumData.Length, this.Width, 0, this.Height / 2, 3, -this.Height * 10 * this.Scale);
                    break;
            }

            buffer.Render();

            oldBuffer = buffer;                                   // Save the buffer (the reason why only one buffer is used globally is that users may adjust the window size, so each frame must adapt)
        }


        //Draw a circle
        public void DrawCycle(Graphics g, Color down, Color up, int width, int height)
        {
            // Calculate the diameter of a circle and choose the smaller value of width, height, and height to ensure that the circle is always a perfect circle
            int diameter = Math.Min(width / 2, height / 2);

            // Calculate the center position of the circle
            int x = (width - diameter) / 2;
            int y = (height - diameter) / 2;

            using (Pen pen = new Pen(up, 3))  // Choose the color and line thickness you want
            {
                g.DrawEllipse(pen, x, y, diameter, diameter);
            }
        }

        /// <summary>
        /// Draw a gradient wave
        /// </summary>
        /// <param name="g">Drawing Objective</param>
        /// <param name="down">Color below</param>
        /// <param name="up">Color above</param>
        /// <param name="spectrumData">Spectrum data</param>
        /// <param name="pointCount">Number of points in waves</param>
        /// <param name="drawingWidth">The width of waves</param>
        /// <param name="xOffset">The starting X-coordinate of the wave</param>
        /// <param name="yOffset">The actual Y coordinate of waves</param>
        /// <param name="scale">Scaling of Spectrum (Using Negative Values to Flip Waves)</param>
        private void DrawGradient(Graphics g, Color down, Color up, double[] spectrumData, int pointCount, int drawingWidth, float xOffset, float yOffset, double scale)
        {
            GraphicsPath path = new GraphicsPath();

            PointF[] points = new PointF[pointCount + 2];
            for (int i = 0; i < pointCount; i++)
            {
                double x = i * drawingWidth / pointCount + xOffset;
                double y = spectrumData[i * spectrumData.Length / pointCount] * scale + yOffset;
                points[i + 1] = new PointF((float)x, (float)y);
            }

            points[0] = new PointF(xOffset, yOffset);
            points[points.Length - 1] = new PointF(xOffset + drawingWidth, yOffset);

            path.AddCurve(points);

            float upP = (float)points.Min(v => v.Y);

            if (Math.Abs(upP - yOffset) < 1)
                return;

            using Brush brush = new LinearGradientBrush(new PointF(0, yOffset), new PointF(0, upP), down, up);
            g.FillPath(brush, path);
        }

        /// <summary>
        /// Draw a gradient bar
        /// </summary>
        /// <param name="g"></param>
        /// <param name="down"></param>
        /// <param name="up"></param>
        /// <param name="spectrumData"></param>
        /// <param name="stripCount"></param>
        /// <param name="drawingWidth"></param>
        /// <param name="xOffset"></param>
        /// <param name="yOffset"></param>
        /// <param name="spacing"></param>
        /// <param name="scale"></param>
        private void DrawGradientBar(Graphics g, Color down, Color up, double[] spectrumData, int stripCount, int drawingWidth, float xOffset, float yOffset, float spacing, double scale)
        {
            float stripWidth = (drawingWidth - spacing * stripCount) / stripCount;
            PointF[] points = new PointF[stripCount];

            for (int i = 0; i < stripCount; i++)
            {
                double x = stripWidth * i + spacing * i + xOffset;
                double y = spectrumData[i * spectrumData.Length / stripCount] * scale;   // height
                points[i] = new PointF((float)x, (float)y);
            }

            float upP = (float)points.Min(v => v.Y < 0 ? yOffset + v.Y : yOffset);
            float downP = (float)points.Max(v => v.Y < 0 ? yOffset : yOffset + v.Y);

            if (downP < yOffset)
                downP = yOffset;


            g.DrawLine(new Pen(down), new PointF(0, yOffset), new PointF(drawingWidth, yOffset));
            if (Math.Abs(upP - downP) < 1)
                return;

            using Brush brush = new LinearGradientBrush(new PointF(0, downP), new PointF(0, upP), down, up);
            for (int i = 0; i < stripCount; i++)
            {
                PointF p = points[i];
                float y = yOffset;
                float height = p.Y;

                if (height < 0)
                {
                    y += height;
                    height = -height;
                }

                g.FillRectangle(brush, new RectangleF(p.X, y, stripWidth, height));
            }

        }

        /// <summary>
        /// Draw a curve
        /// </summary>
        /// <param name="g"></param>
        /// <param name="pen"></param>
        /// <param name="spectrumData"></param>
        /// <param name="pointCount"></param>
        /// <param name="drawingWidth"></param>
        /// <param name="xOffset"></param>
        /// <param name="yOffset"></param>
        /// <param name="scale"></param>
        private void DrawCurve(Graphics g, Pen pen, double[] spectrumData, int pointCount, int drawingWidth, double xOffset, double yOffset, double scale)
        {
            PointF[] points = new PointF[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                double x = i * drawingWidth / pointCount + xOffset;
                double y = spectrumData[i * spectrumData.Length / pointCount] * scale + yOffset;
                points[i] = new PointF((float)x, (float)y);
            }

            g.DrawCurve(pen, points);
        }

        /// <summary>
        /// Draw simple circular lines
        /// </summary>
        /// <param name="g"></param>
        /// <param name="brush"></param>
        /// <param name="spectrumData"></param>
        /// <param name="stripCount"></param>
        /// <param name="xOffset"></param>
        /// <param name="yOffset"></param>
        /// <param name="radius"></param>
        /// <param name="spacing"></param>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        private void DrawCircleStrips(Graphics g, Brush brush, double[] spectrumData, int stripCount, double xOffset, double yOffset, double radius, double spacing, double rotation, double scale)
        {
            double rotationAngle = Math.PI / 180 * rotation;
            double blockWidth = MathF.PI * 2 / stripCount;           // angle
            double stripWidth = blockWidth - MathF.PI / 180 * spacing;                // angle
            PointF[] points = new PointF[stripCount];

            for (int i = 0; i < stripCount; i++)
            {
                double x = blockWidth * i + rotationAngle;      // angle
                double y = spectrumData[i * spectrumData.Length / stripCount] * scale;   // height
                points[i] = new PointF((float)x, (float)y);
            }

            for (int i = 0; i < stripCount; i++)
            {
                PointF p = points[i];
                double sinStart = Math.Sin(p.X);
                double sinEnd = Math.Sin(p.X + stripWidth);
                double cosStart = Math.Cos(p.X);
                double cosEnd = Math.Cos(p.X + stripWidth);

                PointF[] polygon = new PointF[]
                {
                    new PointF((float)(cosStart * radius + xOffset), (float)(sinStart * radius + yOffset)),
                    new PointF((float)(cosEnd * radius + xOffset), (float)(sinEnd * radius + yOffset)),
                    new PointF((float)(cosEnd * (radius + p.Y) + xOffset), (float)(sinEnd * (radius + p.Y) + yOffset)),
                    new PointF((float)(cosStart * (radius + p.Y) + xOffset), (float)(sinStart * (radius + p.Y) + yOffset)),
                };

                g.FillPolygon(brush, polygon);
            }
        }

        /// <summary>
        /// Draw a circular gradient bar
        /// </summary>
        /// <param name="g"></param>
        /// <param name="inner"></param>
        /// <param name="outer"></param>
        /// <param name="spectrumData"></param>
        /// <param name="stripCount"></param>
        /// <param name="xOffset"></param>
        /// <param name="yOffset"></param>
        /// <param name="radius"></param>
        /// <param name="spacing"></param>
        /// <param name="scale"></param>
        private void DrawGradientCircle(Graphics g, Color inner, Color outer, double[] spectrumData, int stripCount, double xOffset, double yOffset, double radius, double spacing, double rotation, double scale)
        {
            double rotationAngle = Math.PI / 180 * rotation;
            double blockWidth = Math.PI * 2 / stripCount;           // angle
            double stripWidth = blockWidth - MathF.PI / 180 * spacing;                // angle
            PointF[] points = new PointF[stripCount];

            for (int i = 0; i < stripCount; i++)
            {
                double x = blockWidth * i + rotationAngle;      // angle
                double y = spectrumData[i * spectrumData.Length / stripCount] * scale;   // height
                points[i] = new PointF((float)x, (float)y);
            }

            double maxHeight = points.Max(v => v.Y);
            double outerRadius = radius + maxHeight;

            PointF[] polygon = new PointF[4];
            for (int i = 0; i < stripCount; i++)
            {
                PointF p = points[i];
                double sinStart = Math.Sin(p.X);
                double sinEnd = Math.Sin(p.X + stripWidth);
                double cosStart = Math.Cos(p.X);
                double cosEnd = Math.Cos(p.X + stripWidth);

                PointF
                    p1 = new PointF((float)(cosStart * radius + xOffset), (float)(sinStart * radius + yOffset)),
                    p2 = new PointF((float)(cosEnd * radius + xOffset), (float)(sinEnd * radius + yOffset)),
                    p3 = new PointF((float)(cosEnd * (radius + p.Y) + xOffset), (float)(sinEnd * (radius + p.Y) + yOffset)),
                    p4 = new PointF((float)(cosStart * (radius + p.Y) + xOffset), (float)(sinStart * (radius + p.Y) + yOffset));

                polygon[0] = p1;
                polygon[1] = p2;
                polygon[2] = p3;
                polygon[3] = p4;


                PointF innerP = new PointF((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
                PointF outerP = new PointF((p3.X + p4.X) / 2, (p3.Y + p4.Y) / 2);

                Vector2 offset = new Vector2(outerP.X - innerP.X, outerP.Y - innerP.Y);
                if (MathF.Sqrt(offset.X * offset.X + offset.Y * offset.Y) < 1)                                // The distance between two points of the gradient brush cannot be too small
                    continue;

                try
                {
                    using LinearGradientBrush brush = new LinearGradientBrush(innerP, outerP, inner, outer);      
                    g.FillPolygon(brush, polygon);                                                           
                }
                catch { }
            }

            DrawCycle(g, inner, outer, this.Width, this.Height);

        }

        /// <summary>
        /// Draw simple lines
        /// </summary>
        /// <param name="g"></param>
        /// <param name="brush"></param>
        /// <param name="spectrumData"></param>
        /// <param name="stripCount"></param>
        /// <param name="drawingWidth"></param>
        /// <param name="xOffset"></param>
        /// <param name="yOffset"></param>
        /// <param name="spacing"></param>
        /// <param name="scale"></param>
        private void DrawStrips(Graphics g, Brush brush, double[] spectrumData, int stripCount, int drawingWidth, float xOffset, float yOffset, float spacing, double scale)
        {
            float stripWidth = (drawingWidth - spacing * stripCount) / stripCount;
            PointF[] points = new PointF[stripCount];

            for (int i = 0; i < stripCount; i++)
            {
                double x = stripWidth * i + spacing * i + xOffset;
                double y = spectrumData[i * spectrumData.Length / stripCount] * scale;   // height
                points[i] = new PointF((float)x, (float)y);
            }

            for (int i = 0; i < stripCount; i++)
            {
                PointF p = points[i];
                float y = yOffset;
                float height = p.Y;

                if (height < 0)
                {
                    y += height;
                    height = -height;
                }

                g.FillRectangle(brush, new RectangleF(p.X, y, stripWidth, height));
            }
        }

        /// <summary>
        /// Draw a gradient border
        /// </summary>
        /// <param name="g"></param>
        /// <param name="inner"></param>
        /// <param name="outer"></param>
        /// <param name="area"></param>
        /// <param name="scale"></param>
        /// <param name="width"></param>
        private void DrawGradientBorder(Graphics g, Color inner, Color outer, Rectangle area, double scale, float width)
        {
            int thickness = (int)(width * scale);
            if (thickness < 1)
                return;

            Rectangle rect = new Rectangle(area.X, area.Y, area.Width, area.Height);

            Rectangle up = new Rectangle(rect.Location, new Size(rect.Width, thickness));
            Rectangle down = new Rectangle(new Point(rect.X, (int)(rect.X + rect.Height - scale * width)), new Size(rect.Width, thickness));
            Rectangle left = new Rectangle(rect.Location, new Size(thickness, rect.Height));
            Rectangle right = new Rectangle(new Point((int)(rect.X + rect.Width - scale * width), rect.Y), new Size(thickness, rect.Height));

            LinearGradientBrush upB = new LinearGradientBrush(up, outer, inner, LinearGradientMode.Vertical);
            LinearGradientBrush downB = new LinearGradientBrush(down, inner, outer, LinearGradientMode.Vertical);
            LinearGradientBrush leftB = new LinearGradientBrush(left, outer, inner, LinearGradientMode.Horizontal);
            LinearGradientBrush rightB = new LinearGradientBrush(right, inner, outer, LinearGradientMode.Horizontal);

            upB.WrapMode = downB.WrapMode = leftB.WrapMode = rightB.WrapMode = WrapMode.TileFlipXY;

            g.FillRectangle(upB, up);
            g.FillRectangle(downB, down);
            g.FillRectangle(leftB, left);
            g.FillRectangle(rightB, right);
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

        //private void MainWindow_Load(object sender, EventArgs e)
        //{

        //}

        //private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        //{
        //    Environment.Exit(0);
        //}

        //private void DrawingPanel_MouseDoubleClick(object sender, MouseEventArgs e)
        //{
        //    WindowState = WindowState != FormWindowState.Maximized ? FormWindowState.Maximized : FormWindowState.Normal;
        //    FormBorderStyle = WindowState == FormWindowState.Maximized ? FormBorderStyle.None : FormBorderStyle.Sizable;
        //}


    }
}
