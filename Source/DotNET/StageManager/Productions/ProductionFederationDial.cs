// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// The Chronicle of a Production dialing the authority it enrolled with.
/// </summary>
public class ProductionFederationDial
{
    /// <summary>
    /// The key of the enrollment code in its Secret when none is named.
    /// </summary>
    public const string DefaultEnrollmentCodeKey = "code";

    /// <summary>
    /// Gets or sets whether Chronicle dials.
    /// </summary>
    [Description("Dials the authority this Chronicle enrolled with.")]
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the local event store whose outbox is served and whose inbox receives events.
    /// </summary>
    [Description("The local event store whose inbox receives the authority's events and whose outbox is served to it.")]
    public string EventStore { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the local namespace.
    /// </summary>
    [Description("The namespace of the event store. Defaults to Default.")]
    public string Namespace { get; set; } = "Default";

    /// <summary>
    /// Gets or sets the logical source name of the authority.
    /// </summary>
    [Description("The source name event store subscriptions use for the authority. Defaults to hosted.")]
    public string LogicalSource { get; set; } = "hosted";

    /// <summary>
    /// Gets or sets whether the public outbox is produced to the authority.
    /// </summary>
    [Description("Also produces this Chronicle's public outbox to the authority over the same connection: the sending side of a one-way link.")]
    public bool Serve { get; set; }

    /// <summary>
    /// Gets or sets the event types served. Empty serves every public event type.
    /// </summary>
    [Description("The event types served. Empty serves every public event type.")]
    public IList<string> ServeEventTypes { get; set; } = [];

    /// <summary>
    /// Gets or sets the Secret holding the enrollment code the authority issued.
    /// </summary>
    [Description("The Secret holding the enrollment code the authority issued, under the key code unless a key is named. Mounted as a file.")]
    public ProductionSecretReference? EnrollmentCodeSecretRef { get; set; }

    /// <summary>
    /// Gets the key of the enrollment code in its Secret.
    /// </summary>
    /// <returns>The key named, or code.</returns>
    public string EnrollmentCodeKey() => string.IsNullOrEmpty(EnrollmentCodeSecretRef?.Key) ? DefaultEnrollmentCodeKey : EnrollmentCodeSecretRef.Key;
}
