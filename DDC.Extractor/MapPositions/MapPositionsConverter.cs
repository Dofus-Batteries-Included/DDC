using DDC.Extractor.Abstractions;
using DDC.Extractor.MapPositions.Models;

namespace DDC.Extractor.MapPositions;

public class MapPositionsConverter : IConverter<Core.DataCenter.Metadata.World.MapPositions, MapPosition>
{
    public MapPosition Convert(Core.DataCenter.Metadata.World.MapPositions data) =>
        new()
        {
            MapId = data.id,
            PosX = data.posX,
            PosY = data.posY,
            NameId = data.nameId,
            SubAreaId = data.subAreaId,
            WorldMap = data.worldMap,
            Outdoor = data.outdoor,
            ShowNameOnFingerpost = data.showNameOnFingerpost,
            FightSnapshot = data.fightSnapshot,
            RoleplaySnapshot = data.roleplaySnapshot,
            HasPriorityOnWorldMap = data.hasPriorityOnWorldmap,
            HasPublicPaddock = data.hasPublicPaddock,
            IsUnderWater = data.isUnderWater,
            IsTransition = data.isTransition,
            MapHasTemplate = data.mapHasTemplate,
            TactialModeTemplateId = data.tacticalModeTemplateId,
            AllowAggression = data.allowAggression,
            AllowChallenge = data.allowChallenge,
            AllowExchanges = data.allowExchanges,
            AllowFightChallenges = data.allowFightChallenges,
            AllowHumanVendor = data.allowHumanVendor,
            AllowMonsterAggression = data.allowMonsterAggression,
            AllowMonsterFight = data.allowMonsterFight,
            AllowMonsterRespawn = data.allowMonsterRespawn,
            AllowMount = data.allowMount,
            AllowObjectDisposal = data.allowObjectDisposal,
            AllowPrism = data.allowPrism,
            AllowPvp1V1 = data.allowPvp1V1,
            AllowPvp3V3 = data.allowPvp3V3,
            AllowSoulCapture = data.allowSoulCapture,
            AllowSoulSummon = data.allowSoulSummon,
            AllowTavernRegen = data.allowTavernRegen,
            AllowTaxCollector = data.allowTaxCollector,
            AllowTeleportEverywhere = data.allowTeleportEverywhere,
            AllowTeleportFrom = data.allowTeleportFrom,
            AllowTeleportTo = data.allowTeleportTo,
            AllowTombMode = data.allowTombMode
        };
}
