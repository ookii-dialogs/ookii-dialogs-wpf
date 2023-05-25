﻿#region Copyright 2009-2021 Ookii Dialogs Contributors
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
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Threading;

namespace Ookii.Dialogs.Wpf
{
    /// <summary>
    /// Represents a dialog that can be used to report progress to the user.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   This class provides a wrapper for the native Windows IProgressDialog API.
    /// </para>
    /// <para>
    ///   The <see cref="ProgressDialog"/> class requires Windows 2000, Windows Me, or newer versions of Windows.
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="false" />
    [DefaultEvent("DoWork"), DefaultProperty("Text"), Description("Represents a dialog that can be used to report progress to the user.")]
    public partial class ProgressDialog : Component, IProgress<int>, IProgress<string>, IServiceProvider
    {
        private class ProgressChangedData
        {
            public string Text { get; set; }
            public string Description { get; set; }
            public object UserState { get; set; }
        }

        private string _windowTitle;
        private string _text;
        private string _description;
        private IProgressDialog _dialog;
        private string _cancellationText;
        private bool _useCompactPathsForText;
        private bool _useCompactPathsForDescription;
        private FreeLibrarySafeHandle _currentAnimationModuleHandle;
        private volatile bool _cancellationPending;
        private CancellationTokenSource _cancellationTokenSource;
        private int _percentProgress;
        private IntPtr _ownerHandle;        

        /// <summary>
        /// Event raised when the dialog is displayed.
        /// </summary>
        /// <remarks>
        /// Use this event to perform the operation that the dialog is showing the progress for.
        /// This event will be raised on a different thread than the UI thread.
        /// </remarks>
        public event DoWorkEventHandler DoWork;

        /// <summary>
        /// Event raised when the operation completes.
        /// </summary>
        public event RunWorkerCompletedEventHandler RunWorkerCompleted;

        /// <summary>
        /// Event raised when <see cref="ReportProgress(int,string,string,object)"/> is called.
        /// </summary>
        public event ProgressChangedEventHandler ProgressChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressDialog"/> class.
        /// </summary>
        public ProgressDialog()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressDialog"/> class, adding it to the specified container.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> to which the component should be added.</param>
        public ProgressDialog(IContainer container)
        {
            if( container != null )
                container.Add(this);

            InitializeComponent();

            ProgressBarStyle = ProgressBarStyle.ProgressBar;
            ShowCancelButton = true;
            MinimizeBox = true;
            // Set a default animation for XP.
            if( !NativeMethods.IsWindowsVistaOrLater )
                Animation = AnimationResource.GetShellAnimation(Ookii.Dialogs.Wpf.ShellAnimation.FlyingPapers);
        }

        /// <summary>
        /// Gets or sets the text in the progress dialog's title bar.
        /// </summary>
        /// <value>
        /// The text in the progress dialog's title bar. The default value is an empty string.
        /// </value>
        /// <remarks>
        /// <para>
        ///   This property must be set before <see cref="ShowDialog(CancellationToken)"/> or <see cref="Show(CancellationToken)"/> is called. Changing property has
        ///   no effect while the dialog is being displayed.
        /// </para>
        /// </remarks>
        [Localizable(true), Category("Appearance"), Description("The text in the progress dialog's title bar."), DefaultValue("")]
        public string WindowTitle
        {
            get { return _windowTitle ?? string.Empty; }
            set { _windowTitle = value; }
        }

