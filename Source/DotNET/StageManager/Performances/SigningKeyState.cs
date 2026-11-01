// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Both generated key rings of a Performance: Ante's invitation signing key and AuthProxy's attestation signing key.
/// </summary>
/// <param name="Invitation">The invitation key ring.</param>
/// <param name="Attestation">The attestation key ring.</param>
/// <param name="Rotation">The rotation counter that produced the current keys.</param>
public record SigningKeyState(KeyRing Invitation, KeyRing Attestation, int Rotation)
{
    /// <summary>
    /// The key of the Secret entry holding the serialized state.
    /// </summary>
    public const string SecretKey = "state.json";

    static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Serializes the state for storing in a Secret.
    /// </summary>
    /// <returns>The JSON.</returns>
    public string Serialize() => JsonSerializer.Serialize(this, _options);

    /// <summary>
    /// Reads the state back from a Secret.
    /// </summary>
    /// <param name="json">The JSON, or null when the Secret does not exist yet.</param>
    /// <returns>The state, or null when there is none or it cannot be read.</returns>
    public static SigningKeyState? Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<SigningKeyState>(json, _options);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
