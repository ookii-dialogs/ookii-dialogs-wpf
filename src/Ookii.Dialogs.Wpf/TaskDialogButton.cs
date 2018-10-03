// Copyright © Sven Groot (Ookii.org) 2009
// BSD license; see license.txt for details.
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Drawing.Design;

namespace Ookii.Dialogs.Wpf
{
    /// <summary>
    /// A button on a <see cref="TaskDialog"/>.
    /// </summary>
    /// <threadsafety instance="false" static="true" />
    public class TaskDialogButton : TaskDialogItem
    {
        private ButtonType _type;
        private bool _elevationRequired;
        private bool _default;
        private string _commandLinkNote;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskDialogButton"/> class.
        /// </summary>
        public TaskDialogButton()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskDialogButton"/> class with the specified button type.
        /// </summary>
        /// <param name="type">The type of the button.</param>
        public TaskDialogButton(ButtonType type)
            : base((int)type)
        {
            _type = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskDialogButton"/> class with the specified container.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> to add the <see cref="TaskDialogButton"/> to.</param>
        public TaskDialogButton(IContainer container)
            : base(container)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskDialogButton"/> class with the specified text.
        /// </summary>
        /// <param name="text">The text of the button.</param>
        public TaskDialogButton(string text)
        {
            Text = text;
        }

        /// <summary>
        /// Gets or sets the type of the button.
        /// </summary>
        /// <value>
        /// One of the <see cref="Ookii.Dialogs.Wpf.ButtonType"/> values that indicates the type of the button. The default value
        /// is <see cref="Ookii.Dialogs.Wpf.ButtonType.Custom"/>.
        /// </value>
        [Category("Appearance"), Description("The type of the button."), DefaultValue(ButtonType.Custom)]
        public ButtonType ButtonType
        {
            get { return _type; }
            set 
            {
                if( value != ButtonType.Custom )
                {
                    CheckDuplicateButton(value, null);
                    _type = value;
                    base.Id = (int)value;
                }
                else
                {
                    _type = value;
                    AutoAssignId();
                    UpdateOwner();
                }
            }
        }

        /// <summary>
        /// Gets or sets the text of the note associated with a command link button.
        /// </summary>
        /// <value>
        /// The text of the note associated with a command link button.
        /// </value>
        /// <remarks>
        /// <para>
        ///   This property applies only to buttons where the <see cref="Type"/> property
        ///   is <see cref="Ookii.Dialogs.Wpf.ButtonType.Custom"/>. For other button types, it is ignored.
        /// </para>
        /// <para>
        ///   In addition, it is used only if the <see cref="TaskDialog.ButtonStyle"/> property is set to
        ///   <see cref="TaskDialogButtonStyle.CommandLinks"/> or <see cref="TaskDialogButtonStyle.CommandLinksNoIcon"/>;
        ///   otherwise, it is ignored.
        /// </para>
        /// </remarks>
        [Localizable(true), Category("Appearance"), Description("The text of the note associated with a command link button."), DefaultValue(""), Editor(typeof(System.ComponentModel.Design.MultilineStringEditor), typeof(UITypeEditor))]
        public string CommandLinkNote
        {
            get { return _commandLinkNote ?? string.Empty; }
            set
            { 
                _commandLinkNote = value;
                UpdateOwner();
            }
        }
	

        /// <summary>
        /// Gets or sets a value that indicates if the button is the default button on the dialog.
        /// </summary>
        /// <value><see langword="true" /> if the button is the default button; otherwise, <see langword="false" />.
        /// The default value is <see langword="false" />.</value>
        /// <remarks>
        /// If no button has this property set to <see langword="true" />, the first button on the dialog will be the default button.
        /// </remarks>
        [Category("Behavior"), Description("Indicates if the button is the default button on the dialog."), DefaultValue(false)]
        public bool Default
        {
            get { return _default; }
            set
            {
                _default = value;
                if( value && Owner != null )
                {
                    foreach( TaskDialogButton button in Owner.Buttons )
                    {
                        if( button != this )
                            button.Default = false;
                    }
                }
                UpdateOwner();
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the Task Dialog button or command link should have a 
        /// User Account Control (UAC) shield icon (in other words, whether the action invoked by the 
        /// button requires elevation). 
        /// </summary>
        /// <value>
        /// <see langword="true" /> if the button contains a UAC shield icon; otherwise, <see langword="false" />.
        /// </value>
        /// <remarks>
        /// Elevation is not performed by the task dialog; the code implementing the operation that results from
        /// the button being clicked is responsible for performing elevation if required.
        /// </remarks>
        [Category("Behavior"), Description("Indicates whether the Task Dialog button or command link should have a User Account Control (UAC) shield icon (in other words, whether the action invoked by the button requires elevation)."), DefaultValue(false)] 
        public bool ElevationRequired
        {
            get { return _elevationRequired; }
            set 
            { 
                _elevationRequired = value;
                if( Owner != null )
                    Owner.SetButtonElevationRequired(this);
            }
        }
	
	
        internal override int Id
        {
            get
            {
                return base.Id;
            }
            set
            {
                if( base.Id != value )
                {
                    if( _type != ButtonType.Custom )
                        throw new InvalidOperationException(Properties.Resources.NonCustomTaskDialogButtonIdError);
                    base.Id = value;
                }
            }
        }

        internal override void AutoAssignId()
        {
            if( _type == ButtonType.Custom )
                base.AutoAssignId();
        }

        internal override void CheckDuplicate(TaskDialogItem itemToExclude)
        {
            CheckDuplicateButton(_type, itemToExclude);
            base.CheckDuplicate(itemToExclude);
        }

        internal NativeMethods.TaskDialogCommonButtonFlags ButtonFlag
        {
            get
            {
                switch( _type )
                {
                case ButtonType.Ok:
                    return NativeMethods.TaskDialogCommonButtonFlags.OkButton;
                case ButtonType.Yes:
                    return NativeMethods.TaskDialogCommonButtonFlags.YesButton;
                case ButtonType.No:
                    return NativeMethods.TaskDialogCommonButtonFlags.NoButton;
                case ButtonType.Cancel:
                    return NativeMethods.TaskDialogCommonButtonFlags.CancelButton;
                case ButtonType.Retry:
                    return NativeMethods.TaskDialogCommonButtonFlags.RetryButton;
                case ButtonType.Close:
                    return NativeMethods.TaskDialogCommonButtonFlags.CloseButton;
                default:
                    return 0;
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
                    return Owner.Buttons;
                return null;
            }
        }

        private void CheckDuplicateButton(ButtonType type, TaskDialogItem itemToExclude)
        {
            if( type != ButtonType.Custom && Owner != null )
            {
                foreach( TaskDialogButton button in Owner.Buttons )
                {
                    if( button != this && button != itemToExclude && button.ButtonType == type )
                        throw new InvalidOperationException(Properties.Resources.DuplicateButtonTypeError);
                }
            }
        }
    }
}
