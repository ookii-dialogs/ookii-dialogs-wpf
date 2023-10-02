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

namespace Ookii.Dialogs.Wpf
{
    class VistaFileDialogEvents : IFileDialogEvents, IFileDialogControlEvents
    {
        private readonly VistaFileDialog _dialog;

        public VistaFileDialogEvents(VistaFileDialog dialog)
        {
            _dialog = dialog ?? throw new ArgumentNullException("dialog");
        }

        #region IFileDialogEvents Members

        HRESULT IFileDialogEvents.OnFileOk(IFileDialog pfd)
        {
            if (_dialog.DoFileOk(pfd))
                return HRESULT.S_OK;
            else
                return HRESULT.S_FALSE;
        }

        HRESULT IFileDialogEvents.OnFolderChanging(IFileDialog pfd, IShellItem psiFolder) => HRESULT.S_OK;

        public void OnFolderChange(IFileDialog pfd)
        {
        }

        public void OnSelectionChange(IFileDialog pfd)
        {
        }

        public unsafe void OnShareViolation(IFileDialog pfd, IShellItem psi, FDE_SHAREVIOLATION_RESPONSE* pResponse)
        {
            *pResponse = FDE_SHAREVIOLATION_RESPONSE.FDESVR_DEFAULT;
        }

        public void OnTypeChange(IFileDialog pfd)
        {
        }

        public unsafe void OnOverwrite(IFileDialog pfd, IShellItem psi, FDE_OVERWRITE_RESPONSE* pResponse)
        {
            *pResponse = FDE_OVERWRITE_RESPONSE.FDEOR_DEFAULT;
        }

        #endregion

        #region IFileDialogControlEvents Members

        public void OnItemSelected(IFileDialogCustomize pfdc, uint dwIDCtl, uint dwIDItem)
        {
        }

        public void OnButtonClicked(IFileDialogCustomize pfdc, uint dwIDCtl)
        {
        }

        public void OnCheckButtonToggled(IFileDialogCustomize pfdc, uint dwIDCtl, BOOL bChecked)
        {
        }

        public void OnControlActivating(IFileDialogCustomize pfdc, uint dwIDCtl)
        {
        }

        #endregion
    }
}
