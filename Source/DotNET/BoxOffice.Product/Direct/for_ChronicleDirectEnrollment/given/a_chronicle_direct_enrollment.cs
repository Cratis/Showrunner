// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Reflection;
using System.Text;
using Cratis.Chronicle;
using Cratis.Chronicle.Federation;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_ChronicleDirectEnrollment.given;

public class a_chronicle_direct_enrollment : Specification
{
    /// <summary>
    /// An <see cref="IChronicleClient"/> that only answers <see cref="IChronicleClient.Federation"/>. The member is a default
    /// interface member, which the mocking library cannot intercept, so the client is proxied directly.
    /// </summary>
    public class FederationOnlyClient : DispatchProxy
    {
        public IFederationEnrollments Federation { get; set; }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) =>
            targetMethod!.Name == "get_" + nameof(IChronicleClient.Federation)
                ? Federation
                : throw new NotSupportedException($"{targetMethod.Name} is not part of what the enrollment uses.");
    }

    protected const string Secret = "the-secret-of-the-code";
    protected const string Host = "chronicle.example.com";
    protected const string Endpoint = "https://" + Host + ":35000";

    protected IFederationEnrollments _federation;
    protected ILogger<ChronicleDirectEnrollment> _logger;
    protected ChronicleDirectEnrollment _enrollment;
    protected IChronicleClient _client;

    protected static string Code(string endpoint = Endpoint) =>
        "cfe1." + Convert.ToBase64String(Encoding.UTF8.GetBytes($"{{\"e\":\"{endpoint}\",\"f\":\"ab\",\"i\":\"id\",\"s\":\"{Secret}\"}}")).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    protected static FederationEnrollmentStatus NotEnrolled() => new(false, string.Empty, string.Empty, null, null, string.Empty, string.Empty, false);

    protected static FederationEnrollmentStatus Enrolled(string endpoint = Endpoint, bool expired = false, bool connected = true, bool exchanged = true, string lastError = "") =>
        new(true, "spiffe://example/customer/1/kernel/a", "01", DateTimeOffset.UtcNow.AddDays(14), DateTimeOffset.UtcNow.AddDays(7), endpoint, "ab", expired, connected, exchanged ? DateTimeOffset.UtcNow : null, lastError);

    protected static FederationRedemption Redemption(string endpoint = Endpoint, bool alreadyEnrolled = false) =>
        new("spiffe://example/customer/1/kernel/a", "01", DateTimeOffset.UtcNow.AddDays(14), endpoint, alreadyEnrolled);

    protected static DirectEnrollmentRequest Request(string code, string? endpoint = null, string expectedHost = "") => new(code, endpoint, expectedHost);

    protected void Status(FederationEnrollmentStatus status) =>
        _federation.GetStatus(Arg.Any<CancellationToken>()).Returns(status);

    protected void StatusFails() =>
        _federation.GetStatus(Arg.Any<CancellationToken>()).Returns<FederationEnrollmentStatus>(_ => throw new HttpRequestException("the kernel is down"));

    protected IEnumerable<string> Logged() =>
        _logger.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ILogger.Log))
            .SelectMany(call => call.GetArguments().Select(argument => argument?.ToString() ?? string.Empty));

    void Establish()
    {
        _federation = Substitute.For<IFederationEnrollments>();
        _federation.GetStatus(Arg.Any<CancellationToken>()).Returns(NotEnrolled());
        var client = DispatchProxy.Create<IChronicleClient, FederationOnlyClient>();
        ((FederationOnlyClient)(object)client).Federation = _federation;
        _logger = Substitute.For<ILogger<ChronicleDirectEnrollment>>();
        _logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        _client = client;
        _enrollment = new(client, _logger) { ConnectionPatience = TimeSpan.Zero };
    }
}
#endif
