// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle.EventSequences;
using Cratis.Metrics;
using Cratis.Showrunner.BoxOffice.given;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Compute.for_TenantComputeAdmission.given;

/// <summary>
/// A tenant admitting compute whose event log answers every once-only append with a given result.
/// </summary>
public class a_tenant_whose_appends_are_answered : a_tenant_admitting_compute
{
    /// <summary>
    /// Creates an admission whose once-only appends are answered.
    /// </summary>
    /// <param name="answer">The result every once-only append answers.</param>
    /// <returns>The admission.</returns>
    protected TenantComputeAdmission AdmissionAnswering(AppendResult answer) => new(
        AppendAnswers.Answering(_eventStore, answer),
        _entitlements,
        _offering,
        _metering,
        Options.Create(_options),
        _clock,
        new UnkeyedMeter<TenantComputeAdmission>(),
        Microsoft.Extensions.Logging.Abstractions.NullLogger<TenantComputeAdmission>.Instance);

    /// <summary>
    /// Admits work that has run out of allowance, which records that it did.
    /// </summary>
    /// <param name="admission">The admission.</param>
    /// <returns>The decision.</returns>
    protected async Task<AdmissionDecision> AdmitWithTheAllowanceUsedUp(TenantComputeAdmission admission)
    {
        Allow(Tokens, 1000);
        Measured(Tenants.Today, Tokens, 1000);
        return await admission.Admit(Tokens);
    }
}
#endif
