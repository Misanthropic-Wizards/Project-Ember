using System.Collections.Generic;
using Content.Shared.Ember.Skills;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Roles;
using NUnit.Framework;
using Robust.Shared.Prototypes;

namespace Content.Tests.Shared.Ember.Skills;

[TestFixture]
[TestOf(typeof(SharedSkillsSystem))]
public sealed class SharedSkillsSystemTests
{
    [Test]
    public void GetLevelCostMatchesSierraCostCurve()
    {
        var skill = MakeSkill("electrical", difficulty: 2);

        Assert.Multiple(() =>
        {
            Assert.That(SharedSkillsSystem.GetLevelCost(skill, SkillLevel.Unskilled), Is.EqualTo(0));
            Assert.That(SharedSkillsSystem.GetLevelCost(skill, SkillLevel.Basic), Is.EqualTo(2));
            Assert.That(SharedSkillsSystem.GetLevelCost(skill, SkillLevel.Trained), Is.EqualTo(2));
            Assert.That(SharedSkillsSystem.GetLevelCost(skill, SkillLevel.Experienced), Is.EqualTo(4));
            Assert.That(SharedSkillsSystem.GetLevelCost(skill, SkillLevel.Master), Is.EqualTo(4));
        });
    }

    [Test]
    public void JobMinAndMaxFallBackToSkillDefaults()
    {
        var skill = MakeSkill("engines", defaultMax: SkillLevel.Trained);
        var job = new JobPrototype();

        Assert.Multiple(() =>
        {
            Assert.That(SharedSkillsSystem.GetMinSkill(job, skill), Is.EqualTo(SkillLevel.Unskilled));
            Assert.That(SharedSkillsSystem.GetMaxSkill(job, skill), Is.EqualTo(SkillLevel.Trained));
        });
    }

    [Test]
    public void RemainingPointsUsesAllocationAboveJobMinimum()
    {
        var skill = MakeSkill("medical", difficulty: 2);
        var job = new JobPrototype
        {
            SkillPoints = 8,
            MinSkills = { [skill.ID] = SkillLevel.Basic },
        };

        var allocation = new Dictionary<ProtoId<SkillPrototype>, byte>
        {
            [skill.ID] = 2, // Basic + 2 = Experienced, costing Trained + Experienced.
        };

        Assert.That(SharedSkillsSystem.GetRemainingPoints(job, new[] { skill }, allocation), Is.EqualTo(2));
    }

    [Test]
    public void AgeSkillPointsMatchSierraHumanCurve()
    {
        var species = new SpeciesPrototype();

        Assert.Multiple(() =>
        {
            Assert.That(SharedSkillsSystem.GetAgeSkillPoints(species, 22), Is.EqualTo(0));
            Assert.That(SharedSkillsSystem.GetAgeSkillPoints(species, 23), Is.EqualTo(3));
            Assert.That(SharedSkillsSystem.GetAgeSkillPoints(species, 30), Is.EqualTo(3));
            Assert.That(SharedSkillsSystem.GetAgeSkillPoints(species, 31), Is.EqualTo(6));
            Assert.That(SharedSkillsSystem.GetAgeSkillPoints(species, 45), Is.EqualTo(6));
            Assert.That(SharedSkillsSystem.GetAgeSkillPoints(species, 46), Is.EqualTo(8));
        });
    }

    [Test]
    public void SkillPointBudgetIncludesAgeUnlessJobDisablesBuffs()
    {
        var species = new SpeciesPrototype();
        var job = new JobPrototype
        {
            SkillPoints = 8,
        };

        Assert.That(SharedSkillsSystem.GetSkillPointBudget(job, species, 46), Is.EqualTo(16));

        job.NoSkillBuffs = true;
        Assert.That(SharedSkillsSystem.GetSkillPointBudget(job, species, 46), Is.EqualTo(8));
    }

    [Test]
    public void RemainingPointsCanUseAgeAdjustedBudget()
    {
        var skill = MakeSkill("science", difficulty: 2);
        var species = new SpeciesPrototype();
        var job = new JobPrototype
        {
            SkillPoints = 8,
        };

        var allocation = new Dictionary<ProtoId<SkillPrototype>, byte>
        {
            [skill.ID] = 3, // Experienced costs 2 + 2 + 4 = 8.
        };
        var budget = SharedSkillsSystem.GetSkillPointBudget(job, species, 31);

        Assert.That(SharedSkillsSystem.GetRemainingPoints(job, new[] { skill }, allocation, budget), Is.EqualTo(6));
    }

    private static SkillPrototype MakeSkill(
        string id,
        int difficulty = 1,
        SkillLevel defaultMax = SkillLevel.Master)
    {
        return new SkillPrototype
        {
            ID = id,
            Category = "test",
            Name = $"skill-{id}-name",
            Description = $"skill-{id}-desc",
            Difficulty = difficulty,
            DefaultMax = defaultMax,
            Levels =
            [
                "skill-level-unskilled",
                "skill-level-basic",
                "skill-level-trained",
                "skill-level-experienced",
                "skill-level-master",
            ],
        };
    }
}
