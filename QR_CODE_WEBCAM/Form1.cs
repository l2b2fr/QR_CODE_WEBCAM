using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZXing;
using static System.Net.Mime.MediaTypeNames;

namespace QR_CODE_WEBCAM
{
    public partial class Form1 : Form
    {
        FilterInfoCollection filterInfoCollection;
        VideoCaptureDevice videoCaptureDevice;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo Device in filterInfoCollection)
                cboCamera.Items.Add(Device.Name);
            if (cboCamera.Items.Count > 0)
            {
                cboCamera.SelectedIndex = 0;
                videoCaptureDevice = new VideoCaptureDevice();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Bắt đầu")
            {
                videoCaptureDevice = new VideoCaptureDevice(filterInfoCollection[cboCamera.SelectedIndex].MonikerString);
                videoCaptureDevice.NewFrame += FinalFrame_NewFrame;
                videoCaptureDevice.Start();
                timer1.Start();
                button1.Text = "Dừng lại!";
            }
            else
            {
                button1.Text = "Bắt đầu";
                if (videoCaptureDevice != null)
                    if (videoCaptureDevice.IsRunning == true)
                        videoCaptureDevice.Stop();
                timer1.Stop();
            }
        }
        private void FinalFrame_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            pictureBox1.Image = (Bitmap)eventArgs.Frame.Clone();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (videoCaptureDevice.IsRunning)
            {
                videoCaptureDevice?.Stop();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                BarcodeReader reader = new BarcodeReader();
                Result result = reader.Decode((Bitmap)pictureBox1.Image);

                if (result != null && result.ResultPoints.Length > 0)
                {
                    // Hiển thị kết quả lên label
                    label1.Text = result.Text;

                    // Tính toán tọa độ và kích thước của hình chữ nhật bao quanh mã QR
                    int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
                    foreach (var point in result.ResultPoints)
                    {
                        if (point.X < minX)
                            minX = (int)point.X;
                        if (point.X > maxX)
                            maxX = (int)point.X;
                        if (point.Y < minY)
                            minY = (int)point.Y;
                        if (point.Y > maxY)
                            maxY = (int)point.Y;
                    }

                    // Tính toán tọa độ tâm của hình chữ nhật
                    int centerX = minX + (maxX - minX) / 2;
                    int centerY = minY + (maxY - minY) / 2;

                    // Tính toán diện tích của hình chữ nhật bao quanh đối tượng
                    int width = maxX - minX;
                    int height = maxY - minY;
                    int area = width * height;

                    // Hiển thị tọa độ tâm và diện tích trên label
                    label1.Text = $"Tọa độ: ({centerX}, {centerY}) Diện tích chung: {area} pixels";

                    // Vẽ hình chữ nhật bao quanh vùng chứa mã QR
                    using (Graphics g = pictureBox1.CreateGraphics())
                    {
                        // Tính toán offset dựa trên chế độ zoom của PictureBox
                        int offsetX = pictureBox1.SizeMode == PictureBoxSizeMode.Zoom
                            ? (pictureBox1.Width - pictureBox1.Image.Width) / 2
                            : 0;
                        int offsetY = pictureBox1.SizeMode == PictureBoxSizeMode.Zoom
                            ? (pictureBox1.Height - pictureBox1.Image.Height) / 2
                            : 0;

                        Rectangle rect = new Rectangle(minX + offsetX, minY + offsetY, width, height);
                        using (Pen pen = new Pen(Color.Green, 10))
                        {
                            g.DrawRectangle(pen, rect);

                            // Vẽ đường viền nét đứt màu vàng xung quanh hình chữ nhật
                            pen.DashPattern = new float[] { 10, 10 };
                            g.DrawRectangle(pen, rect);
                        }

                        // Hiển thị thông tin kết quả QR code gần với hình chữ nhật
                        g.DrawString($"Tọa độ: ({centerX}, {centerY}) Diện tích chung: {area} pixels", new Font("Tahoma", 16f), Brushes.Black, new Point(rect.X - 60, rect.Y - 50));

                        // Vẽ điểm tâm của đối tượng
                        g.FillEllipse(Brushes.Red, centerX + offsetX - 3, centerY + offsetY - 3, 6, 6);
                    }
                    if (videoCaptureDevice.IsRunning)
                    {
                        videoCaptureDevice?.Stop();
                    }
                }

            }

        }
    }
}