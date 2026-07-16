using System.Threading.Tasks;
using SukiUI.Helpers;

namespace SukiUI.Dialogs
{
    public class SukiDialogManager : ISukiDialogManager
    {
        public event SukiDialogManagerEventHandler? OnDialogShown;
        public event SukiDialogManagerEventHandler? OnDialogDismissed;

        private ISukiDialog? _activeDialog;

        public bool TryShowDialog(ISukiDialog dialog)
        {
            if (_activeDialog != null) return false;
            _activeDialog = dialog;
            OnDialogShown?.Invoke(this, new SukiDialogManagerEventArgs(_activeDialog));
            return true;
        }

        public bool TryDismissDialog(ISukiDialog dialog)
        {
            if (_activeDialog == null || _activeDialog != dialog)
                return false;

            var dismissedDialog = _activeDialog;
            _activeDialog = null;
            OnDialogDismissed?.Invoke(this, new SukiDialogManagerEventArgs(dismissedDialog));
            dismissedDialog.OnDismissed?.Invoke(dismissedDialog);

            return true;
        }

        public void DismissDialog()
        {
            if (_activeDialog == null)
                return;

            var dismissedDialog = _activeDialog;
            _activeDialog = null;
            OnDialogDismissed?.Invoke(this, new SukiDialogManagerEventArgs(dismissedDialog));
            dismissedDialog.OnDismissed?.Invoke(dismissedDialog);
        }

        public Task ShowDialogAsync(ISukiDialog dialog)
        {
            // netstandard2.0 has no parameterless TaskCompletionSource, so use the
            // generic form and ignore the bool result -- callers await the Task itself.
            var tcs = new TaskCompletionSource<bool>();

            void OnDismissed(object sender, SukiDialogManagerEventArgs args)
            {
                if (!ReferenceEquals(args.Dialog, dialog)) return;
                OnDialogDismissed -= OnDismissed;
                tcs.TrySetResult(true);
            }

            OnDialogDismissed += OnDismissed;

            if (!TryShowDialog(dialog))
            {
                // Another dialog is already showing -- detach and complete immediately.
                OnDialogDismissed -= OnDismissed;
                tcs.TrySetResult(false);
            }

            return tcs.Task;
        }
    }
}