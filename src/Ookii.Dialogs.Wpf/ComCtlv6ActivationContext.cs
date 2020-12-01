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
