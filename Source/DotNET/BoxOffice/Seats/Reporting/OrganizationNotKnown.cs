// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;

namespace Cratis.Showrunner.BoxOffice.Seats.Reporting;

/// <summary>
/// The exception that is thrown when a seat is to be reported to the operator before Box Office knows which of the
/// operator's organizations the tenant is. The report is retried once the operator has told it.
/// </summary>
/// <param name="namespace">The namespace of the tenant.</param>
public class OrganizationNotKnown(EventStoreNamespaceName @namespace)
    : Exception($"Tenant '{@namespace}' has not been told which organization it is, so its seats can not be reported to the operator yet.");
