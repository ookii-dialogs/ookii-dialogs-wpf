// Copyright © Sven Groot (Ookii.org) 2009
// BSD license; see license.txt for details.
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
