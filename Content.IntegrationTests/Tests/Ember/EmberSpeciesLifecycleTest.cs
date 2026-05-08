using Content.Shared.Ember.Species;
using Content.Shared.Guidebook;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests.Ember;

[TestFixture]
[FixtureLifeCycle(LifeCycle.SingleInstance)]
[TestOf(typeof(EmberSpeciesLifecyclePrototype))]
public sealed class EmberSpeciesLifecycleTest
{
    [Test]
    public async Task ValidateLifecycleEntries()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Connected = true });
        var server = pair.Server;
        await server.WaitIdleAsync();

        var protoMan = server.ResolveDependency<IPrototypeManager>();

        foreach (var lifecycle in protoMan.EnumeratePrototypes<EmberSpeciesLifecyclePrototype>())
        {
            Assert.That(protoMan.TryIndex<SpeciesPrototype>(lifecycle.ID, out var species),
                $"Ember species lifecycle entry {lifecycle.ID} does not match an existing species prototype.");

            Assert.That(lifecycle.Reason, Is.Not.Empty,
                $"Ember species lifecycle entry {lifecycle.ID} must explain why it exists.");

            if (lifecycle.Status == EmberSpeciesLifecycleStatus.Active)
                continue;

            Assert.That(species!.RoundStart, Is.False,
                $"{lifecycle.ID} is marked {lifecycle.Status} and must not be available at round start.");

            Assert.That(protoMan.HasIndex<GuideEntryPrototype>(lifecycle.ID), Is.False,
                $"{lifecycle.ID} is marked {lifecycle.Status} and must not have a public guidebook species entry.");

            if (lifecycle.Status == EmberSpeciesLifecycleStatus.Legacy)
            {
                Assert.That(lifecycle.Reason, Does.Contain("EMBER-LEGACY"),
                    $"{lifecycle.ID} is Legacy and must be searchable as EMBER-LEGACY.");
            }
        }

        await pair.CleanReturnAsync();
    }
}
