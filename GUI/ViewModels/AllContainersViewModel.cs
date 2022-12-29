namespace GUI.ViewModels;
internal class AllContainersViewModel : Base.ViewModel
{
	public AllContainersViewModel()
	{
		Services.Get.StrategiesRequests.RefreshAsync();
	}

}
