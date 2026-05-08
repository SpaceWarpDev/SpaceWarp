namespace SpaceWarp2.API.Backend.SaveGameManager;

public interface ISaveGameApi
{
    public static ISaveGameApi Instance;

    public void UpdateCampaignSaveData();
    public void PopulateSaveData(PluginSaveData saveData);
    public void PopulateCampaignData(PluginSaveData saveData);
}