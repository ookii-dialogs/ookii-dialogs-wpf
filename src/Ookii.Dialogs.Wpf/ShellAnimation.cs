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
using System.Linq;
using System.Text;

namespace Ookii.Dialogs.Wpf
{
    /// <summary>
    /// Resource identifiers for default animations from shell32.dll.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum ShellAnimation
    {
        /// <summary>
        /// An animation representing a file move.
        /// </summary>
        FileMove = 160,
        /// <summary>
        /// An animation representing a file copy.
        /// </summary>
        FileCopy = 161,
        /// <summary>
        /// An animation showing flying papers.
        /// </summary>
        FlyingPapers = 165,
        /// <summary>
        /// An animation showing a magnifying glass over a globe.
        /// </summary>
        SearchGlobe = 166,
        /// <summary>
        /// An animation representing a permament delete.
        /// </summary>
        PermanentDelete = 164,
        /// <summary>
        /// An animation representing deleting an item from the recycle bin.
        /// </summary>
        FromRecycleBinDelete = 163,
        /// <summary>
        /// An animation representing a file move to the recycle bin.
        /// </summary>
        ToRecycleBinDelete = 162,
        /// <summary>
        /// An animation representing a search spanning the local computer.
        /// </summary>
        SearchComputer = 152,
        /// <summary>
        /// An animation representing a search in a document..
        /// </summary>
        SearchDocument = 151,
        /// <summary>
        /// An animation representing a search using a flashlight animation.
        /// </summary>
        SearchFlashlight = 150,
    }
}
