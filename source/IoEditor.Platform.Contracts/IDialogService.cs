namespace IoEditor.Platform;

public interface IDialogService
{
    Task ShowErrorAsync(string message, string title = "Error");
    Task ShowInfoAsync(string message, string title = "");
    Task<bool> ConfirmAsync(string message, string title = "Confirm");
}
