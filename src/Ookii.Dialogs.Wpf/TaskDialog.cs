#region Copyright 2009-2021 Ookii Dialogs Contributors

// Licensed under the BSD 3-Clause License (the "License"); you may not use this file except in compliance with the License. You
// may obtain a copy of the License at
//
// https://opensource.org/licenses/BSD-3-Clause
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS
// IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language
// governing permissions and limitations under the License.

#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

namespace Ookii.Dialogs.Wpf
{
    /// <summary>Displays a Task Dialog.</summary>
    /// <remarks>
    /// The task dialog contains an application-defined message text and title, icons, and any combination of predefined push
    /// buttons. Task Dialogs are supported only on Windows Vista and above. No fallback is provided; if you wish to use task
    /// dialogs and support operating systems older than Windows Vista, you must provide a fallback yourself. Check the <see
    /// cref="OSSupportsTaskDialogs"/> property to see if task dialogs are supported. It is safe to instantiate the <see
    /// cref="TaskDialog"/> class on an older OS, but calling <see cref="Show"/> or <see cref="ShowDialog()"/> will throw an exception.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    [DefaultProperty("MainInstruction"), DefaultEvent("ButtonClicked"), Description("Displays a task dialog."), Designer(typeof(TaskDialogDesigner))]
    public partial class TaskDialog : Component, IWin32Window
    {
        #region Events

        /// <summary>Event raised when the task dialog has been created.</summary>
        /// <remarks>
        /// This event is raised once after calling <see cref="ShowDialog(Window)"/>, after the dialog is created and before it
        /// is displayed.
        /// </remarks>
        [Category("Behavior"), Description("Event raised when the task dialog has been created.")]
        public event EventHandler Created;
        /// <summary>Event raised when the task dialog has been destroyed.</summary>
        /// <remarks>The task dialog window no longer exists when this event is raised.</remarks>
        [Category("Behavior"), Description("Event raised when the task dialog has been destroyed.")]
        public event EventHandler Destroyed;
        /// <summary>Event raised when the user clicks a button on the task dialog.</summary>
        /// <remarks>
        /// Set the <see cref="CancelEventArgs.Cancel"/> property to <see langword="true"/> to prevent the dialog from being closed.
        /// </remarks>
        [Category("Action"), Description("Event raised when the user clicks a button.")]
        public event EventHandler<TaskDialogItemClickedEventArgs> ButtonClicked;
        /// <summary>Event raised when the user clicks a radio button on the task dialog.</summary>
        /// <remarks>The <see cref="CancelEventArgs.Cancel"/> property is ignored for this event.</remarks>
        [Category("Action"), Description("Event raised when the user clicks a button.")]
        public event EventHandler<TaskDialogItemClickedEventArgs> RadioButtonClicked;
        /// <summary>Event raised when the user clicks a hyperlink.</summary>
        [Category("Action"), Description("Event raised when the user clicks a hyperlink.")]
        public event EventHandler<HyperlinkClickedEventArgs> HyperlinkClicked;
        /// <summary>Event raised when the user clicks the verification check box.</summary>
        [Category("Action"), Description("Event raised when the user clicks the verification check box.")]
        public event EventHandler VerificationClicked;
        /// <summary>Event raised periodically while the dialog is displayed.</summary>
        /// <remarks>
        /// <para>
        /// This event is raised only when the <see cref="RaiseTimerEvent"/> property is set to <see langword="true"/>. The
        /// event is raised approximately every 200 milliseconds.
        /// </para>
        /// <para>To reset the tick count, set the <see cref="TimerEventArgs.ResetTickCount"/> property to <see langword="true"/>.</para>
        /// </remarks>
        [Category("Behavior"), Description("Event raised periodically while the dialog is displayed.")]
        public event EventHandler<TimerEventArgs> Timer;
        /// <summary>Event raised when the user clicks the expand button on the task dialog.</summary>
        /// <remarks>
        /// The <see cref="ExpandButtonClickedEventArgs.Expanded"/> property indicates if the expanded information is visible or
        /// not after the click.
        /// </remarks>
        [Category("Action"), Description("Event raised when the user clicks the expand button on the task dialog.")]
        public event EventHandler<ExpandButtonClickedEventArgs> ExpandButtonClicked;
        /// <summary>Event raised when the user presses F1 while the dialog has focus.</summary>
        [Category("Action"), Description("Event raised when the user presses F1 while the dialog has focus.")]
        public event EventHandler HelpRequested;

        #endregion

        #region Fields

        private TaskDialogItemCollection<TaskDialogButton> _buttons;
        private TaskDialogItemCollection<TaskDialogRadioButton> _radioButtons;
        private TASKDIALOGCONFIG _config = new TASKDIALOGCONFIG();
        private TaskDialogIcon _mainIcon;
        private System.Drawing.Icon _customMainIcon;
        private System.Drawing.Icon _customFooterIcon;
        private TaskDialogIcon _footerIcon;
        private Dictionary<int, TaskDialogButton> _buttonsById;
        private Dictionary<int, TaskDialogRadioButton> _radioButtonsById;
        private IntPtr _handle;
        private int _progressBarMarqueeAnimationSpeed = 100;
        private int _progressBarMinimimum;
        private int _progressBarMaximum = 100;
        private int _progressBarValue;
        private ProgressBarState _progressBarState = ProgressBarState.Normal;
        private int _inEventHandler;
        private bool _updatePending;
        private System.Drawing.Icon _windowIcon;

        #endregion

        #region Constructors

        /// <summary>Initializes a new instance of the <see cref="TaskDialog"/> class.</summary>
        public TaskDialog()
        {
            InitializeComponent();

            _config.cbSize = (uint)Marshal.SizeOf(_config);
            _config.pfCallback = new PFTASKDIALOGCALLBACK(TaskDialogCallback);
        }

        /// <summary>Initializes a new instance of the <see cref="TaskDialog"/> class with the specified container.</summary>
        /// <param name="container">The <see cref="IContainer"/> to add the <see cref="TaskDialog"/> to.</param>
        public TaskDialog(IContainer container)
        {
            container?.Add(this);

            InitializeComponent();

            _config.cbSize = (uint)Marshal.SizeOf(_config);
            _config.pfCallback = new PFTASKDIALOGCALLBACK(TaskDialogCallback);
        }

        #endregion

        #region Public Properties

        /// <summary>Gets a value that indicates whether the current operating system supports task dialogs.</summary>
        /// <value>Returns <see langword="true"/> for Windows Vista or later; otherwise <see langword="false"/>.</value>
        public static bool OSSupportsTaskDialogs => NativeMethods.IsWindowsVistaOrLater;

        /// <summary>Gets a list of the buttons on the Task Dialog.</summary>
        /// <value>A list of the buttons on the Task Dialog.</value>
        /// <remarks>
        /// Custom buttons are displayed in the order they have in the collection. Standard buttons will always be displayed in
        /// the Windows-defined order, regardless of the order of the buttons in the collection.
        /// </remarks>
        [Localizable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Category("Appearance"), Description("A list of the buttons on the Task Dialog.")]
        public TaskDialogItemCollection<TaskDialogButton> Buttons => _buttons ??= new TaskDialogItemCollection<TaskDialogButton>(this);

        /// <summary>Gets a list of the radio buttons on the Task Dialog.</summary>
        /// <value>A list of the radio buttons on the Task Dialog.</value>
        [Localizable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Category("Appearance"), Description("A list of the radio buttons on the Task Dialog.")]
        public TaskDialogItemCollection<TaskDialogRadioButton> RadioButtons => _radioButtons ??= new TaskDialogItemCollection<TaskDialogRadioButton>(this);

        /// <summary>Gets or sets the window title of the task dialog.</summary>
        /// <value>The window title of the task dialog. The default is an empty string ("").</value>
        [Localizable(true), Category("Appearance"), Description("The window title of the task dialog."), DefaultValue("")]
        public string WindowTitle
        {
            get => _config.pszWindowTitle.ToString();
            set
            {
                unsafe
                {
                    fixed (char* pvalue = string.IsNullOrEmpty(value) ? null : value)
                        _config.pszWindowTitle = pvalue;
                }
                UpdateDialog();
            }
        }

