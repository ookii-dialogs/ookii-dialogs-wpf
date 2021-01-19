#region Copyright 2009-2021 Ookii Dialogs Contributors
//
// Licensed under the BSD 3-Clause License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://opensource.org/licenses/BSD-3-Clause
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Ookii.Dialogs.Wpf.Interop
{
    [ComImport]
    [Guid(CLSIDGuid.ProgressDialog)]
    internal class ProgressDialogRCW
    {
    }

    [ComImport,
    Guid(IIDGuid.IProgressDialog),
    CoClass(typeof(ProgressDialogRCW))]
    internal interface ProgressDialog : IProgressDialog
    {
    }

    [Flags]
    internal enum ProgressDialogFlags : uint
    {
        Normal = 0x00000000,
        Modal = 0x00000001,
        AutoTime = 0x00000002,
        NoTime = 0x00000004,
        NoMinimize = 0x00000008,
        NoProgressBar = 0x00000010,
        MarqueeProgress = 0x00000020,
        NoCancel = 0x00000040
    }

    [ComImport]
    [Guid(IIDGuid.IProgressDialog)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IProgressDialog
    {

        [PreserveSig]
        void StartProgressDialog(
            IntPtr hwndParent,
            [MarshalAs(UnmanagedType.IUnknown)]
			object punkEnableModless,
            ProgressDialogFlags dwFlags,
            IntPtr pvResevered
            );

        [PreserveSig]
        void StopProgressDialog();

        [PreserveSig]
        void SetTitle(
            [MarshalAs(UnmanagedType.LPWStr)]
			string pwzTitle
            );

        [PreserveSig]
        void SetAnimation(
            SafeModuleHandle hInstAnimation,
            ushort idAnimation
            );

        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Bool)]
        bool HasUserCancelled();

        [PreserveSig]
        void SetProgress(
            uint dwCompleted,
            uint dwTotal
            );
        [PreserveSig]
        void SetProgress64(
            ulong ullCompleted,
            ulong ullTotal
            );

        [PreserveSig]
        void SetLine(
            uint dwLineNum,
            [MarshalAs(UnmanagedType.LPWStr)]
			string pwzString,
            [MarshalAs(UnmanagedType.VariantBool)]
			bool fCompactPath,
            IntPtr pvResevered
            );

        [PreserveSig]
        void SetCancelMsg(
            [MarshalAs(UnmanagedType.LPWStr)]
			string pwzCancelMsg,
            object pvResevered
            );

        [PreserveSig]
        void Timer(
            uint dwTimerAction,
            object pvResevered
            );

    }

}
