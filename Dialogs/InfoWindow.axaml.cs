﻿using Avalonia.Controls;
using Avalonia.Input;

namespace HellDivers2OneKeyStratagem;

public partial class InfoWindow : Window
{
    private readonly MainWindow _mainWindow;

    public InfoWindow()
    {
        _mainWindow = null!;
    }

    public InfoWindow(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;

        DataContext = MainViewModel.Instance;

        M.IsLoading = true;

        try
        {
            InitializeComponent();
        }
        finally
        {
            M.IsLoading = false;
        }
    }

    private MainViewModel M => (MainViewModel)DataContext!;

    public void SetInfo(string info)
    {
        InfoLabel.Content = info;
    }

    private void InfoWindow_OnClosed(object? sender, EventArgs e)
    {
        _mainWindow.NotifyInfoWindowClosed();
    }

    private void Window_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }
}