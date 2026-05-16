using System.Linq;
using Content.Client.Eui;
using Content.Shared.Ember.Skills;
using Content.Shared.Eui;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Client.Ember.Skills;

[UsedImplicitly]
public sealed class AdminSkillsEui : BaseEui
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private readonly SkillsWindow _window;

    public AdminSkillsEui()
    {
        _window = new SkillsWindow();
        _window.OnClose += () => SendMessage(new CloseEuiMessage());
    }

    public override void Opened()
    {
        _window.OpenCentered();
    }

    public override void Closed()
    {
        _window.Close();
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is not AdminSkillsEuiState skillsState)
            return;

        _window.Title = Loc.GetString("skill-admin-window-title", ("target", skillsState.TargetName));
        _window.SetSkills(
            GetOrderedSkills(),
            skill => skillsState.Skills.GetValueOrDefault(skill.ID, SkillLevels.Min),
            GetCategoryName,
            (skill, level) => SendMessage(new AdminSkillsSetLevelMessage(skill.ID, level)));
    }

    private SkillPrototype[] GetOrderedSkills()
    {
        return _prototype.EnumeratePrototypes<SkillPrototype>()
            .OrderBy(GetCategoryOrder)
            .ThenBy(skill => Loc.GetString(skill.Name))
            .ToArray();
    }

    private int GetCategoryOrder(SkillPrototype skill)
    {
        return _prototype.TryIndex(skill.Category, out SkillCategoryPrototype? category)
            ? category.Order
            : int.MaxValue;
    }

    private string GetCategoryName(ProtoId<SkillCategoryPrototype> categoryId)
    {
        return _prototype.TryIndex(categoryId, out SkillCategoryPrototype? category)
            ? Loc.GetString(category.Name)
            : categoryId;
    }
}
