using System;

namespace SpaceWarp2.API.Backend.SaveGameManager;

public enum PersistenceKind
{
    PerSave,
    PerCampaign
}

public static class PersistenceKindExtensions
{
    public static string ToPersistenceString(this PersistenceKind persistenceKind) =>
        persistenceKind switch
        {
            PersistenceKind.PerSave => "per-save",
            PersistenceKind.PerCampaign => "per-campaign",
            _ => throw new ArgumentOutOfRangeException(nameof(persistenceKind), persistenceKind, null)
        };
}