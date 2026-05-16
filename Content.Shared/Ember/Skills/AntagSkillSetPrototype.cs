using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Shared.Ember.Skills;

[Prototype("antagSkillSet")]
public sealed partial class AntagSkillSetPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; set; } = default!;

    [DataField]
    public List<ProtoId<AntagPrototype>> Antags { get; set; } = new();

    [DataField]
    public SkillLevel DefaultLevel { get; set; } = SkillLevel.Unskilled;

    [DataField]
    public Dictionary<ProtoId<SkillPrototype>, SkillLevel> Skills { get; set; } = new();

    [DataField]
    public Dictionary<SkillLevel, int> Choices { get; set; } = new();
}
