// Copyright © Sven Groot (Ookii.org) 2009
// BSD license; see license.txt for details.
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace Ookii.Dialogs.Wpf
{
    /// <summary>
    /// Represents a list of <see cref="TaskDialogItem"/> objects.
    /// </summary>
    /// <typeparam name="T">The type of the task dialog item.</typeparam>
    /// <threadsafety instance="false" static="true" />
    public class TaskDialogItemCollection<T> : Collection<T> where T : TaskDialogItem
    {
        private TaskDialog _owner;

        internal TaskDialogItemCollection(TaskDialog owner)
        {
            _owner = owner;
        }

        /// <summary>
        /// Overrides the <see cref="Collection{T}.ClearItems"/> method.
        /// </summary>
        protected override void ClearItems()
        {
            foreach( T item in this )
            {
                item.Owner = null;
            }
            base.ClearItems();
            _owner.UpdateDialog();
        }

        /// <summary>
        /// Overrides the <see cref="Collection{T}.InsertItem"/> method.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert. May not be <see langword="null" />.</param>
        /// <exception cref="ArgumentNullException"><paramref name="item"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">The <see cref="TaskDialogItem"/> specified in <paramref name="item" /> is already associated with a different task dialog.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="TaskDialogItem"/> specified in <paramref name="item" /> has a duplicate id or button type.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>
        ///   <paramref name="index"/> is less than zero.
        /// </para>
        /// <para>
        ///   -or-
        /// </para>
        /// <para>
        ///   <paramref name="index" /> is equal to or greater than <see cref="Collection{T}.Count"/>.
        /// </para>
        /// </exception>
        protected override void InsertItem(int index, T item)
        {
            if( item == null )
                throw new ArgumentNullException("item");

            if( item.Owner != null )
                throw new ArgumentException(Properties.Resources.TaskDialogItemHasOwnerError);

            item.Owner = _owner;
            try
            {
                item.CheckDuplicate(null);
            }
            catch( InvalidOperationException )
            {
                item.Owner = null;
                throw;
            }
            base.InsertItem(index, item);
            _owner.UpdateDialog();
        }

        /// <summary>
        /// Overrides the <see cref="Collection{T}.RemoveItem"/> method.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>
        ///   <paramref name="index"/> is less than zero.
        /// </para>
        /// <para>
        ///   -or-
        /// </para>
        /// <para>
        ///   <paramref name="index" /> is equal to or greater than <see cref="Collection{T}.Count"/>.
        /// </para>
        /// </exception>
        protected override void RemoveItem(int index)
        {
            base[index].Owner = null;
            base.RemoveItem(index);
            _owner.UpdateDialog();
        }

        /// <summary>
        /// Overrides the <see cref="Collection{T}.SetItem"/> method.
        /// </summary>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <param name="item">The new value for the element at the specified index. May not be <see langword="null" />.</param>
        /// <exception cref="ArgumentNullException"><paramref name="item"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">The <see cref="TaskDialogItem"/> specified in <paramref name="item" /> is already associated with a different task dialog.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="TaskDialogItem"/> specified in <paramref name="item" /> has a duplicate id or button type.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>
        ///   <paramref name="index"/> is less than zero.
        /// </para>
        /// <para>
        ///   -or-
        /// </para>
        /// <para>
        ///   <paramref name="index" /> is equal to or greater than <see cref="Collection{T}.Count"/>.
        /// </para>
        /// </exception>
        protected override void SetItem(int index, T item)
        {
            if( item == null )
                throw new ArgumentNullException("item");

            if( base[index] != item )
            {
                if( item.Owner != null )
                    throw new ArgumentException(Properties.Resources.TaskDialogItemHasOwnerError);
                item.Owner = _owner;
                try
                {
                    item.CheckDuplicate(base[index]);
                }
                catch( InvalidOperationException )
                {
                    item.Owner = null;
                    throw;
                }
                base[index].Owner = null;
                base.SetItem(index, item);
                _owner.UpdateDialog();
            }
        }
    }
}
