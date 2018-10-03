// Copyright © Sven Groot (Ookii.org) 2009
// BSD license; see license.txt for details.
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

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
    public partial class ProgressDialog : Component
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
        private Interop.IProgressDialog _dialog;
        private string _cancellationText;
        private bool _useCompactPathsForText;
        private bool _useCompactPathsForDescription;
        private SafeModuleHandle _currentAnimationModuleHandle;
        private bool _cancellationPending;

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
        ///   This property must be set before <see cref="ShowDialog()"/> or <see cref="Show()"/> is called. Changing property has
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
                if( _dialog != null )
                    _dialog.SetLine(1, Text, UseCompactPathsForText, IntPtr.Zero);
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
                if( _dialog != null )
                    _dialog.SetLine(1, Text, UseCompactPathsForText, IntPtr.Zero);
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
                if( _dialog != null )
                    _dialog.SetLine(2, Description, UseCompactPathsForDescription, IntPtr.Zero);
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
                if( _dialog != null )
                    _dialog.SetLine(2, Description, UseCompactPathsForDescription, IntPtr.Zero);
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
        ///   This property must be set before <see cref="ShowDialog()"/> or <see cref="Show()"/> is called. Changing property has
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
        ///   This property must be set before <see cref="ShowDialog()"/> or <see cref="Show()"/> is called. Changing property has
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
        ///   to modeless dialogs shown by using the <see cref="Show()"/> method.
        /// </note>
        /// <para>
        ///   This property must be set before <see cref="Show()"/> is called. Changing property has
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
        ///   This property must be set before <see cref="ShowDialog()"/> or <see cref="Show()"/> is called. Changing property has
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
        ///   This property must be set before <see cref="ShowDialog()"/> or <see cref="Show()"/> is called. Changing property has
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
        public void Show()
        {
            Show(null);
        }

        /// <summary>
        /// Displays the progress dialog as a modeless dialog.
        /// </summary>
        /// <param name="argument">A parameter for use by the background operation to be executed in the <see cref="DoWork"/> event handler.</param>
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
        public void Show(object argument)
        {
            RunProgressDialog(IntPtr.Zero, argument);
        }

        /// <summary>
        /// Displays the progress dialog as a modal dialog.
        /// </summary>
        /// <remarks>
        /// <para>
        ///   The ShowDialog function for most .Net dialogs will not return until the dialog is closed. However,
        ///   the <see cref="ShowDialog()"/> function for the <see cref="ProgressDialog"/> class will return immediately.
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
        ///   When possible, it is recommended that you use a modeless dialog using the <see cref="Show()"/> function.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">The animation specified in the <see cref="Animation"/> property
        /// could not be loaded.</exception>
        public void ShowDialog()
        {
            ShowDialog(null, null);
        }

        /// <summary>
        /// Displays the progress dialog as a modal dialog.
        /// </summary>
        /// <param name="owner">The window that owns the dialog.</param>
        /// <remarks>
        /// <para>
        ///   The ShowDialog function for most .Net dialogs will not return until the dialog is closed. However,
        ///   the <see cref="ShowDialog()"/> function for the <see cref="ProgressDialog"/> class will return immediately.
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
        ///   When possible, it is recommended that you use a modeless dialog using the <see cref="Show()"/> function.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">The animation specified in the <see cref="Animation"/> property
        /// could not be loaded, or the operation is already running.</exception>
        public void ShowDialog(Window owner)
        {
            ShowDialog(owner, null);
        }

        /// <summary>
        /// Displays the progress dialog as a modal dialog.
        /// </summary>
        /// <param name="owner">The window that owns the dialog.</param>
        /// <param name="argument">A parameter for use by the background operation to be executed in the <see cref="DoWork"/> event handler.</param>
        /// <remarks>
        /// <para>
        ///   The ShowDialog function for most .Net dialogs will not return until the dialog is closed. However,
        ///   the <see cref="ShowDialog()"/> function for the <see cref="ProgressDialog"/> class will return immediately.
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
        ///   When possible, it is recommended that you use a modeless dialog using the <see cref="Show()"/> function.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">The animation specified in the <see cref="Animation"/> property
        /// could not be loaded, or the operation is already running.</exception>
        public void ShowDialog(Window owner, object argument)
        {
            RunProgressDialog(owner == null ? NativeMethods.GetActiveWindow() : new WindowInteropHelper(owner).Handle, argument);
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
            _backgroundWorker.ReportProgress(percentProgress, new ProgressChangedData() { Text = text, Description = description, UserState = userState });
        }

        /// <summary>
        /// Raises the <see cref="DoWork"/> event.
        /// </summary>
        /// <param name="e">The <see cref="DoWorkEventArgs"/> containing data for the event.</param>
        protected virtual void OnDoWork(DoWorkEventArgs e)
        {
            DoWorkEventHandler handler = DoWork;
            if( handler != null )
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

        private void RunProgressDialog(IntPtr owner, object argument)
        {
            if( _backgroundWorker.IsBusy )
                throw new InvalidOperationException(Properties.Resources.ProgressDialogRunning);

            if( Animation != null )
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
            _dialog.SetLine(1, Text, UseCompactPathsForText, IntPtr.Zero);
            _dialog.SetLine(2, Description, UseCompactPathsForDescription, IntPtr.Zero);

            Interop.ProgressDialogFlags flags = Ookii.Dialogs.Wpf.Interop.ProgressDialogFlags.Normal;
            if( owner != IntPtr.Zero )
                flags |= Ookii.Dialogs.Wpf.Interop.ProgressDialogFlags.Modal;
            switch( ProgressBarStyle )
            {
            case ProgressBarStyle.None:
                flags |= Ookii.Dialogs.Wpf.Interop.ProgressDialogFlags.NoProgressBar;
                break;
            case ProgressBarStyle.MarqueeProgressBar:
                if( NativeMethods.IsWindowsVistaOrLater )
                    flags |= Ookii.Dialogs.Wpf.Interop.ProgressDialogFlags.MarqueeProgress;
                else
                    flags |= Ookii.Dialogs.Wpf.Interop.ProgressDialogFlags.NoProgressBar; // Older than Vista doesn't support marquee.
                break;
            }
            if( ShowTimeRemaining )
                flags |= Ookii.Dialogs.Wpf.Interop.ProgressDialogFlags.AutoTime;
            if( !ShowCancelButton )
                flags |= Ookii.Dialogs.Wpf.Interop.ProgressDialogFlags.NoCancel;
            if( !MinimizeBox )
                flags |= Ookii.Dialogs.Wpf.Interop.ProgressDialogFlags.NoMinimize;

            _dialog.StartProgressDialog(owner, null, flags, IntPtr.Zero);
            _backgroundWorker.RunWorkerAsync(argument);
        }

        private void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            OnDoWork(e);
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

            OnRunWorkerCompleted(new RunWorkerCompletedEventArgs((!e.Cancelled && e.Error == null) ? e.Result : null, e.Error, e.Cancelled));
        }

        private void _backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _cancellationPending = _dialog.HasUserCancelled();
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
    }
}
