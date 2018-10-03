// Copyright (c) Sven Groot (Ookii.org) 2009
// See license.txt for details
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.ComponentModel;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf.Interop;
using System.Windows;

namespace Ookii.Dialogs.Wpf
{
    /// <summary>
    /// Prompts the user to select a location for saving a file.
    /// </summary>
    /// <remarks>
    /// This class will use the Vista style save file dialog if possible, and automatically fall back to the old-style 
    /// dialog on versions of Windows older than Vista.
    /// </remarks>
    /// <remarks>
    /// <para>
    ///   Windows Vista provides a new style of common file dialog, with several new features (both from
    ///   the user's and the programmers perspective).
    /// </para>
    /// <para>
    ///   This class will use the Vista-style file dialogs if possible, and automatically fall back to the old-style 
    ///   dialog on versions of Windows older than Vista. This class is aimed at applications that
    ///   target both Windows Vista and older versions of Windows, and therefore does not provide any
    ///   of the new APIs provided by Vista's file dialogs.
    /// </para>
    /// <para>
    ///   This class precisely duplicates the public interface of <see cref="SaveFileDialog"/> so you can just replace
    ///   any instances of <see cref="SaveFileDialog"/> with the <see cref="VistaSaveFileDialog"/> without any further changes
    ///   to your code.
    /// </para>
    /// </remarks>
    /// <threadsafety instance="false" static="true" />
    [Designer("System.Windows.Forms.Design.SaveFileDialogDesigner, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), Description("Prompts the user to open a file.")]
    public sealed class VistaSaveFileDialog : VistaFileDialog
    {
        /// <summary>
        /// Creates a new instance of <see cref="VistaSaveFileDialog" /> class.
        /// </summary>
        public VistaSaveFileDialog()
        {
            if( !IsVistaFileDialogSupported )
                DownlevelDialog = new SaveFileDialog();
        }

        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether the dialog box prompts the user for permission to create a file if the 
        /// user specifies a file that does not exist.
        /// </summary>
        /// <value>
        /// <see langword="true" /> if the dialog box prompts the user before creating a file if the user specifies a file name that does not exist; 
        /// <see langword="false" /> if the dialog box automatically creates the new file without prompting the user for permission. The default 
        /// value is <see langword="false" />.
        /// </value>
        [DefaultValue(false), Category("Behavior"), Description("A value indicating whether the dialog box prompts the user for permission to create a file if the user specifies a file that does not exist.")]
        public bool CreatePrompt
        {
            get
            {
                if( DownlevelDialog != null )
                    return ((SaveFileDialog)DownlevelDialog).CreatePrompt;
                return GetOption(NativeMethods.FOS.FOS_CREATEPROMPT);
            }
            set
            {
                if( DownlevelDialog != null )
                    ((SaveFileDialog)DownlevelDialog).CreatePrompt = value;
                else
                    SetOption(NativeMethods.FOS.FOS_CREATEPROMPT, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Save As dialog box displays a warning if the user 
        /// specifies a file name that already exists.
        /// </summary>
        /// <value>
        /// <see langword="true" /> if the dialog box prompts the user before overwriting an existing file if the user specifies a file 
        /// name that already exists; <see langword="false" /> if the dialog box automatically overwrites the existing file without 
        /// prompting the user for permission. The default value is <see langword="true" />.
        /// </value>
        [Category("Behavior"), DefaultValue(true), Description("A value indicating whether the Save As dialog box displays a warning if the user specifies a file name that already exists.")]
        public bool OverwritePrompt
        {
            get
            {
                if( DownlevelDialog != null )
                    return ((SaveFileDialog)DownlevelDialog).OverwritePrompt;
                return GetOption(NativeMethods.FOS.FOS_OVERWRITEPROMPT);
            }
            set
            {
                if( DownlevelDialog != null )
                    ((SaveFileDialog)DownlevelDialog).OverwritePrompt = value;
                else
                    SetOption(NativeMethods.FOS.FOS_OVERWRITEPROMPT, value);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Resets all properties to their default values.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            if( DownlevelDialog == null )
            {
                OverwritePrompt = true;
            }
        }

        /// <summary>
        /// Opens the file with read/write permission selected by the user.
        /// </summary>
        /// <returns>The read/write file selected by the user.</returns>
        /// <exception cref="System.ArgumentNullException">The file name is <see langword="null" />.</exception>
        public System.IO.Stream OpenFile()
        {
            if( DownlevelDialog != null )
                return ((SaveFileDialog)DownlevelDialog).OpenFile();
            else
            {
                string fileName = FileName;
                return new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Raises the <see cref="VistaFileDialog.FileOk" /> event.
        /// </summary>
        /// <param name="e">A <see cref="System.ComponentModel.CancelEventArgs" /> that contains the event data.</param>        
        protected override void OnFileOk(CancelEventArgs e)
        {
            // For reasons unknown, .Net puts the OFN_FILEMUSTEXIST and OFN_CREATEPROMPT flags on the save file dialog despite 
            // the fact that these flags only works on open file dialogs, and then prompts manually. Similarly, the 
            // FOS_CREATEPROMPT and FOS_FILEMUSTEXIST flags don't actually work on IFileSaveDialog, so we have to implement 
            // the prompt manually.
            if( DownlevelDialog == null )
            {
                if( CheckFileExists && !File.Exists(FileName) )
                {
                    PromptUser(ComDlgResources.FormatString(ComDlgResources.ComDlgResourceId.FileNotFound, Path.GetFileName(FileName)), MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
                    e.Cancel = true;
                    return;
                }
                if( CreatePrompt && !File.Exists(FileName) )
                {
                    if( !PromptUser(ComDlgResources.FormatString(ComDlgResources.ComDlgResourceId.CreatePrompt, Path.GetFileName(FileName)), MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No) )
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }
            base.OnFileOk(e);
        }

        #endregion

        #region Internal Methods

        internal override Ookii.Dialogs.Wpf.Interop.IFileDialog CreateFileDialog()
        {
            return new Ookii.Dialogs.Wpf.Interop.NativeFileSaveDialog();
        }

        #endregion

    }
}
