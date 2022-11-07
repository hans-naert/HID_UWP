using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.Devices.Enumeration;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Storage;
using Windows.Storage.Streams;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HID_UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        HidDevice device;

        // Enumerate HID devices
        private async void enumerateHidDevices()
        {
            ushort vendorId = 0xC251;
            ushort productId = 0x4501; ;//0x4C01;//0x4301;//0x4511;
            ushort usagePage = 0xFF00;
            ushort usageId = 0x0001;

            // Create a selector that gets a HID device using VID/PID and a 
            // VendorDefined usage
            string selector = HidDevice.GetDeviceSelector(usagePage, usageId, vendorId, productId);

            // Enumerate devices using the selector
            var devices = await DeviceInformation.FindAllAsync(selector);

            if (devices.Count > 0)
            {
                // Open the target HID device
                device = await HidDevice.FromIdAsync(devices.ElementAt(0).Id, FileAccessMode.ReadWrite);

                // At this point the device is available to communicate with
                // So we can send/receive HID reports from it or 
                // query it for control descriptions
                USBFound.Text = "device found";
            }
            else
            {
                // There were no HID devices that met the selector criteria
                USBFound.Text = "device not found";
            }
        }

        private void searchHIDButtonClick(object sender, RoutedEventArgs e)
        {
            enumerateHidDevices();
        }


        private async void sendOutputReportAsync(byte reportId, byte firstByte)
        {
            var outputReport = device.CreateOutputReport(reportId);

            var dataWriter = new DataWriter();

            // First byte is always the report id
            dataWriter.WriteByte((Byte)outputReport.Id);
            dataWriter.WriteByte(firstByte);
            //dataWriter.WriteBytes(new byte[63]);

           outputReport.Data = dataWriter.DetachBuffer();

            uint bytesWritten = await device.SendOutputReportAsync(outputReport);
        }

        private async void getNumericInputReportAsync()
        {
            var inputReport = await device.GetInputReportAsync(0x0);

            var data = inputReport.Data;

            var dataReader = DataReader.FromBuffer(data);

            byte[] bytesRead = new byte[2];
            dataReader.ReadBytes(bytesRead);
            InputData.Text = $"byte 0: {bytesRead[0]}  byte 1: {bytesRead[1]}";
        }

        byte toggle = 0x0;

        private void outputReportButtonClick(object sender, RoutedEventArgs e)
        {            
            toggle= (byte) ~toggle;
            sendOutputReportAsync(0x00, toggle);
        }

        private void inputReportButtonClick(object sender, RoutedEventArgs e)
        {
            getNumericInputReportAsync();
        }
    }
}
