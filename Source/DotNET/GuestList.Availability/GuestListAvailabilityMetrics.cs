// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Metrics;

namespace Cratis.Showrunner.GuestList.Availability;

/// <summary>
/// The metrics of GuestList's availability part.
/// </summary>
/// <remarks>
/// <para>
/// <c>cratis.showrunner.guestlist.audience_lag</c> is the time in seconds since the product last received a change to its
/// audiences. It grows while nothing arrives, so a value that keeps climbing past what the operator normally takes between
/// decisions means the link to the operator may be down; the audiences in force are then the last known. No measurement is
/// reported until the first change has been received since the product started.
/// </para>
/// </remarks>
public sealed class GuestListAvailabilityMetrics : IDisposable
{
    /// <summary>
    /// The name of the meter.
    /// </summary>
    public const string MeterName = "Cratis.Showrunner.GuestList";

    /// <summary>
    /// The name of the lag instrument.
    /// </summary>
    public const string LagName = "cratis.showrunner.guestlist.audience_lag";

    readonly Meter _meter;

    /// <summary>
    /// Initializes a new instance of the <see cref="GuestListAvailabilityMetrics"/> class.
    /// </summary>
    /// <param name="meters">The <see cref="IMeterFactory"/>.</param>
    /// <param name="feed">The feed whose lag is reported.</param>
    public GuestListAvailabilityMetrics(IMeterFactory meters, AudienceChangeFeed feed)
    {
        _meter = meters.Create(MeterName);
        _meter.CreateObservableGauge<double>(
            LagName,
            () => feed.Lag is { } lag ? [new Measurement<double>(lag.TotalSeconds)] : Array.Empty<Measurement<double>>(),
            "s",
            "Time since the product last received a change to its audiences");
    }

    /// <inheritdoc/>
    public void Dispose() => _meter.Dispose();
}
