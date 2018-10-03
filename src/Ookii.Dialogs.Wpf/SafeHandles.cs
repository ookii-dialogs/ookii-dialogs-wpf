// Copyright © Sven Groot (Ookii.org) 2009
// BSD license; see license.txt for details.
using Microsoft.Win32.SafeHandles;
using System.Security.Permissions;
using System;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;

namespace Ookii.Dialogs.Wpf
{
    [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
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

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            return NativeMethods.FreeLibrary(handle);
        }
    }

    [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
    class ActivationContextSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public ActivationContextSafeHandle()
            : base(true)
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            NativeMethods.ReleaseActCtx(handle);
            return true;
        }
    }
}