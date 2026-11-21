// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;

namespace Cratis.Showrunner.StageManager.Pulumi.Performances;

/// <summary>
/// One identity provider the application signs users in with.
/// </summary>
public sealed class AuthProxyProviderArgs
{
    /// <summary>
    /// Gets the kind of provider.
    /// </summary>
    public required AuthProxyProviderKind Kind { get; init; }

    /// <summary>
    /// Gets the identity: the model's alias, otherwise the provider name.
    /// </summary>
    public required string Identity { get; init; }

    /// <summary>
    /// Gets the client id. Give it with <see cref="ClientSecret"/> and the component writes a Secret the operator reads, or leave both out to take them from the Cratis environment.
    /// </summary>
    public Input<string>? ClientId { get; init; }

    /// <summary>
    /// Gets the client secret, kept as a Pulumi secret.
    /// </summary>
    public Input<string>? ClientSecret { get; init; }

    /// <summary>
    /// Gets the name of an existing Secret that holds the client id and the client secret, used instead of <see cref="ClientId"/> and <see cref="ClientSecret"/>.
    /// </summary>
    public string? CredentialsSecretName { get; init; }

    /// <summary>
    /// Gets the key of the existing Secret that holds the client id, or null for <c>clientId</c>.
    /// </summary>
    public string? ClientIdKey { get; init; }

    /// <summary>
    /// Gets the key of the existing Secret that holds the client secret, or null for <c>clientSecret</c>.
    /// </summary>
    public string? ClientSecretKey { get; init; }

    /// <summary>
    /// Gets the scopes the provider is asked for, or none to keep the provider's default.
    /// </summary>
    public IList<string> Scopes { get; init; } = [];

    /// <summary>
    /// Builds the spec of the provider, writing the credentials Secret when the credentials are given as values.
    /// </summary>
    /// <param name="context">The context of the Performance.</param>
    /// <returns>The spec.</returns>
    /// <exception cref="ArgumentException">Thrown when the credentials are incomplete or given two ways.</exception>
    internal Dictionary<string, object> ToSpec(PerformanceContext context)
    {
        var hasValues = ClientId is not null || ClientSecret is not null;
        if (hasValues && (ClientId is null || ClientSecret is null))
        {
            throw new ArgumentException($"Give the provider '{Identity}' both ClientId and ClientSecret.");
        }

        if (hasValues && CredentialsSecretName is not null)
        {
            throw new ArgumentException($"Give the provider '{Identity}' ClientId and ClientSecret or CredentialsSecretName, not both.");
        }

        return SpecExtensions.NewSpec()
            .PutEnum("kind", Kind)
            .Put("identity", Identity)
            .Put("credentialsSecretRef", Credentials(context, hasValues))
            .PutList("scopes", Scopes);
    }

    static string Slug(string identity) => new([.. identity.ToLowerInvariant().Select(character => char.IsLetterOrDigit(character) ? character : '-')]);

    Dictionary<string, object>? Credentials(PerformanceContext context, bool hasValues)
    {
        if (hasValues)
        {
            return DeclareCredentials(context);
        }

        return CredentialsSecretName is null ? null : ExistingCredentials();
    }

    Dictionary<string, object> ExistingCredentials() => SpecExtensions.NewSpec()
        .Put("name", CredentialsSecretName)
        .Put("clientIdKey", ClientIdKey)
        .Put("clientSecretKey", ClientSecretKey);

    Dictionary<string, object> DeclareCredentials(PerformanceContext context)
    {
        var secretName = $"{context.KubernetesName}-{Slug(Identity)}-credentials";
        context.Declared.Add(StageManagerSecrets.Create(
            context.Parent,
            context.Provider,
            $"{context.Name}-{Slug(Identity)}-credentials",
            secretName,
            context.Namespace,
            context.Labels,
            [new("clientId", ClientId!), new("clientSecret", ClientSecret!)],
            context.DependsOn));
        return SpecExtensions.NewSpec().Put("name", secretName);
    }
}
