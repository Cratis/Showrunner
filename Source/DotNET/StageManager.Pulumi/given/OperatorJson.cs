// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cratis.Showrunner.StageManager.Pulumi.given;

/// <summary>
/// Reads what a component emitted into the operator's own types, strictly: a property the operator does not have, or one
/// spelled with another case, is an error rather than being ignored.
/// </summary>
public static class OperatorJson
{
    static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    /// <summary>
    /// Converts a value the engine received into JSON.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The JSON.</returns>
    public static string Serialize(object value) => JsonSerializer.Serialize(value, _options);

    /// <summary>
    /// Deserializes a value the engine received into a type of the operator.
    /// </summary>
    /// <typeparam name="T">The type of the operator.</typeparam>
    /// <param name="value">The value.</param>
    /// <returns>The deserialized value.</returns>
    public static T Deserialize<T>(object value) => JsonSerializer.Deserialize<T>(Serialize(value), _options)!;
}
#endif
