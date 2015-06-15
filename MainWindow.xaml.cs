using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Windows.Threading;
using System.IO;
using OpenNUI.CSharp.Library;

namespace OpenNUI.Samples.ColorBasics
{
    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Instant of NuiApplication for Connect OpenNUI Service
        /// </summary>
        NuiApplication nuiApp = null;

        /// <summary>
        /// Instant of NuiSensor
        /// </summary>
        NuiSensor useSensor = null;
        /// <summary>
        /// the Timer for getting frames from OpenNUI Service
        /// </summary>
        private System.Timers.Timer openNUIFrameTimer;
        /// <summary>
        /// worker for openNUIFrameTimer
        /// </summary>
        Action act;

        /// <summary>
        /// bitmap for drawing colorframe
        /// </summary>
        WriteableBitmap bitmap = null;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            // create nuiApp instance and setting NuiApplication's name
            nuiApp = new NuiApplication("OpenNUI.Samples.ColorBasics");

            // register delegate when nui sensor connected, disconnected
            nuiApp.OnSensorConnected += nuiApp_OnSensorConnected;
            nuiApp.OnSensorDisconnected += nuiApp_OnSensorDisconnected;

            try
            {
                nuiApp.Start();
            }
            // when exception catched. it means OpenNUI dosen't installed
            catch
            {
                //show error messagebox
                MessageBox.Show("OpenNUI dosen't installed", "error", MessageBoxButton.OK, MessageBoxImage.Error);

                //program exit
                Environment.Exit(0);
            }

            // worker for openNUIFrameTimer
            act = new Action(delegate()
            {
                //do not work when usesensor is null
                if (useSensor == null)
                    return;

                //get the color frame
                ImageData colorFrame = useSensor.GetColorFrame();

                //do not work when color frame is null (faild to GetColorFrame())
                if (colorFrame == null)
                    return;

                int numPixels = colorFrame.FrameData.Length / sizeof(uint);

                bitmap.Lock();
                unsafe
                {
                    fixed (byte* pSrcData = &colorFrame.FrameData[0])
                    {
                        uint* pCurrent = (uint*)pSrcData;
                        uint* pBitmapData = (uint*)bitmap.BackBuffer;

                        for (int n = 0; n < numPixels; n++)
                        {
                            uint x = *(pCurrent++);
                            *(pBitmapData + n) =
                                (x & 0xFF000000) |
                                (x & 0x00FF0000) |
                                (x & 0x0000FF00) |
                                (x & 0x000000FF);
                        }
                    }
                }
                bitmap.AddDirtyRect(new Int32Rect(0, 0, colorFrame.Description.Width, colorFrame.Description.Height));
                bitmap.Unlock();

            });


            // create openNUIFrameTimer instance
            openNUIFrameTimer = new System.Timers.Timer();

            //set the timer interval to 60fps
            openNUIFrameTimer.Interval = 1000 / 60;

            //set the timer elapsed callback
            openNUIFrameTimer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) =>
            {
                try
                {
                    //use invoke for access to the this wpf window's ui
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, act);
                }
                catch { }
            };

            //set the timer autoreset to true for loop
            openNUIFrameTimer.AutoReset = true;

            //openNUIFrameTimer start
            openNUIFrameTimer.Start();

            // initialize the components (controls) of the window
            InitializeComponent();
        }

        /// <summary>
        /// Handles the nui sensor disconnected
        /// </summary>
        /// <param name="sensor">connected sensor</param>
        void nuiApp_OnSensorDisconnected(NuiSensor sensor)
        {
            if (useSensor == sensor)
            {
                //set usesensor to null when usesensor is disconnected
                useSensor = null;

                //set bitmap to null
                bitmap = null;

                //use invoke for access to the this wpf window's ui
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    //init sensor's name,vendor textbox
                    sensorNameTB.Text = "";
                    sensorVendorTB.Text = "";
                }));
            }
        }

        /// <summary>
        /// Handles the nui sensor connected
        /// </summary>
        /// <param name="sensor">connected sensor</param>
        void nuiApp_OnSensorConnected(NuiSensor sensor)
        {
            if (useSensor == null)
            {
                //set usesensor to now connected sensor when usesensor is empty
                useSensor = sensor;

                //open color frame
                useSensor.OpenColorFrame();

                //use invoke for access to the this wpf window's ui
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    //create bitmap instance
                    bitmap = new WriteableBitmap(
                  useSensor.ColorInfo.Width,
                  useSensor.ColorInfo.Height, 96, 96, PixelFormats.Bgra32, null);

                    //set the colorImage's source to colorframe's bitmap
                    colorImage.Source = bitmap;

                    //update sensor's name,vendor textbox
                    sensorNameTB.Text = useSensor.Name;
                    sensorVendorTB.Text = useSensor.Vendor;
                }));
            }
        }

        /// <summary>
        /// Handles the user clicking on the screenshot button
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Screenshot_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.bitmap != null)
            {
                // create a png bitmap encoder which knows how to save a .png file
                BitmapEncoder encoder = new PngBitmapEncoder();

                // create frame from the writable bitmap and add to encoder
                encoder.Frames.Add(BitmapFrame.Create(this.bitmap));

                string time = System.DateTime.Now.ToString("hh'-'mm'-'ss", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat);

                string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                string path = System.IO.Path.Combine(myPhotos, "OpenNUIScreenshot-Color-" + time + ".png");

                // write the new file to disk
                try
                {
                    // FileStream is IDisposable
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        encoder.Save(fs);
                    }

                }
                catch (IOException)
                {

                }
            }
        }

    }
}
