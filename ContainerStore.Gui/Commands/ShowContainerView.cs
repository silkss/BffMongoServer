﻿using ContainerStore.Data.Models;
using ContainerStore.Gui.Commands.Base;
using ContainerStore.Gui.Views;

namespace ContainerStore.Gui.Commands;

internal class ShowContainerView : Command
{
    public override bool CanExecute(object? parameter) => parameter is Container;

    public override void Execute(object? parameter)
    {
        if (parameter is Container container)
        {
            new ContainerView
            {
                Container = container,
                Owner = App.Current.MainWindow,
            }.Show();
        }
    }
}