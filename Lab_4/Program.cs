using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using PcapDotNet.Core;
using PcapDotNet.Core.Extensions;

namespace Lab_4
{
    class Program
    {
        static void Main(string[] args)
        {
            IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;

            if (!allDevices.Any())
            {
                Console.WriteLine("Nope");
                return;
            }

            for (int i = 0; i < allDevices.Count; i++)
            {
                var device = allDevices[i];

                Console.WriteLine($"{i + 1})\n{"Name".PadRight(20, '.')}: {device.Name}");
                Console.WriteLine($"{"Description".PadRight(20, '.')}: {device.Description}");

                Console.WriteLine($"{"Attributes".PadRight(20, '.')}: {Enum.GetName(typeof(DeviceAttributes), device.Attributes)}");
                Console.WriteLine($"{"GUID".PadRight(20, '.')}: {device.GetGuid()}");
                Console.WriteLine($"{"MAC Address".PadRight(20, '.')}: {device.GetMacAddress()}");
                Console.WriteLine($"{"PNP".PadRight(20, '.')}: {device.GetPnpDeviceId()}");

                Console.WriteLine($"{"Addresses".PadRight(20, '.')}:");
                if (device.Addresses.Any())
                {
                    Console.Write(GetAddressInfo(device.Addresses));
                }

                Console.WriteLine($"{"Network Interface".PadRight(20, '.')}:");
                Console.Write(GetNetworkInterfaceInfo(device.GetNetworkInterface()));

                if (i + 1 != allDevices.Count)
                {
                    Console.WriteLine($"\n{new String('=', 75)}\n");
                }
            }

            Console.ReadLine();
        }

        static string GetNetworkInterfaceInfo(NetworkInterface networkInterface)
        {
            StringBuilder info = new StringBuilder();

            if (networkInterface == null)
            {
                return null;
            }

            info.Append($"\t{"Name".PadRight(25, '.')}: {networkInterface.Name}\n");
            info.Append($"\t{"Description".PadRight(25, '.')}: {networkInterface.Description}\n");
            info.Append($"\t{"Receive Only".PadRight(25, '.')}: {networkInterface.IsReceiveOnly.ToString()}\n");
            info.Append(
                $"\t{"Network Interface Type".PadRight(25, '.')}: {Enum.GetName(typeof(NetworkInterfaceType), networkInterface.NetworkInterfaceType)}\n");
            info.Append(
                $"\t{"Operational Status".PadRight(25, '.')}: {Enum.GetName(typeof(OperationalStatus), networkInterface.OperationalStatus)}\n");
            info.Append($"\t{"Speed".PadRight(25, '.')}: {networkInterface.Speed}\n");
            info.Append(
                $"\t{"Supports Multicast".PadRight(25, '.')}: {networkInterface.SupportsMulticast.ToString()}");

            return info.ToString();
        }

        static string GetAddressInfo(IReadOnlyCollection<DeviceAddress> addresses)
        {
            var addressesInfo = new List<string>();

            foreach (var address in addresses)
            {
                StringBuilder info = new StringBuilder();

                info.Append($"\t{"Address".PadRight(25, '.')}: {address.Address}\n");
                info.Append($"\t{"Broadcast".PadRight(25, '.')}: {address.Broadcast}\n");
                info.Append($"\t{"Destination".PadRight(25, '.')}: {address.Destination}\n");
                info.Append($"\t{"Netmask".PadRight(25, '.')}: {address.Netmask}\n");

                addressesInfo.Add(info.ToString());
            }

            return String.Join("\n", addressesInfo);
        }
    }
}