using System.Numerics;
using Content.Client.Stylesheets;
using Content.Shared.Ember.Skills;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Prototypes;
using Robust.Shared.Maths;

namespace Content.Client.Ember.Skills;

public sealed class SkillsWindow : DefaultWindow
{
    private readonly BoxContainer _content;

    public SkillsWindow()
    {
        Title = Loc.GetString("skill-window-title");
        MinWidth = 900;
        MinHeight = 520;
        SetSize = new Vector2(980, 620);
        Resizable = true;
        AllowOffScreen = DirectionFlag.None;

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
            Margin = new Thickness(8),
            HorizontalExpand = true,
            VerticalExpand = true,
        };

        scroll.AddChild(_content);
        Contents.AddChild(scroll);
    }

    public void SetEmpty()
    {
        _content.DisposeAllChildren();
        _content.AddChild(new Label
        {
            Text = Loc.GetString("skill-window-empty"),
            HorizontalExpand = true,
        });
    }

    public void SetSkills(
        IReadOnlyList<SkillPrototype> skills,
        Func<SkillPrototype, SkillLevel> getLevel,
        Func<ProtoId<SkillCategoryPrototype>, string> getCategoryName,
        Action<SkillPrototype, SkillLevel>? onSelected = null)
    {
        _content.DisposeAllChildren();

        ProtoId<SkillCategoryPrototype>? currentCategory = null;
        BoxContainer? categoryRows = null;

        foreach (var skill in skills)
        {
            if (currentCategory != skill.Category)
            {
                currentCategory = skill.Category;
                categoryRows = AddCategory(getCategoryName(skill.Category));
            }

            categoryRows!.AddChild(CreateSkillRow(skill, getLevel(skill), onSelected));
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
        SkillLevel level,
        Action<SkillPrototype, SkillLevel>? onSelected)
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

        row.AddChild(new SkillLevelBar(
            skill,
            level,
            onSelected == null ? null : _ => true,
            onSelected == null ? null : selected => onSelected(skill, selected)));
        return row;
    }
}
