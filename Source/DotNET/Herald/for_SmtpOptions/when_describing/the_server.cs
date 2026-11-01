// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Herald.for_SmtpOptions.when_describing;

public class the_server : Specification
{
    string _description;

    void Because() => _description = new SmtpOptions { Host = "smtp.example.com", Port = 587, UserName = "ledger", Password = "s3cret-value" }.ToString();

    [Fact] void should_name_the_server() => Assert.Equal("smtp.example.com:587 (StartTls)", _description);
    [Fact] void should_not_show_the_password() => Assert.DoesNotContain("s3cret", _description);
}
#endif
