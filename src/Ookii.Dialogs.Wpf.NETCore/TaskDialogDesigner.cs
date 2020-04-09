// Copyright (c) Sven Groot (Ookii.org) 2009
// BSD license; see LICENSE for details.
using System;
using System.ComponentModel.Design;

namespace Ookii.Dialogs.Wpf
{
    class TaskDialogDesigner : ComponentDesigner
    {
        public override DesignerVerbCollection Verbs
        {
            get
            {
                DesignerVerbCollection verbs = new DesignerVerbCollection
                {
                    new DesignerVerb(Properties.Resources.Preview, new EventHandler(Preview))
                };
                return verbs;
            }
        }

        private void Preview(object sender, EventArgs e)
        {
            ((TaskDialog)Component).ShowDialog();
        }
    }
}
