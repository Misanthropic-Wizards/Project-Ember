namespace Content.Shared.Ember.Skills;

/// <summary>
///     Species-specific age threshold for skill point bonuses.
/// </summary>
[DataDefinition]
public sealed partial class SkillAgePointBracket
{
    [DataField(required: true)]
    public int MinimumAge;

    [DataField(required: true)]
    public int Points;
}
