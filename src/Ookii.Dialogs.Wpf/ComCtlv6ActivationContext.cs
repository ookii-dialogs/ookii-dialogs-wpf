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
using System.IO;
using System.Runtime.InteropServices;

namespace Ookii.Dialogs.Wpf
{
    sealed class ComCtlv6ActivationContext : IDisposable 
    {
        // Private data
        private IntPtr _cookie;
        private static NativeMethods.ACTCTX _enableThemingActivationContext;
        private static ActivationContextSafeHandle _activationContext;
        private static bool _contextCreationSucceeded;
        private static readonly object _contextCreationLock = new object();

        public ComCtlv6ActivationContext(bool enable)
        {
            if( enable && NativeMethods.IsWindowsXPOrLater )
            {
                if( EnsureActivateContextCreated() )
                {
                    if( !NativeMethods.ActivateActCtx(_activationContext, out _cookie) )
                    {
                        // Be sure cookie always zero if activation failed
                        _cookie = IntPtr.Zero;
                    }
                }
            }
        }

        ~ComCtlv6ActivationContext()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if( _cookie != IntPtr.Zero )
            {
                if( NativeMethods.DeactivateActCtx(0, _cookie) )
                {
                    // deactivation succeeded...
                    _cookie = IntPtr.Zero;
                }
            }
        }

        private static bool EnsureActivateContextCreated()
        {
            lock (_contextCreationLock)
            {
                if (_contextCreationSucceeded)
                {
                    return _contextCreationSucceeded;
                }

                const string manifestResourceName = "Ookii.Dialogs.XPThemes.manifest";
                string manifestTempFilePath;

                using (var manifest = typeof(ComCtlv6ActivationContext).Assembly.GetManifestResourceStream(manifestResourceName))
                {
                    if (manifest is null)
                    {
                        throw new InvalidOperationException($"Unable to retrieve {manifestResourceName} embedded resource");
                    }

                    manifestTempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

                    using (var tempFileStream = new FileStream(manifestTempFilePath, FileMode.CreateNew, FileAccess.ReadWrite,
                        FileShare.Delete | FileShare.ReadWrite))
                    {
                        manifest.CopyTo(tempFileStream);
                    }
                }

                _enableThemingActivationContext = new NativeMethods.ACTCTX
                {
                    cbSize = Marshal.SizeOf(typeof(NativeMethods.ACTCTX)),
                    lpSource = manifestTempFilePath,
                };

                // Note this will fail gracefully if file specified
                // by manifestFilePath doesn't exist.
                _activationContext = NativeMethods.CreateActCtx(ref _enableThemingActivationContext);
                _contextCreationSucceeded = !_activationContext.IsInvalid;

                try
                {
                    File.Delete(manifestTempFilePath);
                }
                catch (Exception)
                {
                    // We tried to be tidy but something blocked us :(
                }

                // If we return false, we'll try again on the next call into
                // EnsureActivateContextCreated(), which is fine.
                return _contextCreationSucceeded;
            }
        }
    }
}
