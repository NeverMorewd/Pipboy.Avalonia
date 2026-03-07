using System;
using Avalonia.Controls;

namespace Pipboy.Avalonia.Demo.Pages;

public partial class DateTimePage : UserControl
{
    public DateTimePage()
    {
        InitializeComponent();
        var today = DateTimeOffset.Now;
        DatePicker1.SelectedDate = today;
        DatePicker2.SelectedDate = today;
    }
}
