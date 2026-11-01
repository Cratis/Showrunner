// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Herald.for_HeraldServiceCollectionExtensions.when_adding_herald;

public class without_a_sender : Specification
{
    string _folder;
    OptionsValidationException _exception;

    void Because()
    {
        _folder = Directory.CreateTempSubdirectory("herald-").FullName;
        var services = new ServiceCollection();
        services.AddHerald(options =>
        {
            options.TemplatesFolder = _folder;
            options.Smtp.Host = "smtp.example.com";
        });
        using var provider = services.BuildServiceProvider();
        _exception = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<HeraldOptions>>().Value);
    }

    void Destroy() => Directory.Delete(_folder);

    [Fact] void should_say_what_to_set() => Assert.Contains("HeraldOptions.Sender.Address", _exception.Message);
}
#endif
