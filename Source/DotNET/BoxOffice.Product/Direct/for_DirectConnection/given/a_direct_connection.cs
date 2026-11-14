// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnection.given;

public class a_direct_connection : Specification
{
    protected const string Code = "the-enrollment-code";
    protected const string Endpoint = "https://chronicle.example.com:35000";

    protected BoxOfficeProductOptions _options;
    protected IDirectEnrollment _enrollment;
    protected InMemoryDirectLinkRecord _record;
    protected DirectConnection _connection;

    protected void Redeems() =>
        _enrollment.Redeem(Arg.Any<DirectEnrollmentRequest>(), Arg.Any<CancellationToken>()).Returns(new DirectRedemptionResult(DirectRedemption.Redeemed, Endpoint));

    protected void Connects(DirectLinkHealth health) =>
        _enrollment.Verify(Arg.Any<CancellationToken>()).Returns(health);

    void Establish()
    {
        _options = new() { Direct = new() { Enabled = true } };
        _enrollment = Substitute.For<IDirectEnrollment>();
        _record = new();
        var wrapped = Options.Create(_options);
        _connection = new(_enrollment, _record, new(wrapped, TimeProvider.System), wrapped, Microsoft.Extensions.Logging.Abstractions.NullLogger<DirectConnection>.Instance);
    }
}
#endif
