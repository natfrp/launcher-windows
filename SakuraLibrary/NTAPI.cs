using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.AccessControl;

namespace SakuraLibrary
{
    [Flags]
    public enum ServiceAccessRights
    {
        SERVICE_ALL_ACCESS = 0xF01FF,
        SERVICE_CHANGE_CONFIG = 0x0002,
        SERVICE_ENUMERATE_DEPENDENTS = 0x0008,
        SERVICE_INTERROGATE = 0x0080,
        SERVICE_PAUSE_CONTINUE = 0x0040,
        SERVICE_QUERY_CONFIG = 0x0001,
        SERVICE_QUERY_STATUS = 0x0004,

        SERVICE_START = 0x0010,
        SERVICE_STOP = 0x0020,
        SERVICE_USER_DEFINED_CONTROL = 0x0100,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SECURITY_DESCRIPTOR
    {
        public byte revision;
        public byte size;
        public short control;
        public IntPtr owner;
        public IntPtr group;
        public IntPtr sacl;
        public IntPtr dacl;
    }

    public static class NTAPI
    {
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        public static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] uint dwFlags, [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool QueryServiceObjectSecurity(IntPtr serviceHandle, SecurityInfos secInfo, ref SECURITY_DESCRIPTOR lpSecDesrBuf, uint bufSize, out uint bufSizeNeeded);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool QueryServiceObjectSecurity(SafeHandle serviceHandle, SecurityInfos secInfo, byte[] lpSecDesrBuf, uint bufSize, out uint bufSizeNeeded);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool SetServiceObjectSecurity(SafeHandle serviceHandle, SecurityInfos secInfos, byte[] lpSecDesrBuf);
    }
}
