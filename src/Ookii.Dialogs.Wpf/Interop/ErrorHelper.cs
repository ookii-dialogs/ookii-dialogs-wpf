using System;
using System.Collections.Generic;
using System.Text;

namespace Ookii.Dialogs.Wpf.Interop
{
    internal enum HRESULT : long
    {
        S_FALSE = 0x0001,
        S_OK = 0x0000,
        E_INVALIDARG = 0x80070057,
        E_OUTOFMEMORY = 0x8007000E,
        ERROR_CANCELLED = 0x800704C7
    }
}
