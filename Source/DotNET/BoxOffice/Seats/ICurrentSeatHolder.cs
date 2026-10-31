// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Seats;

/// <summary>
/// Tells who the signed-in user is as a seat holder.
/// </summary>
public interface ICurrentSeatHolder
{
    /// <summary>
    /// Gets who the signed-in user is, or <see langword="null"/> when nobody is signed in.
    /// </summary>
    SeatHolder? Current { get; }
}
