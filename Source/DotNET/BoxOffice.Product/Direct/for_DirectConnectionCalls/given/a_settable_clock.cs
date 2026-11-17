// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionCalls.given;

/// <summary>
/// A <see cref="TimeProvider"/> whose time the spec moves.
/// </summary>
public class a_settable_clock : TimeProvider
{
    /// <summary>
    /// Gets or sets the current time.
    /// </summary>
    public DateTimeOffset Now { get; set; } = new(2026, 11, 17, 8, 0, 0, TimeSpan.Zero);

    /// <inheritdoc/>
    public override DateTimeOffset GetUtcNow() => Now;
}
#endif
