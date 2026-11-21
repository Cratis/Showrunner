// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// One fixed parameter of a remedy, sent as a property of its JSON body.
/// </summary>
/// <param name="Name">The parameter.</param>
/// <param name="Value">Its fixed value.</param>
public record RemedyParameterDefinition(string Name, string Value);
