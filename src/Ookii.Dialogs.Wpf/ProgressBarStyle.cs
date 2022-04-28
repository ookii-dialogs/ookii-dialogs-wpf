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
    /// Indicates the type of progress on a task dialog.
    /// </summary>
    public enum ProgressBarStyle
    {
        /// <summary>
        /// No progress bar is displayed on the dialog.
        /// </summary>
        None,
        /// <summary>
        /// A regular progress bar is displayed on the dialog.
        /// </summary>
        ProgressBar,
        /// <summary>
        /// A marquee progress bar is displayed on the dialog. Use this value for operations
        /// that cannot report concrete progress information.
        /// </summary>
        MarqueeProgressBar
    }
}
