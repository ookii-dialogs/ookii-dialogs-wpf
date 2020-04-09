// Copyright (c) Sven Groot (Ookii.org) 2006
// See LICENSE for details
using System;

namespace Ookii.Dialogs.Wpf
{
    class VistaFileDialogEvents : Interop.IFileDialogEvents, Interop.IFileDialogControlEvents
    {
        const uint S_OK = 0;
        const uint S_FALSE = 1;
        const uint E_NOTIMPL = 0x80004001;

        private VistaFileDialog _dialog;

        public VistaFileDialogEvents(VistaFileDialog dialog)
        {
            if( dialog == null )
                throw new ArgumentNullException("dialog");

            _dialog = dialog;
        }

        #region IFileDialogEvents Members

        public Interop.HRESULT OnFileOk(Interop.IFileDialog pfd)
        {
            if( _dialog.DoFileOk(pfd) )
                return Interop.HRESULT.S_OK;
            else
                return Interop.HRESULT.S_FALSE;
        }

        public Interop.HRESULT OnFolderChanging(Interop.IFileDialog pfd, Interop.IShellItem psiFolder)
        {
            return Interop.HRESULT.S_OK;
        }

        public void OnFolderChange(Interop.IFileDialog pfd)
        {
        }

        public void OnSelectionChange(Interop.IFileDialog pfd)
        {
        }

        public void OnShareViolation(Interop.IFileDialog pfd, Interop.IShellItem psi, out NativeMethods.FDE_SHAREVIOLATION_RESPONSE pResponse)
        {
            pResponse = NativeMethods.FDE_SHAREVIOLATION_RESPONSE.FDESVR_DEFAULT;
        }

        public void OnTypeChange(Interop.IFileDialog pfd)
        {
        }

        public void OnOverwrite(Interop.IFileDialog pfd, Interop.IShellItem psi, out NativeMethods.FDE_OVERWRITE_RESPONSE pResponse)
        {
            pResponse = NativeMethods.FDE_OVERWRITE_RESPONSE.FDEOR_DEFAULT;
        }

        #endregion

        #region IFileDialogControlEvents Members

        public void OnItemSelected(Interop.IFileDialogCustomize pfdc, int dwIDCtl, int dwIDItem)
        {
        }

        public void OnButtonClicked(Interop.IFileDialogCustomize pfdc, int dwIDCtl)
        {
        }

        public void OnCheckButtonToggled(Interop.IFileDialogCustomize pfdc, int dwIDCtl, bool bChecked)
        {
        }

        public void OnControlActivating(Interop.IFileDialogCustomize pfdc, int dwIDCtl)
        {
        }

        #endregion


    }
}
