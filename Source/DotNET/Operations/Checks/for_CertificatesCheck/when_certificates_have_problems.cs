// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks.for_CertificatesCheck;

public class when_certificates_have_problems : a_check
{
    protected override IWatchdogCheck Check => new CertificatesCheck(_reader);

    void Establish()
    {
        string At(int days) => _time.Now.AddDays(days).UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
        _reader.Certificates.AddRange([
            new CertificateInfo("shop", "healthy", "True", At(60)),
            new CertificateInfo("shop", "expiring", "True", At(13)),
            new CertificateInfo("shop", "renewal-stuck", "False", At(5)),
            new CertificateInfo("shop", "never-issued", "False", null),
            new CertificateInfo("shop", "no-condition-yet", null, null),
            new CertificateInfo("shop", "garbled", "True", "not a date")]);
    }

    async Task Because() => await Run();

    [Fact] void should_not_report_a_healthy_certificate() => Keys.Any(key => key.Contains("shop/healthy")).ShouldBeFalse();
    [Fact] void should_report_a_certificate_that_expires_within_fourteen_days() => Keys.ShouldContain("certificate:shop/expiring:expiry");
    [Fact] void should_report_a_certificate_that_is_not_ready() => Keys.ShouldContain("certificate:shop/renewal-stuck:ready");
    [Fact] void should_also_report_the_expiry_of_one_that_is_not_ready_but_was_issued() => Keys.ShouldContain("certificate:shop/renewal-stuck:expiry");
    [Fact] void should_report_only_that_a_never_issued_certificate_is_not_ready() => Keys.Where(key => key.Contains("never-issued")).ShouldContainOnly("certificate:shop/never-issued:ready");
    [Fact] void should_treat_a_certificate_with_no_status_yet_as_not_ready() => Keys.ShouldContain("certificate:shop/no-condition-yet:ready");
    [Fact] void should_report_an_unreadable_expiry() => Keys.ShouldContain("certificate:shop/garbled:unreadable");
}
#endif
