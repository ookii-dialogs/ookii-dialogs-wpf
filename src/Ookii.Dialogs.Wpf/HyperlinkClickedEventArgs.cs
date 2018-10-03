// Copyright © Sven Groot (Ookii.org) 2009
// BSD license; see license.txt for details.
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
