 README.md |
|:---|

<div align="center">

<img src="assets/ookii-dialogs-wpf-nuget.png" alt="Ookii.Dialogs.Wpf" width="100" />

</div>

<h1 align="center">Ookii.Dialogs.Wpf</h1>
<div align="center">

A class library for WPF applications providing several common dialogs. Included are classes for task dialogs, credential dialog, progress dialog, and common file dialogs.

[![NuGet Version](http://img.shields.io/nuget/v/Ookii.Dialogs.Wpf.svg?style=flat)](https://www.nuget.org/packages/Ookii.Dialogs.Wpf) [![NuGet Downloads](https://img.shields.io/nuget/dt/Ookii.Dialogs.Wpf.svg)](https://www.nuget.org/packages/Ookii.Dialogs.Wpf) [![.NET](https://img.shields.io/badge/.NET%20-%3E%3D%205.0-512bd4)](https://dotnet.microsoft.com/download) [![.NET Core](https://img.shields.io/badge/.NET%20Core-%3E%3D%203.1-512bd4)](https://dotnet.microsoft.com/download) [![.NET Framework](https://img.shields.io/badge/.NET%20Framework-%3E%3D%204.5-512bd4)](https://dotnet.microsoft.com/download)
</div>

## Give a Star! :star:

If you like or are using this project please give it a star. Thanks!

## Getting started

Install the [Ookii.Dialogs.Wpf](https://www.nuget.org/packages/Ookii.Dialogs.Wpf/) package from NuGet:

```powershell
Install-Package Ookii.Dialogs.Wpf
```

The included sample application [`Ookii.Dialogs.Sample.Wpf`](sample/Ookii.Dialogs.Wpf.Sample/) demonstrate the dialogs for WPF. View the source of this application to see how to use the dialogs.

### Windows Forms compatibility

If you're looking to use these common dialogs on a Windows Forms application, check out [Ookii.Dialogs.WinForms](https://github.com/augustoproiete/ookii-dialogs-winforms).

## Included dialogs

### Task dialog

[Task dialogs](https://docs.microsoft.com/en-us/windows/desktop/Controls/task-dialogs-overview) are a new type of dialog first introduced in Windows Vista. They provide a superset of the message box functionality.

![A task dialog as it appears on Windows 10](assets/sample-task-dialog-win10.png)

![A task dialog with command links as it appears on Windows 10](assets/sample-task-dialog-command-links-win10.png)

The `Ookii.Dialogs.Wpf.TaskDialog` class provide access to the task dialog functionality. The `TaskDialog` class inherits from `System.ComponentModel.Component` and offers full support for the Component designer of Visual Studio.

The `TaskDialog` class requires Windows Vista or a later version of Windows. Windows XP is not supported. Note that it is safe to instantiate the `TaskDialog` class and set any of its properties; only when the dialog is shown will a `NotSupportedException` be thrown on unsupported operating systems.

### Progress dialog

Progress dialogs are a common dialog to show progress during operations that may take a long time. They are used extensively in the Windows shell, and an API has been available since Windows 2000.

![A progress dialog as it appears on Windows 10](assets/sample-progress-dialog-win10.png)

The `Ookii.Dialogs.Wpf.ProgressDialog` class provide a wrapper for the Windows progress dialog API. The `ProgressDialog` class inherits from `System.ComponentModel.Component` and offers full support for the Component designer of Visual Studio. The `ProgressDialog` class resembles the `System.ComponentModel.BackgroundWorker` class and can be used in much the same way as that class.

The progress dialog's behaviour of the `ShowDialog` function is slightly different than that of other .NET dialogs; It is recommended to use a non-modal dialog with the `Show` function.

The `ProgressDialog` class is supported on Windows XP and later versions of Windows. However, the progress dialog has a very different appearance on Windows Vista and later (the image above shows the Windows 10 version), so it is recommended to test on the operating systems your application is supported to see if it appears to your satisfaction.

When using Windows 7 or later, the `ProgressDialog` class automatically provides progress notification in the application's task bar button.

### Credential dialog

The `Ookii.Dialogs.Wpf.CredentialDialog` class provide wrappers for the `CredUI` functionality first introduced in Windows XP. This class provides functionality for saving and retrieving generic credentials, as well as displaying the credential UI dialog. This class does not support all functionality of `CredUI`; only generic credentials are supported, thing such as domain credentials or alternative authentication providers (e.g. smart cards or biometric devices) are not supported.

![A credential dialog as it appears on Windows 10](assets/sample-credential-dialog-win10.png)

The `CredentialDialog` class inherits from `System.ComponentModel.Component` and offers full support for the Component designer of Visual Studio.

On Windows XP, the `CredentialDialog` class will use the `CredUIPromptForCredentials` function to show the dialog; on Windows Vista and later, the `CredUIPromptForWindowsCredentials` function is used instead to show the new dialog introduced with Windows Vista. Because of the difference in appearance in the two versions (the image above shows the Vista version), it is recommended to test on both operating systems to see if it appears to your satisfaction.

### Vista-style common file dialogs

Windows Vista introduced a new style of common file dialogs. As of .NET 3.5 SP1, the Windows Forms `OpenFileDialog` and `SaveFileDialog` class will automatically use the new style under most circumstances; however, some settings (such as setting `ShowReadOnly` to `true`) still cause it to revert to the old dialog. The `FolderBrowserDialog` still uses the old style. In WPF, the `Microsoft.Win32.OpenFileDialog` and `SaveFileDialog` classes still use the old style dialogs, and a folder browser dialog is not provided at all.

![The Vista-style folder browser dialog as it appears on Windows 10](assets/sample-folderbrowser-dialog-win10.png)

The `Ookii.Dialogs.Wpf.VistaOpenFileDialog`, `Ookii.Dialogs.Wpf.VistaSaveFileDialog` and `Ookii.Dialogs.Wpf.VistaFolderBrowserDialog` classes provide these dialogs for WPF (note that in the case of the `OpenFileDialog` and `SaveFileDialog` it is recommended to use the built-in .NET classes unless you hit one of the scenarios where those classes use the old dialogs).

The classes have been designed to resemble the original WPF classes to make it easy to switch. When the classes are used on Windows XP, they will automatically fall back to the old style dialog; this is also true for the `VistaFolderBrowserDialog`; that class provides a complete implementation of a folder browser dialog for WPF, old as well as new style.

## .NET Core 3.1 & .NET 5 pre-requisites **before** Ookii.Dialogs.Wpf v3.1.0

> **NOTE: Starting with Ookii.Dialogs.Wpf v3.1.0 an app.manifest is no longer required when using in .NET 5 or .NET Core 3.1 apps**

Ookii Dialogs leverages the components and visual styles of the Windows Common Controls library (`comctl32.dll`), and WPF applications targeting .NET Core 3.1 and/or .NET 5 must add an [application manifest](https://docs.microsoft.com/en-us/windows/win32/sbscs/application-manifests) (`app.manifest`) to their projects with a reference to `Microsoft.Windows.Common-Controls`.

Without the application manifest, you'll get an error with a message similar to the below:

```
System.EntryPointNotFoundException: 'Unable to find an entry point named '...' in DLL 'comctl32.dll'.'
```

In Visual Studio, you can right click on your project and go to Add -> New Item... and select `Application Manifest File (Windows Only)`. That will create a template that you can customize and delete the parts you don't want and/or uncomment the parts that you want. The only required part for Ookii Dialogs to work is the `dependentAssembly` entry with the `Microsoft.Windows.Common-Controls` dependency.

```xml
<?xml version="1.0" encoding="utf-8"?>
<assembly manifestVersion="1.0" xmlns="urn:schemas-microsoft-com:asm.v1">
  <!-- (...) -->

  <!-- Enable themes for Windows common controls and dialogs (Windows XP and later) -->
  <dependency>
    <dependentAssembly>
      <assemblyIdentity
          type="win32"
          name="Microsoft.Windows.Common-Controls"
          version="6.0.0.0"
          processorArchitecture="*"
          publicKeyToken="6595b64144ccf1df"
          language="*"
        />
    </dependentAssembly>
  </dependency>
</assembly>
```

For more information, visit the following links:

- [EntryPointNotFoundException when instantiating a TaskDialog](https://github.com/augustoproiete/ookii-dialogs-wpf/issues/23)
- [WPF System.EntryPointNotFoundException: Unable to find an entry point named 'TaskDialogIndirect' in DLL 'comctl32.dll'](https://github.com/augustoproiete-repros/repro-wpf-net5-comctl32-entrypointnotfoundexception)
- [Calls to comctl32.dll succeed in .NET 4.8, but fail in .NET 5 with System.EntryPointNotFoundException](https://github.com/dotnet/wpf/issues/3815)

## Release History

Click on the [Releases](https://github.com/augustoproiete/ookii-dialogs-wpf/releases) tab on GitHub.

---

_Copyright &copy; 2009-2020 Ookii Dialogs Contributors - Provided under the [BSD 3-Clause "New" or "Revised" License](LICENSE)._
