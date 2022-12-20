namespace GreatOptionTrader.Commands.ConnectorCommands;

internal class DisconnectCommand : Base.Command
{
    public override bool CanExecute(object? parameter) => Services.Get.Connector is not null;
    public override void Execute(object? parameter) => Services.Get.Connector.Disconnect();
}
