// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice;

/// <summary>
/// The names of the authorization policies Box Office's commands ask the product to define.
/// </summary>
public static class BoxOfficePolicies
{
    /// <summary>
    /// Who may assign and release seats. Assigning seats is for the product's maintainers and the policy is the
    /// product's: the product registers a policy of this name, with Arc (<c>AddArcAuthorizationPolicy</c>) or with ASP.NET
    /// Core, and the application does not start until it has. Box Office grants nobody by default.
    /// </summary>
    public const string ManageSeats = "BoxOffice.ManageSeats";
}
