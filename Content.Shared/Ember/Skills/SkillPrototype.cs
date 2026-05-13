using Robust.Shared.Prototypes;

namespace Content.Shared.Ember.Skills;

[Prototype("skillCategory")]
public sealed partial class SkillCategoryPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; set; } = default!;

    [DataField(required: true)]
    public string Name { get; set; } = default!;

    [DataField]
    public int Order { get; set; }
}

[Prototype("skill")]
public sealed partial class SkillPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; set; } = default!;

    [DataField(required: true)]
    public ProtoId<SkillCategoryPrototype> Category { get; set; }

    [DataField(required: true)]
    public string Name { get; set; } = default!;

    [DataField(required: true)]
    public string Description { get; set; } = default!;

    [DataField(required: true)]
    public List<string> Levels { get; set; } = new();

    [DataField]
    public int Difficulty { get; set; } = 1;

    [DataField]
    public SkillLevel DefaultMax { get; set; } = SkillLevel.Trained;

    [DataField]
    public Dictionary<ProtoId<SkillPrototype>, SkillLevel> Prerequisites { get; set; } = new();
}
