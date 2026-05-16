using System.Linq;
using Content.Server.Administration.Logs;
using Content.Server.Administration.Managers;
using Content.Server.EUI;
using Content.Shared.Administration;
using Content.Shared.Database;
using Content.Shared.Ember.Skills;
using Content.Shared.Eui;
using Robust.Shared.Prototypes;

namespace Content.Server.Ember.Skills;

public sealed class AdminSkillsEui : BaseEui
{
    [Dependency] private readonly IAdminManager _admins = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private readonly EntityUid _target;

    public AdminSkillsEui(EntityUid target)
    {
        IoCManager.InjectDependencies(this);
        _target = target;
    }

    public override EuiStateBase GetNewState()
    {
        return new AdminSkillsEuiState(
            _entity.GetNetEntity(_target),
            _entity.GetComponent<MetaDataComponent>(_target).EntityName,
            GetSkillValues());
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (!_admins.HasAdminFlag(Player, AdminFlags.Admin))
            return;

        if (msg is not AdminSkillsSetLevelMessage setLevel ||
            !_entity.EntityExists(_target) ||
            !_prototype.TryIndex(setLevel.Skill, out var skill))
            return;

        var level = ClampToSkill(skill, setLevel.Level);
        var component = EnsureAdminSkillSet();
        component.BaseSkills[skill.ID] = level;

        _entity.Dirty(_target, component);
        _entity.EventBus.RaiseLocalEvent(_target, new SkillsLoadedEvent(_target, component));
        _adminLog.Add(LogType.Action,
            $"{Player:actor} set {skill.ID} skill on {_entity.ToPrettyString(_target):subject} to {level}");
        StateDirty();
    }

    private Dictionary<ProtoId<SkillPrototype>, SkillLevel> GetSkillValues()
    {
        var component = EnsureAdminSkillSet();
        var values = new Dictionary<ProtoId<SkillPrototype>, SkillLevel>();

        foreach (var skill in _prototype.EnumeratePrototypes<SkillPrototype>().OrderBy(skill => skill.ID))
        {
            values[skill.ID] = ClampToSkill(
                skill,
                component.BaseSkills.GetValueOrDefault(skill.ID, component.DefaultLevel));
        }

        return values;
    }

    private SkillSetComponent EnsureAdminSkillSet()
    {
        var component = _entity.EnsureComponent<SkillSetComponent>(_target);
        component.DefaultLevel = SkillLevels.Min;

        foreach (var skill in _prototype.EnumeratePrototypes<SkillPrototype>())
        {
            component.BaseSkills.TryAdd(
                skill.ID,
                ClampToSkill(skill, component.BaseSkills.GetValueOrDefault(skill.ID, SkillLevels.Min)));
        }

        return component;
    }

    private static SkillLevel ClampToSkill(SkillPrototype skill, SkillLevel level)
    {
        var max = Math.Min((int) SkillLevels.Max, (int) SkillLevels.Min + skill.Levels.Count - 1);
        return (SkillLevel) Math.Clamp((int) level, (int) SkillLevels.Min, max);
    }
}
