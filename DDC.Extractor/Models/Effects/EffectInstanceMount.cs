using System;
using System.Collections.Generic;
using System.Linq;
using DDC.Extractor.Extensions;

namespace DDC.Extractor.Models.Effects;

public class EffectInstanceMount : EffectInstance
{
    public EffectInstanceMount(Core.DataCenter.Metadata.Effect.Instance.EffectInstanceMount instance) : base(instance)
    {
        Id = instance.id;
        ExpirationDate = new DateTime(instance.expirationDate);
        Model = instance.model;
        Name = instance.name;
        Owner = instance.owner;
        Level = instance.level;
        Sex = instance.sex;
        IsRideable = instance.isRideable;
        IsFeconded = instance.isFeconded;
        IsFecondationReady = instance.isFecondationReady;
        ReproductionCount = instance.reproductionCount;
        ReproductionCountMax = instance.reproductionCountMax;
        Effects = instance.effects.ToCSharpList().Select(e => e.ToInstance()).ToArray();
        Capacities = instance.capacities.ToCSharpList();
    }

    public long Id { get; set; }
    public DateTime ExpirationDate { get; set; }
    public int Model { get; set; }
    public string Name { get; set; }
    public string Owner { get; set; }
    public int Level { get; set; }
    public bool Sex { get; set; }
    public bool IsRideable { get; set; }
    public bool IsFeconded { get; set; }
    public bool IsFecondationReady { get; set; }
    public int ReproductionCount { get; set; }
    public int ReproductionCountMax { get; set; }
    public IReadOnlyList<EffectInstance> Effects { get; set; }
    public IReadOnlyList<int> Capacities { get; set; }
}
