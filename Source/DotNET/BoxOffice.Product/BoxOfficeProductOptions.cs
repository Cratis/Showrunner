// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Product;

/// <summary>
/// The options of a managed product that is run by an operator.
/// </summary>
public class BoxOfficeProductOptions
{
    /// <summary>
    /// The path AuthProxy calls when a person signs in, and when a person returns with a session that already exists.
    /// </summary>
    public const string DefaultSignInPath = "/.cratis/showrunner/sign-in";

    readonly List<Action<BoxOfficeOptions>> _boxOfficeConfigurations = [];

    /// <summary>
    /// Gets or sets a value indicating whether the product receives AuthProxy's sign-in callback and reports who was active.
    /// It is on by default: the callback is only ever sent when AuthProxy is configured to send it.
    /// </summary>
    public bool ReportSignIns { get; set; } = true;

    /// <summary>
    /// Gets or sets the path that receives AuthProxy's sign-in callback. It is the path AuthProxy's
    /// <c>SignInCallback.Path</c> names for this product.
    /// </summary>
    public string SignInPath { get; set; } = DefaultSignInPath;

    /// <summary>
    /// Gets or sets the secret AuthProxy sends with a sign-in callback, so the product can tell AuthProxy from any other
    /// caller on its network. Leave it empty only when the network alone keeps others from the product. Supply it from a
    /// secret store or the environment, never from a committed file.
    /// </summary>
    public string SignInSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets the configurations of Box Office, which the product options carry so a product configures everything in one place.
    /// </summary>
    internal IReadOnlyList<Action<BoxOfficeOptions>> BoxOfficeConfigurations => _boxOfficeConfigurations;

    /// <summary>
    /// Configures Box Office, which the product runs inside, such as the operator's event stores.
    /// </summary>
    /// <param name="configure">Configures the <see cref="BoxOfficeOptions"/>.</param>
    /// <returns>The options for continuation.</returns>
    public BoxOfficeProductOptions ConfigureBoxOffice(Action<BoxOfficeOptions> configure)
    {
        _boxOfficeConfigurations.Add(configure);
        return this;
    }
}
