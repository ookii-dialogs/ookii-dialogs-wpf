#region Copyright 2009-2021 Ookii Dialogs Contributors
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Security.Permissions;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ookii.Dialogs.Wpf
{
    /// <summary>
    /// Represents a dialog box that allows the user to enter generic credentials.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   This class is meant for generic credentials; it does not provide access to all the functionality
    ///   of the Windows CredUI API. Features such as Windows domain credentials or alternative security
    ///   providers (e.g. smartcards or biometric devices) are not supported.
    /// </para>
    /// <para>
    ///   The <see cref="CredentialDialog"/> class provides methods for storing and retrieving credentials,
    ///   and also manages automatic persistence of credentials by using the "Save password" checkbox on
    ///   the credentials dialog. To specify the target for which the credentials should be saved, set the
    ///   <see cref="Target"/> property.
    /// </para>
    /// <note>
    ///   This class requires Windows XP or later.
    /// </note>
    /// </remarks>
    /// <threadsafety instance="false" static="true" />
    [DefaultProperty("MainInstruction"), DefaultEvent("UserNameChanged"), Description("Allows access to credential UI for generic credentials.")]
    public partial class CredentialDialog : Component
    {
        private string _confirmTarget;
        private BOOL _isSaveChecked;
        private string _target;

        private static readonly Dictionary<string, System.Net.NetworkCredential> _applicationInstanceCredentialCache = new Dictionary<string, NetworkCredential>();
        private string _caption;
        private string _text;
        private string _windowTitle;

        /// <summary>
        /// Event raised when the <see cref="UserName"/> property changes.
        /// </summary>
        [Category("Property Changed"), Description("Event raised when the value of the UserName property changes.")]
        public event EventHandler UserNameChanged;
        /// <summary>
        /// Event raised when the <see cref="Password"/> property changes.
        /// </summary>
        [Category("Property Changed"), Description("Event raised when the value of the Password property changes.")]
        public event EventHandler PasswordChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="CredentialDialog"/> class.
        /// </summary>
        public CredentialDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CredentialDialog"/> class with the specified container.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> to add the component to.</param>
        public CredentialDialog(IContainer container)
        {
            container?.Add(this);

            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets whether to use the application instance credential cache.
        /// </summary>
        /// <value>
        /// <see langword="true" /> when credentials are saved in the application instance cache; <see langref="false" /> if they are not.
        /// The default value is <see langword="false" />.
        /// </value>
        /// <remarks>
        /// <para>
        ///   The application instance credential cache stores credentials in memory while an application is running. When the
        ///   application exits, this cache is not persisted.
        /// </para>
        /// <para>
        ///   When the <see cref="UseApplicationInstanceCredentialCache"/> property is set to <see langword="true"/>, credentials that
        ///   are confirmed with <see cref="ConfirmCredentials"/> when the user checked the "save password" option will be stored
        ///   in the application instance cache as well as the operating system credential store.
        /// </para>
        /// <para>
        ///   When <see cref="ShowDialog()"/> is called, and credentials for the specified <see cref="Target"/> are already present in
        ///   the application instance cache, the dialog will not be shown and the cached credentials are returned, even if
        ///   <see cref="ShowUIForSavedCredentials"/> is <see langword="true"/>.
        /// </para>
        /// <para>
        ///   The application instance credential cache allows you to prevent prompting the user again for the lifetime of the
        ///   application if the "save password" checkbox was checked, but when the application is restarted you can prompt again
        ///   (initializing the dialog with the saved credentials). To get this behaviour, the <see cref="ShowUIForSavedCredentials"/>
        ///   property must be set to <see langword="true"/>.
        /// </para>
        /// </remarks>
        [Category("Behavior"), Description("Indicates whether to use the application instance credential cache."), DefaultValue(false)]
        public bool UseApplicationInstanceCredentialCache { get; set; }

        /// <summary>
        /// Gets or sets whether the "save password" checkbox is checked.
        /// </summary>
        /// <value>
        /// <see langword="true" /> if the "save password" is checked; otherwise, <see langword="false" />.
        /// The default value is <see langword="false" />.
        /// </value>
        /// <remarks>
        /// The value of this property is only valid if the dialog box is displayed with a save checkbox.
        /// Set this property before showing the dialog to determine the initial checked value of the save checkbox.
        /// </remarks>
        [Category("Appearance"), Description("Indicates whether the \"Save password\" checkbox is checked."), DefaultValue(false)]
        public bool IsSaveChecked
        {
            get => _isSaveChecked;
            set
            {
                _confirmTarget = null;
                _isSaveChecked = value;
            }
        }

        /// <summary>
        /// Gets the password the user entered in the dialog.
        /// </summary>
        /// <value>
        /// The password entered in the password field of the credentials dialog.
        /// </value>
        [Browsable(false)]
        public string Password
        {
            get => Credentials.Password;
            private set
            {
                _confirmTarget = null;
                Credentials.Password = value;
                OnPasswordChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the optional entropy to increase the complexity of the password encryption.
        /// This property is only used if the user chose to save the credentials.
        /// </summary>
        /// <value>
        /// A byte array with values of your choosing.
        /// The default value is <see langword="null" /> for no added complexity.
        /// </value>
        [Browsable(false)]
        public byte[] AdditionalEntropy { get; set; }

        /// <summary>
        /// Gets the user-specified user name and password in a <see cref="NetworkCredential"/> object.
        /// </summary>
        /// <value>
        /// A <see cref="NetworkCredential"/> instance containing the user name and password specified on the dialog.
        /// </value>
        [Browsable(false)]
        public NetworkCredential Credentials { get; } = new NetworkCredential();

        /// <summary>
        /// Gets the user name the user entered in the dialog.
        /// </summary>
        /// <value>
        /// The user name entered in the user name field of the credentials dialog.
        /// The default value is an empty string ("").
        /// </value>
        [Browsable(false)]
        public string UserName
        {
            get => Credentials.UserName ?? string.Empty;
            private set
            {
                _confirmTarget = null;
                Credentials.UserName = value;
                OnUserNameChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the target for the credentials, typically a server name.
        /// </summary>
        /// <value>
        /// The target for the credentials. The default value is an empty string ("").
        /// </value>
        /// <remarks>
        /// Credentials are stored on a per user, not on a per application basis. To ensure that credentials stored by different 
        /// applications do not conflict, you should prefix the target with an application-specific identifer, e.g. 
        /// "Company_Application_target".
        /// </remarks>
        [Category("Behavior"), Description("The target for the credentials, typically the server name prefixed by an application-specific identifier."), DefaultValue("")]
        public string Target
        {
            get => _target ?? string.Empty;
            set
            {
                _target = value;
                _confirmTarget = null;
            }
        }

        /// <summary>
        /// Gets or sets the title of the credentials dialog.
        /// </summary>
        /// <value>
        /// The title of the credentials dialog. The default value is an empty string ("").
        /// </value>
        /// <remarks>
        /// <para>
        ///   This property is not used on Windows Vista and newer versions of windows; the window title will always be "Windows Security"
        ///   in that case.
        /// </para>
        /// </remarks>
        [Localizable(true), Category("Appearance"), Description("The title of the credentials dialog."), DefaultValue("")]
        public string WindowTitle
        {
            get => _windowTitle ?? string.Empty;
            set => _windowTitle = value;
        }

        /// <summary>
        /// Gets or sets a brief message to display in the dialog box.
        /// </summary>
        /// <value>
        /// A brief message that will be displayed in the dialog box. The default value is an empty string ("").
        /// </value>
        /// <remarks>
        /// <para>
        ///   On Windows Vista and newer versions of Windows, this text is displayed using a different style to set it apart
        ///   from the other text. In the default style, this text is a slightly larger and colored blue. The style is identical
        ///   to the main instruction of a task dialog.
        /// </para>
        /// <para>
        ///   On Windows XP, this text is not distinguished from other text. It's display mode depends on the <see cref="DownlevelTextMode"/>
        ///   property.
        /// </para>
        /// </remarks>
        [Localizable(true), Category("Appearance"), Description("A brief message that will be displayed in the dialog box."), DefaultValue("")]
        public string MainInstruction
        {
            get => _caption ?? string.Empty;
            set => _caption = value;
        }

        /// <summary>
        /// Gets or sets additional text to display in the dialog.
        /// </summary>
        /// <value>
        /// Additional text to display in the dialog. The default value is an empty string ("").
        /// </value>
        /// <remarks>
        /// <para>
        ///   On Windows Vista and newer versions of Windows, this text is placed below the <see cref="MainInstruction"/> text.
        /// </para>
        /// <para>
        ///   On Windows XP, how and if this text is displayed depends on the value of the <see cref="DownlevelTextMode"/>
        ///   property.
        /// </para>
        /// </remarks>
        [Localizable(true), Category("Appearance"), Description("Additional text to display in the dialog."), DefaultValue(""), Editor(typeof(System.ComponentModel.Design.MultilineStringEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string Content
        {
            get => _text ?? string.Empty;
            set => _text = value;
        }

        /// <summary>
        /// Gets or sets a value that indicates how the text of the <see cref="MainInstruction"/> and <see cref="Content"/> properties
        /// is displayed on Windows XP.
        /// </summary>
        /// <value>
        /// One of the values of the <see cref="Ookii.Dialogs.Wpf.DownlevelTextMode"/> enumeration. The default value is
        /// <see cref="Ookii.Dialogs.Wpf.DownlevelTextMode.MainInstructionAndContent"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   Windows XP does not support the distinct visual style of the main instruction, so there is no visual difference between the
        ///   text of the <see cref="CredentialDialog.MainInstruction"/> and <see cref="CredentialDialog.Content"/> properties. Depending
        ///   on your requirements, you may wish to hide either the main instruction or the content text.
        /// </para>
        /// <para>
        ///   This property has no effect on Windows Vista and newer versions of Windows.
        /// </para>
        /// </remarks>
        [Localizable(true), Category("Appearance"), Description("Indicates how the text of the MainInstruction and Content properties is displayed on Windows XP."), DefaultValue(DownlevelTextMode.MainInstructionAndContent)]
        public DownlevelTextMode DownlevelTextMode { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether a check box is shown on the dialog that allows the user to choose whether to save
        /// the credentials or not.
        /// </summary>
        /// <value>
        /// <see langword="true" /> when the "save password" checkbox is shown on the credentials dialog; otherwise, <see langword="false"/>.
        /// The default value is <see langword="false" />.
        /// </value>
        /// <remarks>
        /// When this property is set to <see langword="true" />, you must call the <see cref="ConfirmCredentials"/> method to save the
        /// credentials. When this property is set to <see langword="false" />, the credentials will never be saved, and you should not call
        /// the <see cref="ConfirmCredentials"/> method.
        /// </remarks>
        [Category("Appearance"), Description("Indicates whether a check box is shown on the dialog that allows the user to choose whether to save the credentials or not."), DefaultValue(false)]
        public bool ShowSaveCheckBox { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the dialog should be displayed even when saved credentials exist for the 
        /// specified target.
        /// </summary>
        /// <value>
        /// <see langword="true" /> if the dialog is displayed even when saved credentials exist; otherwise, <see langword="false" />.
        /// The default value is <see langword="false" />.
        /// </value>
        /// <remarks>
        /// <para>
        ///   This property applies only when the <see cref="ShowSaveCheckBox"/> property is <see langword="true" />.
        /// </para>
        /// <para>
        ///   Note that even if this property is <see langword="true" />, if the proper credentials exist in the 
        ///   application instance credentials cache the dialog will not be displayed.
        /// </para>
        /// </remarks>
        [Category("Behavior"), Description("Indicates whether the dialog should be displayed even when saved credentials exist for the specified target."), DefaultValue(false)]
        public bool ShowUIForSavedCredentials { get; set; }

        /// <summary>
        /// Gets a value that indicates whether the current credentials were retrieved from a credential store.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the current credentials returned by the <see cref="UserName"/>, <see cref="Password"/>,
        /// and <see cref="Credentials"/> properties were retrieved from either the application instance credential cache
        /// or the operating system's credential store; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   You can use this property to determine if the credentials dialog was shown after a call to <see cref="ShowDialog()"/>.
        ///   If the dialog was shown, this property will be <see langword="false"/>; if the credentials were retrieved from the
        ///   application instance cache or the credential store and the dialog was not shown it will be <see langword="true"/>.
        /// </para>
        /// <para>
        ///   If the <see cref="ShowUIForSavedCredentials"/> property is set to <see langword="true"/>, and the dialog is shown
        ///   but populated with stored credentials, this property will still return <see langword="false"/>.
        /// </para>
        /// </remarks>
        public bool IsStoredCredential { get; private set; }


        /// <summary>
        /// Shows the credentials dialog as a modal dialog.
        /// </summary>
        /// <returns><see langword="true" /> if the user clicked OK; otherwise, <see langword="false" />.</returns>
        /// <remarks>
        /// <para>
        ///   The credentials dialog will not be shown if one of the following conditions holds:
        /// </para>
        /// <list type="bullet">
        ///   <item>
        ///     <description>
        ///       <see cref="UseApplicationInstanceCredentialCache"/> is <see langword="true"/> and the application instance
        ///       credential cache contains credentials for the specified <see cref="Target"/>, even if <see cref="ShowUIForSavedCredentials"/>
        ///       is <see langword="true"/>.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       <see cref="ShowSaveCheckBox"/> is <see langword="true"/>, <see cref="ShowUIForSavedCredentials"/> is <see langword="false"/>, and the operating system credential store
        ///       for the current user contains credentials for the specified <see cref="Target"/>.
        ///     </description>
        ///   </item>
        /// </list>
        /// <para>
        ///   In these cases, the <see cref="Credentials"/>, <see cref="UserName"/> and <see cref="Password"/> properties will
        ///   be set to the saved credentials and this function returns immediately, returning <see langword="true" />.
        /// </para>
        /// <para>
        ///   If the <see cref="ShowSaveCheckBox"/> property is <see langword="true"/>, you should call <see cref="ConfirmCredentials"/>
        ///   after validating if the provided credentials are correct.
        /// </para>
        /// </remarks>
        /// <exception cref="CredentialException">An error occurred while showing the credentials dialog.</exception>
        /// <exception cref="InvalidOperationException"><see cref="Target"/> is an empty string ("").</exception>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public bool ShowDialog()
        {
            return ShowDialog(null);
        }

        /// <summary>
        /// Shows the credentials dialog as a modal dialog with the specified owner.
        /// </summary>
        /// <param name="owner">The <see cref="IntPtr"/> Win32 handle that owns the credentials dialog.</param>
        /// <returns><see langword="true" /> if the user clicked OK; otherwise, <see langword="false" />.</returns>
        /// <remarks>
        /// <para>
        ///   The credentials dialog will not be shown if one of the following conditions holds:
        /// </para>
        /// <list type="bullet">
        ///   <item>
        ///     <description>
        ///       <see cref="UseApplicationInstanceCredentialCache"/> is <see langword="true"/> and the application instance
        ///       credential cache contains credentials for the specified <see cref="Target"/>, even if <see cref="ShowUIForSavedCredentials"/>
        ///       is <see langword="true"/>.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       <see cref="ShowSaveCheckBox"/> is <see langword="true"/>, <see cref="ShowUIForSavedCredentials"/> is <see langword="false"/>, and the operating system credential store
        ///       for the current user contains credentials for the specified <see cref="Target"/>.
        ///     </description>
        ///   </item>
        /// </list>
        /// <para>
        ///   In these cases, the <see cref="Credentials"/>, <see cref="UserName"/> and <see cref="Password"/> properties will
        ///   be set to the saved credentials and this function returns immediately, returning <see langword="true" />.
        /// </para>
        /// <para>
        ///   If the <see cref="ShowSaveCheckBox"/> property is <see langword="true"/>, you should call <see cref="ConfirmCredentials"/>
        ///   after validating if the provided credentials are correct.
        /// </para>
        /// </remarks>
        /// <exception cref="CredentialException">An error occurred while showing the credentials dialog.</exception>
        /// <exception cref="InvalidOperationException"><see cref="Target"/> is an empty string ("").</exception>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public bool ShowDialog(IntPtr owner)
        {
            if (string.IsNullOrEmpty(_target))
                throw new InvalidOperationException(Properties.Resources.CredentialEmptyTargetError);

            HWND ownerHandle = owner == default ? NativeMethods.GetActiveWindow() : (HWND)owner;

            UserName = "";
            Password = "";
            IsStoredCredential = false;

            if (RetrieveCredentialsFromApplicationInstanceCache())
            {
                IsStoredCredential = true;
                _confirmTarget = Target;
                return true;
            }

            bool storedCredentials = false;
            if (ShowSaveCheckBox && RetrieveCredentials())
            {
                IsSaveChecked = true;
                if (!ShowUIForSavedCredentials)
                {
                    IsStoredCredential = true;
                    _confirmTarget = Target;
                    return true;
                }
                storedCredentials = true;
            }

            bool result = NativeMethods.IsWindowsVistaOrLater
                ? PromptForCredentialsCredUIWin(ownerHandle, storedCredentials)
                : PromptForCredentialsCredUI(ownerHandle, storedCredentials);
            return result;
        }

        /// <summary>
        /// Shows the credentials dialog as a modal dialog with the specified owner.
        /// </summary>
        /// <param name="owner">The <see cref="Window"/> that owns the credentials dialog.</param>
        /// <returns><see langword="true" /> if the user clicked OK; otherwise, <see langword="false" />.</returns>
        /// <remarks>
        /// <para>
        ///   The credentials dialog will not be shown if one of the following conditions holds:
        /// </para>
        /// <list type="bullet">
        ///   <item>
        ///     <description>
        ///       <see cref="UseApplicationInstanceCredentialCache"/> is <see langword="true"/> and the application instance
        ///       credential cache contains credentials for the specified <see cref="Target"/>, even if <see cref="ShowUIForSavedCredentials"/>
        ///       is <see langword="true"/>.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       <see cref="ShowSaveCheckBox"/> is <see langword="true"/>, <see cref="ShowUIForSavedCredentials"/> is <see langword="false"/>, and the operating system credential store
        ///       for the current user contains credentials for the specified <see cref="Target"/>.
        ///     </description>
        ///   </item>
        /// </list>
        /// <para>
        ///   In these cases, the <see cref="Credentials"/>, <see cref="UserName"/> and <see cref="Password"/> properties will
        ///   be set to the saved credentials and this function returns immediately, returning <see langword="true" />.
        /// </para>
        /// <para>
        ///   If the <see cref="ShowSaveCheckBox"/> property is <see langword="true"/>, you should call <see cref="ConfirmCredentials"/>
        ///   after validating if the provided credentials are correct.
        /// </para>
        /// </remarks>
        /// <exception cref="CredentialException">An error occurred while showing the credentials dialog.</exception>
        /// <exception cref="InvalidOperationException"><see cref="Target"/> is an empty string ("").</exception>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public bool ShowDialog(Window owner)
        {
            IntPtr ownerHandle = owner == null ? (IntPtr)NativeMethods.GetActiveWindow() : new WindowInteropHelper(owner).Handle;
            return ShowDialog(ownerHandle);
        }

        /// <summary>
        /// Confirms the validity of the credential provided by the user.
        /// </summary>
        /// <param name="confirm"><see langword="true" /> if the credentials that were specified on the dialog are valid; otherwise, <see langword="false" />.</param>
        /// <remarks>
        /// Call this function after calling <see cref="ShowDialog()" /> when <see cref="ShowSaveCheckBox"/> is <see langword="true" />.
        /// Only when this function is called with <paramref name="confirm"/> set to <see langword="true" /> will the credentials be
        /// saved in the credentials store and/or the application instance credential cache.
        /// </remarks>
        /// <exception cref="InvalidOperationException"><see cref="ShowDialog()"/> was not called, or the user did not click OK, or <see cref="ShowSaveCheckBox"/> was <see langword="false" />
        /// at the call, or the value of <see cref="Target"/> or <see cref="IsSaveChecked"/>
        /// was changed after the call.</exception>
        /// <exception cref="CredentialException">There was an error saving the credentials.</exception>
        public void ConfirmCredentials(bool confirm)
        {
            if (_confirmTarget == null || _confirmTarget != Target)
                throw new InvalidOperationException(Properties.Resources.CredentialPromptNotCalled);

            _confirmTarget = null;

            if (IsSaveChecked && confirm)
            {
                if (UseApplicationInstanceCredentialCache)
                {
                    lock (_applicationInstanceCredentialCache)
                    {
                        _applicationInstanceCredentialCache[Target] = new System.Net.NetworkCredential(UserName, Password);
                    }
                }

                StoreCredential(Target, Credentials, AdditionalEntropy);
            }
        }

        /// <summary>
        /// Stores the specified credentials in the operating system's credential store for the currently logged on user.
        /// </summary>
        /// <param name="target">The target name for the credentials.</param>
        /// <param name="credential">The credentials to store.</param>
        /// <param name="additionalEntropy">Additional entropy for encrypting the password.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>
        ///   <paramref name="target"/> is <see langword="null" />.
        /// </para>
        /// <para>
        ///   -or-
        /// </para>
        /// <para>
        ///   <paramref name="credential"/> is <see langword="null" />.
        /// </para>
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="target"/> is an empty string ("").</exception>
        /// <exception cref="CredentialException">An error occurred storing the credentials.</exception>
        /// <remarks>
        /// <note>
        ///   The <see cref="NetworkCredential.Domain"/> property is ignored and will not be stored, even if it is
        ///   not <see langword="null" />.
        /// </note>
        /// <para>
        ///   If the credential manager already contains credentials for the specified <paramref name="target"/>, they
        ///   will be overwritten; this can even overwrite credentials that were stored by another application. Therefore 
        ///   it is strongly recommended that you prefix the target name to ensure uniqueness, e.g. using the
        ///   form "Company_ApplicationName_www.example.com".
        /// </para>
        /// </remarks>
        public unsafe static void StoreCredential(string target, NetworkCredential credential, byte[] additionalEntropy = null)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (target.Length == 0)
                throw new ArgumentException(Properties.Resources.CredentialEmptyTargetError, nameof(target));
            if (credential == null)
                throw new ArgumentNullException(nameof(credential));

            fixed (char* userNamePtr = credential.UserName)
            fixed (char* targetPtr = target)
            {
                var c = new CREDENTIALW
                {
                    UserName = userNamePtr,
                    TargetName = targetPtr,
                    Persist = CRED_PERSIST.CRED_PERSIST_ENTERPRISE
                };
                byte[] encryptedPassword = EncryptPassword(credential.Password, additionalEntropy);
                c.CredentialBlob = (byte*)System.Runtime.InteropServices.Marshal.AllocHGlobal(encryptedPassword.Length);
                try
                {
                    System.Runtime.InteropServices.Marshal.Copy(encryptedPassword, 0, (IntPtr)c.CredentialBlob, encryptedPassword.Length);
                    c.CredentialBlobSize = (uint)encryptedPassword.Length;
                    c.Type = CRED_TYPE.CRED_TYPE_GENERIC;
                    if (!NativeMethods.CredWrite(c, 0))
                        throw new CredentialException(System.Runtime.InteropServices.Marshal.GetLastWin32Error());
                }
                finally
                {
                    System.Runtime.InteropServices.Marshal.FreeCoTaskMem((IntPtr)c.CredentialBlob);
                }
            }
        }

        /// <summary>
        /// Retrieves credentials for the specified target from the operating system's credential store for the current user.
        /// </summary>
        /// <param name="target">The target name for the credentials.</param>
        /// <param name="additionalEntropy">The same entropy value that was used when storing the credentials.</param>
        /// <returns>The credentials if they were found; otherwise, <see langword="null" />.</returns>
        /// <remarks>
        /// <para>
        ///   If the requested credential was not originally stored using the <see cref="CredentialDialog"/> class (but e.g. by 
        ///   another application), the password may not be decoded correctly.
        /// </para>
        /// <para>
        ///   This function does not check the application instance credential cache for the credentials; for that you can use
        ///   the <see cref="RetrieveCredentialFromApplicationInstanceCache"/> function.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="target"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException"><paramref name="target"/> is an empty string ("").</exception>
        /// <exception cref="CredentialException">An error occurred retrieving the credentials.</exception>
        public unsafe static NetworkCredential RetrieveCredential(string target, byte[] additionalEntropy = null)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (target.Length == 0)
                throw new ArgumentException(Properties.Resources.CredentialEmptyTargetError, nameof(target));

            NetworkCredential cred = RetrieveCredentialFromApplicationInstanceCache(target);
            if (cred != null)
                return cred;

            var result = NativeMethods.CredRead(target, (uint)CRED_TYPE.CRED_TYPE_GENERIC, 0, out var credential);
            int error = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
            if (result)
            {
                try
                {
                    CREDENTIALW c = (CREDENTIALW)System.Runtime.InteropServices.Marshal.PtrToStructure(new IntPtr(credential), typeof(CREDENTIALW));
                    byte[] encryptedPassword = new byte[c.CredentialBlobSize];
                    System.Runtime.InteropServices.Marshal.Copy((IntPtr)c.CredentialBlob, encryptedPassword, 0, encryptedPassword.Length);
                    cred = new NetworkCredential(c.UserName.ToString(), DecryptPassword(encryptedPassword, additionalEntropy));
                }
                finally
                {
                    NativeMethods.CredFree(credential);
                }
                return cred;
            }
            else
            {
                return error == (int)WIN32_ERROR.ERROR_NOT_FOUND ? null : throw new CredentialException(error);
            }
        }

        /// <summary>
        /// Tries to get the credentials for the specified target from the application instance credential cache.
        /// </summary>
        /// <param name="target">The target for the credentials, typically a server name.</param>
        /// <returns>The credentials that were found in the application instance cache; otherwise, <see langword="null" />.</returns>
        /// <remarks>
        /// <para>
        ///   This function will only check the the application instance credential cache; the operating system's credential store
        ///   is not checked. To retrieve credentials from the operating system's store, use <see cref="RetrieveCredential"/>.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="target"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException"><paramref name="target"/> is an empty string ("").</exception>
        public static NetworkCredential RetrieveCredentialFromApplicationInstanceCache(string target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (target.Length == 0)
                throw new ArgumentException(Properties.Resources.CredentialEmptyTargetError, nameof(target));

            lock (_applicationInstanceCredentialCache)
            {
                if (_applicationInstanceCredentialCache.TryGetValue(target, out NetworkCredential cred))
                {
                    return cred;
                }
            }
            return null;
        }

        /// <summary>
        /// Deletes the credentials for the specified target.
        /// </summary>
        /// <param name="target">The name of the target for which to delete the credentials.</param>
        /// <returns><see langword="true"/> if the credential was deleted from either the application instance cache or
        /// the operating system's store; <see langword="false"/> if no credentials for the specified target could be found
        /// in either store.</returns>
        /// <remarks>
        /// <para>
        ///   The credentials for the specified target will be removed from the application instance credential cache
        ///   and the operating system's credential store.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="target"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException"><paramref name="target"/> is an empty string ("").</exception>
        /// <exception cref="CredentialException">An error occurred deleting the credentials from the operating system's credential store.</exception>
        public static bool DeleteCredential(string target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (target.Length == 0)
                throw new ArgumentException(Properties.Resources.CredentialEmptyTargetError, nameof(target));

            bool found = false;
            lock (_applicationInstanceCredentialCache)
            {
                found = _applicationInstanceCredentialCache.Remove(target);
            }

            if (NativeMethods.CredDelete(target, (uint)CRED_TYPE.CRED_TYPE_GENERIC, 0))
            {
                found = true;
            }
            else
            {
                int error = Marshal.GetLastWin32Error();
                if (error != (int)WIN32_ERROR.ERROR_NOT_FOUND)
                    throw new CredentialException(error);
            }
            return found;
        }

        /// <summary>
        /// Raises the <see cref="UserNameChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> containing data for the event.</param>
        protected virtual void OnUserNameChanged(EventArgs e)
        {
            UserNameChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="PasswordChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> containing data for the event.</param>
        protected virtual void OnPasswordChanged(EventArgs e)
        {
            PasswordChanged?.Invoke(this, e);
        }

        private unsafe bool PromptForCredentialsCredUI(HWND owner, bool storedCredentials)
        {
            CREDUI_INFOW info = CreateCredUIInfo(owner, true);
            CREDUI_FLAGS flags = CREDUI_FLAGS.CREDUI_FLAGS_GENERIC_CREDENTIALS | CREDUI_FLAGS.CREDUI_FLAGS_DO_NOT_PERSIST | CREDUI_FLAGS.CREDUI_FLAGS_ALWAYS_SHOW_UI;
            if (ShowSaveCheckBox)
                flags |= CREDUI_FLAGS.CREDUI_FLAGS_SHOW_SAVE_CHECK_BOX;

            Span<char> userSpan = stackalloc char[(int)NativeMethods.CREDUI_MAX_USERNAME_LENGTH];
            UserName.AsSpan().CopyTo(userSpan);
            Span<char> pwSpan = stackalloc char[NativeMethods.CREDUI_MAX_PASSWORD_LENGTH];
            Password.AsSpan().CopyTo(pwSpan);
            WIN32_ERROR result;
            fixed (BOOL* b = &_isSaveChecked)
                result = (WIN32_ERROR)NativeMethods.CredUIPromptForCredentials(info, Target, ref Unsafe.AsRef<SecHandle>((void*)0), 0, ref userSpan, NativeMethods.CREDUI_MAX_USERNAME_LENGTH, ref pwSpan, NativeMethods.CREDUI_MAX_PASSWORD_LENGTH, b, flags);

            switch (result)
            {
                case WIN32_ERROR.NO_ERROR:
                    UserName = userSpan.ToCleanString();
                    Password = pwSpan.ToCleanString();
                    if (ShowSaveCheckBox)
                    {
                        _confirmTarget = Target;
                        // If the credential was stored previously but the user has now cleared the save checkbox,
                        // we want to delete the credential.
                        if (storedCredentials && !IsSaveChecked)
                            DeleteCredential(Target);
                    }
                    return true;
                case WIN32_ERROR.ERROR_CANCELLED:
                    return false;
                default:
                    throw new CredentialException((int)result);
            }
        }

        private unsafe bool PromptForCredentialsCredUIWin(HWND owner, bool storedCredentials)
        {
            CREDUI_INFOW info = CreateCredUIInfo(owner, false);
            CREDUIWIN_FLAGS flags = CREDUIWIN_FLAGS.CREDUIWIN_GENERIC;
            if (ShowSaveCheckBox)
                flags |= CREDUIWIN_FLAGS.CREDUIWIN_CHECKBOX;

            IntPtr inBuffer = IntPtr.Zero;
            IntPtr outBuffer = IntPtr.Zero;
            try
            {
                uint inBufferSize = 0;
                if (UserName.Length > 0)
                {
                    Span<char> userSpan = stackalloc char[(int)NativeMethods.CREDUI_MAX_USERNAME_LENGTH];
                    UserName.AsSpan().CopyTo(userSpan);
                    Span<char> pwSpan = stackalloc char[NativeMethods.CREDUI_MAX_PASSWORD_LENGTH];
                    Password.AsSpan().CopyTo(pwSpan);
                    fixed (char* user = userSpan)
                    fixed (char* pw = pwSpan)
                    fixed (BOOL* b = &_isSaveChecked)
                    {

                        NativeMethods.CredPackAuthenticationBuffer(0, user, pw, (byte*)0, ref inBufferSize);
                        if (inBufferSize > 0)
                        {
                            inBuffer = Marshal.AllocCoTaskMem((int)inBufferSize);
                            if (!NativeMethods.CredPackAuthenticationBuffer(0, user, pw, (byte*)inBuffer, ref inBufferSize))
                                throw new CredentialException(Marshal.GetLastWin32Error());
                        }
                    }
                }

                uint outBufferSize;
                uint package = 0;
                WIN32_ERROR result;
                fixed (BOOL* b = &_isSaveChecked)
                {
                    result = (WIN32_ERROR)NativeMethods.CredUIPromptForWindowsCredentials(info, 0, ref package, (void*)inBuffer, inBufferSize, out var poutBuffer, out outBufferSize, b, flags);
                    outBuffer = (IntPtr)poutBuffer;
                }

                switch (result)
                {
                    case WIN32_ERROR.NO_ERROR:
                        Span<char> userSpan = stackalloc char[(int)NativeMethods.CREDUI_MAX_USERNAME_LENGTH];
                        UserName.AsSpan().CopyTo(userSpan);
                        Span<char> pwSpan = stackalloc char[NativeMethods.CREDUI_MAX_PASSWORD_LENGTH];
                        Password.AsSpan().CopyTo(pwSpan);
                        uint userNameSize = (uint)userSpan.Length;
                        uint passwordSize = (uint)pwSpan.Length;
                        uint domainSize = 0;
                        BOOL res;

                        fixed (char* user = userSpan)
                        fixed (char* pw = pwSpan)
                            res = NativeMethods.CredUnPackAuthenticationBuffer(0, (void*)outBuffer, outBufferSize, user, ref userNameSize, null, &domainSize, pw, ref passwordSize);

                        if (!res)
                            throw new CredentialException(Marshal.GetLastWin32Error());
                        UserName = userSpan.ToCleanString();
                        Password = pwSpan.ToCleanString();
                        if (ShowSaveCheckBox)
                        {
                            _confirmTarget = Target;
                            // If the credential was stored previously but the user has now cleared the save checkbox,
                            // we want to delete the credential.
                            if (storedCredentials && !IsSaveChecked)
                                DeleteCredential(Target);
                        }
                        return true;
                    case WIN32_ERROR.ERROR_CANCELLED:
                        return false;
                    default:
                        throw new CredentialException((int)result);
                }
            }
            finally
            {
                if (inBuffer != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(inBuffer);
                if (outBuffer != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(outBuffer);
            }
        }

        private unsafe CREDUI_INFOW CreateCredUIInfo(HWND owner, bool downlevelText)
        {
            var info = new CREDUI_INFOW
            {
                hwndParent = owner
            };
            info.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(info);
            if (downlevelText)
            {
                fixed (char* pWindowTitle = WindowTitle)
                    info.pszCaptionText = pWindowTitle;
                switch (DownlevelTextMode)
                {
                    case DownlevelTextMode.MainInstructionAndContent:
                        string text = MainInstruction.Length == 0
                                    ? Content
                                    : Content.Length == 0 ? MainInstruction : MainInstruction + Environment.NewLine + Environment.NewLine + Content;

                        fixed (char* pText = text)
                            info.pszMessageText = pText;
                        break;
                    case DownlevelTextMode.MainInstructionOnly:
                        fixed (char* pMainInstruction = MainInstruction)
                            info.pszMessageText = pMainInstruction;
                        break;
                    case DownlevelTextMode.ContentOnly:
                        fixed (char* pContent = Content)
                            info.pszMessageText = pContent;
                        break;
                }
            }
            else
            {
                // Vista and later don't use the window title.
                fixed (char* pContent = Content)
                fixed (char* pMainInstruction = MainInstruction)
                {
                    info.pszMessageText = pContent;
                    info.pszCaptionText = pMainInstruction;
                }
            }
            return info;
        }

        private bool RetrieveCredentials()
        {
            NetworkCredential credential = RetrieveCredential(Target, AdditionalEntropy);
            if (credential != null)
            {
                UserName = credential.UserName;
                Password = credential.Password;
                return true;
            }
            return false;
        }

        private static byte[] EncryptPassword(string password, byte[] additionalEntropy)
        {
            byte[] protectedData = System.Security.Cryptography.ProtectedData.Protect(System.Text.Encoding.UTF8.GetBytes(password), additionalEntropy, System.Security.Cryptography.DataProtectionScope.CurrentUser);
            return protectedData;
        }

        private static string DecryptPassword(byte[] encrypted, byte[] additionalEntropy)
        {
            try
            {
                return Encoding.UTF8.GetString(System.Security.Cryptography.ProtectedData.Unprotect(encrypted, additionalEntropy, System.Security.Cryptography.DataProtectionScope.CurrentUser));
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                return string.Empty;
            }
        }

        private bool RetrieveCredentialsFromApplicationInstanceCache()
        {
            if (UseApplicationInstanceCredentialCache)
            {
                NetworkCredential credential = RetrieveCredentialFromApplicationInstanceCache(Target);
                if (credential != null)
                {
                    UserName = credential.UserName;
                    Password = credential.Password;
                    return true;
                }
            }
            return false;
        }
    }
}
