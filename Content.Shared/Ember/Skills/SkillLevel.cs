namespace Content.Shared.Ember.Skills;

public enum SkillLevel : byte
{
    Unskilled = 1,
    Basic = 2,
    Trained = 3,
    Experienced = 4,
    Master = 5,
}

public static class SkillLevels
{
    public const SkillLevel Min = SkillLevel.Unskilled;
    public const SkillLevel Max = SkillLevel.Master;
    public const SkillLevel Default = SkillLevel.Experienced;
    public const SkillLevel Baseline = SkillLevel.Trained;
}
