using System.Numerics;
using Content.Client.Stylesheets;
using Content.Shared.Ember.Skills;
using Content.Shared.Roles;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;

namespace Content.Client.Ember.Skills;

public sealed class SkillSetupWindow : DefaultWindow
{
    private readonly Label _jobLabel;
    private readonly Label _pointsLabel;
    private readonly ProgressBar _pointsBar;
    private readonly BoxContainer _content;

    private JobPrototype? _job;
    private IReadOnlyList<SkillPrototype> _skills = Array.Empty<SkillPrototype>();
    private Func<ProtoId<SkillCategoryPrototype>, string>? _getCategoryName;
    private Action<JobPrototype, SkillPrototype, SkillLevel, SkillLevel>? _onSelected;

    public SkillSetupWindow()
    {
        MinWidth = 900;
        MinHeight = 540;
        SetSize = new Vector2(1000, 640);
        Resizable = true;
        AllowOffScreen = DirectionFlag.None;

        var root = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
        };

        _jobLabel = new Label
        {
            HorizontalAlignment = HAlignment.Center,
            HorizontalExpand = true,
        };
        root.AddChild(_jobLabel);

        _pointsLabel = new Label
        {
            HorizontalAlignment = HAlignment.Center,
            HorizontalExpand = true,
        };
        root.AddChild(_pointsLabel);

        _pointsBar = new ProgressBar
        {
            MaxValue = 1,
            Value = 0,
            MaxHeight = 8,
            Margin = new Thickness(0, 5),
        };
        root.AddChild(_pointsBar);

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
        root.AddChild(scroll);
        Contents.AddChild(root);
    }

    public void SetSkills(
        JobPrototype job,
        IReadOnlyList<SkillPrototype> skills,
        IReadOnlyDictionary<ProtoId<SkillPrototype>, byte> allocation,
        int skillPointBudget,
        Func<ProtoId<SkillCategoryPrototype>, string> getCategoryName,
        Action<JobPrototype, SkillPrototype, SkillLevel, SkillLevel> onSelected)
    {
        _job = job;
        _skills = skills;
        _getCategoryName = getCategoryName;
        _onSelected = onSelected;

        Title = Loc.GetString("humanoid-profile-editor-skills-window-title", ("job", job.LocalizedName));
        Rebuild(allocation, skillPointBudget);
    }

    private void Rebuild(
        IReadOnlyDictionary<ProtoId<SkillPrototype>, byte> allocation,
        int skillPointBudget)
    {
        if (_job == null || _getCategoryName == null)
            return;

        var values = SharedSkillsSystem.GetFinalSkillValues(_job, _skills, allocation);
        var remaining = SharedSkillsSystem.GetRemainingPoints(_job, _skills, allocation, skillPointBudget);

        _jobLabel.Text = Loc.GetString("humanoid-profile-editor-skills-job-label",
            ("job", _job.LocalizedName));
        _pointsLabel.Text = Loc.GetString("humanoid-profile-editor-skills-points-label",
            ("points", remaining),
            ("max", skillPointBudget));
        _pointsBar.MaxValue = Math.Max(1, skillPointBudget);
        _pointsBar.Value = Math.Max(0, remaining);

        _content.DisposeAllChildren();

        ProtoId<SkillCategoryPrototype>? currentCategory = null;
        BoxContainer? categoryRows = null;

        foreach (var skill in _skills)
        {
            if (currentCategory != skill.Category)
            {
                currentCategory = skill.Category;
                categoryRows = AddCategory(_getCategoryName(skill.Category));
            }

            var min = SharedSkillsSystem.GetMinSkill(_job, skill);
            var max = SharedSkillsSystem.GetMaxSkill(_job, skill);
            var level = values.GetValueOrDefault(skill.ID, min);
            var availablePoints = remaining + SharedSkillsSystem.GetLevelCost(_job, skill, level);

            categoryRows!.AddChild(CreateSkillRow(skill, min, max, level, values, availablePoints));
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
        SkillLevel min,
        SkillLevel max,
        SkillLevel level,
        IReadOnlyDictionary<ProtoId<SkillPrototype>, SkillLevel> values,
        int availablePoints)
    {
        var job = _job!;
        var row = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            HorizontalExpand = true,
            SeparationOverride = 8,
            Margin = new Thickness(0, 2),
        };

        var labelBox = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            MinWidth = 280,
            HorizontalExpand = true,
        };

        labelBox.AddChild(new Label
        {
            Text = Loc.GetString(skill.Name),
            ToolTip = Loc.GetString(skill.Description),
            HorizontalExpand = true,
            ClipText = true,
        });

        row.AddChild(labelBox);
        row.AddChild(new SkillLevelBar(
            skill,
            level,
            target => CanSetSkillLevel(job, skill, target, min, max, values, availablePoints),
            target => _onSelected?.Invoke(job, skill, target, min)));

        return row;
    }

    private static bool CanSetSkillLevel(
        JobPrototype job,
        SkillPrototype skill,
        SkillLevel target,
        SkillLevel min,
        SkillLevel max,
        IReadOnlyDictionary<ProtoId<SkillPrototype>, SkillLevel> values,
        int availablePoints)
    {
        if (target < min || target > max)
            return false;

        if (SharedSkillsSystem.GetLevelCost(job, skill, target) > availablePoints)
            return false;

        var targetValues = new Dictionary<ProtoId<SkillPrototype>, SkillLevel>(values)
        {
            [skill.ID] = target,
        };

        return SharedSkillsSystem.CheckPrerequisites(skill, targetValues);
    }
}
