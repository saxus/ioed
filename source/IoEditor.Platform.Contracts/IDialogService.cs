namespace IoEditor.Platform;

public enum SaveConfirmResult { Save, Discard, Cancel }

public interface IDialogService
{
    Task ShowErrorAsync(string message, string title = "Error");
    Task ShowInfoAsync(string message, string title = "");
    Task<bool> ConfirmAsync(string message, string title = "Confirm");
    Task<SaveConfirmResult> ShowSaveConfirmAsync(string message, string title = "Unsaved changes");
}
