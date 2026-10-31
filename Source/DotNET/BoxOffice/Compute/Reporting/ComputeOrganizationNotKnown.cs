// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;

namespace Cratis.Showrunner.BoxOffice.Compute.Reporting;

/// <summary>
/// The exception that is thrown when the operator can not be told about a tenant's compute, because the tenant has not
/// been told which organization it is.
/// </summary>
/// <param name="namespace">The tenant's namespace.</param>
public class ComputeOrganizationNotKnown(EventStoreNamespaceName @namespace)
    : Exception($"Tenant '{@namespace}' has not been told which organization it is, so its allowance running out can not be reported to the operator yet.");
