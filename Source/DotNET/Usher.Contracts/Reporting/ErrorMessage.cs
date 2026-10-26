// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Compliance;
using Cratis.Chronicle.Compliance.GDPR;

namespace Cratis.Showrunner.Usher.Contracts.Reporting;

/// <summary>
/// The message of the error the reporter hit, as the application raised it.
/// </summary>
/// <param name="Value">The underlying value.</param>
[PII]
[ComplianceDetails("The message of an error the reporter hit; an application can put what the reporter entered into it, so it can identify them. Shown to support to work on the issue.")]
public record ErrorMessage(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="ErrorMessage"/>.
    /// </summary>
    public static readonly ErrorMessage NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to an <see cref="ErrorMessage"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ErrorMessage(string value) => new(value);
}
