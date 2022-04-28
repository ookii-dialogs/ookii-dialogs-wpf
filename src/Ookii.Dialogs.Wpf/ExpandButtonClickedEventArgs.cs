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

namespace Ookii.Dialogs.Wpf
{
    /// <summary>
    /// Provides data for the <see cref="TaskDialog.ExpandButtonClicked"/> event.
    /// </summary>
    /// <threadsafety instance="false" static="true" />
    public class ExpandButtonClickedEventArgs : EventArgs
    {
        private bool _expanded;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpandButtonClickedEventArgs"/> class with the specified expanded state.
        /// </summary>
        /// <param name="expanded"><see langword="true" /> if the the expanded content on the dialog is shown; otherwise, <see langword="false" />.</param>
        public ExpandButtonClickedEventArgs(bool expanded)
        {
            _expanded = expanded;
        }

        /// <summary>
        /// Gets a value that indicates if the expanded content on the dialog is shown.
        /// </summary>
        /// <value><see langword="true" /> if the expanded content on the dialog is shown; otherwise, <see langword="false" />.</value>
        public bool Expanded
        {
            get { return _expanded; }
        }
	
    }
}
