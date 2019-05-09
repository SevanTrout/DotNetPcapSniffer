using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using PcapDotNet.Core;
using PcapDotNet.Core.Extensions;
using PcapDotNet.Packets;
using PcapDotNet.Packets.IpV4;

namespace Lab_4
{
    public static class Encoder
    {
        public static char GetCP866Char(int charCode)
        {
            return Encoding.GetEncoding(866).GetChars(new byte[] {(byte) charCode}).First();
        }
    }

    internal class PacketInfoTable
    {
        public static void CreateTableHeader()
        {
            var tableHeader = $"{Encoder.GetCP866Char(0xC9)}" +
                              $"{new string(Encoder.GetCP866Char(0xCD), 25)}{Encoder.GetCP866Char(0xCB)}" +
                              $"{new string(Encoder.GetCP866Char(0xCD), 25)}{Encoder.GetCP866Char(0xCB)}" +
                              $"{new string(Encoder.GetCP866Char(0xCD), 25)}{Encoder.GetCP866Char(0xCB)}" +
                              $"{new string(Encoder.GetCP866Char(0xCD), 10)}{Encoder.GetCP866Char(0xBB)}\n" +
                              $"{Encoder.GetCP866Char(0xBA)}" +
                              $"{"Source Address".PadRight(25, ' ')}{Encoder.GetCP866Char(0xBA)}" +
                              $"{"Destination Address".PadRight(25, ' ')}{Encoder.GetCP866Char(0xBA)}" +
                              $"{"Timestamp".PadRight(25, ' ')}{Encoder.GetCP866Char(0xBA)}" +
                              $"{"Length".PadRight(10, ' ')}{Encoder.GetCP866Char(0xBA)}\n" +
                              $"{Encoder.GetCP866Char(0xC8)}" +
                              $"{new string(Encoder.GetCP866Char(0xCD), 25)}{Encoder.GetCP866Char(0xCA)}" +
                              $"{new string(Encoder.GetCP866Char(0xCD), 25)}{Encoder.GetCP866Char(0xCA)}" +
                              $"{new string(Encoder.GetCP866Char(0xCD), 25)}{Encoder.GetCP866Char(0xCA)}" +
                              $"{new string(Encoder.GetCP866Char(0xCD), 10)}{Encoder.GetCP866Char(0xBC)}";

            Console.WriteLine($"{tableHeader}");
        }

        public static void CreateTableRow(string sourceAddress, string destinationAddress, string timestamp,
            string length)
        {
            var tableRow = $"{Encoder.GetCP866Char(0xB3)}" +
                           $"{sourceAddress.PadRight(25, ' ')}{Encoder.GetCP866Char(0xB3)}" +
                           $"{destinationAddress.PadRight(25, ' ')}{Encoder.GetCP866Char(0xB3)}" +
                           $"{timestamp.PadRight(25, ' ')}{Encoder.GetCP866Char(0xB3)}" +
                           $"{length.PadRight(10, ' ')}{Encoder.GetCP866Char(0xB3)}\n" +
                           $"{Encoder.GetCP866Char(0xB3)}" +
                           $"{new string(' ', 25)}{Encoder.GetCP866Char(0xB3)}" +
                           $"{new string(' ', 25)}{Encoder.GetCP866Char(0xB3)}" +
                           $"{new string(' ', 25)}{Encoder.GetCP866Char(0xB3)}" +
                           $"{new string(' ', 10)}{Encoder.GetCP866Char(0xB3)}";

            Console.WriteLine(tableRow);
        }
    }

    public class Program
    {
        private static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.GetEncoding(866);

            IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;

            if (!allDevices.Any())
            {
                Console.WriteLine("No devices");
                return;
            }

            PrintDevicesInfo(allDevices);

            int deviceIndex = GetDeviceIndex(allDevices.Count);
            PacketDevice selectedDevice = allDevices[deviceIndex - 1];

            using (var communicator = selectedDevice.Open(65536, PacketDeviceOpenAttributes.Promiscuous, 1000))
            {
                Console.WriteLine($"Listening on {selectedDevice.Description}...");

                using (BerkeleyPacketFilter filter = communicator.CreateFilter("tcp"))
                {
                    communicator.SetFilter(filter);
                }

                PacketInfoTable.CreateTableHeader();
                communicator.ReceivePackets(0, PacketHandler);
            }

            Console.Write("Press any key to exit.");
            Console.ReadLine();
        }

        private static void PacketHandler(Packet packet)
        {
            var ip = packet.Ethernet.IpV4;
            var tcp = ip.Tcp;

            var sourceAddress = $"{ip.Source}:{tcp.SourcePort}";
            var destinationAddress = $"{ip.Destination}:{tcp.DestinationPort}";
            var timestamp = $"{packet.Timestamp:yyyy-MM-dd hh:mm:ss.fff}";
            var length = $"{tcp.Length}";

            PacketInfoTable.CreateTableRow(sourceAddress, destinationAddress, timestamp, length);
        }

        private static void PrintDevicesInfo(IList<LivePacketDevice> allDevices)
        {
            for (var i = 0; i < allDevices.Count; i++)
            {
                var device = allDevices[i];

                Console.WriteLine($"{i + 1})\n{"Name".PadRight(20, '.')}: {device.Name}");
                Console.WriteLine($"{"Description".PadRight(20, '.')}: {device.Description}");

                Console.WriteLine($"{"Attributes".PadRight(20, '.')}: " +
                                  $"{Enum.GetName(typeof(DeviceAttributes), device.Attributes)}");
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
                    Console.WriteLine($"\n{new string(Encoder.GetCP866Char(0xCD), 100)}\n");
                }
            }

            Console.WriteLine();
        }

        private static int GetDeviceIndex(int maxDeviceIndex)
        {
            int deviceIndex;
            do
            {
                Console.Write($"Enter the interface number (1-{maxDeviceIndex}): ");
                var deviceIndexString = Console.ReadLine();
                if (!int.TryParse(deviceIndexString, out deviceIndex) ||
                    deviceIndex < 1 || deviceIndex > maxDeviceIndex)
                {
                    deviceIndex = 0;
                }
            } while (deviceIndex == 0);

            return deviceIndex;
        }

        private static string GetNetworkInterfaceInfo(NetworkInterface networkInterface)
        {
            StringBuilder info = new StringBuilder();

            if (networkInterface == null)
            {
                return null;
            }

            info.Append($"\t{"Name".PadRight(25, '.')}: {networkInterface.Name}\n");
            info.Append($"\t{"Description".PadRight(25, '.')}: {networkInterface.Description}\n");
            info.Append($"\t{"Receive Only".PadRight(25, '.')}: {networkInterface.IsReceiveOnly.ToString()}\n");
            info.Append($"\t{"Network Interface Type".PadRight(25, '.')}: " +
                        $"{Enum.GetName(typeof(NetworkInterfaceType), networkInterface.NetworkInterfaceType)}\n");
            info.Append($"\t{"Operational Status".PadRight(25, '.')}: " +
                        $"{Enum.GetName(typeof(OperationalStatus), networkInterface.OperationalStatus)}\n");
            info.Append($"\t{"Speed".PadRight(25, '.')}: {networkInterface.Speed}\n");
            info.Append($"\t{"Supports Multicast".PadRight(25, '.')}: " +
                        $"{networkInterface.SupportsMulticast.ToString()}");

            return info.ToString();
        }

        private static string GetAddressInfo(IEnumerable<DeviceAddress> addresses)
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

            return string.Join("\n", addressesInfo);
        }
    }
}