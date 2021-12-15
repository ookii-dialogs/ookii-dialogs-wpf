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
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Windows.Win32
{
    static partial class NativeMethods
    {
        public const int ErrorFileNotFound = 2;

        public static bool IsWindowsVistaOrLater
        {
            get
            {
                return Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version >= new Version(6, 0, 6000);
            }
        }

        public static bool IsWindowsXPOrLater
        {
            get
            {
                return Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version >= new Version(5, 1, 2600);
            }
        }

        public static IShellItem CreateItemFromParsingName(string path)
        {
            // https://github.com/microsoft/CsWin32/issues/434#issuecomment-956966139
            var guid = new Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe"); // IID_IShellItem
            var hr = SHCreateItemFromParsingName(path, default, guid/*typeof(IShellItem).GUID*/, out var o);
            if (hr != 0)
                throw new global::System.ComponentModel.Win32Exception(hr);

            IShellItem shellItem = (IShellItem)o;
            return shellItem;
        }

        internal const int CREDUI_MAX_USERNAME_LENGTH = 256 + 1 + 256;
        internal const int CREDUI_MAX_PASSWORD_LENGTH = 256;

        // Implementation of HRESULT_FROM_WIN32 macro
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int HRESULT_FROM_WIN32(int errorCode)
        {
            if ((errorCode & 0x80000000) == 0x80000000)
            {
                return errorCode;
            }

            return (errorCode & 0x0000FFFF) | unchecked((int)0x80070000);
        }
    }
}
