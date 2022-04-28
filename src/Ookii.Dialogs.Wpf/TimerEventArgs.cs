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
    /// Provides data for the <see cref="TaskDialog.Timer"/> event.
    /// </summary>
    /// <threadsafety instance="false" static="true" />
    public class TimerEventArgs : EventArgs
    {
        private int _tickCount;
        private bool _resetTickCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimerEventArgs"/> class with the specified tick count.
        /// </summary>
        /// <param name="tickCount">The tick count.</param>
        public TimerEventArgs(int tickCount)
        {
            _tickCount = tickCount;
        }
        
        /// <summary>
        /// Gets or sets a value that indicates whether the tick count should be reset.
        /// </summary>
        /// <value>
        /// <see langword="true" /> to reset the tick count after the event handler returns; otherwise, <see langword="false" />.
        /// The default value is <see langword="false" />.
        /// </value>
        public bool ResetTickCount
        {
            get { return _resetTickCount; }
            set { _resetTickCount = value; }
        }

        /// <summary>
        /// Gets the current tick count of the timer.
        /// </summary>
        /// <value>
        /// The number of milliseconds that has elapsed since the dialog was created or since the last time the event handler returned
        /// with the <see cref="ResetTickCount"/> property set to <see langword="true" />.
        /// </value>
        public int TickCount
        {
            get { return _tickCount; }
        }
	
    }
}
