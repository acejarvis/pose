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

            var scanner = new TuyaScanner();
            scanner.OnNewDeviceInfoReceived += Scanner_OnNewDeviceInfoReceived;
            Console.WriteLine("Scanning local network for Tuya devices, press any key to stop.");
            scanner.Start();
            Console.ReadKey();
            scanner.Stop();

            var devices = initializeDevices();
            DeviceControl(2, devices);
        }

        // Test Device Control

        static async Task DeviceControl(int index, List<TuyaPlug> deviceList)
        {
            //var status = await deviceList[index].GetStatus();
            await deviceList[index].SetStatus(false);

        }


        static async Task TurnON()
        {
            var device = new TuyaPlug()
            {
                IP = "IP",
                LocalKey = "localkey",
                Id = "id"
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
            var devices = new List<TuyaPlug>();
            devices.Add(new TuyaPlug()
            {
                IP = "IP",
                LocalKey = "localkey",
                Id = "id"
            });
            return devices;
        }
    }
}
