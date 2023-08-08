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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32.System.Diagnostics.Debug;
using Windows.Win32.System.LibraryLoader;

namespace Ookii.Dialogs.Wpf.Interop
{
    class Win32Resources : IDisposable
    {
        private readonly SafeHandle _moduleHandle;
        private const int _bufferSize = 500;

        public Win32Resources(string module)
        {
            _moduleHandle = NativeMethods.LoadLibraryEx(module, default, LOAD_LIBRARY_FLAGS.LOAD_LIBRARY_AS_DATAFILE);
            if (_moduleHandle.IsInvalid)
                throw new System.ComponentModel.Win32Exception(System.Runtime.InteropServices.Marshal.GetLastWin32Error());
        }

        public unsafe string LoadString(uint id)
        {
            CheckDisposed();

            Span<char> buffer = stackalloc char[_bufferSize];
            fixed (char* pBuffer = buffer)
            {
                if (NativeMethods.LoadString(_moduleHandle, id, pBuffer, _bufferSize + 1) == 0)
                    throw new System.ComponentModel.Win32Exception(System.Runtime.InteropServices.Marshal.GetLastWin32Error());
            }
            return buffer.ToString();
        }

        public unsafe string FormatString(uint id, params string[] args)
        {
            CheckDisposed();

            PWSTR buffer = new();
            string source = LoadString(id);

            // For some reason FORMAT_MESSAGE_FROM_HMODULE doesn't work so we use this way.
            FORMAT_MESSAGE_OPTIONS flags = FORMAT_MESSAGE_OPTIONS.FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_OPTIONS.FORMAT_MESSAGE_ARGUMENT_ARRAY | FORMAT_MESSAGE_OPTIONS.FORMAT_MESSAGE_FROM_STRING;

            IntPtr sourcePtr = System.Runtime.InteropServices.Marshal.StringToHGlobalAuto(source);
            try
            {
                fixed (char* pargs = args[0])
                {
                    if (NativeMethods.FormatMessage(flags, (void*)sourcePtr, id, 0, (PWSTR)Unsafe.AsPointer(ref buffer), 0, (sbyte**)&pargs) == 0)
                        throw new System.ComponentModel.Win32Exception(System.Runtime.InteropServices.Marshal.GetLastWin32Error());
                }
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal(sourcePtr);
            }

            string result = buffer.ToString();
            // FreeHGlobal calls LocalFree
            System.Runtime.InteropServices.Marshal.FreeHGlobal((IntPtr)Unsafe.AsPointer(ref buffer));

            return result;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                _moduleHandle.Dispose();
        }

        private void CheckDisposed()
        {
            if (_moduleHandle.IsClosed)
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
