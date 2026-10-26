// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.WorkItems;

/// <summary>
/// The address the reporter can open a work item at in its tracker. It is <see cref="NotSet"/> when the tracker is
/// not reachable for the reporter, for example a private repository. Only an absolute <c>https</c> or <c>http</c>
/// address is a web address the reporter is offered; see <see cref="IsWebAddress"/>.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record WorkItemUrl(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="WorkItemUrl"/>.
    /// </summary>
    public static readonly WorkItemUrl NotSet = new(string.Empty);

    /// <summary>
    /// Gets whether the address is an absolute <c>https</c> or <c>http</c> address. Anything else, such as a
    /// <c>javascript:</c> or <c>data:</c> address, is never offered to the reporter as a link.
    /// </summary>
    public bool IsWebAddress =>
        Uri.TryCreate(Value, UriKind.Absolute, out var address) &&
        (address.Scheme == Uri.UriSchemeHttps || address.Scheme == Uri.UriSchemeHttp);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="WorkItemUrl"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator WorkItemUrl(string value) => new(value);
}
