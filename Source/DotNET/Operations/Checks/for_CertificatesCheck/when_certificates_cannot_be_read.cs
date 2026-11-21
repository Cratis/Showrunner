// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks.for_CertificatesCheck;

public class when_certificates_cannot_be_read : a_check
{
    protected override IWatchdogCheck Check => new CertificatesCheck(_reader);

    void Establish() => _reader.Failing[nameof(IClusterReader.ListCertificates)] = 404;

    async Task Because() => await Run();

    [Fact] void should_not_treat_an_unreadable_source_as_healthy() => Keys.ShouldContainOnly("certificates:api");
}
#endif
