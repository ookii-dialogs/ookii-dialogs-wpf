using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ookii.Dialogs.Wpf
{
    /// <summary>
    /// An enumeration that displays how the text in the <see cref="CredentialDialog.MainInstruction"/> and <see cref="CredentialDialog.Content"/>
    /// properties is displayed on a credential dialog in Windows XP.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   Windows XP does not support the distinct visual style of the main instruction, so there is no visual difference between the
    ///   text of the <see cref="CredentialDialog.MainInstruction"/> and <see cref="CredentialDialog.Content"/> properties. Depending
    ///   on the scenario, you may wish to hide either the main instruction or the content text.
    /// </para>
    /// </remarks>
    public enum DownlevelTextMode
    {
        /// <summary>
        /// The text of the <see cref="CredentialDialog.MainInstruction"/> and <see cref="CredentialDialog.Content"/> properties is
        /// concatenated together, separated by an empty line.
        /// </summary>
        MainInstructionAndContent,
        /// <summary>
        /// Only the text of the <see cref="CredentialDialog.MainInstruction"/> property is shown.
        /// </summary>
        MainInstructionOnly,
        /// <summary>
        /// Only the text of the <see cref="CredentialDialog.Content"/> property is shown.
        /// </summary>
        ContentOnly
    }
}
