// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Herald.Sending;

/// <summary>
/// Turns an exception into a <see cref="FailureReason"/> that is safe to keep in an event and a log: the description
/// Herald wrote, never the mail server's password nor the recipient's address.
/// </summary>
/// <param name="options">The <see cref="HeraldOptions"/>.</param>
public class FailureReasons(IOptions<HeraldOptions> options)
{
    const int MaxLength = 300;

    /// <summary>
    /// Describes a failure.
    /// </summary>
    /// <param name="exception">The failure.</param>
    /// <param name="address">The address the email was for, to leave out of the description.</param>
    /// <returns>The reason.</returns>
    public FailureReason For(Exception exception, string address)
    {
        // Only Herald's own exceptions carry words written for this purpose; the message of anything else may hold what a
        // server said, so only its kind is kept.
        var text = exception is Rendering.EmailCannotBeRendered or Transport.EmailTransportException
            ? exception.Message
            : $"Delivery failed with {exception.GetType().Name}.";

        text = text.ReplaceLineEndings(" ");
        foreach (var secret in new[] { options.Value.Smtp.Password, address }.Where(secret => secret.Length > 0))
        {
            text = text.Replace(secret, "***", StringComparison.OrdinalIgnoreCase);
        }

        return text.Length > MaxLength ? text[..MaxLength] : text;
    }
}
