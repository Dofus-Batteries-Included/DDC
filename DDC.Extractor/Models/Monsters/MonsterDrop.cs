namespace DDC.Extractor.Models.Monsters;

public class MonsterDrop
{
    public MonsterDrop(Core.DataCenter.Metadata.Monster.MonsterDrop drop)
    {
        DropId = drop.dropId;
        MonsterId = drop.monsterId;
        ObjectId = drop.objectId;
        PercentDropForGrade1 = drop.percentDropForGrade1;
        PercentDropForGrade2 = drop.percentDropForGrade2;
        PercentDropForGrade3 = drop.percentDropForGrade3;
        PercentDropForGrade4 = drop.percentDropForGrade4;
        PercentDropForGrade5 = drop.percentDropForGrade5;
        Count = drop.count;
        Criteria = drop.criteria;
        HasCriteria = drop.hasCriteria;
        HiddenIfInvalidCriteria = drop.hiddenIfInvalidCriteria;
    }

    public int DropId { get; set; }
    public int MonsterId { get; set; }
    public int ObjectId { get; set; }
    public float PercentDropForGrade1 { get; set; }
    public float PercentDropForGrade2 { get; set; }
    public float PercentDropForGrade3 { get; set; }
    public float PercentDropForGrade4 { get; set; }
    public float PercentDropForGrade5 { get; set; }
    public int Count { get; set; }
    public string Criteria { get; set; }
    public bool HasCriteria { get; set; }
    public bool HiddenIfInvalidCriteria { get; set; }
}
