namespace DDC.Extractor.Models.Jobs;

public class Job
{
    public Job(Core.DataCenter.Metadata.Job.Jobs job)
    {
        Id = job.id;
        IconId = job.iconId;
        NameId = job.nameId;
        HasLegendaryCraft = job.hasLegendaryCraft;
    }

    public int Id { get; set; }
    public int IconId { get; set; }
    public int NameId { get; set; }
    public bool HasLegendaryCraft { get; set; }
}
