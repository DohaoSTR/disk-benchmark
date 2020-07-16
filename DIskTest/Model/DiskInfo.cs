using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace DIskTest
{
    class DiskInfo
    {
        public struct MemStatus
        {
            internal uint dwLength;
            internal uint dwMemoryLoad;
            internal ulong ullTotalPhys;
            internal ulong ullAvailPhys;
            internal ulong ullTotalPageFile;
            internal ulong ullAvailPageFile;
            internal ulong ullTotalVirtual;
            internal ulong ullAvailVirtual;
            internal ulong ullAvailExtendedVirtual;
        }

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GlobalMemoryStatusEx(ref MemStatus lpBuffer);

        private static MemStatus memStatus = new MemStatus();

        private static void UpdateWindowsMemStatus()
        {
            if (!GlobalMemoryStatusEx(ref memStatus)) 
                throw new Win32Exception("Ошибка при получении информации о памяти Windows!");
        }

        static DiskInfo()
        {
            memStatus.dwLength = checked((uint)Marshal.SizeOf(typeof(MemStatus)));
            UpdateWindowsMemStatus();
            TotalRam = (long)memStatus.ullTotalPhys;
        }

        public const string fileName = "DIskTestFile.dat";

        public static long TotalRam
        {
            get;
        }

        public static DriveInfo[] GetEligibleDrives()
        {
            var drives = DriveInfo.GetDrives().AsEnumerable();
            return drives.ToArray();
        }

        public static string GetTempFilePath(string drivePath)
        {
            string path;
            var sysRoot = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            if (sysRoot == drivePath)
            {
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), fileName);
            }
            else path = Path.Combine(drivePath, fileName);
            return path;
        }
    }
}
