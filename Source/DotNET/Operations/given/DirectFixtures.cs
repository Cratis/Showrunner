// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Operations.given;

/// <summary>
/// The wire shapes Cratis Direct's own contract produces, captured by running Direct's <c>Operations.Contracts</c> sources (the
/// snapshot of <c>for_ConfigurationSignature/given/a_signed_snapshot</c> plus a remedy, with the key 0x00..0x1F).
/// </summary>
/// <remarks>
/// Source: Cratis/Direct <c>Source/Operations.Contracts</c>, last changed at commit 663311fc8769ceaf94f762cf430cbf06d65b2b03
/// (repository head 0acd89e7b when captured). When Direct's contract changes, regenerate these and the conformance specs fail
/// until the re-declared types in <c>Link/Protocol</c> follow.
/// </remarks>
public static class DirectFixtures
{
    /// <summary>
    /// The signing key (base64 of bytes 0..31).
    /// </summary>
    public const string Key = "AAECAwQFBgcICQoLDA0ODxAREhMUFRYXGBkaGxwdHh8=";

    /// <summary>
    /// The digest Direct computes for the snapshot's content.
    /// </summary>
    public const string Digest = "88c65ed659c1048a6ee7a577dd0d9cd47999662a5759882c867ebdaecb41982c";

    /// <summary>
    /// The signature Direct computes over <see cref="Canonical"/>.
    /// </summary>
    public const string Signature = "64pwlufOtKKQ45TIBCfaeiBsJc44QB5dOhKSuA6XMMg=";

    /// <summary>
    /// The canonical (ordered) snapshot - what is signed.
    /// </summary>
    public const string Canonical = """{"instanceName":"prod-edge-a","revision":3,"issuedAt":"2026-09-29T12:00:00+00:00","expiresAt":"2026-09-30T12:00:00+00:00","digest":"88c65ed659c1048a6ee7a577dd0d9cd47999662a5759882c867ebdaecb41982c","checks":[{"name":"liveness","resource":"7d3f7f1e-9a53-4e0b-9f0e-6c1b1f2a3b4c","method":"Head","baseUrl":"https://a.internal","path":"/live","expectedStatusFrom":200,"expectedStatusTo":204,"timeout":"00:00:05","interval":"00:00:30","credential":"probe"},{"name":"readiness","resource":"7d3f7f1e-9a53-4e0b-9f0e-6c1b1f2a3b4c","method":"Get","baseUrl":"https://a.internal","path":"/ready","expectedStatusFrom":200,"expectedStatusTo":299,"timeout":"00:00:05","interval":"00:00:30","credential":""}],"remedies":[{"name":"restart-web","resource":"7d3f7f1e-9a53-4e0b-9f0e-6c1b1f2a3b4c","method":"Post","baseUrl":"https://a.internal","path":"/restart","parameters":[{"name":"scope","value":"web"}],"timeout":"00:00:10","credential":"admin","verification":"readiness","onlineMode":"Automatic","offlineMode":"Never","maxAttempts":2,"cooldown":"00:10:00","evidenceFreshness":"00:05:00"}],"secrets":{"admin":"other","probe":"token"},"offline":{"observeAndQueue":true,"remediesPermitted":false}}""";

    /// <summary>
    /// The signed configuration as Direct serializes it on the wire.
    /// </summary>
    public static string Signed => "{\"snapshot\":" + Canonical + ",\"signature\":\"" + Signature + "\"}";

    /// <summary>
    /// A claim request.
    /// </summary>
    public const string ClaimRequest = """{"runtimeVersion":"1.2.3","session":"session-1","variant":"Slim"}""";

    /// <summary>
    /// A first claim response.
    /// </summary>
    public const string ClaimResponse = """{"instanceName":"prod-edge-a","eventStore":"watchdog-prod-edge-a","fencingEpoch":1,"credential":"credential","signingKey":"AAECAwQFBgcICQoLDA0ODxAREhMUFRYXGBkaGxwdHh8=","leaseExpiresAt":"2026-09-29T12:02:00+00:00"}""";

    /// <summary>
    /// An outbox envelope carrying a failed check.
    /// </summary>
    public const string Envelope = """{"instanceName":"prod-edge-a","sequence":7,"eventId":"11111111-1111-1111-1111-111111111111","causationId":"00000000-0000-0000-0000-000000000000","type":"check-failed","occurred":"2026-09-29T12:00:00+00:00","payload":{"check":"readiness","resource":"7d3f7f1e-9a53-4e0b-9f0e-6c1b1f2a3b4c","statusCode":503,"reason":"unexpected status","consecutiveFailures":2,"configurationRevision":3}}""";

    /// <summary>
    /// A heartbeat envelope.
    /// </summary>
    public const string Heartbeat = """{"instanceName":"prod-edge-a","sequence":8,"eventId":"33333333-3333-3333-3333-333333333333","causationId":"00000000-0000-0000-0000-000000000000","type":"heartbeat","occurred":"2026-09-29T12:00:00+00:00","payload":{"appliedRevision":3,"runtimeVersion":"1.2.3","durableQueue":true,"queueDepth":1}}""";

    /// <summary>
    /// A batch receipt with a gap.
    /// </summary>
    public const string Receipt = """{"watermark":7,"accepted":1,"duplicates":0,"rejected":0,"gaps":[{"from":3,"to":4}]}""";

    /// <summary>
    /// A remedy request.
    /// </summary>
    public const string RemedyRequest = """{"request":"22222222-2222-2222-2222-222222222222","remedy":"restart-web","configurationRevision":3,"evidence":"11111111-1111-1111-1111-111111111111","evidenceObservedAt":"2026-09-29T12:00:00+00:00"}""";

    /// <summary>
    /// A remedy authorization list.
    /// </summary>
    public const string Authorizations = """{"authorized":[{"request":"22222222-2222-2222-2222-222222222222","remedy":"restart-web","resource":"7d3f7f1e-9a53-4e0b-9f0e-6c1b1f2a3b4c","configurationRevision":3,"fencingEpoch":1,"expiresAt":"2026-09-29T12:10:00+00:00"}]}""";
}
#endif
