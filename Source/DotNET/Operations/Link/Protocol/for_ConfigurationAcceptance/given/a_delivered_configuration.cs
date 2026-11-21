// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Operations.Link.Protocol.for_ConfigurationAcceptance.given;

public abstract class a_delivered_configuration : Specification
{
    protected static readonly string _key = ConfigurationSignature.NewKey();
    protected static readonly DateTimeOffset _now = new(2026, 7, 1, 12, 0, 0, TimeSpan.Zero);

    protected WatchdogConfigurationSnapshot _snapshot;
    protected ConfigurationAcceptanceReason _reason;

    protected static WatchdogConfigurationSnapshot Snapshot(long revision, DateTimeOffset issued, DateTimeOffset expires, string instance = "prod")
    {
        var snapshot = new WatchdogConfigurationSnapshot(instance, revision, issued, expires, string.Empty, [], [], new Dictionary<string, string>(), new OfflinePolicy(true, false));
        return snapshot with { Digest = ConfigurationDigest.Of(snapshot) };
    }

    protected static SignedWatchdogConfiguration Signed(WatchdogConfigurationSnapshot snapshot, string? key = null) => ConfigurationSignature.Sign(snapshot, key ?? _key);

    void Establish() => _snapshot = Snapshot(2, _now, _now.AddHours(1));
}
#endif
