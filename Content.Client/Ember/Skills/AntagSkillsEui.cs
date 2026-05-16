using System.Linq;
using Content.Client.Eui;
using Content.Shared.Ember.Skills;
using Content.Shared.Eui;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Client.Ember.Skills;

[UsedImplicitly]
public sealed class AntagSkillsEui : BaseEui
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private readonly AntagSkillsWindow _window;

    public AntagSkillsEui()
    {
        _window = new AntagSkillsWindow();
        _window.OnSelect += (skill, level) => SendMessage(new AntagSkillsSelectMessage(skill, level));
        _window.OnDeselect += skill => SendMessage(new AntagSkillsDeselectMessage(skill));
        _window.OnSubmit += () => SendMessage(new AntagSkillsSubmitMessage());
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
        if (state is not AntagSkillsEuiState antagState)
            return;

        _window.Title = Loc.GetString("skill-antag-window-title", ("target", antagState.TargetName));
        _window.SetSkills(
            GetOrderedSkills(),
            antagState.Skills,
            antagState.Selected,
            antagState.Remaining,
            antagState.CanChoose,
            GetCategoryName);
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
