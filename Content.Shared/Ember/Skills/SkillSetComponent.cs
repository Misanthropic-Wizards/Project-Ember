using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;

namespace Content.Shared.Ember.Skills;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SkillSetComponent : Component
{
    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<SkillPrototype>, SkillLevel> BaseSkills = new();

    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<SkillPrototype>, int> Modifiers = new();

    [DataField, AutoNetworkedField]
    public SkillLevel DefaultLevel = SkillLevels.Default;
}
