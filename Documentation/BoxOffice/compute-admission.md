---
title: Compute admission
description: How a product decides, before a unit of work starts, whether the tenant may run it - concurrency, the operator's allowance, the tenant's budget and what happens beyond them.
---

Metering says what a tenant used. Admission decides, before a unit of work starts, whether the tenant may start it. A product calls `IComputeAdmission.Admit` before it starts work, such as an agent run, and ends the work when it stops.

```csharp
public class WorkDispatch(IComputeAdmission admission)
{
    public async Task Start(Run run)
    {
        var decision = await admission.Admit("agent-runs");
        if (!decision.IsAdmitted)
        {
            // Turn the run away, or wait until decision.ParkedUntil.
            return;
        }

        try
        {
            await Execute(run);
        }
        finally
        {
            if (decision.Work is { } work)
            {
                await admission.End(work);
            }
        }
    }
}
```

`Admit` throws `MeterNotDeclared` for a meter the [offering](./offering.md) does not declare. Admission needs the offering and [metering](./metering.md): `AddBoxOfficeOffering` and `AddBoxOfficeMetering` provide what `IComputeAdmission` reads, and resolving it fails when they are missing.

## What can be decided

| Outcome | Meaning |
| --- | --- |
| `Admitted` | Start the work. When the meter has a concurrency limit the decision carries the `Work` to end when the work stops. |
| `AtConcurrencyLimit` | The tenant already runs as many units of work of the meter as it may. Try again later. |
| `AllowanceExhausted` | The tenant used all the operator allows for the period, and the allowance's overage is `Block`. |
| `BudgetReached` | The tenant used all its budget for the period, and the overage is `Block`. |
| `Parked` | The allowance or the budget is used up and the overage is `Park`. `ParkedUntil` is when the period renews, and `Because` says which limit parked the work. |

The checks run in this order, so the first that fails decides:

1. **Allowance.** The period's usage is read from the metering buckets and compared with the operator's `AllowanceSet` for the meter. A meter the operator set no allowance for allows nothing. The period is a day or a month, counted in UTC; a month starts on the 1st.
2. **Budget.** A budget is a lower ceiling than the allowance, set per meter by a maintainer of the product, for example to stop a tenant from spending its whole allowance on one purpose. Usage is compared with it the same way.
3. **Concurrency.** For a meter declared with a concurrency limit and given a limit, the tenant may run at most that many units of work at the same time.

## Overage

What happens to work beyond the allowance or budget is the allowance's overage, set by the operator per meter:

- `Block` turns the work away. The decision says `AllowanceExhausted` or `BudgetReached`, and the product decides what to tell its user.
- `Park` makes the work wait. The decision is `Parked` with the time the period renews, midnight UTC at the start of the next day or of the 1st of the next month, so the product can schedule the work then.

Billing in arrears is not part of Box Office yet.

## Concurrency is decided from facts

A concurrency limit is decided from the tenant's own start and stop facts in the event log, never from a projection that could be behind. Admitting a unit of work appends `ComputeWorkStarted`, guarded by a concurrency scope over the start and stop facts it read. When several instances of the product ask for the last slot at the same time, one append wins; the others find the facts changed, read them again, see the slot is taken and are turned away with `AtConcurrencyLimit`. Ending the work appends `ComputeWorkStopped`, once however many times it is ended.

Only a meter declared with `hasConcurrencyLimit: true` and given a limit records anything. Work of any other meter is admitted or refused from usage alone and leaves no facts.

The facts of a meter are kept per month, and admission reads the current and the previous month. Work that never ended, because the product crashed while it ran, would hold its slot for ever, so it counts as running only for `MaximumWorkDuration` (12 hours by default, at most seven days):

```csharp
builder.AddBoxOffice(boxOffice => boxOffice.MaximumWorkDuration = TimeSpan.FromHours(4));
```

Set it above the longest unit of work the product runs.

## Usage trails the work

Usage is what [metering](./metering.md) has measured, so it trails the work by the metering's poll interval, and work that is running has not been measured yet. A tenant can go slightly beyond its allowance by the work that was running when it was reached. The concurrency limit is exact.

## The allowance running out is recorded once

The first refusal for an allowance in a period appends `ComputeAllowanceReached` to the tenant's stream, and Box Office reports it to the operator as the public `AllowanceExhausted(TenantId, Meter, Day, Overage)` under the tenant's organization. Later refusals in the same period append nothing: they are logged and counted, because an attempt that was turned away is not a fact about the tenant. The next period records it again. A budget reached is not reported to the operator; the operator did not set it.

Two metrics count what admission does, `boxoffice.compute.admitted` and `boxoffice.compute.refused`, tagged with the meter and, for refusals, the outcome.

## Budgets and concurrency limits

The product's maintainers set a tenant's budgets and concurrency limits with two commands. Both are authorized by the `BoxOffice.ManageCompute` policy, which the product registers the same way as `BoxOffice.ManageSeats` for [seats](./seats.md); until it has, the commands and the query fail rather than succeed.

| Command | What it does |
| --- | --- |
| `SetComputeBudget(Meter, Quantity)` | Sets how much of the meter the tenant may use in the allowance's period. It must be more than zero and not more than the operator's allowance. |
| `SetComputeConcurrencyLimit(Meter, Limit)` | Sets how many units of work of the meter the tenant may run at once. The meter must be declared with a concurrency limit, and the limit at least one. |

Setting what is already set appends nothing. If the operator later lowers the allowance below a budget, the allowance is what turns work away. The `CurrentLimits` query returns the tenant's `ComputeLimits` for the product's maintainers, one `MeterLimit` per meter with its `Budget` and `ConcurrencyLimit`; see [Frontend proxies](./frontend-proxies.md).

## Reading where a tenant stands

The `CurrentUsage` query returns one `MeterStanding` per meter the product declares, ordered by name, for the product's maintainers; it is authorized by `BoxOffice.ManageCompute` like the commands above. Each carries what the tenant has used since its allowance period began, the allowance and its period, what happens beyond it (`Block` or `Park`), the first day of the next period, the budget and concurrency limit when they are set, and whether the meter has a concurrency limit at all. It reads what [metering](./metering.md) has measured, the same figures admission decides from, so it trails running work in the same way. The [usage page](./react.md#pages) shows it.

