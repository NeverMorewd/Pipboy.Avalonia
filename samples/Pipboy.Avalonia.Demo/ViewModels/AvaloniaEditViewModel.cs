using AvaloniaEdit;
using AvaloniaEdit.Editing;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Pipboy.Avalonia.Demo.ViewModels;

public partial class AvaloniaEditViewModel : ObservableObject
{
    [RelayCommand]
    private void CopyMouse(TextArea textArea)
    {
        ApplicationCommands.Copy.Execute(null, textArea);
    }

    [RelayCommand]
    private void CutMouse(TextArea textArea)
    {
        ApplicationCommands.Cut.Execute(null, textArea);
    }

    [RelayCommand]
    private void PasteMouse(TextArea textArea)
    {
        ApplicationCommands.Paste.Execute(null, textArea);
    }

    [RelayCommand]
    private void SelectAllMouse(TextArea textArea)
    {
        ApplicationCommands.SelectAll.Execute(null, textArea);
    }

    [RelayCommand]
    private void UndoMouse(TextArea textArea)
    {
        ApplicationCommands.Undo.Execute(null, textArea);
    }
}
