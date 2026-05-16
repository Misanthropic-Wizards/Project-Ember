using System.Linq;
using System.Numerics;
using Content.Client.Stylesheets;
using Content.Shared.Ember.Skills;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;

namespace Content.Client.Ember.Skills;

public sealed class AntagSkillsWindow : DefaultWindow
{
    private readonly BoxContainer _content;
    private readonly Label _summary;
    private readonly Button _submitButton;

    public event Action<ProtoId<SkillPrototype>, SkillLevel>? OnSelect;
    public event Action<ProtoId<SkillPrototype>>? OnDeselect;
    public event Action? OnSubmit;

    public AntagSkillsWindow()
    {
        MinWidth = 900;
        MinHeight = 560;
        SetSize = new Vector2(980, 660);
        Resizable = true;
        AllowOffScreen = DirectionFlag.None;

        var root = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
            Margin = new Thickness(8),
        };

        _summary = new Label
        {
            HorizontalExpand = true,
            Margin = new Thickness(0, 0, 0, 6),
        };
        root.AddChild(_summary);

        _submitButton = new Button
        {
            Text = Loc.GetString("skill-antag-submit"),
            HorizontalAlignment = HAlignment.Center,
            Margin = new Thickness(0, 0, 0, 8),
        };
        _submitButton.OnPressed += _ => OnSubmit?.Invoke();
        root.AddChild(_submitButton);

        var scroll = new ScrollContainer
        {
            HorizontalExpand = true,
            VerticalExpand = true,
            HScrollEnabled = true,
            VScrollEnabled = true,
        };

        _content = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
        };

        scroll.AddChild(_content);
        root.AddChild(scroll);
        Contents.AddChild(root);
    }

    public void SetSkills(
        IReadOnlyList<SkillPrototype> skills,
        IReadOnlyDictionary<ProtoId<SkillPrototype>, SkillLevel> current,
        IReadOnlyDictionary<ProtoId<SkillPrototype>, SkillLevel> selected,
        IReadOnlyDictionary<SkillLevel, int> remaining,
        bool canChoose,
        Func<ProtoId<SkillCategoryPrototype>, string> getCategoryName)
    {
        _content.DisposeAllChildren();
        _submitButton.Disabled = !canChoose;
        _submitButton.Visible = canChoose;
        _summary.Text = canChoose
            ? Loc.GetString("skill-antag-summary", ("remaining", FormatRemaining(remaining)))
            : Loc.GetString("skill-antag-summary-locked");

        ProtoId<SkillCategoryPrototype>? currentCategory = null;
        BoxContainer? categoryRows = null;

        foreach (var skill in skills)
        {
            if (currentCategory != skill.Category)
            {
                currentCategory = skill.Category;
                categoryRows = AddCategory(getCategoryName(skill.Category));
            }

            categoryRows!.AddChild(CreateSkillRow(skill, current, selected, remaining, canChoose));
        }
    }

    private BoxContainer AddCategory(string name)
    {
        var panel = new PanelContainer
        {
            Margin = new Thickness(0, 0, 0, 8),
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#202124"),
                BorderColor = Color.FromHex("#3A3A3D"),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 6,
                ContentMarginRightOverride = 6,
                ContentMarginTopOverride = 4,
                ContentMarginBottomOverride = 6,
            },
        };

        var box = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
        };

        box.AddChild(new Label
        {
            Text = name,
            StyleClasses = { StyleBase.StyleClassLabelHeading },
            Margin = new Thickness(0, 0, 0, 4),
        });

        var rows = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
        };

        box.AddChild(rows);
        panel.AddChild(box);
        _content.AddChild(panel);
        return rows;
    }

    private BoxContainer CreateSkillRow(
        SkillPrototype skill,
        IReadOnlyDictionary<ProtoId<SkillPrototype>, SkillLevel> current,
        IReadOnlyDictionary<ProtoId<SkillPrototype>, SkillLevel> selected,
        IReadOnlyDictionary<SkillLevel, int> remaining,
        bool canChoose)
    {
        var row = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            HorizontalExpand = true,
            SeparationOverride = 8,
            Margin = new Thickness(0, 2),
        };

        row.AddChild(new Label
        {
            Text = Loc.GetString(skill.Name),
            ToolTip = Loc.GetString(skill.Description),
            MinWidth = 260,
            HorizontalExpand = true,
            ClipText = true,
        });

        var levelBox = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 0,
        };

        var currentLevel = current.GetValueOrDefault(skill.ID, SkillLevels.Min);
        selected.TryGetValue(skill.ID, out var selectedLevel);

        for (var value = (int) SkillLevels.Min; value <= (int) SkillLevels.Max; value++)
        {
            var level = (SkillLevel) value;
            var hasLevel = value <= skill.Levels.Count;
            var alreadyHas = hasLevel && currentLevel >= level;
            var isSelected = hasLevel && selectedLevel == level;
            var canSelect = CanSelect(skill, level, currentLevel, selectedLevel, remaining, canChoose);

            var button = new Button
            {
                Text = hasLevel ? SkillLevelBar.GetLevelName(skill, level) : string.Empty,
                ToggleMode = hasLevel && canChoose,
                Pressed = alreadyHas || isSelected,
                Disabled = !hasLevel || alreadyHas || !canSelect,
                MinWidth = 118,
                HorizontalExpand = false,
                ClipText = false,
                ToolTip = hasLevel ? SkillLevelBar.GetLevelName(skill, level) : null,
            };

            if (alreadyHas || isSelected)
                button.AddStyleClass(StyleBase.ButtonCaution);

            if (value == (int) SkillLevels.Min)
                button.AddStyleClass(StyleBase.ButtonOpenRight);
            else if (value == (int) SkillLevels.Max)
                button.AddStyleClass(StyleBase.ButtonOpenLeft);
            else
                button.AddStyleClass(StyleBase.ButtonOpenBoth);

            if (hasLevel && canChoose && !alreadyHas)
            {
                button.OnPressed += _ =>
                {
                    if (isSelected)
                        OnDeselect?.Invoke(skill.ID);
                    else
                        OnSelect?.Invoke(skill.ID, level);
                };
            }

            levelBox.AddChild(button);
        }

        row.AddChild(levelBox);
        return row;
    }

    private static bool CanSelect(
        SkillPrototype skill,
        SkillLevel level,
        SkillLevel current,
        SkillLevel selected,
        IReadOnlyDictionary<SkillLevel, int> remaining,
        bool canChoose)
    {
        if (!canChoose || current >= level || skill.Prerequisites.Count > 0)
            return false;

        if (selected == level)
            return true;

        return remaining.GetValueOrDefault(level) > 0;
    }

    private static string FormatRemaining(IReadOnlyDictionary<SkillLevel, int> remaining)
    {
        return string.Join(", ",
            remaining
                .Where(pair => pair.Value > 0)
                .OrderBy(pair => pair.Key)
                .Select(pair => Loc.GetString(
                    "skill-antag-remaining-entry",
                    ("level", Loc.GetString($"skill-level-{pair.Key.ToString().ToLowerInvariant()}")),
                    ("count", pair.Value))));
    }
}
