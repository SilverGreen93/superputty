/*
 * https://github.com/jimradford/superputty/blob/master/License.txt
 */

using System;
using System.Windows.Forms;
using SuperPutty;
using SuperPutty.Utils;

namespace SuperPuTTY.Scripting
{
    public static partial class Commands
    {
        /// <summary>prompt a user for input</summary>
        /// <param name="arg">The pre-parsed string to send</param>
        /// <returns>A string containing commands to send with variables replaced with a carriage return sent at the end</returns>
        internal static CommandData PromptHandler(string arg)
        {
            // frmPrompt rather than Microsoft.VisualBasic.Interaction.InputBox: that dragged in
            // the whole VB assembly for one dialog, and its box cannot be cancelled without
            // returning an empty string indistinguishable from one typed as such
            string result;
            using (frmPrompt promptDialog = new frmPrompt(arg))
            {
                result = promptDialog.ShowDialog() == DialogResult.OK ? promptDialog.GetResult() : string.Empty;
            }

            CommandData data = new CommandData(result, new KeyEventArgs(Keys.Enter), TimeSpan.FromMilliseconds(50));
            return data;
        }
    }
}
