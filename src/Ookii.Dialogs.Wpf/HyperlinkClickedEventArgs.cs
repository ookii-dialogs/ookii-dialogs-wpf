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
    /// Class that provides data for the <see cref="TaskDialog.HyperlinkClicked"/> event.
    /// </summary>
    /// <threadsafety instance="false" static="true" />
    public class HyperlinkClickedEventArgs : EventArgs
    {
        private string _href;

        /// <summary>
        /// Creates a new instance of the <see cref="HyperlinkClickedEventArgs"/> class with the specified URL.
        /// </summary>
        /// <param name="href">The URL of the hyperlink.</param>
        public HyperlinkClickedEventArgs(string href)
        {
            _href = href;
        }

        /// <summary>
        /// Gets the URL of the hyperlink that was clicked.
        /// </summary>
        /// <value>
        /// The value of the href attribute of the hyperlink.
        /// </value>
        public string Href
        {
            get { return _href; }
        }
	
    }
}
