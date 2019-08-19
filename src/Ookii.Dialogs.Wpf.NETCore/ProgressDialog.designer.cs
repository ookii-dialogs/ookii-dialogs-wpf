namespace Ookii.Dialogs.Wpf
{
    partial class ProgressDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><see langword="true" /> if managed resources should be disposed; otherwise, <see langword="false" />.</param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if( disposing )
                {
                    if( components != null )
                        components.Dispose();
                    if( _currentAnimationModuleHandle != null )
                    {
                        _currentAnimationModuleHandle.Dispose();
                        _currentAnimationModuleHandle = null;
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._backgroundWorker = new System.ComponentModel.BackgroundWorker();
            // 
            // _backgroundWorker
            // 
            this._backgroundWorker.WorkerReportsProgress = true;
            this._backgroundWorker.WorkerSupportsCancellation = true;
            this._backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this._backgroundWorker_DoWork);
            this._backgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this._backgroundWorker_RunWorkerCompleted);
            this._backgroundWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this._backgroundWorker_ProgressChanged);

        }

        #endregion

        private System.ComponentModel.BackgroundWorker _backgroundWorker;

    }
}
