﻿using System.Windows;

namespace GUI.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainView : Window
{
    public MainView()
    {
        InitializeComponent();

#if DEBUG
        this.Topmost = false;
#else
        this.Topmost = false;
#endif
    }
}
