// Copyright © Sven Groot (Ookii.org) 2009
// BSD license; see license.txt for details.
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Collections;

namespace Ookii.Dialogs.Wpf
{
    /// <summary>
    /// Represents a button or radio button on a task dialog.
    /// </summary>
    /// <threadsafety instance="false" static="true" />
    [ToolboxItem(false), DesignTimeVisible(false), DefaultProperty("Text"), DefaultEvent("Click")]
    public abstract partial class TaskDialogItem : Component
    {
        private TaskDialog _owner;
        private int _id;
        private bool _enabled = true;
        private string _text;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskDialogItem"/> class.
        /// </summary>
        protected TaskDialogItem()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskDialogItem"/> class with the specified container.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> to add the <see cref="TaskDialogItem"/> to.</param>
        protected TaskDialogItem(IContainer container)
        {
            if( container != null )
                container.Add(this);

            InitializeComponent();
        }

        internal TaskDialogItem(int id)
        {
            InitializeComponent();

            // The item cannot have an owner at this point, so it's not needed to check for duplicates,
            // which is why we can safely use the field and not the property, avoiding the virtual method call.
            _id = id;
        }

        /// <summary>
        /// Gets the <see cref="TaskDialog"/> that owns this <see cref="TaskDialogItem"/>.
        /// </summary>
        /// <value>
        /// The <see cref="TaskDialog"/> that owns this <see cref="TaskDialogItem"/>.
        /// </value>
        /// <remarks>
        /// This property is set automatically when the <see cref="TaskDialogItem"/> is added
        /// to the <see cref="TaskDialog.Buttons"/> or <see cref="TaskDialog.RadioButtons"/>
        /// collection of a <see cref="TaskDialog"/>.
        /// </remarks>
        [Browsable(false)]
        public TaskDialog Owner
        {
            get { return _owner; }
            internal set 
            { 
                _owner = value;
                AutoAssignId();
            }
        }

        /// <summary>
        /// Gets or sets the text of the item.
        /// </summary>
        /// <value>
        /// The text of the item. The default value is an empty string ("").
        /// </value>
        /// <remarks>
        /// <para>
        ///   For buttons, this property is ignored if <see cref="TaskDialogButton.ButtonType"/> is any value other 
        ///   than <see cref="ButtonType.Custom"/>.
        /// </para>
        /// </remarks>
        [Localizable(true), Category("Appearance"), Description("The text of the item."), DefaultValue("")]
        public string Text
        {
            get { return _text ?? string.Empty; }
            set 
            {
                _text = value;
                UpdateOwner();
            }
        }
        
        /// <summary>
        /// Gets or sets a value that indicates whether the item is enabled.
        /// </summary>
        /// <value>
        /// <see langword="true" /> if this item is enabled; otherwise, <see langword="false" />.
        /// </value>
        /// <remarks>
        /// If a button or radio button is not enabled, it will be grayed out and cannot be
        /// selected or clicked.
        /// </remarks>
        [Category("Behavior"), Description("Indicates whether the item is enabled."), DefaultValue(true)]
        public bool Enabled
        {
            get { return _enabled; }
            set 
            { 
                _enabled = value;
                if( Owner != null )
                {
                    Owner.SetItemEnabled(this);
                }
            }
        }

        /// <summary>
        /// Gets or sets the ID of the item.
        /// </summary>
        /// <value>
        /// The unique identifier of the item.
        /// </value>
        /// <remarks>
        /// <para>
        ///   The identifier of an item must be unique for the type of item on the task dialog (i.e. no two
        ///   buttons can have the same id, no two radio buttons can have the same id, but a radio button
        ///   can have the same id as a button).
        /// </para>
        /// <para>
        ///   If this property is zero when the <see cref="TaskDialogItem"/> is added to the <see cref="TaskDialog.Buttons"/>
        ///   or <see cref="TaskDialog.RadioButtons"/> collection of a task dialog, it will automatically be set
        ///   to the next available id value.
        /// </para>
        /// </remarks>
        [Category("Data"), Description("The id of the item."), DefaultValue(0)]
        internal virtual int Id
        {
            get { return _id; }
            set 
            {
                CheckDuplicateId(null, value);
                _id = value;
                UpdateOwner();
            }
        }

        /// <summary>
        /// Simulates a click on the task dialog item.
        /// </summary>
        /// <remarks>
        /// This method is available only while the task dialog is being displayed. You would typically call
        /// it from one of the events fired by the <see cref="TaskDialog"/> class while the dialog is visible.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// <para>The task dialog is not being displayed</para>
        /// <para>-or-</para>
        /// <para>The item has no associated task dialog.</para>
        /// </exception>
        public void Click()
        {
            if( Owner == null )
                throw new InvalidOperationException(Properties.Resources.NoAssociatedTaskDialogError);

            Owner.ClickItem(this);
        }

        /// <summary>
        /// When implemented in a derived class, gets the item collection on a task dialog that this type of item is
        /// part of.
        /// </summary>
        /// <value>
        /// For <see cref="TaskDialogButton"/> items, the <see cref="TaskDialog.Buttons"/>
        /// collection of the <see cref="TaskDialog"/> instance this item is part of. For <see cref="TaskDialogRadioButton"/> items, the <see cref="TaskDialog.RadioButtons"/>
        /// collection of the <see cref="TaskDialog"/> instance this item is part of. If the <see cref="TaskDialogItem"/> is not
        /// currently associated with a <see cref="TaskDialog"/>, <see langword="null" />.
        /// </value>
        /// <remarks>
        /// The collection returned by this property is used to determine if there are any items with duplicate IDs.
        /// </remarks>
        protected abstract IEnumerable ItemCollection
        {
            get;
        }

        /// <summary>
        /// Causes a full update of the owner dialog.
        /// </summary>
        /// <remarks>
        /// <para>
        ///   When this method is called, the owner dialog will be updated to reflect the
        ///   current state of the object.
        /// </para>
        /// <para>
        ///   When the <see cref="TaskDialogItem"/> has no owner, or the owner is not being
        ///   displayed, this method has no effect.
        /// </para>
        /// </remarks>
        protected void UpdateOwner()
        {
            if( Owner != null )
                Owner.UpdateDialog();
        }

        internal virtual void CheckDuplicate(TaskDialogItem itemToExclude)
        {
            CheckDuplicateId(itemToExclude, _id);
        }

        internal virtual void AutoAssignId()
        {
            if( ItemCollection != null )
            {
                int highestId = 9;
                foreach( TaskDialogItem item in ItemCollection )
                {
                    if( item.Id > highestId )
                        highestId = item.Id;
                }
                Id = highestId + 1;
            }
        }

        private void CheckDuplicateId(TaskDialogItem itemToExclude, int id)
        {
            if( id != 0 )
            {
                IEnumerable items = ItemCollection;
                if( items != null )
                {
                    foreach( TaskDialogItem item in items )
                    {
                        if( item != this && item != itemToExclude && item.Id == id )
                            throw new InvalidOperationException(Properties.Resources.DuplicateItemIdError);
                    }
                }
            }
        }
    }
}
