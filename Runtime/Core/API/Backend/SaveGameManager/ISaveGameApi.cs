namespace SpaceWarp.API.Backend.SaveGameManager;

public interface ISaveGameApi
{
    public static ISaveGameApi Instance;

    public void UpdateCampaignSaveData();
}