        /// <summary>Gets or sets the dialog's main instruction.</summary>
        /// <value>The dialog's main instruction. The default is an empty string ("").</value>
        /// <remarks>
        /// The main instruction of a task dialog will be displayed in a larger font and a different color than the other text
        /// of the task dialog.
        /// </remarks>
        [Localizable(true), Category("Appearance"), Description("The dialog's main instruction."), DefaultValue(""), Editor(typeof(System.ComponentModel.Design.MultilineStringEditor), typeof(UITypeEditor))]
        public string MainInstruction
        {
            get => _config.pszMainInstruction.ToString();
            set
            {
                unsafe
                {
                    fixed (char* pMainInstruction = string.IsNullOrEmpty(value) ? null : value)
                        _config.pszMainInstruction = pMainInstruction;
                }
                SetElementText(TASKDIALOG_ELEMENTS.TDE_MAIN_INSTRUCTION, MainInstruction);
            }
        }

        /// <summary>Gets or sets the dialog's primary content.</summary>
        /// <value>The dialog's primary content. The default is an empty string ("").</value>
        [Localizable(true), Category("Appearance"), Description("The dialog's primary content."), DefaultValue(""), Editor(typeof(System.ComponentModel.Design.MultilineStringEditor), typeof(UITypeEditor))]
        public string Content
        {
            get => _config.pszContent.ToString();
            set
            {
                unsafe
                {
                    fixed (char* pContent = string.IsNullOrEmpty(value) ? null : value)
                        _config.pszContent = pContent;
                }
                SetElementText(TASKDIALOG_ELEMENTS.TDE_CONTENT, Content);
            }
        }

        /// <summary>Gets or sets the icon to be used in the title bar of the dialog.</summary>
        /// <value>An <see cref="System.Drawing.Icon"/> that represents the icon of the task dialog's window.</value>
        /// <remarks>
        /// This property is used only when the dialog is shown as a modeless dialog; if the dialog is modal, it will have no icon.
        /// </remarks>
        [Localizable(true), Category("Appearance"), Description("The icon to be used in the title bar of the dialog. Used only when the dialog is shown as a modeless dialog."), DefaultValue(null)]
        public System.Drawing.Icon WindowIcon
        {
            get
            {
                if (IsDialogRunning)
                {
                    IntPtr icon = NativeMethods.SendMessage((HWND)Handle, NativeMethods.WM_GETICON, (nuint)NativeMethods.ICON_SMALL, IntPtr.Zero);
                    return System.Drawing.Icon.FromHandle(icon);
                }
                return _windowIcon;
            }
            set => _windowIcon = value;
        }

        /// <summary>Gets or sets the icon to display in the task dialog.</summary>
        /// <value>
        /// A <see cref="TaskDialogIcon"/> that indicates the icon to display in the main content area of the task dialog. The
        /// default is <see cref="TaskDialogIcon.Custom"/>.
        /// </value>
        /// <remarks>
        /// When this property is set to <see cref="TaskDialogIcon.Custom"/>, use the <see cref="CustomMainIcon"/> property to
        /// specify the icon to use.
        /// </remarks>
        [Localizable(true), Category("Appearance"), Description("The icon to display in the task dialog."), DefaultValue(TaskDialogIcon.Custom)]
        public TaskDialogIcon MainIcon
        {
            get => _mainIcon;
            set
            {
                if (_mainIcon != value)
                {
                    _mainIcon = value;
                    UpdateDialog();
                }
            }
        }

        /// <summary>Gets or sets a custom icon to display in the dialog.</summary>
        /// <value>
        /// An <see cref="System.Drawing.Icon"/> that represents the icon to display in the main content area of the task
        /// dialog, or <see langword="null"/> if no custom icon is used. The default value is <see langword="null"/>.
        /// </value>
        /// <remarks>This property is ignored if the <see cref="MainIcon"/> property has a value other than <see cref="TaskDialogIcon.Custom"/>.</remarks>
        [Localizable(true), Category("Appearance"), Description("A custom icon to display in the dialog."), DefaultValue(null)]
        public System.Drawing.Icon CustomMainIcon
        {
            get => _customMainIcon;
            set
            {
                if (_customMainIcon != value)
                {
                    _customMainIcon = value;
                    UpdateDialog();
                }
            }
        }

        /// <summary>Gets or sets the icon to display in the footer area of the task dialog.</summary>
        /// <value>
        /// A <see cref="TaskDialogIcon"/> that indicates the icon to display in the footer area of the task dialog. The default
        /// is <see cref="TaskDialogIcon.Custom"/>.
        /// </value>
        /// <remarks>
        /// <para>
        /// When this property is set to <see cref="TaskDialogIcon.Custom"/>, use the <see cref="CustomFooterIcon"/> property to
        /// specify the icon to use.
        /// </para>
        /// <para>The footer icon is displayed only if the <see cref="Footer"/> property is not an empty string ("").</para>
        /// </remarks>
        [Localizable(true), Category("Appearance"), Description("The icon to display in the footer area of the task dialog."), DefaultValue(TaskDialogIcon.Custom)]
        public TaskDialogIcon FooterIcon
        {
            get => _footerIcon;
            set
            {
                if (_footerIcon != value)
                {
                    _footerIcon = value;
                    UpdateDialog();
                }
            }
        }

