using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Content.Shared.Eui;

namespace Content.Shared.Ember.Skills;

[Serializable, NetSerializable]
public sealed class AdminSkillsEuiState(
    NetEntity target,
    string targetName,
    Dictionary<ProtoId<SkillPrototype>, SkillLevel> skills)
    : EuiStateBase
{
    public readonly NetEntity Target = target;
    public readonly string TargetName = targetName;
    public readonly Dictionary<ProtoId<SkillPrototype>, SkillLevel> Skills = skills;
}

[Serializable, NetSerializable]
public sealed class AdminSkillsSetLevelMessage(
    ProtoId<SkillPrototype> skill,
    SkillLevel level)
    : EuiMessageBase
{
    public readonly ProtoId<SkillPrototype> Skill = skill;
    public readonly SkillLevel Level = level;
}

[Serializable, NetSerializable]
public sealed class AntagSkillsEuiState(
    string targetName,
    Dictionary<ProtoId<SkillPrototype>, SkillLevel> skills,
    Dictionary<ProtoId<SkillPrototype>, SkillLevel> selected,
    Dictionary<SkillLevel, int> remaining,
    bool canChoose)
    : EuiStateBase
{
    public readonly string TargetName = targetName;
    public readonly Dictionary<ProtoId<SkillPrototype>, SkillLevel> Skills = skills;
    public readonly Dictionary<ProtoId<SkillPrototype>, SkillLevel> Selected = selected;
    public readonly Dictionary<SkillLevel, int> Remaining = remaining;
    public readonly bool CanChoose = canChoose;
}

[Serializable, NetSerializable]
public sealed class AntagSkillsSelectMessage(
    ProtoId<SkillPrototype> skill,
    SkillLevel level)
    : EuiMessageBase
{
    public readonly ProtoId<SkillPrototype> Skill = skill;
    public readonly SkillLevel Level = level;
}

[Serializable, NetSerializable]
public sealed class AntagSkillsDeselectMessage(ProtoId<SkillPrototype> skill) : EuiMessageBase
{
    public readonly ProtoId<SkillPrototype> Skill = skill;
}

[Serializable, NetSerializable]
public sealed class AntagSkillsSubmitMessage : EuiMessageBase;