        /// <summary>
        /// Gets or sets a short description of the operation being carried out.
        /// </summary>
        /// <value>
        /// A short description of the operation being carried. The default value is an empty string.
        /// </value>
        /// <remarks>
        /// <para>
        ///   This is the primary message to the user.
        /// </para>
        /// <para>
        ///   This property can be changed while the dialog is running, but may only be changed from the thread which
        ///   created the progress dialog. The recommended method to change this value while the dialog is running
        ///   is to use the <see cref="ReportProgress(int,string,string)"/> method.
        /// </para>
        /// </remarks>
        [Localizable(true), Category("Appearance"), Description("A short description of the operation being carried out.")]
        public string Text
        {
            get { return _text ?? string.Empty; }
            set 
            { 
                _text = value;
                unsafe
                {
                    if (_dialog != null)
                        _dialog.SetLine(1, Text, UseCompactPathsForText, default);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether path strings in the <see cref="Text"/> property should be compacted if
        /// they are too large to fit on one line.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to compact path strings if they are too large to fit on one line; otherwise,
        /// <see langword="false"/>. The default value is <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// <note>
        ///   This property requires Windows Vista or later. On older versions of Windows, it has no effect.
        /// </note>
        /// <para>
        ///   This property can be changed while the dialog is running, but may only be changed from the thread which
        ///   created the progress dialog.
        /// </para>
        /// </remarks>
        [Category("Behavior"), Description("Indicates whether path strings in the Text property should be compacted if they are too large to fit on one line."), DefaultValue(false)]
        public bool UseCompactPathsForText
        {
            get { return _useCompactPathsForText; }
            set 
            {
                _useCompactPathsForText = value;
                unsafe
                {
                    if ( _dialog != null )
                    _dialog.SetLine(1, Text, UseCompactPathsForText, default);
                }
            }
        }
	
        /// <summary>
        /// Gets or sets additional details about the operation being carried out.
        /// </summary>
        /// <value>
        /// Additional details about the operation being carried out. The default value is an empty string.
        /// </value>
        /// <remarks>
        /// This text is used to provide additional details beyond the <see cref="Text"/> property.
        /// </remarks>
        /// <remarks>
        /// <para>
        ///   This property can be changed while the dialog is running, but may only be changed from the thread which
        ///   created the progress dialog. The recommended method to change this value while the dialog is running
        ///   is to use the <see cref="ReportProgress(int,string,string)"/> method.
        /// </para>
        /// </remarks>
        [Localizable(true), Category("Appearance"), Description("Additional details about the operation being carried out."), DefaultValue("")]
        public string Description
        {
            get { return _description ?? string.Empty; }
            set 
            { 
                _description = value;
                unsafe
                {
                    if (_dialog != null)
                        _dialog.SetLine(2, Description, UseCompactPathsForDescription, default);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether path strings in the <see cref="Description"/> property should be compacted if
        /// they are too large to fit on one line.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to compact path strings if they are too large to fit on one line; otherwise,
        /// <see langword="false"/>. The default value is <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// <note>
        ///   This property requires Windows Vista or later. On older versions of Windows, it has no effect.
        /// </note>
        /// <para>
        ///   This property can be changed while the dialog is running, but may only be changed from the thread which
        ///   created the progress dialog.
        /// </para>
        /// </remarks>
        [Category("Behavior"), Description("Indicates whether path strings in the Description property should be compacted if they are too large to fit on one line."), DefaultValue(false)]
        public bool UseCompactPathsForDescription
        {
            get { return _useCompactPathsForDescription; }
            set
            {
                _useCompactPathsForDescription = value;
                unsafe
                {
                    if (_dialog != null)
                        _dialog.SetLine(2, Description, UseCompactPathsForDescription, default);
                }
            }
        }

        /// <summary>
        /// Gets or sets the text that will be shown after the Cancel button is pressed.
        /// </summary>
        /// <value>
        /// The text that will be shown after the Cancel button is pressed.
        /// </value>
        /// <remarks>
        /// <para>
        ///   This property must be set before <see cref="ShowDialog(CancellationToken)"/> or <see cref="Show(CancellationToken)"/> is called. Changing property has
        ///   no effect while the dialog is being displayed.
        /// </para>
        /// </remarks>
        [Localizable(true), Category("Appearance"), Description("The text that will be shown after the Cancel button is pressed."), DefaultValue("")]
        public string CancellationText
        {
            get { return _cancellationText ?? string.Empty; }
            set { _cancellationText = value; }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether an estimate of the remaining time will be shown.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if an estimate of remaining time will be shown; otherwise, <see langword="false"/>. The
        /// default value is <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   This property must be set before <see cref="ShowDialog(CancellationToken)"/> or <see cref="Show(CancellationToken)"/> is called. Changing property has
        ///   no effect while the dialog is being displayed.
        /// </para>
        /// </remarks>
        [Category("Appearance"), Description("Indicates whether an estimate of the remaining time will be shown."), DefaultValue(false)]
        public bool ShowTimeRemaining { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the dialog has a cancel button.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the dialog has a cancel button; otherwise, <see langword="false"/>. The default
        /// value is <see langword="true"/>.
        /// </value>
        /// <remarks>
        /// <note>
        ///   This property requires Windows Vista or later; on older versions of Windows, the cancel button will always
        ///   be displayed.
        /// </note>
        /// <para>
        ///   The event handler for the <see cref="DoWork"/> event must periodically check the value of the
        ///   <see cref="CancellationPending"/> property to see if the operation has been cancelled if this
        ///   property is <see langword="true"/>.
        /// </para>
        /// <para>
        ///   Setting this property to <see langword="false"/> is not recommended unless absolutely necessary.
        /// </para>
        /// </remarks>
        [Category("Appearance"), Description("Indicates whether the dialog has a cancel button. Do not set to false unless absolutely necessary."), DefaultValue(true)]
        public bool ShowCancelButton { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the progress dialog has a minimize button.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the dialog has a minimize button; otherwise, <see langword="false"/>. The default
        /// value is <see langword="true"/>.
        /// </value>
        /// <remarks>
        /// <note>
        ///   This property has no effect on modal dialogs (which do not have a minimize button). It only applies
        ///   to modeless dialogs shown by using the <see cref="Show(CancellationToken)"/> method.
        /// </note>
        /// <para>
        ///   This property must be set before <see cref="Show(CancellationToken)"/> is called. Changing property has
        ///   no effect while the dialog is being displayed.
        /// </para>
        /// </remarks>
        [Category("Window Style"), Description("Indicates whether the progress dialog has a minimize button."), DefaultValue(true)]
        public bool MinimizeBox { get; set; }

        /// <summary>
        /// Gets a value indicating whether the user has requested cancellation of the operation.
        /// </summary>
        /// <value>
        /// <see langword="true" /> if the user has cancelled the progress dialog; otherwise, <see langword="false" />. The default is <see langword="false" />.
        /// </value>
        /// <remarks>
        /// The event handler for the <see cref="DoWork"/> event must periodically check this property and abort the operation
        /// if it returns <see langword="true"/>.
        /// </remarks>
        [Browsable(false)]
        public bool CancellationPending
        {
            get
            {
                _backgroundWorker.ReportProgress(-1); // Call with an out-of-range percentage will update the value of
                                                      // _cancellationPending but do nothing else.

                return _cancellationPending;
            }
        }

        /// <summary>
        /// Gets or sets the animation to show on the progress dialog.
        /// </summary>
        /// <value>
        /// An instance of <see cref="AnimationResource"/> which specifies the animation to show, or <see langword="null"/>
        /// to show no animation. The default value is <see langword="null"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   This property has no effect on Windows Vista or later. On Windows XP, this property will default to
        ///   a flying papers animation.
        /// </para>
        /// <para>
        ///   This property must be set before <see cref="ShowDialog(CancellationToken)"/> or <see cref="Show(CancellationToken)"/> is called. Changing property has
        ///   no effect while the dialog is being displayed.
        /// </para>
        /// </remarks>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AnimationResource Animation { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether a regular or marquee style progress bar should be used.
        /// </summary>
        /// <value>
        /// One of the values of <see cref="Ookii.Dialogs.Wpf.ProgressBarStyle"/>. 
        /// The default value is <see cref="Ookii.Dialogs.Wpf.ProgressBarStyle.ProgressBar"/>.
        /// </value>
        /// <remarks>
        /// <note>
        ///   Operating systems older than Windows Vista do not support marquee progress bars on the progress dialog. On those operating systems, the
        ///   progress bar will be hidden completely if this property is <see cref="Ookii.Dialogs.Wpf.ProgressBarStyle.MarqueeProgressBar"/>.
        /// </note>
        /// <para>
        ///   When this property is set to <see cref="Ookii.Dialogs.Wpf.ProgressBarStyle.ProgressBar" />, use the <see cref="ReportProgress(int)"/> method to set
        ///   the value of the progress bar. When this property is set to <see cref="Ookii.Dialogs.Wpf.ProgressBarStyle.MarqueeProgressBar"/>
        ///   you can still use the <see cref="ReportProgress(int,string,string)"/> method to update the text of the dialog,
        ///   but the percentage will be ignored.
        /// </para>
        /// <para>
        ///   This property must be set before <see cref="ShowDialog(CancellationToken)"/> or <see cref="Show(CancellationToken)"/> is called. Changing property has
        ///   no effect while the dialog is being displayed.
        /// </para>
        /// </remarks>
        [Category("Appearance"), Description("Indicates the style of the progress bar."), DefaultValue(ProgressBarStyle.ProgressBar)]
        public ProgressBarStyle ProgressBarStyle { get; set; }


        /// <summary>
        /// Gets a value that indicates whether the <see cref="ProgressDialog"/> is running an asynchronous operation.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the <see cref="ProgressDialog"/> is running an asynchronous operation; 
        /// otherwise, <see langword="false"/>.
        /// </value>
        [Browsable(false)]
        public bool IsBusy
        {
            get { return _backgroundWorker.IsBusy; }
        }

        /// <summary>
        /// Displays the progress dialog as a modeless dialog.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// <para>
        ///   This function will not block the parent window and will return immediately.
        /// </para>
        /// <para>
        ///   Although this function returns immediately, you cannot use the UI thread to do any processing. The dialog
        ///   will not function correctly unless the UI thread continues to handle window messages, so that thread may
        ///   not be blocked by some other activity. All processing related to the progress dialog must be done in
        ///   the <see cref="DoWork"/> event handler.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">The animation specified in the <see cref="Animation"/> property
        /// could not be loaded.</exception>
        public void Show(CancellationToken cancellationToken = default)
        {
            Show(null, cancellationToken);
        }

        /// <summary>
        /// Displays the progress dialog as a modeless dialog.
        /// </summary>
        /// <param name="argument">A parameter for use by the background operation to be executed in the <see cref="DoWork"/> event handler.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// <para>
        ///   This function will not block the parent window and return immediately.
        /// </para>
        /// <para>
        ///   Although this function returns immediately, you cannot use the UI thread to do any processing. The dialog
        ///   will not function correctly unless the UI thread continues to handle window messages, so that thread may
        ///   not be blocked by some other activity. All processing related to the progress dialog must be done in
        ///   the <see cref="DoWork"/> event handler.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">The animation specified in the <see cref="Animation"/> property
        /// could not be loaded.</exception>
        public void Show(object argument, CancellationToken cancellationToken = default)
        {
            RunProgressDialog(IntPtr.Zero, argument, cancellationToken);
        }

        /// <summary>
        /// Displays the progress dialog as a modal dialog.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// <para>
        ///   The ShowDialog function for most .Net dialogs will not return until the dialog is closed. However,
        ///   the <see cref="ShowDialog(CancellationToken)"/> function for the <see cref="ProgressDialog"/> class will return immediately.
        ///   The parent window will be disabled as with all modal dialogs.
        /// </para>
        /// <para>
        ///   Although this function returns immediately, you cannot use the UI thread to do any processing. The dialog
        ///   will not function correctly unless the UI thread continues to handle window messages, so that thread may
        ///   not be blocked by some other activity. All processing related to the progress dialog must be done in
        ///   the <see cref="DoWork"/> event handler.
        /// </para>
        /// <para>
        ///   The progress dialog's window will appear in the taskbar. This behaviour is also contrary to most .Net dialogs,
        ///   but is part of the underlying native progress dialog API so cannot be avoided.
        /// </para>
        /// <para>
        ///   When possible, it is recommended that you use a modeless dialog using the <see cref="Show(CancellationToken)"/> function.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">The animation specified in the <see cref="Animation"/> property
        /// could not be loaded.</exception>
        public void ShowDialog(CancellationToken cancellationToken = default)
        {
            ShowDialog(null, null, cancellationToken);
        }

        /// <summary>
        /// Displays the progress dialog as a modal dialog.
        /// </summary>
        /// <param name="owner">The window that owns the dialog.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// <para>
        ///   The ShowDialog function for most .Net dialogs will not return until the dialog is closed. However,
        ///   the <see cref="ShowDialog(CancellationToken)"/> function for the <see cref="ProgressDialog"/> class will return immediately.
        ///   The parent window will be disabled as with all modal dialogs.
        /// </para>
        /// <para>
        ///   Although this function returns immediately, you cannot use the UI thread to do any processing. The dialog
        ///   will not function correctly unless the UI thread continues to handle window messages, so that thread may
        ///   not be blocked by some other activity. All processing related to the progress dialog must be done in
        ///   the <see cref="DoWork"/> event handler.
        /// </para>
        /// <para>
        ///   The progress dialog's window will appear in the taskbar. This behaviour is also contrary to most .Net dialogs,
        ///   but is part of the underlying native progress dialog API so cannot be avoided.
        /// </para>
        /// <para>
        ///   When possible, it is recommended that you use a modeless dialog using the <see cref="Show(CancellationToken)"/> function.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">The animation specified in the <see cref="Animation"/> property
        /// could not be loaded, or the operation is already running.</exception>
        public void ShowDialog(Window owner, CancellationToken cancellationToken = default)
        {
            ShowDialog(owner, null, cancellationToken);
        }

        /// <summary>
        /// Displays the progress dialog as a modal dialog.
        /// </summary>
        /// <param name="owner">The <see cref="IntPtr"/> Win32 handle that is the owner of this dialog.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// <para>
        ///   The ShowDialog function for most .Net dialogs will not return until the dialog is closed. However,
        ///   the <see cref="ShowDialog(CancellationToken)"/> function for the <see cref="ProgressDialog"/> class will return immediately.
        ///   The parent window will be disabled as with all modal dialogs.
        /// </para>
        /// <para>
        ///   Although this function returns immediately, you cannot use the UI thread to do any processing. The dialog
        ///   will not function correctly unless the UI thread continues to handle window messages, so that thread may
        ///   not be blocked by some other activity. All processing related to the progress dialog must be done in
        ///   the <see cref="DoWork"/> event handler.
        /// </para>
        /// <para>
        ///   The progress dialog's window will appear in the taskbar. This behaviour is also contrary to most .Net dialogs,
        ///   but is part of the underlying native progress dialog API so cannot be avoided.
        /// </para>
        /// <para>
        ///   When possible, it is recommended that you use a modeless dialog using the <see cref="Show(CancellationToken)"/> function.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">The animation specified in the <see cref="Animation"/> property
        /// could not be loaded, or the operation is already running.</exception>
        public void ShowDialog(IntPtr owner, CancellationToken cancellationToken = default)
        {
            ShowDialog(owner, null, cancellationToken);
        }

        /// <summary>
        /// Displays the progress dialog as a modal dialog.
        /// </summary>
        /// <param name="owner">The window that owns the dialog.</param>
        /// <param name="argument">A parameter for use by the background operation to be executed in the <see cref="DoWork"/> event handler.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// <para>
        ///   The ShowDialog function for most .Net dialogs will not return until the dialog is closed. However,
        ///   the <see cref="ShowDialog(CancellationToken)"/> function for the <see cref="ProgressDialog"/> class will return immediately.
        ///   The parent window will be disabled as with all modal dialogs.
        /// </para>
        /// <para>
        ///   Although this function returns immediately, you cannot use the UI thread to do any processing. The dialog
        ///   will not function correctly unless the UI thread continues to handle window messages, so that thread may
        ///   not be blocked by some other activity. All processing related to the progress dialog must be done in
        ///   the <see cref="DoWork"/> event handler.
        /// </para>
        /// <para>
        ///   The progress dialog's window will appear in the taskbar. This behaviour is also contrary to most .Net dialogs,
        ///   but is part of the underlying native progress dialog API so cannot be avoided.
        /// </para>
        /// <para>
        ///   When possible, it is recommended that you use a modeless dialog using the <see cref="Show(CancellationToken)"/> function.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">The animation specified in the <see cref="Animation"/> property
        /// could not be loaded, or the operation is already running.</exception>
        public void ShowDialog(Window owner, object argument, CancellationToken cancellationToken = default)
        {
            RunProgressDialog(owner is null ? NativeMethods.GetActiveWindow() : new WindowInteropHelper(owner).Handle, argument, cancellationToken);
        }

        /// <summary>
        /// Displays the progress dialog as a modal dialog.
        /// </summary>
        /// <param name="owner">The <see cref="IntPtr"/> Win32 handle that is the owner of this dialog.</param>
        /// <param name="argument">A parameter for use by the background operation to be executed in the <see cref="DoWork"/> event handler.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <remarks>
        /// <para>
        ///   The ShowDialog function for most .Net dialogs will not return until the dialog is closed. However,
        ///   the <see cref="ShowDialog(CancellationToken)"/> function for the <see cref="ProgressDialog"/> class will return immediately.
        ///   The parent window will be disabled as with all modal dialogs.
        /// </para>
        /// <para>
        ///   Although this function returns immediately, you cannot use the UI thread to do any processing. The dialog
        ///   will not function correctly unless the UI thread continues to handle window messages, so that thread may
        ///   not be blocked by some other activity. All processing related to the progress dialog must be done in
        ///   the <see cref="DoWork"/> event handler.
        /// </para>
        /// <para>
        ///   The progress dialog's window will appear in the taskbar. This behaviour is also contrary to most .Net dialogs,
        ///   but is part of the underlying native progress dialog API so cannot be avoided.
        /// </para>
        /// <para>
        ///   When possible, it is recommended that you use a modeless dialog using the <see cref="Show(CancellationToken)"/> function.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">The animation specified in the <see cref="Animation"/> property
        /// could not be loaded, or the operation is already running.</exception>
        public void ShowDialog(IntPtr owner, object argument, CancellationToken cancellationToken = default)
        {
            RunProgressDialog(owner == IntPtr.Zero ? NativeMethods.GetActiveWindow() : owner, argument, cancellationToken);
        }

        /// <summary>
        /// Updates the dialog's progress bar.
        /// </summary>
        /// <param name="value">The percentage, from 0 to 100, of the operation that is complete.</param>
        void IProgress<int>.Report(int value)
        {
            ReportProgress(value, null, null, null);
        }

        /// <summary>
        /// Updates the dialog's progress bar.
        /// </summary>
        /// <param name="value">The new value of the progress dialog's primary text message, or <see langword="null"/> to leave the value unchanged.</param>
        void IProgress<string>.Report(string value)
        {
            ReportProgress(_percentProgress, value, null, null);
        }

        /// <summary>
        /// Updates the dialog's progress bar.
        /// </summary>
        /// <param name="percentProgress">The percentage, from 0 to 100, of the operation that is complete.</param>
        /// <remarks>
        /// <para>
        ///   Call this method from the <see cref="DoWork"/> event handler if you want to report progress.
        /// </para>
        /// <para>
        ///   This method has no effect is <see cref="ProgressBarStyle"/> is <see cref="Ookii.Dialogs.Wpf.ProgressBarStyle.MarqueeProgressBar"/>
        ///   or <see cref="Ookii.Dialogs.Wpf.ProgressBarStyle.None"/>.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="percentProgress"/> is out of range.</exception>
        /// <exception cref="InvalidOperationException">The progress dialog is not currently being displayed.</exception>
        public void ReportProgress(int percentProgress)
        {
            ReportProgress(percentProgress, null, null, null);
        }

        /// <summary>
        /// Updates the dialog's progress bar.
        /// </summary>
        /// <param name="percentProgress">The percentage, from 0 to 100, of the operation that is complete.</param>
        /// <param name="text">The new value of the progress dialog's primary text message, or <see langword="null"/> to leave the value unchanged.</param>
        /// <param name="description">The new value of the progress dialog's additional description message, or <see langword="null"/> to leave the value unchanged.</param>
        /// <remarks>Call this method from the <see cref="DoWork"/> event handler if you want to report progress.</remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="percentProgress"/> is out of range.</exception>
        /// <exception cref="InvalidOperationException">The progress dialog is not currently being displayed.</exception>
        public void ReportProgress(int percentProgress, string text, string description)
        {
            ReportProgress(percentProgress, text, description, null);
        }

        /// <summary>
        /// Updates the dialog's progress bar.
        /// </summary>
        /// <param name="percentProgress">The percentage, from 0 to 100, of the operation that is complete.</param>
        /// <param name="text">The new value of the progress dialog's primary text message, or <see langword="null"/> to leave the value unchanged.</param>
        /// <param name="description">The new value of the progress dialog's additional description message, or <see langword="null"/> to leave the value unchanged.</param>
        /// <param name="userState">A state object that will be passed to the <see cref="ProgressChanged"/> event handler.</param>
        /// <remarks>Call this method from the <see cref="DoWork"/> event handler if you want to report progress.</remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="percentProgress"/> is out of range.</exception>
        /// <exception cref="InvalidOperationException">The progress dialog is not currently being displayed.</exception>
        public void ReportProgress(int percentProgress, string text, string description, object userState)
        {
            if( percentProgress < 0 || percentProgress > 100 )
                throw new ArgumentOutOfRangeException("percentProgress");
            if( _dialog == null )
                throw new InvalidOperationException(Properties.Resources.ProgressDialogNotRunningError);

            // we need to cache the latest percentProgress so IProgress<string>.Report(text) can report the percent progress correctly.
            _percentProgress = percentProgress;

            _backgroundWorker.ReportProgress(percentProgress, new ProgressChangedData() { Text = text, Description = description, UserState = userState });
        }

        /// <summary>
        /// Raises the <see cref="DoWork"/> event.
        /// </summary>
        /// <param name="e">The <see cref="ProgressDialogDoWorkEventArgs"/> containing data for the event.</param>
        protected virtual void OnDoWork(ProgressDialogDoWorkEventArgs e)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            OnDoWork((DoWorkEventArgs)e);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        /// Raises the <see cref="DoWork"/> event.
        /// </summary>
        /// <param name="e">The <see cref="DoWorkEventArgs"/> containing data for the event.</param>
        [Obsolete("OnDoWork(DoWorkEventArgs) is obsolete and will be removed in a future release. Use OnDoWork(ProgressDialogDoWorkEventArgs) instead.")]
        protected virtual void OnDoWork(DoWorkEventArgs e)
        {
            DoWorkEventHandler handler = DoWork;
            if (handler is null)
            {
                return;
            }
            
            handler(this, e);
        }

        /// <summary>
        /// Raises the <see cref="RunWorkerCompleted"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> containing data for the event.</param>
        protected virtual void OnRunWorkerCompleted(RunWorkerCompletedEventArgs e)
        {
            RunWorkerCompletedEventHandler handler = RunWorkerCompleted;
            if( handler != null )
                handler(this, e);
        }

        /// <summary>
        /// Raises the <see cref="ProgressChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="ProgressChangedEventArgs"/> containing data for the event.</param>
        protected virtual void OnProgressChanged(ProgressChangedEventArgs e)
        {
            ProgressChangedEventHandler handler = ProgressChanged;
            if( handler != null )
                handler(this, e);
        }

        private unsafe void RunProgressDialog(IntPtr owner, object argument, CancellationToken cancellationToken)
        {
            if (_backgroundWorker.IsBusy || !(_cancellationTokenSource is null))
            {
                throw new InvalidOperationException(Properties.Resources.ProgressDialogRunning);
            }

            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            if ( Animation != null )
            {
                try
                {
                    _currentAnimationModuleHandle = Animation.LoadLibrary();
                }
                catch( Win32Exception ex )
                {
                    throw new InvalidOperationException(string.Format(System.Globalization.CultureInfo.CurrentCulture, Properties.Resources.AnimationLoadErrorFormat, ex.Message), ex);
                }
                catch( System.IO.FileNotFoundException ex )
                {
                    throw new InvalidOperationException(string.Format(System.Globalization.CultureInfo.CurrentCulture, Properties.Resources.AnimationLoadErrorFormat, ex.Message), ex);
                }
            }

            _cancellationPending = false;
            _dialog = new Interop.ProgressDialog();
            _dialog.SetTitle(WindowTitle);
            if( Animation != null )
                _dialog.SetAnimation(_currentAnimationModuleHandle, (ushort)Animation.ResourceId);

            if( CancellationText.Length > 0 )
                _dialog.SetCancelMsg(CancellationText, null);
            _dialog.SetLine(1, Text, UseCompactPathsForText, default);
            _dialog.SetLine(2, Description, UseCompactPathsForDescription, default);

            uint flags = NativeMethods.PROGDLG_NORMAL;
            if( owner != IntPtr.Zero )
                flags |= NativeMethods.PROGDLG_MODAL;
            switch( ProgressBarStyle )
            {
            case ProgressBarStyle.None:
                flags |= NativeMethods.PROGDLG_NOPROGRESSBAR;
                break;
            case ProgressBarStyle.MarqueeProgressBar:
                if( NativeMethods.IsWindowsVistaOrLater )
                    flags |= NativeMethods.PROGDLG_MARQUEEPROGRESS;
                else
                    flags |= NativeMethods.PROGDLG_NOPROGRESSBAR; // Older than Vista doesn't support marquee.
                break;
            }
            if( ShowTimeRemaining )
                flags |= NativeMethods.PROGDLG_AUTOTIME;
            if( !ShowCancelButton )
                flags |= NativeMethods.PROGDLG_NOCANCEL;
            if( !MinimizeBox )
                flags |= NativeMethods.PROGDLG_NOMINIMIZE;

            _ownerHandle = owner;

            _dialog.StartProgressDialog((HWND)owner, null, flags, default);
            _backgroundWorker.RunWorkerAsync(argument);
        }

        private void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var cancellationToken = _cancellationTokenSource?.Token ?? CancellationToken.None;

            var eventArgs = new ProgressDialogDoWorkEventArgs(e.Argument, cancellationToken)
            {
                Cancel = e.Cancel,
                Result = e.Result,
            };

            OnDoWork(eventArgs);

            e.Cancel = eventArgs.Cancel;
            e.Result = eventArgs.Result;
        }

        private void _backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _dialog.StopProgressDialog();
            Marshal.ReleaseComObject(_dialog);
            _dialog = null;
            if( _currentAnimationModuleHandle != null )
            {
                _currentAnimationModuleHandle.Dispose();
                _currentAnimationModuleHandle = null;
            }

            if (_ownerHandle != IntPtr.Zero)
                NativeMethods.EnableWindow((HWND)_ownerHandle, true);

            var cancellationTokenSource = _cancellationTokenSource;
            if (!(cancellationTokenSource is null))
            {
                cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            OnRunWorkerCompleted(new RunWorkerCompletedEventArgs((!e.Cancelled && e.Error == null) ? e.Result : null, e.Error, e.Cancelled));
        }

        private void _backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var cancellationTokenSource = _cancellationTokenSource;

            var cancellationRequestedByUser = _dialog.HasUserCancelled();
            var cancellationRequestedByCode = cancellationTokenSource?.IsCancellationRequested ?? false;

            if (cancellationRequestedByUser && !cancellationRequestedByCode)
            {
                cancellationTokenSource?.Cancel();
            }

            _cancellationPending = cancellationRequestedByUser || cancellationRequestedByCode;

            // ReportProgress doesn't allow values outside this range. However, CancellationPending will call
            // BackgroundWorker.ReportProgress directly with a value that is outside this range to update the value of the property.
            if( e.ProgressPercentage >= 0 && e.ProgressPercentage <= 100 )
            {
                _dialog.SetProgress((uint)e.ProgressPercentage, 100);
                ProgressChangedData data = e.UserState as ProgressChangedData;
                if( data != null )
                {
                    if( data.Text != null )
                        Text = data.Text;
                    if( data.Description != null )
                        Description = data.Description;
                    OnProgressChanged(new ProgressChangedEventArgs(e.ProgressPercentage, data.UserState));
                }
            }
        }

        /// <summary>
        /// Used to retrieve services from the "Sender" event args.
        /// </summary>
        /// <param name="serviceType">
        /// The service to retrieve. currently, only supports one of these:<br/>
        /// <see cref="IProgress{T}"/> with <see cref="int"/>.<br/>
        /// <see cref="IProgress{T}"/> with <see cref="string"/>.<br/>
        /// <see cref="System.Threading.CancellationTokenSource"/>.<br/>
        /// </param>
        /// <returns>An object that can be casted to the requested service.</returns>
        object IServiceProvider.GetService(Type serviceType)
        {
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));

            if (serviceType == typeof(IProgress<int>)) return this;
            if (serviceType == typeof(IProgress<string>)) return this;

            throw new ArgumentException($"Unsupported service {serviceType}", nameof(serviceType));            
        }
    }
}
