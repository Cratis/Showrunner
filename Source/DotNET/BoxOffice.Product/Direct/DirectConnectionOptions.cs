// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Product.Direct;

/// <summary>
/// The options of the one-time page that links the product to its Chronicle with an enrollment code.
/// </summary>
public class DirectConnectionOptions
{
    /// <summary>
    /// The path of the one-time setup page.
    /// </summary>
    public const string DefaultPath = "/.cratis/showrunner/direct";

    /// <summary>
    /// Gets or sets a value indicating whether the setup page, and redemption from configuration, exist. It is off by
    /// default; while it is off the path does not exist and answers not found.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the path of the setup page.
    /// </summary>
    public string Path { get; set; } = DefaultPath;

    /// <summary>
    /// Gets or sets the subjects, as AuthProxy presents them, who may use the page. Nobody may use it while the list is
    /// empty: the first person to arrive never becomes the administrator.
    /// </summary>
    public IList<string> Administrators { get; set; } = [];

    /// <summary>
    /// Gets or sets the one host the product may be linked to. A manually typed endpoint is only accepted when this is
    /// set and the endpoint is on this host over HTTPS; an endpoint carried by the code must be on it too.
    /// </summary>
    public string ExpectedEndpointHost { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the page may open again once the link has been closed or is stuck, which
    /// only happens when the link is also unrecoverable.
    /// </summary>
    public bool AllowReenrollment { get; set; }

    /// <summary>
    /// Gets or sets an enrollment code to redeem at startup without the page. Supply it from a secret store or the
    /// environment, never from a committed file. Redeeming it again once enrolled does nothing.
    /// </summary>
    public string EnrollmentCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets how many attempts the page and the configured code may make in <see cref="AttemptWindow"/>.
    /// </summary>
    public int MaximumAttempts { get; set; } = 5;

    /// <summary>
    /// Gets or sets the window attempts are counted in.
    /// </summary>
    public TimeSpan AttemptWindow { get; set; } = TimeSpan.FromMinutes(15);
}
