using System;
using System.Threading.Tasks;
using System.Net.Http;
using m4rcus.TuyaCore;
using com.clusterrr.TuyaNet;

namespace TuyaDeviceScanner
{
    class Program
    {

        private static readonly HttpClient client = new HttpClient();

        static void Main(string[] args)
        {
            var scanner = new TuyaScanner();
            scanner.OnNewDeviceInfoReceived += Scanner_OnNewDeviceInfoReceived;
            Console.WriteLine("Scanning local network for Tuya devices, press any key to stop.");
            scanner.Start();
            Console.ReadKey();
            scanner.Stop();
            // TurnON().GetAwaiter().GetResult(); // Uncomment to test actual turning ON/OFF
        }

        // Test Device Control
        static async Task TurnON()
        {
            var device = new TuyaPlug()
            { 
                IP = "YOUR_TUYA_DEVICE_IP", // Please get IP from the scanner
                LocalKey = "YOUR_TUYA_LOCAL_KEY", // Following instructions to get localkey of your device, require tuya IoT platform account
                Id = "YOUR_TUYA_DEVICE_ID" // Please get deviceId from the scanner
            };
            var status = await device.GetStatus();
            await device.SetStatus(!status.Powered); // toggle power
        }

        private static void Scanner_OnNewDeviceInfoReceived(object sender, TuyaDeviceScanInfo e)
        {
            Console.WriteLine($"New device found! IP: {e.IP}, ID: {e.GwId}, version: {e.Version}");
        }
    }
}
