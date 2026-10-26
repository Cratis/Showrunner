// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Integration.given;

/// <summary>
/// Waits for something that happens asynchronously in the kernel.
/// </summary>
public static class Eventually
{
    /// <summary>
    /// How long to wait by default.
    /// </summary>
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Polls until a value is there.
    /// </summary>
    /// <typeparam name="T">The type of value.</typeparam>
    /// <param name="get">Gets the value, or <see langword="null"/> while it is not there yet.</param>
    /// <param name="what">What is waited for, for the timeout message.</param>
    /// <param name="timeout">How long to wait.</param>
    /// <returns>The value.</returns>
    /// <exception cref="TimeoutException">The value was not there in time.</exception>
    public static async Task<T> Get<T>(Func<Task<T?>> get, string what, TimeSpan? timeout = default)
        where T : class
    {
        var deadline = DateTimeOffset.UtcNow + (timeout ?? DefaultTimeout);
        while (true)
        {
            if (await get() is { } value)
            {
                return value;
            }

            if (DateTimeOffset.UtcNow > deadline)
            {
                throw new TimeoutException($"Timed out waiting for {what}.");
            }

            await Task.Delay(250);
        }
    }
}
