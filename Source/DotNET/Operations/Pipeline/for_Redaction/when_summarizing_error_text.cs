// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Operations.Pipeline.for_Redaction;

public class when_summarizing_error_text : Specification
{
    string _redacted;
    string _long;

    void Because()
    {
        _redacted = Redaction.Summarize("backup failed:\n  connect mongodb://admin:hunter2@db.internal:27017/?x=1 with Bearer abc.def.ghi and password=hunter3, secret_key: s3cr3t token=\"t0k3n\"");
        _long = Redaction.Summarize(new string('x', 1000));
    }

    [Fact] void should_redact_connection_strings() => _redacted.ShouldNotContain("db.internal");
    [Fact] void should_redact_bearer_tokens() => _redacted.ShouldNotContain("abc.def.ghi");
    [Fact] void should_redact_passwords() => _redacted.ShouldNotContain("hunter3");
    [Fact] void should_redact_secret_keys() => _redacted.ShouldNotContain("s3cr3t");
    [Fact] void should_redact_quoted_tokens() => _redacted.ShouldNotContain("t0k3n");
    [Fact] void should_say_what_was_removed() => _redacted.ShouldContain("[redacted URL]");
    [Fact] void should_collapse_it_to_one_line() => _redacted.ShouldNotContain("\n");
    [Fact] void should_truncate_to_300_characters() => _long.Length.ShouldEqual(300);
}
#endif
