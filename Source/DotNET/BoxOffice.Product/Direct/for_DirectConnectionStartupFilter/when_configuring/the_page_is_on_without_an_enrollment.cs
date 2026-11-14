// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionStartupFilter.when_configuring;

public class the_page_is_on_without_an_enrollment : Specification
{
    Exception? _exception;

    async Task Because() =>
        _exception = await Catch.Exception(() => Pipeline.Run(new BoxOfficeProductOptions { Direct = new() { Enabled = true } }, null));

    [Fact] void should_fail_at_startup_rather_than_serve_a_page_that_cannot_enroll() => _exception.ShouldBeOfExactType<InvalidOperationException>();
}
#endif
