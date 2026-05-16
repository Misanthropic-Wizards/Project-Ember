using Content.Shared.Ember.Skills;
using Robust.Shared.Prototypes;

namespace Content.Server.Ember.Skills;

[RegisterComponent]
public sealed partial class AntagSkillSelectionComponent : Component
{
    [DataField]
    public ProtoId<AntagSkillSetPrototype> Set = "GenericAntag";

    [DataField]
    public Dictionary<ProtoId<SkillPrototype>, SkillLevel> Selected = new();

    [DataField]
    public bool Committed;
}
