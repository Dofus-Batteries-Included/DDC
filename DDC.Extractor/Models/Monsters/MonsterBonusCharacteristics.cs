namespace DDC.Extractor.Models.Monsters;

public class MonsterBonusCharacteristics
{
    public MonsterBonusCharacteristics(Core.DataCenter.Metadata.Monster.MonsterBonusCharacteristics characteristics)
    {
        LifePoints = characteristics.lifePoints;
        Strength = characteristics.strength;
        Wisdom = characteristics.wisdom;
        Chance = characteristics.chance;
        Agility = characteristics.agility;
        Intelligence = characteristics.intelligence;
        EarthResistance = characteristics.earthResistance;
        AirResistance = characteristics.airResistance;
        FireResistance = characteristics.fireResistance;
        WaterResistance = characteristics.waterResistance;
        NeutralResistance = characteristics.neutralResistance;
        TackleEvade = characteristics.tackleEvade;
        TackleBlock = characteristics.tackleBlock;
        BonusEarthDamage = characteristics.bonusEarthDamage;
        BonusAirDamage = characteristics.bonusAirDamage;
        BonusFireDamage = characteristics.bonusFireDamage;
        BonusWaterDamage = characteristics.bonusWaterDamage;
        ApRemoval = characteristics.aPRemoval;
    }

    public int LifePoints { get; set; }
    public ushort Strength { get; set; }
    public ushort Wisdom { get; set; }
    public ushort Chance { get; set; }
    public ushort Agility { get; set; }
    public ushort Intelligence { get; set; }
    public short EarthResistance { get; set; }
    public short AirResistance { get; set; }
    public short FireResistance { get; set; }
    public short WaterResistance { get; set; }
    public short NeutralResistance { get; set; }
    public byte TackleEvade { get; set; }
    public byte TackleBlock { get; set; }
    public byte BonusEarthDamage { get; set; }
    public byte BonusAirDamage { get; set; }
    public byte BonusFireDamage { get; set; }
    public byte BonusWaterDamage { get; set; }
    public byte ApRemoval { get; set; }
}
