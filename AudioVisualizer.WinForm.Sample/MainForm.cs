﻿using AudioVisualizer.Core;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AudioVisualizer.WinForm.Sample
{
    public partial class MainForm : Form
    {
        WasapiCapture capture;             // 音频捕获
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            capture = new WasapiLoopbackCapture();          // 捕获电脑发出的声音
            capture.WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(8192, 1);      // 指定捕获的格式, 单声道, 32位深度, IeeeFloat 编码, 8192采样率
            capture.DataAvailable += Capture_DataAvailable;                          // 订阅事件

            audioVisualizer1.AudioSampleRate = capture.WaveFormat.SampleRate;

            capture.StartRecording();

            audioVisualizer1.Scale = 5;
            audioVisualizer1.VisualEffict = VisualEffict.SpectrumBar;
            audioVisualizer1.Start();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            capture.StopRecording();
            audioVisualizer1.Stop();
        }

        /// <summary>
        /// 当捕获有数据的时候, 就怼到可视化器里面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Capture_DataAvailable(object? sender, WaveInEventArgs e)
        {
            int length = e.BytesRecorded / 4;           // 采样的数量 (每一个采样是 4 字节)
            double[] result = new double[length];       // 声明结果

            for (int i = 0; i < length; i++)
                result[i] = BitConverter.ToSingle(e.Buffer, i * 4);      // 取出采样值

            audioVisualizer1.PushSampleData(result);          // 将新的采样存储到 可视化器 中
        }
    }
}
