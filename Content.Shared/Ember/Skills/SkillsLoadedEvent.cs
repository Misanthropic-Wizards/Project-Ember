namespace Content.Shared.Ember.Skills;

public sealed class SkillsLoadedEvent : EntityEventArgs
{
    public EntityUid Mob { get; }
    public SkillSetComponent Component { get; }

    public SkillsLoadedEvent(EntityUid mob, SkillSetComponent component)
    {
        Mob = mob;
        Component = component;
    }
}
