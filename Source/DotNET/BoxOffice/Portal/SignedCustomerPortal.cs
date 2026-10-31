// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Cratis.Chronicle;
using Cratis.Showrunner.BoxOffice.Seats;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Portal;

/// <summary>
/// Links to the operator's customer portal that the portal can trust: each carries the tenant, what the person wants to
/// do, and when the link stops working, signed with a key the product and the operator share.
/// </summary>
/// <param name="eventStore">The event store of the tenant, whose namespace is the tenant.</param>
/// <param name="options">The Box Office options.</param>
/// <param name="time">The clock.</param>
public class SignedCustomerPortal(IEventStore eventStore, IOptions<BoxOfficeOptions> options, TimeProvider time) : ICustomerPortal
{
    /// <inheritdoc/>
    public bool IsAvailable => options.Value.CustomerPortalUrl is not null && !string.IsNullOrEmpty(options.Value.CustomerPortalSigningKey);

    /// <summary>
    /// Gets the text the signature is made over, so the portal can make it the same way.
    /// </summary>
    /// <param name="tenant">The tenant.</param>
    /// <param name="intent">What the person wants to do.</param>
    /// <param name="offer">The offer, or empty.</param>
    /// <param name="person">The person as `provider:subject`, or empty.</param>
    /// <param name="expires">When the link stops working, in seconds since 1970-01-01 UTC.</param>
    /// <returns>The text.</returns>
    public static string Canonical(string tenant, string intent, string offer, string person, long expires) =>
        string.Join('\n', tenant, intent, offer, person, expires.ToString(CultureInfo.InvariantCulture));

    /// <inheritdoc/>
    public string LinkFor(PortalIntent intent, Guid? offer, SeatHolder? person)
    {
        if (!IsAvailable)
        {
            throw new CustomerPortalNotConfigured();
        }

        var configured = options.Value;
        var tenant = eventStore.Namespace.Value;
        var intentName = intent.ToString().ToLowerInvariant();
        var offerText = offer?.ToString("D") ?? string.Empty;
        var personText = person is null ? string.Empty : $"{person.IdentityProvider.Value}:{person.Subject.Value}";
        var expires = (time.GetUtcNow() + configured.CustomerPortalLinkLifetime).ToUnixTimeSeconds();

        var signature = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(configured.CustomerPortalSigningKey!),
            Encoding.UTF8.GetBytes(Canonical(tenant, intentName, offerText, personText, expires)));

        var query = HttpUtility.ParseQueryString(string.Empty);
        query["tenant"] = tenant;
        query["intent"] = intentName;
        if (offerText.Length > 0)
        {
            query["offer"] = offerText;
        }

        if (personText.Length > 0)
        {
            query["person"] = personText;
        }

        query["expires"] = expires.ToString(CultureInfo.InvariantCulture);
        query["signature"] = Convert.ToBase64String(signature).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        var builder = new UriBuilder(configured.CustomerPortalUrl!) { Query = query.ToString() };
        return builder.Uri.AbsoluteUri;
    }
}
