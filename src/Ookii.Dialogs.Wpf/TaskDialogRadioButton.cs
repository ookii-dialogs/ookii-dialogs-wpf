// Copyright © Sven Groot (Ookii.org) 2009
// BSD license; see license.txt for details.
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Ookii.Dialogs.Wpf
{
    /// <summary>
    /// A radio button on a task dialog.
    /// </summary>
    /// <threadsafety static="true" instance="false" />
    public class TaskDialogRadioButton : TaskDialogItem
    {
        private bool _checked;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskDialogRadioButton"/> class.
        /// </summary>
        public TaskDialogRadioButton()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskDialogRadioButton"/> class with the specified container.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> to add the <see cref="TaskDialogRadioButton"/> to.</param>
        public TaskDialogRadioButton(IContainer container)
            : base(container)
        {
        }
        
        /// <summary>
        /// Gets or sets a value that indicates whether the radio button is checked.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the radio button is checked; otherwise, <see langword="false"/>.
        /// The default value is <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// Setting this property while the dialog is being displayed has no effect. Instead, use the <see cref="TaskDialogItem.Click"/>
        /// method to check a particular radio button.
        /// </remarks>
        [Category("Appearance"), Description("Indicates whether the radio button is checked."), DefaultValue(false)]
        public bool Checked
        {
            get { return _checked; }
            set 
            {
                _checked = value;
                if( value && Owner != null )
                {
                    foreach( TaskDialogRadioButton radioButton in Owner.RadioButtons )
                    {
                        if( radioButton != this )
                            radioButton.Checked = false;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the collection that items of this type are part of.
        /// </summary>
        /// <value>
        /// If the <see cref="TaskDialogButton"/> is currently associated with a <see cref="TaskDialog"/>, the
        /// <see cref="TaskDialog.Buttons"/> collection of that <see cref="TaskDialog"/>; otherwise, <see langword="null" />.
        /// </value>
        protected override System.Collections.IEnumerable ItemCollection
        {
            get 
            {
                if( Owner != null )
                    return Owner.RadioButtons;
                return null;
            }
        }
    }
}
