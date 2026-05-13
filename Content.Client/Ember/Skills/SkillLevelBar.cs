using Content.Client.Stylesheets;
using Content.Shared.Ember.Skills;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.Ember.Skills;

public sealed class SkillLevelBar : BoxContainer
{
    public SkillLevelBar(
        SkillPrototype skill,
        SkillLevel current,
        Func<SkillLevel, bool>? canSelect = null,
        Action<SkillLevel>? onSelected = null)
    {
        Orientation = LayoutOrientation.Horizontal;
        SeparationOverride = 0;

        for (var value = (int) SkillLevels.Min; value <= (int) SkillLevels.Max; value++)
        {
            var level = (SkillLevel) value;
            var hasLevel = value <= skill.Levels.Count;
            var filled = hasLevel && level <= current;
            var selectable = hasLevel && (canSelect?.Invoke(level) ?? false);

            var button = new Button
            {
                Text = hasLevel ? GetLevelName(skill, level) : string.Empty,
                ToggleMode = onSelected != null,
                Pressed = filled,
                Disabled = onSelected != null && !selectable,
                MinWidth = 118,
                HorizontalExpand = false,
                ClipText = false,
                ToolTip = hasLevel ? GetLevelName(skill, level) : null,
            };

            if (filled)
                button.AddStyleClass(StyleBase.ButtonCaution);

            if (value == (int) SkillLevels.Min)
                button.AddStyleClass(StyleBase.ButtonOpenRight);
            else if (value == (int) SkillLevels.Max)
                button.AddStyleClass(StyleBase.ButtonOpenLeft);
            else
                button.AddStyleClass(StyleBase.ButtonOpenBoth);

            if (hasLevel && onSelected != null)
                button.OnPressed += _ => onSelected(level);

            AddChild(button);
        }
    }

    public static string GetLevelName(SkillPrototype skill, SkillLevel level)
    {
        var levelIndex = (int) level - 1;
        if (levelIndex >= 0 && levelIndex < skill.Levels.Count)
            return Loc.GetString(skill.Levels[levelIndex]);

        return Loc.GetString($"skill-level-{level.ToString().ToLowerInvariant()}");
    }
}
