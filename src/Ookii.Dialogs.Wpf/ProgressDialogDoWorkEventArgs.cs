using System.ComponentModel;
using System.Threading;

namespace Ookii.Dialogs.Wpf
{
    /// <summary>
    /// Provides data for the System.ComponentModel.BackgroundWorker.DoWork event handler.
    /// </summary>
    public class ProgressDialogDoWorkEventArgs : DoWorkEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressDialogDoWorkEventArgs"/> class.
        /// </summary>
        /// <param name="argument">Specifies an argument for an asynchronous operation.</param>
        /// <param name="cancellationToken">Specifies a cancellation token for an asynchronous operation.</param>
        public ProgressDialogDoWorkEventArgs(object argument, CancellationToken cancellationToken)
            : base(argument)
        {
            CancellationToken = cancellationToken;
        }

        /// <summary>
        /// Gets a value that represents the CancellationToken of an asynchronous operation.
        /// </summary>
        public CancellationToken CancellationToken { get; }
    }
}
