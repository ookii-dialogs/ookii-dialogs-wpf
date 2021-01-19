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
using System.Text;

namespace Ookii.Dialogs.Wpf.Interop
{
    class Win32Resources : IDisposable
    {
        private SafeModuleHandle _moduleHandle;
        private const int _bufferSize = 500;

        public Win32Resources(string module)
        {
            _moduleHandle = NativeMethods.LoadLibraryEx(module, IntPtr.Zero, NativeMethods.LoadLibraryExFlags.LoadLibraryAsDatafile);
            if( _moduleHandle.IsInvalid )
                throw new System.ComponentModel.Win32Exception(System.Runtime.InteropServices.Marshal.GetLastWin32Error());
        }

        public string LoadString(uint id)
        {
            CheckDisposed();

            StringBuilder buffer = new StringBuilder(_bufferSize);
            if( NativeMethods.LoadString(_moduleHandle, id, buffer, buffer.Capacity + 1) == 0 )
                throw new System.ComponentModel.Win32Exception(System.Runtime.InteropServices.Marshal.GetLastWin32Error());
            return buffer.ToString();
        }

        public string FormatString(uint id, params string[] args)
        {
            CheckDisposed();

            IntPtr buffer = IntPtr.Zero;
            string source = LoadString(id);

            // For some reason FORMAT_MESSAGE_FROM_HMODULE doesn't work so we use this way.
            NativeMethods.FormatMessageFlags flags = NativeMethods.FormatMessageFlags.FORMAT_MESSAGE_ALLOCATE_BUFFER | NativeMethods.FormatMessageFlags.FORMAT_MESSAGE_ARGUMENT_ARRAY | NativeMethods.FormatMessageFlags.FORMAT_MESSAGE_FROM_STRING;

            IntPtr sourcePtr = System.Runtime.InteropServices.Marshal.StringToHGlobalAuto(source);
            try
            {
                if( NativeMethods.FormatMessage(flags, sourcePtr, id, 0, ref buffer, 0, args) == 0 )
                    throw new System.ComponentModel.Win32Exception(System.Runtime.InteropServices.Marshal.GetLastWin32Error());
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal(sourcePtr);
            }

            string result = System.Runtime.InteropServices.Marshal.PtrToStringAuto(buffer);
            // FreeHGlobal calls LocalFree
            System.Runtime.InteropServices.Marshal.FreeHGlobal(buffer);

            return result;
        }

        protected virtual void Dispose(bool disposing)
        {
            if( disposing )
                _moduleHandle.Dispose();
        }

        private void CheckDisposed()
        {
            if( _moduleHandle.IsClosed )
            {
                throw new ObjectDisposedException("Win32Resources");
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion    
    }
}
