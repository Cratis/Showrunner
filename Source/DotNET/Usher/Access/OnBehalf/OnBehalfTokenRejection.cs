// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// Why a token on a call from AuthProxy was not accepted.
/// </summary>
public enum OnBehalfTokenRejection
{
    /// <summary>
    /// The call carried no bearer token.
    /// </summary>
    Missing = 0,

    /// <summary>
    /// The token is not a well-formed signed token.
    /// </summary>
    Malformed = 1,

    /// <summary>
    /// The token is signed with a key that is not configured.
    /// </summary>
    UnknownKey = 2,

    /// <summary>
    /// The signature does not verify, or the token is not RS256.
    /// </summary>
    InvalidSignature = 3,

    /// <summary>
    /// The token is from another issuer.
    /// </summary>
    WrongIssuer = 4,

    /// <summary>
    /// The token is for another audience.
    /// </summary>
    WrongAudience = 5,

    /// <summary>
    /// The token expired, or lives longer than Usher accepts.
    /// </summary>
    Expired = 6,

    /// <summary>
    /// The token is not valid yet.
    /// </summary>
    NotYetValid = 7,

    /// <summary>
    /// The token was issued for another purpose than the endpoint's.
    /// </summary>
    WrongPurpose = 8,

    /// <summary>
    /// The token is bound to another HTTP method.
    /// </summary>
    WrongMethod = 9,

    /// <summary>
    /// The token is bound to another target.
    /// </summary>
    WrongTarget = 10,

    /// <summary>
    /// The token is bound to another body.
    /// </summary>
    WrongBody = 11,

    /// <summary>
    /// The token lacks a claim the session needs.
    /// </summary>
    MissingClaims = 12,

    /// <summary>
    /// The token was already used.
    /// </summary>
    Replayed = 13
}
