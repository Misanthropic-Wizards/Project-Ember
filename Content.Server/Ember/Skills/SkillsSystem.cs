using System.Linq;
using Content.Shared.Ember.Skills;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server.Ember.Skills;

public sealed class SkillsSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent ev)
    {
        ApplyProfileSkills(ev.Mob, ev.JobId, ev.Profile);
    }

    public void ApplyProfileSkills(
        EntityUid mob,
        string? jobId,
        HumanoidCharacterProfile profile)
    {
        JobPrototype? job = null;
        Dictionary<ProtoId<SkillPrototype>, byte>? allocation = null;
        SpeciesPrototype? species = null;

        _prototype.TryIndex(profile.Species, out species);

        if (jobId != null && _prototype.TryIndex<JobPrototype>(jobId, out var indexedJob))
        {
            job = indexedJob;
            profile.SkillPreferences.TryGetValue(jobId, out allocation);
        }

        var skills = _prototype.EnumeratePrototypes<SkillPrototype>()
            .OrderBy(skill => skill.ID)
            .ToArray();
        var cleanAllocation = allocation ?? new Dictionary<ProtoId<SkillPrototype>, byte>();
        if (job != null)
        {
            var budget = SharedSkillsSystem.GetSkillPointBudget(job, species, profile.Age);
            cleanAllocation = SharedSkillsSystem.SanitizeAllocation(job, skills, cleanAllocation, budget);
        }

        var finalValues = SharedSkillsSystem.GetFinalSkillValues(
            job,
            skills,
            cleanAllocation);
        var component = EnsureComp<SkillSetComponent>(mob);

        component.BaseSkills.Clear();
        component.Modifiers.Clear();
        foreach (var (skill, level) in finalValues)
        {
            component.BaseSkills[skill] = level;
        }

        Dirty(mob, component);
        RaiseLocalEvent(mob, new SkillsLoadedEvent(mob, component));
    }
}
