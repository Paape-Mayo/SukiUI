using System.Threading.Tasks;

namespace SukiUI.Dialogs
{
    public interface ISukiDialogManager
    {
        /// <summary>
        /// Raised whenever a dialog is shown.
        /// </summary>
        event SukiDialogManagerEventHandler? OnDialogShown;

        /// <summary>
        /// Raised whenever a dialog is dismissed.
        /// </summary>
        event SukiDialogManagerEventHandler? OnDialogDismissed;

        /// <summary>
        /// Attempts to show a dialog - If one is already shown this will simply return false and not show the dialog.
        /// </summary>
        bool TryShowDialog(ISukiDialog dialog);

        /// <summary>
        /// Attempts to dismiss a dialog - If the specified dialog has already been dismissed, this will return false.
        /// </summary>
        bool TryDismissDialog(ISukiDialog dialog);

        /// <summary>
        /// Dismisses the currently active dialog, if there is one.
        /// </summary>
        void DismissDialog();

        /// <summary>
        /// Shows <paramref name="dialog"/> and returns a task that completes
        /// once it is dismissed. If another dialog is already showing the
        /// task completes immediately without doing anything.
        /// </summary>
        Task ShowDialogAsync(ISukiDialog dialog);
    }
}