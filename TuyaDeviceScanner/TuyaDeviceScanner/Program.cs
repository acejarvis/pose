using System;
using System.Threading.Tasks;
using System.Net.Http;
using m4rcus.TuyaCore;
using com.clusterrr.TuyaNet;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace TuyaDeviceScanner
{
    class Program
    {


        static void Main(string[] args)
        {

            //var scanner = new TuyaScanner();
            //scanner.OnNewDeviceInfoReceived += Scanner_OnNewDeviceInfoReceived;
            //Console.WriteLine("Scanning local network for Tuya devices, press any key to stop.");
            //scanner.Start();
            //Console.ReadKey();
            //scanner.Stop();

            GetDeviceStatus().GetAwaiter().GetResult(); // uncomment to test actual turning on/off
            //var devices = initializeDevices();
            //DeviceControl(2, devices);
        }

        // Test Device Control

        static async Task DeviceControl(int index, List<TuyaPlug> deviceList)
        {
            //var status = await deviceList[index].GetStatus();
            await deviceList[index].SetStatus(false);

        }

        static async Task GetDeviceStatus()
        {
            UnityWebRequest request = UnityWebRequest.Get("https://www.my-server.com");
            var status = request.SendWebRequest();
            Console.WriteLine(status);
        }

        static async Task TurnON()
        {
            var device = new TuyaPlug()
            {
                IP = "192.168.31.88",
                LocalKey = "90552857e69fc11c",
                Id = "137107483c71bf2296d3"
            };
            var status = await device.GetStatus();
            await device.SetStatus(!status.Powered); // toggle power
        }

        private static void Scanner_OnNewDeviceInfoReceived(object sender, TuyaDeviceScanInfo e)
        {
            Console.WriteLine($"New device found! IP: {e.IP}, ID: {e.GwId}, version: {e.Version}");
        }

        static List<TuyaPlug> initializeDevices()
        {
            var deviceList = new List<TuyaPlug>();

            var device0 = new TuyaPlug()
            {
                IP = "192.168.31.88",
                LocalKey = "90552857e69fc11c",
                Id = "137107483c71bf2296d3"
            };
            deviceList.Add(device0);

            var device1 = new TuyaPlug()
            {
                IP = "192.168.31.194",
                LocalKey = "df12a78a691fc089",
                Id = "137107483c71bf225465"
            };
            deviceList.Add(device1);

            var device2 = new TuyaPlug()
            {
                IP = "192.168.31.68",
                LocalKey = "8196c2b6154ede04",
                Id = "eb6f00bbeca9f3a971jxxx"
            };
            deviceList.Add(device2);

            return deviceList;
        }
    }
}
