using System.Linq;
using Content.Server.EUI;
using Content.Shared.Ember.Skills;
using Content.Shared.Eui;
using Robust.Shared.Prototypes;

namespace Content.Server.Ember.Skills;

public sealed class AntagSkillsEui : BaseEui
{
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private readonly EntityUid _target;
    private readonly ProtoId<AntagSkillSetPrototype> _set;

    public AntagSkillsEui(EntityUid target, ProtoId<AntagSkillSetPrototype> set)
    {
        IoCManager.InjectDependencies(this);
        _target = target;
        _set = set;
    }

    public override EuiStateBase GetNewState()
    {
        var selection = _entity.EnsureComponent<AntagSkillSelectionComponent>(_target);
        var skillSet = _entity.EnsureComponent<SkillSetComponent>(_target);

        return new AntagSkillsEuiState(
            _entity.GetComponent<MetaDataComponent>(_target).EntityName,
            GetSkillValues(skillSet),
            new Dictionary<ProtoId<SkillPrototype>, SkillLevel>(selection.Selected),
            GetRemainingChoices(selection),
            !selection.Committed);
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (Player.AttachedEntity != _target ||
            !_entity.EntityExists(_target) ||
            !_prototype.TryIndex(_set, out var set))
            return;

        var selection = _entity.EnsureComponent<AntagSkillSelectionComponent>(_target);
        if (selection.Committed)
            return;

        switch (msg)
        {
            case AntagSkillsSelectMessage select:
                TrySelect(set, selection, select.Skill, select.Level);
                break;
            case AntagSkillsDeselectMessage deselect:
                selection.Selected.Remove(deselect.Skill);
                _entity.Dirty(_target, selection);
                break;
            case AntagSkillsSubmitMessage:
                CommitSelection(selection);
                break;
        }

        StateDirty();
    }

    private void TrySelect(
        AntagSkillSetPrototype set,
        AntagSkillSelectionComponent selection,
        ProtoId<SkillPrototype> skillId,
        SkillLevel level)
    {
        if (!_prototype.TryIndex(skillId, out var skill) ||
            !CanSelect(set, selection, skill, level))
            return;

        selection.Selected[skill.ID] = level;
        _entity.Dirty(_target, selection);
    }

    private bool CanSelect(
        AntagSkillSetPrototype set,
        AntagSkillSelectionComponent selection,
        SkillPrototype skill,
        SkillLevel level)
    {
        if (!set.Choices.TryGetValue(level, out var maxChoices) || maxChoices <= 0)
            return false;

        if (skill.Prerequisites.Count > 0)
            return false;

        if ((int) level > (int) SkillLevels.Min + skill.Levels.Count - 1)
            return false;

        var skillSet = _entity.EnsureComponent<SkillSetComponent>(_target);
        var current = skillSet.BaseSkills.GetValueOrDefault(skill.ID, skillSet.DefaultLevel);
        if (current >= level)
            return false;

        var alreadySelectedAtLevel = selection.Selected.TryGetValue(skill.ID, out var selected) && selected == level;
        if (alreadySelectedAtLevel)
            return true;

        return selection.Selected.Count(pair => pair.Value == level) < maxChoices;
    }

    private void CommitSelection(AntagSkillSelectionComponent selection)
    {
        var skillSet = _entity.EnsureComponent<SkillSetComponent>(_target);

        foreach (var (skillId, level) in selection.Selected)
        {
            if (!_prototype.TryIndex(skillId, out var skill))
                continue;

            var current = skillSet.BaseSkills.GetValueOrDefault(skill.ID, skillSet.DefaultLevel);
            var clamped = ClampToSkill(skill, level);
            if (current < clamped)
                skillSet.BaseSkills[skill.ID] = clamped;
        }

        selection.Committed = true;
        _entity.Dirty(_target, selection);
        _entity.Dirty(_target, skillSet);
        _entity.EventBus.RaiseLocalEvent(_target, new SkillsLoadedEvent(_target, skillSet));
    }

    private Dictionary<ProtoId<SkillPrototype>, SkillLevel> GetSkillValues(SkillSetComponent skillSet)
    {
        var values = new Dictionary<ProtoId<SkillPrototype>, SkillLevel>();

        foreach (var skill in _prototype.EnumeratePrototypes<SkillPrototype>().OrderBy(skill => skill.ID))
        {
            values[skill.ID] = ClampToSkill(
                skill,
                skillSet.BaseSkills.GetValueOrDefault(skill.ID, skillSet.DefaultLevel));
        }

        return values;
    }

    private Dictionary<SkillLevel, int> GetRemainingChoices(AntagSkillSelectionComponent selection)
    {
        if (!_prototype.TryIndex(_set, out var set))
            return new Dictionary<SkillLevel, int>();

        var remaining = new Dictionary<SkillLevel, int>();
        foreach (var (level, choices) in set.Choices)
        {
            remaining[level] = choices - selection.Selected.Count(pair => pair.Value == level);
        }

        return remaining;
    }

    private static SkillLevel ClampToSkill(SkillPrototype skill, SkillLevel level)
    {
        var max = Math.Min((int) SkillLevels.Max, (int) SkillLevels.Min + skill.Levels.Count - 1);
        return (SkillLevel) Math.Clamp((int) level, (int) SkillLevels.Min, max);
    }
}
