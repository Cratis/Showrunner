// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;

namespace Cratis.Showrunner.Herald;

/// <summary>
/// Represents how an application configures Herald.
/// </summary>
/// <remarks>
/// The sender, the mail server and its password come from here and nowhere else: they are never a command, an event
/// or a read model, and Herald never logs them.
/// </remarks>
public class HeraldOptions
{
    /// <summary>
    /// Gets or sets the folder holding the templates: <c>{kind}.json</c> declaring the values of an email and
    /// <c>{kind}.{language}.html</c> for each language. Required.
    /// </summary>
    public string TemplatesFolder { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the folder holding the images the templates refer to as <c>cid:{file name}</c>. Optional when no
    /// template has an image.
    /// </summary>
    public string ImagesFolder { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the language used when an email is not available in the language asked for. Defaults to <c>en</c>.
    /// </summary>
    public EmailLanguage DefaultLanguage { get; set; } = "en";

    /// <summary>
    /// Gets or sets who the emails come from. Required.
    /// </summary>
    public EmailSenderOptions Sender { get; set; } = new();

    /// <summary>
    /// Gets or sets the brand color and footer shared by every template.
    /// </summary>
    public EmailLookOptions Look { get; set; } = new();

    /// <summary>
    /// Gets or sets how emails leave the application. Defaults to <see cref="EmailTransportKind.Smtp"/>.
    /// </summary>
    public EmailTransportKind Transport { get; set; } = EmailTransportKind.Smtp;

    /// <summary>
    /// Gets or sets the mail server, used by <see cref="EmailTransportKind.Smtp"/>.
    /// </summary>
    public SmtpOptions Smtp { get; set; } = new();

    /// <summary>
    /// Gets or sets the folder <see cref="EmailTransportKind.Folder"/> writes <c>.eml</c> files to.
    /// </summary>
    public string PickupFolder { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets how failed deliveries are retried.
    /// </summary>
    public EmailRetryOptions Retry { get; set; } = new();

    /// <summary>
    /// Gets or sets how often Herald looks for queued emails that are due. Defaults to five seconds.
    /// </summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets or sets the event store namespace the facts are recorded in. Defaults to the default namespace.
    /// </summary>
    public EventStoreNamespaceName Namespace { get; set; } = EventStoreNamespaceName.Default;
}
