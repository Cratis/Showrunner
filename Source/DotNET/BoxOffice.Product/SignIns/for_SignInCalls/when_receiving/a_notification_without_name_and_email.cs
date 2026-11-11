// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.given;

namespace Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.when_receiving;

public class a_notification_without_name_and_email : a_sign_in_callback
{
    int _status;
    Person _holder;

    async Task Because()
    {
        _status = await Answer(Call("""{"kind":"SignedIn","subject":"1001","identityProvider":"github","tenant":"fjord-logistics"}"""));
        await ReadOutbox();
        _holder = ((SeatHolderActive)_outbox.Single().Content).Holder;
    }

    [Fact] void should_accept_it() => _status.ShouldEqual(StatusCodes.Status204NoContent);
    [Fact] void should_report_an_empty_name() => _holder.Name.Value.ShouldEqual(string.Empty);
    [Fact] void should_report_an_empty_email() => _holder.Email.Value.ShouldEqual(string.Empty);
}
#endif
