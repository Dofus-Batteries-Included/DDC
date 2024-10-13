using Core.DataCenter.Metadata.World;

namespace DDC.Extractor.Models;

public class MapPosition
{
    public MapPosition(MapPositions data)
    {
        MapId = data.id;
        PosX = data.posX;
        PosY = data.posY;
        NameId = data.nameId;
        SubAreaId = data.subAreaId;
        WorldMap = data.worldMap;
        Outdoor = data.outdoor;
        ShowNameOnFingerpost = data.showNameOnFingerpost;
        FightSnapshot = data.fightSnapshot;
        RoleplaySnapshot = data.roleplaySnapshot;
        HasPriorityOnWorldMap = data.hasPriorityOnWorldmap;
        HasPublicPaddock = data.hasPublicPaddock;
        IsUnderWater = data.isUnderWater;
        IsTransition = data.isTransition;
        MapHasTemplate = data.mapHasTemplate;
        TactialModeTemplateId = data.tacticalModeTemplateId;
        AllowAggression = data.allowAggression;
        AllowChallenge = data.allowChallenge;
        AllowExchanges = data.allowExchanges;
        AllowFightChallenges = data.allowFightChallenges;
        AllowHumanVendor = data.allowHumanVendor;
        AllowMonsterAggression = data.allowMonsterAggression;
        AllowMonsterFight = data.allowMonsterFight;
        AllowMonsterRespawn = data.allowMonsterRespawn;
        AllowMount = data.allowMount;
        AllowObjectDisposal = data.allowObjectDisposal;
        AllowPrism = data.allowPrism;
        AllowPvp1V1 = data.allowPvp1V1;
        AllowPvp3V3 = data.allowPvp3V3;
        AllowSoulCapture = data.allowSoulCapture;
        AllowSoulSummon = data.allowSoulSummon;
        AllowTavernRegen = data.allowTavernRegen;
        AllowTaxCollector = data.allowTaxCollector;
        AllowTeleportEverywhere = data.allowTeleportEverywhere;
        AllowTeleportFrom = data.allowTeleportFrom;
        AllowTeleportTo = data.allowTeleportTo;
        AllowTombMode = data.allowTombMode;
    }

    public long MapId { get; init; }
    public int PosX { get; init; }
    public int PosY { get; init; }
    public int NameId { get; init; }
    public int SubAreaId { get; init; }
    public int WorldMap { get; init; }
    public bool Outdoor { get; init; }
    public bool ShowNameOnFingerpost { get; init; }
    public string FightSnapshot { get; init; } = "";
    public string RoleplaySnapshot { get; init; } = "";
    public bool HasPriorityOnWorldMap { get; init; }
    public bool HasPublicPaddock { get; init; }
    public bool IsUnderWater { get; init; }
    public bool IsTransition { get; init; }
    public bool MapHasTemplate { get; init; }
    public int TactialModeTemplateId { get; init; }
    public bool AllowAggression { get; init; }
    public bool AllowChallenge { get; init; }
    public bool AllowExchanges { get; init; }
    public bool AllowFightChallenges { get; init; }
    public bool AllowHumanVendor { get; init; }
    public bool AllowMonsterAggression { get; init; }
    public bool AllowMonsterFight { get; init; }
    public bool AllowMonsterRespawn { get; init; }
    public bool AllowMount { get; init; }
    public bool AllowObjectDisposal { get; init; }
    public bool AllowPrism { get; init; }
    public bool AllowPvp1V1 { get; init; }
    public bool AllowPvp3V3 { get; init; }
    public bool AllowSoulCapture { get; init; }
    public bool AllowSoulSummon { get; init; }
    public bool AllowTavernRegen { get; init; }
    public bool AllowTaxCollector { get; init; }
    public bool AllowTeleportEverywhere { get; init; }
    public bool AllowTeleportFrom { get; init; }
    public bool AllowTeleportTo { get; init; }
    public bool AllowTombMode { get; init; }
}
