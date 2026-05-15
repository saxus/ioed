using System.ComponentModel;
using System.Windows.Input;

namespace IoEditor.Desktop.ViewModels.Panels;

/// <summary>Common base for all panels shown in the shell's nav rail and content area.</summary>
internal abstract class EditorPanelViewModelBase : INotifyPropertyChanged
{
    private bool _isActive;

    /// <summary>True when this panel is the currently selected one in the shell.</summary>
    public bool IsActive
    {
        get => _isActive;
        internal set
        {
            if (_isActive == value)
            {
                return;
            }

            _isActive = value;
            RaisePropertyChanged(nameof(IsActive));
        }
    }

    /// <summary>Label used for nav rail tooltip and window title contributions.</summary>
    public abstract string Title { get; }

    /// <summary>Command that selects this panel; injected by the shell when added to OpenPanels.</summary>
    public ICommand? SelectCommand { get; internal set; }

    /// <summary>When false the shell does not render a close affordance for this panel.</summary>
    public virtual bool IsClosable => true;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void RaisePropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
