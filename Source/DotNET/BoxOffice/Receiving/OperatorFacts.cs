// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Contracts.Offers;

namespace Cratis.Showrunner.BoxOffice.Receiving;

/// <summary>
/// The operator opened a trial for the tenant.
/// </summary>
/// <param name="Ends">The last day of the trial.</param>
/// <param name="Seats">The seats the trial has.</param>
[EventType]
public record OperatorOpenedTrial(TrialEnd Ends, SeatCount Seats);

/// <summary>
/// The operator moved the last day of the tenant's trial.
/// </summary>
/// <param name="Ends">The new last day of the trial.</param>
[EventType]
public record OperatorMovedTrialEnd(TrialEnd Ends);

/// <summary>
/// The operator closed the tenant's trial.
/// </summary>
/// <param name="Closure">How the trial closed.</param>
[EventType]
public record OperatorClosedTrial(TrialClosure Closure);

/// <summary>
/// The operator set how many seats the tenant may assign.
/// </summary>
/// <param name="Capacity">The total number of seats.</param>
/// <param name="Until">The last day the seats are the tenant's.</param>
/// <param name="Source">Whether the seats were granted or bought.</param>
[EventType]
public record OperatorSetSeatCapacity(SeatCount Capacity, EntitlementEnd Until, SeatCapacitySource Source);

/// <summary>
/// The operator granted the tenant a module.
/// </summary>
/// <param name="Module">The module.</param>
/// <param name="Until">The last day the tenant has the module.</param>
[EventType]
public record OperatorGrantedModule(ModuleId Module, EntitlementEnd Until);

/// <summary>
/// The operator let the tenant try a module until a day.
/// </summary>
/// <param name="Module">The module.</param>
/// <param name="Ends">The last day of the trial of the module.</param>
[EventType]
public record OperatorGrantedModuleTrial(ModuleId Module, TrialEnd Ends);

/// <summary>
/// The operator let the tenant look at a module it does not have.
/// </summary>
/// <param name="Module">The module.</param>
/// <param name="Until">The last day the tenant may look at the module.</param>
[EventType]
public record OperatorOpenedModulePreview(ModuleId Module, EntitlementEnd Until);

/// <summary>
/// The operator took a module away from the tenant.
/// </summary>
/// <param name="Module">The module.</param>
[EventType]
public record OperatorRevokedModule(ModuleId Module);

/// <summary>
/// The operator set how much of a meter the tenant may use.
/// </summary>
/// <param name="Meter">The meter.</param>
/// <param name="Quantity">How much of the meter the tenant may use in a period.</param>
/// <param name="Period">The period the quantity is for.</param>
/// <param name="Overage">What happens beyond the quantity.</param>
[EventType]
public record OperatorSetAllowance(MeterId Meter, MeterQuantity Quantity, AllowancePeriod Period, OverageBehavior Overage);

/// <summary>
/// The operator suspended the tenant.
/// </summary>
/// <param name="Cause">Why the tenant was suspended.</param>
[EventType]
public record OperatorSuspendedTenant(SuspensionCause Cause);

/// <summary>
/// The operator lifted the suspension of the tenant.
/// </summary>
[EventType]
public record OperatorReinstatedTenant();

/// <summary>
/// The operator's organization behind the tenant is known, so the product can report back under it.
/// </summary>
/// <param name="Organization">The operator's identity for the organization.</param>
[EventType]
public record TenantOrganizationIdentified(OrganizationId Organization);

/// <summary>
/// The operator published an offer.
/// </summary>
/// <param name="Name">The name of the offer.</param>
/// <param name="Modules">The modules the offer includes.</param>
/// <param name="IncludedSeats">The seats the offer includes.</param>
/// <param name="Prices">What the offer costs.</param>
[EventType]
public record OperatorPublishedOffer(OfferName Name, IEnumerable<ModuleId> Modules, SeatCount IncludedSeats, IEnumerable<OfferPrice> Prices);

/// <summary>
/// The operator stopped selling an offer.
/// </summary>
[EventType]
public record OperatorWithdrewOffer();
