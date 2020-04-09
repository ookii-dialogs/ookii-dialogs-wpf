// Copyright (c) Sven Groot (Ookii.org) 2009
// BSD license; see LICENSE for details.

namespace Ookii.Dialogs.Wpf
{
    /// <summary>
    /// Indicates the display style of custom buttons on a task dialog.
    /// </summary>
    public enum TaskDialogButtonStyle
    {
        /// <summary>
        /// Custom buttons are displayed as regular buttons.
        /// </summary>
        Standard,
        /// <summary>
        /// Custom buttons are displayed as command links using a standard task dialog glyph.
        /// </summary>
        CommandLinks,
        /// <summary>
        /// Custom buttons are displayed as command links without a glyph.
        /// </summary>
        CommandLinksNoIcon
    }
}
