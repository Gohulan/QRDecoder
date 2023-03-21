using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using ZXing;

namespace QRDecoder
{

    public partial class Form1 : Form
    {

        private readonly Webcam webcam;
        private readonly BarcodeReader barcodeReader;

        public Form1()
        {
            InitializeComponent();

            webcam = new Webcam();
            barcodeReader = new BarcodeReader();

           
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var cameras = webcam.GetCameras();
            foreach (var camera in cameras)
            {
                comboBox1.Items.Add(camera);
            }
            if (cameras.Length > 0)
            {
                comboBox1.SelectedIndex = 0;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Open a file dialog to select a QR code image file
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "QR Code Images (*.png;*.jpg;*.jpeg;*.gif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Decode the selected image file
                var result = DecodeQrCode(openFileDialog.FileName);
                if (result != null)
                {
                    textBox1.Text = result.Text;
                }
                else
                {
                    textBox1.Text = "Failed to decode the QR code.";
                }

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var selectedCamera = (Camera)comboBox1.SelectedItem;
            webcam.Start(selectedCamera);

            // Start the timer to capture and decode QR codes from the camera
            timer1.Enabled = true;
            timer1.Start();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Stop the camera and the timer
            webcam.Stop();
            timer1.Enabled = false;
            timer1.Stop();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // Capture a frame from the camera
            var frame = webcam.CaptureFrame();

            // Decode the QR code from the captured frame
            var result = barcodeReader.Decode(frame);

            if (result != null)
            {
                textBox1.Text = result.Text;
            }

            // Show the camera preview in pictureBox1
            pictureBox1.Image = frame;
        }

        private Result DecodeQrCode(string filePath)
        {
            // Read the QR code image file
            var imageBytes = File.ReadAllBytes(filePath);

            // Decode the QR code
            var barcodeBitmap = (Bitmap)Bitmap.FromStream(new MemoryStream(imageBytes));
            var result = barcodeReader.Decode(barcodeBitmap);

            return result;
        }
    }

    public class Webcam
    {
        private VideoCaptureDevice videoCaptureDevice;
        private Bitmap currentFrame;

        public Webcam()
        {
            currentFrame = new Bitmap(1, 1);
            videoCaptureDevice = new VideoCaptureDevice();
        }

        public Camera[] GetCameras()
        {
            // Get the list of available cameras
            var cameras = new Camera[0];
            try
            {
                var videoCaptureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                cameras = new Camera[videoCaptureDevices.Count];
                for (var i = 0; i < videoCaptureDevices.Count; i++)
                {
                    var camera = new Camera();
                    camera.Name = videoCaptureDevices[i].Name;
                    camera.MonikerString = videoCaptureDevices[i].MonikerString;
                    cameras[i] = camera;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while getting the list of cameras: " + ex.Message);
            }
            return cameras;
        }

        public void Start(Camera camera)
        {
            // Start capturing video from the selected camera
            videoCaptureDevice = new VideoCaptureDevice(camera.MonikerString);
            videoCaptureDevice.NewFrame += VideoCaptureDevice_NewFrame;
            videoCaptureDevice.Start();
        }

        public void Stop()
        {
            // Stop capturing video
            videoCaptureDevice.SignalToStop();
            videoCaptureDevice.WaitForStop();
        }

        public Bitmap CaptureFrame()
        {
            // Clone the current frame to avoid threading issues
            var clonedFrame = (Bitmap)currentFrame.Clone();
            return clonedFrame;
        }

        private void VideoCaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // Copy the new frame to the current frame bitmap
            currentFrame.Dispose();
            currentFrame = (Bitmap)eventArgs.Frame.Clone();
        }

    }
}
public class Camera
{
    public string Name { get; set; }
    public string MonikerString { get; set; }

    public override string ToString()
    {
        return Name;
    }
}
