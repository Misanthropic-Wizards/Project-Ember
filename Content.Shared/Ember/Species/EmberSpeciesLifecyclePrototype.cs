using Robust.Shared.Prototypes;

namespace Content.Shared.Ember.Species;

[Prototype("emberSpeciesLifecycle")]
public sealed partial class EmberSpeciesLifecyclePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public EmberSpeciesLifecycleStatus Status { get; private set; } = EmberSpeciesLifecycleStatus.Active;

    [DataField]
    public bool KeepAssets { get; private set; } = true;

    [DataField(required: true)]
    public string Reason { get; private set; } = default!;
}

public enum EmberSpeciesLifecycleStatus : byte
{
    Active,
    Dormant,
    Legacy,
}
