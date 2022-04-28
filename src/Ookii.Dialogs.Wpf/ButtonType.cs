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
    /// Represents the type of a task dialog button.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
    public enum ButtonType
    {
        /// <summary>
        /// The button is a custom button.
        /// </summary>
        Custom = 0,
        /// <summary>
        /// The button is the common OK button.
        /// </summary>
        Ok = 1,
        /// <summary>
        /// The button is the common Yes button.
        /// </summary>
        Yes = 6,
        /// <summary>
        /// The button is the common No button.
        /// </summary>
        No = 7,
        /// <summary>
        /// The button is the common Cancel button.
        /// </summary>
        Cancel = 2,
        /// <summary>
        /// The button is the common Retry button.
        /// </summary>
        Retry = 4,
        /// <summary>
        /// The button is the common Close button.
        /// </summary>
        Close = 8
    }
}
