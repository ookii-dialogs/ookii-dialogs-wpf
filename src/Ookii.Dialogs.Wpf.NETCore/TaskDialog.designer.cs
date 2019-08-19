namespace Ookii.Dialogs.Wpf
{
    partial class TaskDialog
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
                    {
                        components.Dispose();
                        components = null;
                    }
                    if( _buttons != null )
                    {
                        foreach( TaskDialogButton button in _buttons )
                        {
                            button.Dispose();
                        }
                        _buttons.Clear();
                    }
                    if( _radioButtons != null )
                    {
                        foreach( TaskDialogRadioButton radioButton in _radioButtons )
                        {
                            radioButton.Dispose();
                        }
                        _radioButtons.Clear();
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
            components = new System.ComponentModel.Container();
        }

        #endregion
    }
}
