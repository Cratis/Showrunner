// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text.Json;
using Cratis.Showrunner.Operations.Cluster;

namespace Cratis.Showrunner.Operations.Kubernetes.for_KubernetesMapper;

public class when_reading_custom_resources : Specification
{
    const string Certificates = """{"items":[{"metadata":{"namespace":"shop","name":"tls"},"status":{"notAfter":"2026-09-01T00:00:00Z","conditions":[{"type":"Ready","status":"True"}]}},{"metadata":{"namespace":"shop","name":"new"},"status":{}}]}""";
    const string Backups = """{"items":[{"metadata":{"name":"b1","creationTimestamp":"2026-07-01T10:00:00Z"},"status":{"state":"ready","completed":"2026-07-01T10:05:00Z"}},{"metadata":{"name":"b2","creationTimestamp":"2026-07-01T11:00:00Z"},"status":{"state":"error","error":"boom"}}]}""";

    IReadOnlyList<CertificateInfo> _certificates;
    IReadOnlyList<PbmBackupInfo> _backups;

    void Because()
    {
        _certificates = KubernetesMapper.Certificates(JsonElement.Parse(Certificates));
        _backups = KubernetesMapper.PbmBackups(JsonElement.Parse(Backups));
    }

    [Fact] void should_read_a_ready_certificate_and_its_expiry() => _certificates[0].ShouldEqual(new CertificateInfo("shop", "tls", "True", "2026-09-01T00:00:00Z"));
    [Fact] void should_read_a_certificate_with_no_status_yet() => _certificates[1].ShouldEqual(new CertificateInfo("shop", "new", null, null));
    [Fact] void should_read_a_ready_backup_by_its_completion_time() => _backups[0].Completed.ShouldEqual(new DateTimeOffset(2026, 7, 1, 10, 5, 0, TimeSpan.Zero));
    [Fact] void should_read_the_error_of_a_failed_backup() => (_backups[1].State, _backups[1].Error).ShouldEqual(("error", "boom"));
}
#endif
