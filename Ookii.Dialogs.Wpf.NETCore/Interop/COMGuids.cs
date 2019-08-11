using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Ookii.Dialogs.Wpf.Interop
{
    internal static class IIDGuid
    {
        internal const string IModalWindow = "b4db1657-70d7-485e-8e3e-6fcb5a5c1802";
        internal const string IFileDialog = "42f85136-db7e-439c-85f1-e4075d135fc8";
        internal const string IFileOpenDialog = "d57c7288-d4ad-4768-be02-9d969532d960";
        internal const string IFileSaveDialog = "84bccd23-5fde-4cdb-aea4-af64b83d78ab";
        internal const string IFileDialogEvents = "973510DB-7D7F-452B-8975-74A85828D354";
        internal const string IFileDialogControlEvents = "36116642-D713-4b97-9B83-7484A9D00433";
        internal const string IFileDialogCustomize = "e6fdd21a-163f-4975-9c8c-a69f1ba37034";
        internal const string IShellItem = "43826D1E-E718-42EE-BC55-A1E261C37BFE";
        internal const string IShellItemArray = "B63EA76D-1F85-456F-A19C-48159EFA858B";
        internal const string IKnownFolder = "38521333-6A87-46A7-AE10-0F16706816C3";
        internal const string IKnownFolderManager = "44BEAAEC-24F4-4E90-B3F0-23D258FBB146";
        internal const string IPropertyStore = "886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99";
        internal const string IProgressDialog = "EBBC7C04-315E-11d2-B62F-006097DF5BD4";
    }

    internal static class CLSIDGuid
    {
        internal const string FileOpenDialog = "DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7";
        internal const string FileSaveDialog = "C0B4E2F3-BA21-4773-8DBA-335EC946EB8B";
        internal const string KnownFolderManager = "4df0c730-df9d-4ae3-9153-aa6b82e9795a";
        internal const string ProgressDialog = "F8383852-FCD3-11d1-A6B9-006097DF5BD4";
    }

    internal static class KFIDGuid
    {
        internal const string ComputerFolder = "0AC0837C-BBF8-452A-850D-79D08E667CA7";
        internal const string Favorites = "1777F761-68AD-4D8A-87BD-30B759FA33DD";
        internal const string Documents = "FDD39AD0-238F-46AF-ADB4-6C85480369C7";
        internal const string Profile = "5E6C858F-0E22-4760-9AFE-EA3317B67173";
    }
}
