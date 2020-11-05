// Copyright (c) Sven Groot (Ookii.org) 2009
// BSD license; see LICENSE for details.
using Microsoft.Win32.SafeHandles;
using System.Security.Permissions;
using System;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;

namespace Ookii.Dialogs.Wpf
{
    //TODO: Check if CER has to be replaced or removed.
#if NETFRAMEWORK
    [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
#endif
    class SafeModuleHandle : SafeHandle
    {
        public SafeModuleHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }
#if NETFRAMEWORK
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
        protected override bool ReleaseHandle()
        {
            return NativeMethods.FreeLibrary(handle);
        }
    }

#if NETFRAMEWORK
    [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
#endif
    class ActivationContextSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public ActivationContextSafeHandle()
            : base(true)
        {
        }

#if NETFRAMEWORK
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
        protected override bool ReleaseHandle()
        {
            NativeMethods.ReleaseActCtx(handle);
            return true;
        }
    }
}
