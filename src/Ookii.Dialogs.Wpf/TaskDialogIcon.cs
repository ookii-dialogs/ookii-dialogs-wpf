// Copyright © Sven Groot (Ookii.org) 2009
// BSD license; see license.txt for details.
using System;
using System.Collections.Generic;
using System.Text;

namespace Ookii.Dialogs.Wpf
{
    /// <summary>
    /// Indicates the icon to use for a task dialog.
    /// </summary>
    public enum TaskDialogIcon
    {
        /// <summary>
        /// A custom icon or no icon if no custom icon is specified.
        /// </summary>
        Custom,
        /// <summary>
        /// System warning icon.
        /// </summary>
        Warning = 0xFFFF, // MAKEINTRESOURCEW(-1)
        /// <summary>
        /// System Error icon.
        /// </summary>
        Error = 0xFFFE, // MAKEINTRESOURCEW(-2)
        /// <summary>
        /// System Information icon.
        /// </summary>
        Information = 0xFFFD, // MAKEINTRESOURCEW(-3)
        /// <summary>
        /// Shield icon.
        /// </summary>
        Shield = 0xFFFC, // MAKEINTRESOURCEW(-4)
    }
}
