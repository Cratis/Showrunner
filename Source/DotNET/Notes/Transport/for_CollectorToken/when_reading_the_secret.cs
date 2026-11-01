// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Transport.for_CollectorToken;

public class when_reading_the_secret : Specification
{
    string _path;
    string _fromFile;
    string _afterRotation;
    string _fromOptions;
    Exception _missing;

    async Task Because()
    {
        _path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(_path, "first-secret\n");
            var options = new NotesOptions { CollectorToken = "in-options", CollectorTokenFile = _path };
            var token = new CollectorToken(Options.Create(options));
            _fromFile = await token.Read();
            await File.WriteAllTextAsync(_path, "rotated-secret");
            _afterRotation = await token.Read();
            _fromOptions = await new CollectorToken(Options.Create(new NotesOptions { CollectorToken = " in-options " })).Read();
            _missing = await Record.ExceptionAsync(() => new CollectorToken(Options.Create(new NotesOptions())).Read());
        }
        finally
        {
            File.Delete(_path);
        }
    }

    [Fact] void should_prefer_the_file_and_trim_it() => _fromFile.ShouldEqual("first-secret");
    [Fact] void should_read_a_rotated_secret_without_a_restart() => _afterRotation.ShouldEqual("rotated-secret");
    [Fact] void should_fall_back_to_the_option() => _fromOptions.ShouldEqual("in-options");
    [Fact] void should_refuse_no_secret() => _missing.ShouldBeOfExactType<InvalidOperationException>();
}
#endif
