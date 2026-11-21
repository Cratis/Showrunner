// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// A remedy was not allowed to run.
/// </summary>
/// <param name="Remedy">The remedy's name.</param>
/// <param name="Resource">The resource.</param>
/// <param name="Outcome">The assurance outcome, as its name: <c>Block</c> or <c>RequiresHumanReview</c>.</param>
/// <param name="Reason">The <see cref="OfflineRemedyReason"/> (or Direct's refusal), as its name.</param>
/// <param name="Explanation">Why, in words.</param>
public record RemedyDenied(string Remedy, Guid Resource, string Outcome, string Reason, string Explanation);
