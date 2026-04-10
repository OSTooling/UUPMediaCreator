using System.Runtime.InteropServices;

namespace Windows.Internal.Flighting
{
    internal class NativeMethods
    {
        [DllImport("wincorlib.dll", CharSet = CharSet.Unicode, EntryPoint = "#129", SetLastError = true)]
        internal static extern int GetActivationFactoryByPCWSTR(string typeName, ref Guid typeGuid, out IntPtr ppOut);
    }
}