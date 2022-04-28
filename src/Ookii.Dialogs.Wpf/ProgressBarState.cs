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
    /// Represents the state of the progress bar on the task dialog.
    /// </summary>
    public enum ProgressBarState
    {
        /// <summary>
        /// Normal state.
        /// </summary>
        Normal,
        /// <summary>
        /// Error state
        /// </summary>
        Error,
        /// <summary>
        /// Paused state
        /// </summary>
        Paused
    }
}
