// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;

namespace Cratis.Showrunner.StageManager.Pulumi.Productions;

/// <summary>
/// The Chronicle of a Production dialing the authority it enrolled with. It opens no listener and needs no open port.
/// </summary>
public sealed class FederationDialArgs
{
    /// <summary>
    /// The key of the enrollment code in the Secret the component writes when it is given an <see cref="EnrollmentCode"/>.
    /// </summary>
    public const string DefaultEnrollmentCodeKey = "code";

    /// <summary>
    /// Gets a value indicating whether Chronicle dials. Defaults to true; the operator requires an enrollment code whenever it is.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the local event store whose inbox receives the authority's events and whose outbox is served to it.
    /// </summary>
    public required string EventStore { get; init; }

    /// <summary>
    /// Gets the namespace of the event store, or null for the operator's default (<c>Default</c>).
    /// </summary>
    public string? Namespace { get; init; }

    /// <summary>
    /// Gets the source name event store subscriptions use for the authority, or null for the operator's default (<c>hosted</c>).
    /// </summary>
    public string? LogicalSource { get; init; }

    /// <summary>
    /// Gets a value indicating whether the public outbox is also produced to the authority over the same connection.
    /// </summary>
    public bool Serve { get; init; }

    /// <summary>
    /// Gets the event types served, or none to serve every public event type.
    /// </summary>
    public IList<string> ServeEventTypes { get; init; } = [];

    /// <summary>
    /// Gets the enrollment code the authority issued. It is written, as a Pulumi secret, to the Secret named by
    /// <see cref="EnrollmentCodeSecretName"/> under the key <see cref="EnrollmentCodeSecretKey"/>.
    /// </summary>
    public Input<string>? EnrollmentCode { get; init; }

    /// <summary>
    /// Gets the name of the Secret holding the enrollment code. Created when <see cref="EnrollmentCode"/> is given, otherwise it
    /// must already exist. Defaults to <c>&lt;name&gt;-enrollment-code</c>.
    /// </summary>
    public string? EnrollmentCodeSecretName { get; init; }

    /// <summary>
    /// Gets the key of the enrollment code in that Secret. Defaults to <c>code</c>.
    /// </summary>
    public string? EnrollmentCodeSecretKey { get; init; }

    /// <summary>
    /// Builds the spec of the dial settings.
    /// </summary>
    /// <param name="enrollmentCodeSecret">The Secret holding the enrollment code, if any.</param>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec(SecretReferenceArgs? enrollmentCodeSecret) =>
        SpecExtensions.NewSpec()
            .Put("enabled", Enabled)
            .Put("eventStore", EventStore)
            .Put("namespace", Namespace)
            .Put("logicalSource", LogicalSource)
            .PutWhenTrue("serve", Serve)
            .PutList("serveEventTypes", ServeEventTypes)
            .Put("enrollmentCodeSecretRef", enrollmentCodeSecret is null ? null : SpecExtensions.SecretReference(enrollmentCodeSecret.Name, enrollmentCodeSecret.Key));
}
