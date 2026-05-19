using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.Collections.Generic;

namespace Pipboy.Avalonia.Demo.Pages;

public partial class ManagedFileChooserPage : UserControl
{
    public ManagedFileChooserPage()
    {
        InitializeComponent();
        OpenFileButton.Click += OpenFileDialog;
        SelectFolderButton.Click += SelectFolderDialog;
        SaveFileButton.Click += SaveFileDialog;
    }

    private async void OpenFileDialog(object? sender, RoutedEventArgs args)
    {
        var sp = GetStorageProvider();
        if (sp is null) return;
        _ = await sp.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "Open File",
            FileTypeFilter = GetFileTypes(),
            AllowMultiple = true,
        });
    }

    private async void SelectFolderDialog(object? sender, RoutedEventArgs args)
    {
        var sp = GetStorageProvider();
        if (sp is null) return;
        _ = await sp.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = "Select Folder",
            AllowMultiple = true,
        });
    }

    private async void SaveFileDialog(object? sender, RoutedEventArgs args)
    {
        var sp = GetStorageProvider();
        if (sp is null) return;
        _ = await sp.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            Title = "Save File",
        });
    }

    private IStorageProvider? GetStorageProvider()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        return topLevel?.StorageProvider;
    }

    private static List<FilePickerFileType>? GetFileTypes()
    {
        return
        [
            FilePickerFileTypes.All,
            FilePickerFileTypes.TextPlain
        ];
    }
}