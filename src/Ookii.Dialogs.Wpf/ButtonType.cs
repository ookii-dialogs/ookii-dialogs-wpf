// Copyright (c) Sven Groot (Ookii.org) 2009
// BSD license; see LICENSE for details.

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
