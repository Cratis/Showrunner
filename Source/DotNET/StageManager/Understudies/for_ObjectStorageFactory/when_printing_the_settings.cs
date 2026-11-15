// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.for_ObjectStorageFactory;

public class when_printing_the_settings : Specification
{
    string _text;

    void Because() => _text = new ObjectStorageSettings { AccessKey = "AKIA-visible", SecretKey = "secret-visible", AccountKey = "account-visible" }.ToString();

    [Fact] void should_not_show_the_access_key() => _text.Contains("AKIA-visible").ShouldBeFalse();
    [Fact] void should_not_show_the_secret_key() => _text.Contains("secret-visible").ShouldBeFalse();
    [Fact] void should_not_show_the_account_key() => _text.Contains("account-visible").ShouldBeFalse();
}
#endif
