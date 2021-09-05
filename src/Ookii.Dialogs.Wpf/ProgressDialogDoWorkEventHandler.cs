namespace Ookii.Dialogs.Wpf
{
    /// <summary>
    /// Represents the method that will handle the <see cref="ProgressDialog.DoWork"/>
    /// event. This class cannot be inherited.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A <see cref="ProgressDialogDoWorkEventArgs"/> that contains the event data.</param>
    public delegate void ProgressDialogDoWorkEventHandler(object sender, ProgressDialogDoWorkEventArgs e);
}
