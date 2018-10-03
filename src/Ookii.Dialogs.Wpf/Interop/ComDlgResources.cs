// Copyright (c) Sven Groot (Ookii.org) 2006
// See license.txt for details
using System;
using System.Collections.Generic;
using System.Text;

namespace Ookii.Dialogs.Wpf.Interop
{
    static class ComDlgResources
    {
        public enum ComDlgResourceId
        {
            OpenButton = 370,
            Open = 384,
            FileNotFound = 391,
            CreatePrompt = 402,
            ReadOnly = 427,
            ConfirmSaveAs = 435
        }

        private static Win32Resources _resources = new Win32Resources("comdlg32.dll");

        public static string LoadString(ComDlgResourceId id)
        {
            return _resources.LoadString((uint)id);
        }

        public static string FormatString(ComDlgResourceId id, params string[] args)
        {
            return _resources.FormatString((uint)id, args);
        }
    }
}