        /// <summary>Gets or sets a custom icon to display in the footer area of the task dialog.</summary>
        /// <value>
        /// An <see cref="System.Drawing.Icon"/> that represents the icon to display in the footer area of the task dialog, or
        /// <see langword="null"/> if no custom icon is used. The default value is <see langword="null"/>.
        /// </value>
        /// <remarks>
        /// <para>This property is ignored if the <see cref="FooterIcon"/> property has a value other than <see cref="TaskDialogIcon.Custom"/>.</para>
        /// <para>The footer icon is displayed only if the <see cref="Footer"/> property is not an empty string ("").</para>
        /// </remarks>
        [Localizable(true), Category("Appearance"), Description("A custom icon to display in the footer area of the task dialog."), DefaultValue(null)]
        public System.Drawing.Icon CustomFooterIcon
        {
            get => _customFooterIcon;
            set
            {
                if (_customFooterIcon != value)
                {
                    _customFooterIcon = value;
                    // TODO: This and customMainIcon don't need to use UpdateDialog, they can use TDM_UPDATE_ICON
                    UpdateDialog();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether custom buttons should be displayed as normal buttons or command links.
        /// </summary>
        /// <value>
        /// A <see cref="TaskDialogButtonStyle"/> that indicates the display style of custom buttons on the dialog. The default
        /// value is <see cref="TaskDialogButtonStyle.Standard"/>.
        /// </value>
        /// <remarks>
        /// <para>This property affects only custom buttons, not standard ones.</para>
        /// <para>
        /// If a custom button is being displayed on a task dialog with <see cref="ButtonStyle"/> set to <see
        /// cref="TaskDialogButtonStyle.CommandLinks"/> or <see
        /// cref="TaskDialogButtonStyle.CommandLinksNoIcon"/>, you delineate the command from the note by
        /// placing a line break in the string specified by <see cref="TaskDialogItem.Text"/> property.
        /// </para>
        /// </remarks>
        [Category("Behavior"), Description("Indicates whether custom buttons should be displayed as normal buttons or command links."), DefaultValue(TaskDialogButtonStyle.Standard)]
        public TaskDialogButtonStyle ButtonStyle
        {
            get => GetFlag(TASKDIALOG_FLAGS.TDF_USE_COMMAND_LINKS_NO_ICON) ? TaskDialogButtonStyle.CommandLinksNoIcon :
                    GetFlag(TASKDIALOG_FLAGS.TDF_USE_COMMAND_LINKS) ? TaskDialogButtonStyle.CommandLinks :
                    TaskDialogButtonStyle.Standard;
            set
            {
                SetFlag(TASKDIALOG_FLAGS.TDF_USE_COMMAND_LINKS, value == TaskDialogButtonStyle.CommandLinks);
                SetFlag(TASKDIALOG_FLAGS.TDF_USE_COMMAND_LINKS_NO_ICON, value == TaskDialogButtonStyle.CommandLinksNoIcon);
                UpdateDialog();
            }
        }

        /// <summary>Gets or sets the label for the verification checkbox.</summary>
        /// <value>
        /// The label for the verification checkbox, or an empty string ("") if no verification checkbox is shown. The default
        /// value is an empty string ("").
        /// </value>
        /// <remarks>If no text is set, the verification checkbox will not be shown.</remarks>
        [Localizable(true), Category("Appearance"), Description("The label for the verification checkbox."), DefaultValue("")]
        public string VerificationText
        {
            get => _config.pszVerificationText.ToString() ?? string.Empty;
            set
            {
                string realValue = string.IsNullOrEmpty(value) ? null : value;
                if (_config.pszVerificationText.ToString() != realValue)
                {
                    unsafe
                    {
                        fixed (char* prealValue = realValue)
                            _config.pszVerificationText = prealValue;
                    }
                    UpdateDialog();
                }
            }
        }

        /// <summary>Gets or sets a value that indicates whether the verification checkbox is checked ot not.</summary>
        /// <value><see langword="true"/> if the verficiation checkbox is checked; otherwise, <see langword="false"/>.</value>
        /// <remarks>
        /// <para>
        /// Set this property before displaying the dialog to determine the initial state of the check box. Use this property
        /// after displaying the dialog to determine whether the check box was checked when the user closed the dialog.
        /// </para>
        /// <note> This property is only used if <see cref="VerificationText"/> is not an empty string (""). </note>
        /// </remarks>
        [Category("Behavior"), Description("Indicates whether the verification checkbox is checked ot not."), DefaultValue(false)]
        public bool IsVerificationChecked
        {
            get => GetFlag(TASKDIALOG_FLAGS.TDF_VERIFICATION_FLAG_CHECKED);
            set
            {
                if (value != IsVerificationChecked)
                {
                    SetFlag(TASKDIALOG_FLAGS.TDF_VERIFICATION_FLAG_CHECKED, value);
                    if (IsDialogRunning)
                        ClickVerification(value, false);
                }
            }
        }

        /// <summary>Gets or sets additional information to be displayed on the dialog.</summary>
        /// <value>Additional information to be displayed on the dialog. The default value is an empty string ("").</value>
        /// <remarks>
        /// <para>
        /// When this property is not an empty string (""), a control is shown on the task dialog that allows the user to expand
        /// and collapse the text specified in this property.
        /// </para>
        /// <para>The text is collapsed by default unless <see cref="ExpandedByDefault"/> is set to <see langword="true"/>.</para>
        /// <para>
        /// The expanded text is shown in the main content area of the dialog, unless <see cref="ExpandFooterArea"/> is set to
        /// <see langword="true"/>, in which case it is shown in the footer area.
        /// </para>
        /// </remarks>
        [Localizable(true), Category("Appearance"), Description("Additional information to be displayed on the dialog."), DefaultValue(""), Editor(typeof(System.ComponentModel.Design.MultilineStringEditor), typeof(UITypeEditor))]
        public string ExpandedInformation
        {
            get => _config.pszExpandedInformation.ToString() ?? string.Empty;
            set
            {
                unsafe
                {
                    fixed (char* pExpandedInformation = string.IsNullOrEmpty(value) ? null : value)
                        _config.pszExpandedInformation = pExpandedInformation;
                }
                SetElementText(TASKDIALOG_ELEMENTS.TDE_EXPANDED_INFORMATION, ExpandedInformation);
            }
        }

        /// <summary>
        /// Gets or sets the text to use for the control for collapsing the expandable information specified in <see cref="ExpandedInformation"/>.
        /// </summary>
        /// <value>
        /// The text to use for the control for collapsing the expandable information, or an empty string ("") if the operating
        /// system's default text is to be used. The default is an empty string ("")
        /// </value>
        /// <remarks>
        /// <para>
        /// If this text is not specified and <see cref="CollapsedControlText"/> is specified, the value of <see
        /// cref="CollapsedControlText"/> will be used for this property as well. If neither is specified, the operating
        /// system's default text is used.
        /// </para>
        /// <note> The control for collapsing or expanding the expandable information is displayed only if <see
        /// cref="ExpandedInformation"/> is not an empty string ("") </note>
        /// </remarks>
        [Localizable(true), Category("Appearance"), Description("The text to use for the control for collapsing the expandable information."), DefaultValue("")]
        public string ExpandedControlText
        {
            get => _config.pszExpandedControlText.ToString() ?? string.Empty;
            set
            {
                string realValue = string.IsNullOrEmpty(value) ? null : value;
                if (_config.pszExpandedControlText.ToString() != realValue)
                {
                    unsafe
                    {
                        fixed (char* prealValue = realValue)
                            _config.pszExpandedControlText = prealValue;
                    }
                    UpdateDialog();
                }
            }
        }

        /// <summary>
        /// Gets or sets the text to use for the control for expading the expandable information specified in <see cref="ExpandedInformation"/>.
        /// </summary>
        /// <value>
        /// The text to use for the control for expanding the expandable information, or an empty string ("") if the operating
        /// system's default text is to be used. The default is an empty string ("")
        /// </value>
        /// <remarks>
        /// <para>
        /// If this text is not specified and <see cref="ExpandedControlText"/> is specified, the value of <see
        /// cref="ExpandedControlText"/> will be used for this property as well. If neither is specified, the operating system's
        /// default text is used.
        /// </para>
        /// <note> The control for collapsing or expanding the expandable information is displayed only if <see
        /// cref="ExpandedInformation"/> is not an empty string ("") </note>
        /// </remarks>
        [Localizable(true), Category("Appearance"), Description("The text to use for the control for expanding the expandable information."), DefaultValue("")]
        public string CollapsedControlText
        {
            get => _config.pszCollapsedControlText.ToString() ?? string.Empty;
            set
            {
                string realValue = string.IsNullOrEmpty(value) ? null : value;
                if (_config.pszCollapsedControlText.ToString() != realValue)
                {
                    unsafe
                    {
                        fixed (char* prealValue = realValue)
                            _config.pszCollapsedControlText = prealValue;
                    }
                    UpdateDialog();
                }
            }
        }

        /// <summary>Gets or sets the text to be used in the footer area of the task dialog.</summary>
        /// <value>
        /// The text to be used in the footer area of the task dialog, or an empty string ("") if the footer area is not
        /// displayed. The default value is an empty string ("").
        /// </value>
        [Localizable(true), Category("Appearance"), Description("The text to be used in the footer area of the task dialog."), DefaultValue(""), Editor(typeof(System.ComponentModel.Design.MultilineStringEditor), typeof(UITypeEditor))]
        public string Footer
        {
            get => _config.pszFooter.ToString() ?? string.Empty;
            set
            {
                unsafe
                {
                    fixed (char* pvalue = string.IsNullOrEmpty(value) ? null : value)
                        _config.pszFooter = pvalue;
                }
                SetElementText(TASKDIALOG_ELEMENTS.TDE_FOOTER, Footer);
            }
        }

        /// <summary>Specifies the width of the task dialog's client area in DLU's.</summary>
        /// <value>
        /// The width of the task dialog's client area in DLU's, or 0 to have the task dialog calculate the ideal width. The
        /// default value is 0.
        /// </value>
        [Localizable(true), Category("Appearance"), Description("the width of the task dialog's client area in DLU's. If 0, task dialog will calculate the ideal width."), DefaultValue(0)]
        public int Width
        {
            get => (int)_config.cxWidth;
            set
            {
                if (_config.cxWidth != (uint)value)
                {
                    _config.cxWidth = (uint)value;
                    UpdateDialog();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether hyperlinks are allowed for the <see cref="Content"/>, <see
        /// cref="ExpandedInformation"/> and <see cref="Footer"/> properties.
        /// </summary>
        /// <value>
        /// <see langword="true"/> when hyperlinks are allowed for the <see cref="Content"/>, <see cref="ExpandedInformation"/>
        /// and <see cref="Footer"/> properties; otherwise, <see langword="false"/>. The default value is <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// <para>
        /// When this property is <see langword="true"/>, the <see cref="Content"/>, <see cref="ExpandedInformation"/> and <see
        /// cref="Footer"/> properties can use hyperlinks in the following form: <c>&lt;A HREF="executablestring"&gt;Hyperlink Text&lt;/A&gt;</c>
        /// </para>
        /// <note> Enabling hyperlinks when using content from an unsafe source may cause security vulnerabilities. </note>
        /// <para>
        /// Task dialogs will not actually execute hyperlinks. To take action when the user presses a hyperlink, handle the <see
        /// cref="HyperlinkClicked"/> event.
        /// </para>
        /// </remarks>
        [Category("Behavior"), Description("Indicates whether hyperlinks are allowed for the Content, ExpandedInformation and Footer properties."), DefaultValue(false)]
        public bool EnableHyperlinks
        {
            get => GetFlag(TASKDIALOG_FLAGS.TDF_ENABLE_HYPERLINKS);
            set
            {
                if (EnableHyperlinks != value)
                {
                    SetFlag(TASKDIALOG_FLAGS.TDF_ENABLE_HYPERLINKS, value);
                    UpdateDialog();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates that the dialog should be able to be closed using Alt-F4, Escape and the title
        /// bar's close button even if no cancel button is specified.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the dialog can be closed using Alt-F4, Escape and the title bar's close button even if no
        /// cancel button is specified; otherwise, <see langword="false"/>. The default value is <see langword="false"/>.
        /// </value>
        [Category("Behavior"), Description("Indicates that the dialog should be able to be closed using Alt-F4, Escape and the title bar's close button even if no cancel button is specified."), DefaultValue(false)]
        public bool AllowDialogCancellation
        {
            get => GetFlag(TASKDIALOG_FLAGS.TDF_ALLOW_DIALOG_CANCELLATION);
            set
            {
                if (AllowDialogCancellation != value)
                {
                    SetFlag(TASKDIALOG_FLAGS.TDF_ALLOW_DIALOG_CANCELLATION, value);
                    UpdateDialog();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates that the string specified by the <see cref="ExpandedInformation"/> property
        /// should be displayed at the bottom of the dialog's footer area instead of immediately after the dialog's content.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the string specified by the <see cref="ExpandedInformation"/> property should be displayed
        /// at the bottom of the dialog's footer area instead of immediately after the dialog's content; otherwise, <see
        /// langword="false"/>. The default value is <see langword="false"/>.
        /// </value>
        [Category("Behavior"), Description("Indicates that the string specified by the ExpandedInformation property should be displayed at the bottom of the dialog's footer area instead of immediately after the dialog's content."), DefaultValue(false)]
        public bool ExpandFooterArea
        {
            get => GetFlag(TASKDIALOG_FLAGS.TDF_EXPAND_FOOTER_AREA);
            set
            {
                if (ExpandFooterArea != value)
                {
                    SetFlag(TASKDIALOG_FLAGS.TDF_EXPAND_FOOTER_AREA, value);
                    UpdateDialog();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates that the string specified by the <see cref="ExpandedInformation"/> property
        /// should be displayed by default.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the string specified by the <see cref="ExpandedInformation"/> property should be displayed
        /// by default; <see langword="false"/> if it is hidden by default. The default value is <see langword="false"/>.
        /// </value>
        [Category("Behavior"), Description("Indicates that the string specified by the ExpandedInformation property should be displayed by default."), DefaultValue(false)]
        public bool ExpandedByDefault
        {
            get => GetFlag(TASKDIALOG_FLAGS.TDF_EXPANDED_BY_DEFAULT);
            set
            {
                if (ExpandedByDefault != value)
                {
                    SetFlag(TASKDIALOG_FLAGS.TDF_EXPANDED_BY_DEFAULT, value);
                    UpdateDialog();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the <see cref="Timer"/> event is raised periodically while the dialog is visible.
        /// </summary>
        /// <value>
        /// <see langword="true"/> when the <see cref="Timer"/> event is raised periodically while the dialog is visible;
        /// otherwise, <see langword="false"/>. The default value is <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// The <see cref="Timer"/> event will be raised approximately every 200 milliseconds if this property is <see langword="true"/>.
        /// </remarks>
        [Category("Behavior"), Description("Indicates whether the Timer event is raised periodically while the dialog is visible."), DefaultValue(false)]
        public bool RaiseTimerEvent
        {
            get => GetFlag(TASKDIALOG_FLAGS.TDF_CALLBACK_TIMER);
            set
            {
                if (RaiseTimerEvent != value)
                {
                    SetFlag(TASKDIALOG_FLAGS.TDF_CALLBACK_TIMER, value);
                    UpdateDialog();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the dialog is centered in the parent window instead of the screen.
        /// </summary>
        /// <value>
        /// <see langword="true"/> when the dialog is centered relative to the parent window; <see langword="false"/> when it is
        /// centered on the screen. The default value is <see langword="false"/>.
        /// </value>
        [Category("Layout"), Description("Indicates whether the dialog is centered in the parent window instead of the screen."), DefaultValue(false)]
        public bool CenterParent
        {
            get => GetFlag(TASKDIALOG_FLAGS.TDF_POSITION_RELATIVE_TO_WINDOW);
            set
            {
                if (CenterParent != value)
                {
                    SetFlag(TASKDIALOG_FLAGS.TDF_POSITION_RELATIVE_TO_WINDOW, value);
                    UpdateDialog();
                }
            }
        }

        /// <summary>Gets or sets a value that indicates whether text is displayed right to left.</summary>
        /// <value>
        /// <see langword="true"/> when the content of the dialog is displayed right to left; otherwise, <see
        /// langword="false"/>. The default value is <see langword="false"/>.
        /// </value>
        [Localizable(true), Category("Appearance"), Description("Indicates whether text is displayed right to left."), DefaultValue(false)]
        public bool RightToLeft
        {
            get => GetFlag(TASKDIALOG_FLAGS.TDF_RTL_LAYOUT);
            set
            {
                if (RightToLeft != value)
                {
                    SetFlag(TASKDIALOG_FLAGS.TDF_RTL_LAYOUT, value);
                    UpdateDialog();
                }
            }
        }

        /// <summary>Gets or sets a value that indicates whether the dialog has a minimize box on its caption bar.</summary>
        /// <value>
        /// <see langword="true"/> if the dialog has a minimize box on its caption bar when modeless; otherwise, <see
        /// langword="false"/>. The default is <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// A task dialog can only have a minimize box if it is displayed as a modeless dialog. The minimize box will never
        /// appear when using the designer "Preview" option, since that displays the dialog modally.
        /// </remarks>
        [Category("Window Style"), Description("Indicates whether the dialog has a minimize box on its caption bar."), DefaultValue(false)]
        public bool MinimizeBox
        {
            get => GetFlag(TASKDIALOG_FLAGS.TDF_CAN_BE_MINIMIZED);
            set
            {
                if (MinimizeBox != value)
                {
                    SetFlag(TASKDIALOG_FLAGS.TDF_CAN_BE_MINIMIZED, value);
                    UpdateDialog();
                }
            }
        }

        /// <summary>Gets or sets the type of progress bar displayed on the dialog.</summary>
        /// <value>
        /// A <see cref="Wpf.ProgressBarStyle"/> that indicates the type of progress bar shown on the task dialog.
        /// </value>
        /// <remarks>
        /// <para>
        /// If this property is set to <see cref="ProgressBarStyle.MarqueeProgressBar"/>, the marquee will
        /// scroll as long as the dialog is visible.
        /// </para>
        /// <para>
        /// If this property is set to <see cref="ProgressBarStyle.ProgressBar"/>, the value of the <see
        /// cref="ProgressBarValue"/> property must be updated to advance the progress bar. This can be done e.g. by an
        /// asynchronous operation or from the <see cref="Timer"/> event.
        /// </para>
        /// <note> Updating the value of the progress bar using the <see cref="ProgressBarValue"/> while the dialog is visible
        /// property may only be done from the thread on which the task dialog was created. </note>
        /// </remarks>
        [Category("Behavior"), Description("The type of progress bar displayed on the dialog."), DefaultValue(ProgressBarStyle.None)]
        public ProgressBarStyle ProgressBarStyle
        {
            get
            {
                if (GetFlag(TASKDIALOG_FLAGS.TDF_SHOW_MARQUEE_PROGRESS_BAR))
                    return ProgressBarStyle.MarqueeProgressBar;
                else
                    return GetFlag(TASKDIALOG_FLAGS.TDF_SHOW_PROGRESS_BAR) ? ProgressBarStyle.ProgressBar : ProgressBarStyle.None;
            }
            set
            {
                SetFlag(TASKDIALOG_FLAGS.TDF_SHOW_MARQUEE_PROGRESS_BAR, value == ProgressBarStyle.MarqueeProgressBar);
                SetFlag(TASKDIALOG_FLAGS.TDF_SHOW_PROGRESS_BAR, value == ProgressBarStyle.ProgressBar);
                UpdateProgressBarStyle();
            }
        }

        /// <summary>Gets or sets the marquee animation speed of the progress bar in milliseconds.</summary>
        /// <value>The marquee animation speed of the progress bar in milliseconds. The default value is 100.</value>
        /// <remarks>This property is only used if the <see cref="ProgressBarStyle"/> property is <see cref="ProgressBarStyle.MarqueeProgressBar"/>.</remarks>
        [Category("Behavior"), Description("The marquee animation speed of the progress bar in milliseconds."), DefaultValue(100)]
        public int ProgressBarMarqueeAnimationSpeed
        {
            get => _progressBarMarqueeAnimationSpeed;
            set
            {
                _progressBarMarqueeAnimationSpeed = value;
                UpdateProgressBarMarqueeSpeed();
            }
        }

        /// <summary>Gets or sets the lower bound of the range of the task dialog's progress bar.</summary>
        /// <value>The lower bound of the range of the task dialog's progress bar. The default value is 0.</value>
        /// <remarks>This property is only used if the <see cref="ProgressBarStyle"/> property is <see cref="ProgressBarStyle.ProgressBar"/>.</remarks>
        /// <exception cref="ArgumentOutOfRangeException">The new property value is not smaller than <see cref="ProgressBarMaximum"/>.</exception>
        [Category("Behavior"), Description("The lower bound of the range of the task dialog's progress bar."), DefaultValue(0)]
        public int ProgressBarMinimum
        {
            get => _progressBarMinimimum;
            set
            {
                if (_progressBarMaximum <= value)
                    throw new ArgumentOutOfRangeException(nameof(value));
                _progressBarMinimimum = value;
                UpdateProgressBarRange();
            }
        }

        /// <summary>Gets or sets the upper bound of the range of the task dialog's progress bar.</summary>
        /// <value>The upper bound of the range of the task dialog's progress bar. The default value is 100.</value>
        /// <remarks>This property is only used if the <see cref="ProgressBarStyle"/> property is <see cref="ProgressBarStyle.ProgressBar"/>.</remarks>
        /// <exception cref="ArgumentOutOfRangeException">The new property value is not larger than <see cref="ProgressBarMinimum"/>.</exception>
        [Category("Behavior"), Description("The upper bound of the range of the task dialog's progress bar."), DefaultValue(100)]
        public int ProgressBarMaximum
        {
            get => _progressBarMaximum;
            set
            {
                if (value <= _progressBarMinimimum)
                    throw new ArgumentOutOfRangeException(nameof(value));
                _progressBarMaximum = value;
                UpdateProgressBarRange();
            }
        }

        /// <summary>Gets or sets the current value of the task dialog's progress bar.</summary>
        /// <value>The current value of the task dialog's progress bar. The default value is 0.</value>
        /// <remarks>
        /// This property is only used if the <see cref="ProgressBarStyle"/> property is <see
        /// cref="ProgressBarStyle.ProgressBar"/>. <note> Updating the value of the progress bar while the
        /// dialog is visible may only be done from the thread on which the task dialog was created. </note>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The new property value is smaller than <see cref="ProgressBarMinimum"/> or larger than <see cref="ProgressBarMaximum"/>.
        /// </exception>
        [Category("Behavior"), Description("The current value of the task dialog's progress bar."), DefaultValue(0)]
        public int ProgressBarValue
        {
            get => _progressBarValue;
            set
            {
                if (value < ProgressBarMinimum || value > ProgressBarMaximum)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _progressBarValue = value;
                UpdateProgressBarValue();
            }
        }

        /// <summary>Gets or sets the state of the task dialog's progress bar.</summary>
        /// <value>
        /// A <see cref="Wpf.ProgressBarState"/> indicating the state of the task dialog's progress bar. The
        /// default value is <see cref="ProgressBarState.Normal"/>.
        /// </value>
        /// <remarks>
        /// This property is only used if the <see cref="Wpf.ProgressBarStyle"/> property is <see cref="ProgressBarStyle.ProgressBar"/>.
        /// </remarks>
        [Category("Behavior"), Description("The state of the task dialog's progress bar."), DefaultValue(ProgressBarState.Normal)]
        public ProgressBarState ProgressBarState
        {
            get => _progressBarState;
            set
            {
                _progressBarState = value;
                UpdateProgressBarState();
            }
        }

        /// <summary>Gets or sets an object that contains data about the dialog.</summary>
        /// <value>An object that contains data about the dialog. The default value is <see langword="null"/>.</value>
        /// <remarks>Use this property to store arbitrary information about the dialog.</remarks>
        [Category("Data"), Description("User-defined data about the component."), DefaultValue(null)]
        public object Tag { get; set; }

        #endregion

        #region Public methods

        /// <summary>Shows the task dialog as a modeless dialog.</summary>
        /// <returns>
        /// The button that the user clicked. Can be <see langword="null"/> if the user cancelled the dialog using the title bar
        /// close button.
        /// </returns>
        /// <remarks>
        /// <note> Although the dialog is modeless, this method does not return until the task dialog is closed. </note>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// <para>One of the properties or a combination of properties is not valid.</para>
        /// <para>-or-</para>
        /// <para>The dialog is already running.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">Task dialogs are not supported on the current operating system.</exception>
        public TaskDialogButton Show()
        {
            return ShowDialog(IntPtr.Zero);
        }

        /// <summary>Shows the task dialog as a modal dialog.</summary>
        /// <returns>
        /// The button that the user clicked. Can be <see langword="null"/> if the user cancelled the dialog using the title bar
        /// close button.
        /// </returns>
        /// <remarks>
        /// The dialog will use the active window as its owner. If the current process has no active window, the dialog will be
        /// displayed as a modeless dialog (identical to calling <see cref="Show"/>).
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// <para>One of the properties or a combination of properties is not valid.</para>
        /// <para>-or-</para>
        /// <para>The dialog is already running.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">Task dialogs are not supported on the current operating system.</exception>
        public TaskDialogButton ShowDialog()
        {
            return ShowDialog((Window)null);
        }

        /// <summary>Shows the task dialog as a modal dialog.</summary>
        /// <param name="owner">The <see cref="Window"/> that is the owner of this task dialog.</param>
        /// <returns>
        /// The button that the user clicked. Can be <see langword="null"/> if the user cancelled the dialog using the title bar
        /// close button.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// <para>One of the properties or a combination of properties is not valid.</para>
        /// <para>-or-</para>
        /// <para>The dialog is already running.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">Task dialogs are not supported on the current operating system.</exception>
        public TaskDialogButton ShowDialog(Window owner)
        {
            IntPtr ownerHandle = owner == null ? (IntPtr)NativeMethods.GetActiveWindow() : new WindowInteropHelper(owner).Handle;
            return ShowDialog(ownerHandle);
        }

        /// <summary>Shows the task dialog as a modal dialog.</summary>
        /// <param name="owner">The <see cref="IntPtr"/> Win32 handle that is the owner of this task dialog.</param>
        /// <returns>
        /// The button that the user clicked. Can be <see langword="null"/> if the user cancelled the dialog using the title bar
        /// close button.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// <para>One of the properties or a combination of properties is not valid.</para>
        /// <para>-or-</para>
        /// <para>The dialog is already running.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">Task dialogs are not supported on the current operating system.</exception>
        /// <exception cref="InvalidOperationException">Thrown if task dialog is already being displayed.</exception>
        /// <exception cref="InvalidOperationException">Thrown if no buttons are present.</exception>
        public unsafe TaskDialogButton ShowDialog(IntPtr owner)
        {
            if (!OSSupportsTaskDialogs)
                throw new NotSupportedException(Properties.Resources.TaskDialogsNotSupportedError);

            if (IsDialogRunning)
                throw new InvalidOperationException(Properties.Resources.TaskDialogRunningError);

            if (_buttons is null || _buttons.Count == 0)
                throw new InvalidOperationException(Properties.Resources.TaskDialogNoButtonsError);

            _config.hwndParent = (HWND)owner;
            _config.dwCommonButtons = 0;
            _config.pButtons = default;
            _config.cButtons = 0;
            List<TASKDIALOG_BUTTON> buttons = SetupButtons();
            List<TASKDIALOG_BUTTON> radioButtons = SetupRadioButtons();

            SetupIcon();

            try
            {
                MarshalButtons(buttons, out var pButtons, out _config.cButtons);
                _config.pButtons = (TASKDIALOG_BUTTON*)pButtons;
                MarshalButtons(radioButtons, out var pRadioButtons, out _config.cRadioButtons);
                _config.pRadioButtons = (TASKDIALOG_BUTTON*)pRadioButtons;
                int buttonId;
                int radioButton;
                BOOL verificationFlagChecked;
                using (new ComCtlv6ActivationContext(true))
                {
                    NativeMethods.TaskDialogIndirect(_config, &buttonId, &radioButton, &verificationFlagChecked);
                }
                IsVerificationChecked = verificationFlagChecked;

                if (_radioButtonsById.TryGetValue(radioButton, out TaskDialogRadioButton selectedRadioButton))
                    selectedRadioButton.Checked = true;

                return _buttonsById.TryGetValue(buttonId, out TaskDialogButton selectedButton) ? selectedButton : null;
            }
            finally
            {
                var pButtons = (IntPtr)_config.pButtons;
                var pRadioButtons = (IntPtr)_config.pRadioButtons;
                CleanUpButtons(ref pButtons, ref _config.cButtons);
                CleanUpButtons(ref pRadioButtons, ref _config.cRadioButtons);
            }
        }

        /// <summary>Simulates a click on the verification checkbox of the <see cref="TaskDialog"/>, if it exists.</summary>
        /// <param name="checkState">
        /// <see langword="true"/> to set the state of the checkbox to be checked; <see langword="false"/> to set it to be unchecked.
        /// </param>
        /// <param name="setFocus"><see langword="true"/> to set the keyboard focus to the checkbox; otherwise <see langword="false"/>.</param>
        /// <exception cref="InvalidOperationException">The task dialog is not being displayed.</exception>
        public void ClickVerification(bool checkState, bool setFocus)
        {
            if (!IsDialogRunning)
                throw new InvalidOperationException(Properties.Resources.TaskDialogNotRunningError);

            NativeMethods.SendMessage((HWND)Handle, (int)TASKDIALOG_MESSAGES.TDM_CLICK_VERIFICATION, (nuint)(checkState ? 1 : 0), new IntPtr(setFocus ? 1 : 0));
        }

        #endregion

        #region Protected methods

        /// <summary>Raises the <see cref="HyperlinkClicked"/> event.</summary>
        /// <param name="e">The <see cref="HyperlinkClickedEventArgs"/> containing the data for the event.</param>
        protected virtual void OnHyperlinkClicked(HyperlinkClickedEventArgs e)
        {
            HyperlinkClicked?.Invoke(this, e);
        }

        /// <summary>Raises the <see cref="ButtonClicked"/> event.</summary>
        /// <param name="e">The <see cref="TaskDialogItemClickedEventArgs"/> containing the data for the event.</param>
        protected virtual void OnButtonClicked(TaskDialogItemClickedEventArgs e)
        {
            ButtonClicked?.Invoke(this, e);
        }

        /// <summary>Raises the <see cref="RadioButtonClicked"/> event.</summary>
        /// <param name="e">The <see cref="TaskDialogItemClickedEventArgs"/> containing the data for the event.</param>
        protected virtual void OnRadioButtonClicked(TaskDialogItemClickedEventArgs e)
        {
            RadioButtonClicked?.Invoke(this, e);
        }

        /// <summary>Raises the <see cref="VerificationClicked"/> event.</summary>
        /// <param name="e">The <see cref="EventArgs"/> containing the data for the event.</param>
        protected virtual void OnVerificationClicked(EventArgs e)
        {
            VerificationClicked?.Invoke(this, e);
        }

        /// <summary>Raises the <see cref="Created"/> event.</summary>
        /// <param name="e">The <see cref="EventArgs"/> containing the data for the event.</param>
        protected virtual void OnCreated(EventArgs e)
        {
            Created?.Invoke(this, e);
        }

        /// <summary>Raises the <see cref="Timer"/> event.</summary>
        /// <param name="e">The <see cref="TimerEventArgs"/> containing the data for the event.</param>
        protected virtual void OnTimer(TimerEventArgs e)
        {
            Timer?.Invoke(this, e);
        }

        /// <summary>Raises the <see cref="Destroyed"/> event.</summary>
        /// <param name="e">The <see cref="EventArgs"/> containing the data for the event.</param>
        protected virtual void OnDestroyed(EventArgs e)
        {
            Destroyed?.Invoke(this, e);
        }

        /// <summary>Raises the <see cref="ExpandButtonClicked"/> event.</summary>
        /// <param name="e">The <see cref="ExpandButtonClickedEventArgs"/> containing the data for the event.</param>
        protected virtual void OnExpandButtonClicked(ExpandButtonClickedEventArgs e)
        {
            ExpandButtonClicked?.Invoke(this, e);
        }

        /// <summary>Raises the <see cref="HelpRequested"/> event.</summary>
        /// <param name="e">The <see cref="EventArgs"/> containing the data for the event.</param>
        protected virtual void OnHelpRequested(EventArgs e)
        {
            HelpRequested?.Invoke(this, e);
        }

        #endregion

        #region Internal Members

        internal void SetItemEnabled(TaskDialogItem item)
        {
            if (IsDialogRunning)
            {
                NativeMethods.SendMessage((HWND)Handle, item is TaskDialogButton ? (uint)TASKDIALOG_MESSAGES.TDM_ENABLE_BUTTON : (uint)TASKDIALOG_MESSAGES.TDM_ENABLE_RADIO_BUTTON, (nuint)item.Id, (nint)(item.Enabled ? 1 : 0));
            }
        }

        internal void SetButtonElevationRequired(TaskDialogButton button)
        {
            if (IsDialogRunning)
            {
                NativeMethods.SendMessage((HWND)Handle, (int)TASKDIALOG_MESSAGES.TDM_SET_BUTTON_ELEVATION_REQUIRED_STATE, (nuint)button.Id, new IntPtr(button.ElevationRequired ? 1 : 0));
            }
        }

        internal void ClickItem(TaskDialogItem item)
        {
            if (!IsDialogRunning)
                throw new InvalidOperationException(Properties.Resources.TaskDialogNotRunningError);

            NativeMethods.SendMessage((HWND)Handle, (uint)(item is TaskDialogButton ? TASKDIALOG_MESSAGES.TDM_CLICK_BUTTON : TASKDIALOG_MESSAGES.TDM_CLICK_RADIO_BUTTON), (nuint)item.Id, IntPtr.Zero);
        }

        #endregion

        #region Private members

        private bool IsDialogRunning =>
                // Intentially not using the Handle property, since the cross-thread call check should not be performed here.
                _handle != IntPtr.Zero;

        internal unsafe void UpdateDialog()
        {
            if (IsDialogRunning)
            {
                // If the navigate page message is sent from within the callback, the navigation won't take place until the
                // callback returns. Any further messages sent after the navigate page message before the end of the callback
                // will then be lost as the navigation occurs. For that reason, we defer it all the way until the end.
                if (_inEventHandler > 0)
                    _updatePending = true;
                else
                {
                    _updatePending = false;
                    var pButtons = (IntPtr)_config.pButtons;
                    var pRadioButtons = (IntPtr)_config.pRadioButtons;
                    CleanUpButtons(ref pButtons, ref _config.cButtons);
                    CleanUpButtons(ref pRadioButtons, ref _config.cRadioButtons);
                    _config.dwCommonButtons = 0;

                    List<TASKDIALOG_BUTTON> buttons = SetupButtons();
                    List<TASKDIALOG_BUTTON> radioButtons = SetupRadioButtons();

                    SetupIcon();

                    MarshalButtons(buttons, out pButtons, out _config.cButtons);
                    _config.pButtons = (TASKDIALOG_BUTTON*)pButtons;
                    MarshalButtons(radioButtons, out pRadioButtons, out _config.cRadioButtons);
                    _config.pRadioButtons = (TASKDIALOG_BUTTON*)pRadioButtons;

                    int size = Marshal.SizeOf(_config);
                    IntPtr memory = Marshal.AllocHGlobal(size);
                    try
                    {
                        Marshal.StructureToPtr(_config, memory, false);
                        NativeMethods.SendMessage((HWND)Handle, (int)TASKDIALOG_MESSAGES.TDM_NAVIGATE_PAGE, 0, memory);
                    }
                    finally
                    {
                        Marshal.DestroyStructure(memory, typeof(TASKDIALOGCONFIG));
                        Marshal.FreeHGlobal(memory);
                    }
                }
            }
        }

        private static void CleanUpButtons(ref IntPtr buttons, ref uint count)
        {
            if (buttons != IntPtr.Zero)
            {
                int elementSize = Marshal.SizeOf(typeof(TASKDIALOG_BUTTON));
                for (int x = 0; x < count; ++x)
                {
                    // This'll be safe until they introduce 128 bit machines. :) It's the only way to do it without unsafe code.
                    IntPtr offset = new IntPtr(buttons.ToInt64() + x * elementSize);
                    Marshal.DestroyStructure(offset, typeof(TASKDIALOG_BUTTON));
                }
                Marshal.FreeHGlobal(buttons);
                buttons = IntPtr.Zero;
                count = 0;
            }
        }

        private static void MarshalButtons(List<TASKDIALOG_BUTTON> buttons, out IntPtr buttonsPtr, out uint count)
        {
            buttonsPtr = IntPtr.Zero;
            count = 0;
            if (buttons.Count > 0)
            {
                int elementSize = Marshal.SizeOf(typeof(TASKDIALOG_BUTTON));
                buttonsPtr = Marshal.AllocHGlobal(elementSize * buttons.Count);
                for (int x = 0; x < buttons.Count; ++x)
                {
                    // This'll be safe until they introduce 128 bit machines. :) It's the only way to do it without unsafe code.
                    IntPtr offset = new IntPtr(buttonsPtr.ToInt64() + x * elementSize);
                    Marshal.StructureToPtr(buttons[x], offset, false);
                }
                count = (uint)buttons.Count;
            }
        }

        private void SetElementText(TASKDIALOG_ELEMENTS element, string text)
        {
            if (IsDialogRunning)
            {
                IntPtr newTextPtr = Marshal.StringToHGlobalUni(text);
                try
                {
                    IntPtr result = NativeMethods.SendMessage((HWND)Handle, (int)TASKDIALOG_MESSAGES.TDM_SET_ELEMENT_TEXT, (nuint)element, newTextPtr);
                }
                finally
                {
                    if (newTextPtr != IntPtr.Zero)
                        Marshal.FreeHGlobal(newTextPtr);
                }
            }
        }

        private void SetupIcon()
        {
            SetupIcon(MainIcon, CustomMainIcon, TASKDIALOG_FLAGS.TDF_USE_HICON_MAIN);
            SetupIcon(FooterIcon, CustomFooterIcon, TASKDIALOG_FLAGS.TDF_USE_HICON_FOOTER);
        }

        private void SetupIcon(TaskDialogIcon icon, System.Drawing.Icon customIcon, TASKDIALOG_FLAGS flag)
        {
            SetFlag(flag, false);
            if (icon == TaskDialogIcon.Custom)
            {
                if (customIcon != null)
                {
                    SetFlag(flag, true);
                    if (flag == TASKDIALOG_FLAGS.TDF_USE_HICON_MAIN)
                        _config.Anonymous1.hMainIcon = (HICON)customIcon.Handle;
                    else
                        _config.Anonymous2.hFooterIcon = (HICON)customIcon.Handle;
                }
            }
            else
            {
                if (flag == TASKDIALOG_FLAGS.TDF_USE_HICON_MAIN)
                    _config.Anonymous1.hMainIcon = (HICON)new IntPtr((int)icon);
                else
                    _config.Anonymous2.hFooterIcon = (HICON)new IntPtr((int)icon);
            }
        }

        private unsafe List<TASKDIALOG_BUTTON> SetupButtons()
        {
            _buttonsById = new Dictionary<int, TaskDialogButton>();
            List<TASKDIALOG_BUTTON> buttons = new List<TASKDIALOG_BUTTON>();
            _config.nDefaultButton = 0;
            foreach (TaskDialogButton button in Buttons)
            {
                if (button.Id < 1)
                    throw new InvalidOperationException(Properties.Resources.InvalidTaskDialogItemIdError);
                _buttonsById.Add(button.Id, button);
                if (button.Default)
                    _config.nDefaultButton = button.Id;
                if (button.ButtonType == ButtonType.Custom)
                {
                    if (string.IsNullOrEmpty(button.Text))
                        throw new InvalidOperationException(Properties.Resources.TaskDialogEmptyButtonLabelError);

                    TASKDIALOG_BUTTON taskDialogButton;
                    var text = button.Text;
                    if (ButtonStyle == TaskDialogButtonStyle.CommandLinks || ButtonStyle == TaskDialogButtonStyle.CommandLinksNoIcon && !string.IsNullOrEmpty(button.CommandLinkNote))
                        text += "\n" + button.CommandLinkNote;

                    fixed (char* pText = text)
                    {
                        taskDialogButton = new TASKDIALOG_BUTTON
                        {
                            nButtonID = button.Id,
                            pszButtonText = pText
                        };
                    }
                    buttons.Add(taskDialogButton);
                }
                else
                {
                    _config.dwCommonButtons |= button.ButtonFlag;
                }
            }
            return buttons;
        }

        private unsafe List<TASKDIALOG_BUTTON> SetupRadioButtons()
        {
            _radioButtonsById = new Dictionary<int, TaskDialogRadioButton>();
            List<TASKDIALOG_BUTTON> radioButtons = new List<TASKDIALOG_BUTTON>();
            _config.nDefaultRadioButton = 0;
            foreach (TaskDialogRadioButton radioButton in RadioButtons)
            {
                if (string.IsNullOrEmpty(radioButton.Text))
                    throw new InvalidOperationException(Properties.Resources.TaskDialogEmptyButtonLabelError);
                if (radioButton.Id < 1)
                    throw new InvalidOperationException(Properties.Resources.InvalidTaskDialogItemIdError);
                _radioButtonsById.Add(radioButton.Id, radioButton);
                if (radioButton.Checked)
                    _config.nDefaultRadioButton = radioButton.Id;
                TASKDIALOG_BUTTON taskDialogButton = new TASKDIALOG_BUTTON();
                taskDialogButton.nButtonID = radioButton.Id;
                fixed (char* pText = radioButton.Text)
                {
                    taskDialogButton.pszButtonText = pText;
                }
                radioButtons.Add(taskDialogButton);
            }
            SetFlag(TASKDIALOG_FLAGS.TDF_NO_DEFAULT_RADIO_BUTTON, _config.nDefaultRadioButton == 0);
            return radioButtons;
        }

        private void SetFlag(TASKDIALOG_FLAGS flag, bool value)
        {
            if (value)
                _config.dwFlags |= flag;
            else
                _config.dwFlags &= ~flag;
        }

        private bool GetFlag(TASKDIALOG_FLAGS flag)
        {
            return (_config.dwFlags & flag) != 0;
        }

        private HRESULT TaskDialogCallback(HWND hwnd, uint uNotification, WPARAM wParam, LPARAM lParam, nint dwRefData)
        {
            Interlocked.Increment(ref _inEventHandler);
            try
            {
                switch ((TASKDIALOG_NOTIFICATIONS)uNotification)
                {
                    case TASKDIALOG_NOTIFICATIONS.TDN_CREATED:
                        _handle = hwnd;
                        DialogCreated();
                        OnCreated(EventArgs.Empty);
                        break;
                    case TASKDIALOG_NOTIFICATIONS.TDN_DESTROYED:
                        _handle = IntPtr.Zero;
                        OnDestroyed(EventArgs.Empty);
                        break;
                    case TASKDIALOG_NOTIFICATIONS.TDN_NAVIGATED:
                        DialogCreated();
                        break;
                    case TASKDIALOG_NOTIFICATIONS.TDN_HYPERLINK_CLICKED:
                        string url = Marshal.PtrToStringUni(lParam);
                        OnHyperlinkClicked(new HyperlinkClickedEventArgs(url));
                        break;
                    case TASKDIALOG_NOTIFICATIONS.TDN_BUTTON_CLICKED:
                        TaskDialogButton button;
                        if (_buttonsById.TryGetValue((int)(nuint)wParam, out button))
                        {
                            TaskDialogItemClickedEventArgs e = new TaskDialogItemClickedEventArgs(button);
                            OnButtonClicked(e);
                            if (e.Cancel)
                                return HRESULT.S_FALSE;
                        }
                        break;
                    case TASKDIALOG_NOTIFICATIONS.TDN_VERIFICATION_CLICKED:
                        IsVerificationChecked = (int)(nuint)wParam == 1;
                        OnVerificationClicked(EventArgs.Empty);
                        break;
                    case TASKDIALOG_NOTIFICATIONS.TDN_RADIO_BUTTON_CLICKED:
                        TaskDialogRadioButton radioButton;
                        if (_radioButtonsById.TryGetValue((int)(nuint)wParam, out radioButton))
                        {
                            radioButton.Checked = true; // there's no way to click a radio button without checking it, is there?
                            TaskDialogItemClickedEventArgs e = new TaskDialogItemClickedEventArgs(radioButton);
                            OnRadioButtonClicked(e);
                        }
                        break;
                    case TASKDIALOG_NOTIFICATIONS.TDN_TIMER:
                        TimerEventArgs timerEventArgs = new TimerEventArgs((int)(nuint)wParam);
                        OnTimer(timerEventArgs);
                        return timerEventArgs.ResetTickCount ? HRESULT.S_FALSE : HRESULT.S_OK;
                    case TASKDIALOG_NOTIFICATIONS.TDN_EXPANDO_BUTTON_CLICKED:
                        OnExpandButtonClicked(new ExpandButtonClickedEventArgs((int)(nuint)wParam != 0));
                        break;
                    case TASKDIALOG_NOTIFICATIONS.TDN_HELP:
                        OnHelpRequested(EventArgs.Empty);
                        break;
                }
                return HRESULT.S_OK;
            }
            finally
            {
                Interlocked.Decrement(ref _inEventHandler);
                if (_updatePending)
                    UpdateDialog();
            }
        }

        private void DialogCreated()
        {
            if (_config.hwndParent == IntPtr.Zero && _windowIcon != null)
            {
                NativeMethods.SendMessage((HWND)Handle, NativeMethods.WM_SETICON, (nuint)NativeMethods.ICON_SMALL, _windowIcon.Handle);
            }

            foreach (TaskDialogButton button in Buttons)
            {
                if (!button.Enabled)
                    SetItemEnabled(button);
                if (button.ElevationRequired)
                    SetButtonElevationRequired(button);
            }
            UpdateProgressBarStyle();
            UpdateProgressBarMarqueeSpeed();
            UpdateProgressBarRange();
            UpdateProgressBarValue();
            UpdateProgressBarState();
        }

        private void UpdateProgressBarStyle()
        {
            if (IsDialogRunning)
            {
                NativeMethods.SendMessage((HWND)Handle, (int)TASKDIALOG_MESSAGES.TDM_SET_MARQUEE_PROGRESS_BAR, (nuint)(ProgressBarStyle == ProgressBarStyle.MarqueeProgressBar ? 1 : 0), IntPtr.Zero);
            }
        }

        private void UpdateProgressBarMarqueeSpeed()
        {
            if (IsDialogRunning)
            {
                NativeMethods.SendMessage((HWND)Handle, (int)TASKDIALOG_MESSAGES.TDM_SET_PROGRESS_BAR_MARQUEE, (nuint)(ProgressBarMarqueeAnimationSpeed > 0 ? 1 : 0), (nint)ProgressBarMarqueeAnimationSpeed);
            }
        }

        private void UpdateProgressBarRange()
        {
            if (IsDialogRunning)
            {
                NativeMethods.SendMessage((HWND)Handle, (int)TASKDIALOG_MESSAGES.TDM_SET_PROGRESS_BAR_RANGE, 0, new IntPtr(ProgressBarMaximum << 16 | ProgressBarMinimum));
            }
            if (ProgressBarValue < ProgressBarMinimum)
                ProgressBarValue = ProgressBarMinimum;
            if (ProgressBarValue > ProgressBarMaximum)
                ProgressBarValue = ProgressBarMaximum;
        }

        private void UpdateProgressBarValue()
        {
            if (IsDialogRunning)
            {
                NativeMethods.SendMessage((HWND)Handle, (int)TASKDIALOG_MESSAGES.TDM_SET_PROGRESS_BAR_POS, (nuint)ProgressBarValue, IntPtr.Zero);
            }
        }

        private void UpdateProgressBarState()
        {
            if (IsDialogRunning)
            {
                NativeMethods.SendMessage((HWND)Handle, (int)TASKDIALOG_MESSAGES.TDM_SET_PROGRESS_BAR_STATE, (nuint)ProgressBarState + 1, IntPtr.Zero);
            }
        }

        private unsafe void CheckCrossThreadCall()
        {
            IntPtr handle = _handle;
            if (handle != IntPtr.Zero)
            {
                uint processId;
                var windowThreadId = NativeMethods.GetWindowThreadProcessId((HWND)handle, &processId);
                var threadId = NativeMethods.GetCurrentThreadId();
                if (windowThreadId != threadId)
                    throw new InvalidOperationException(Properties.Resources.TaskDialogIllegalCrossThreadCallError);
            }
        }

        #endregion

        #region IWin32Window Members

        /// <summary>Gets the window handle of the task dialog.</summary>
        /// <value>
        /// The window handle of the task dialog when it is being displayed, or <see cref="IntPtr.Zero"/> when the dialog is not
        /// being displayed.
        /// </value>
        [Browsable(false)]
        public IntPtr Handle
        {
            get
            {
                CheckCrossThreadCall();
                return _handle;
            }
        }

        #endregion
    }
}
