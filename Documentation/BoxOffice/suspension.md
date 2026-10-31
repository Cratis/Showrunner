---
title: Suspension and read-only tenants
description: How a suspended tenant, or one whose trial closed without seats, can read but not change anything, and which commands stay allowed.
---

A tenant is read-only while the operator has it suspended, or when its trial closed and no seats were granted or bought to replace the trial's. A read-only tenant can read everything and change nothing.

## The filter

Box Office adds a command filter that runs before every command. While the tenant is read-only it refuses the command with an authorization failure whose reason is one of:

| Reason | When |
| --- | --- |
| `TenantSuspended` | The operator suspended the tenant and has not reinstated it. The suspension wins when the trial has also closed. |
| `TrialClosed` | The trial's last day has passed, or the operator closed it, and no seats were granted or bought. |

`TenantReinstated` lifts a suspension immediately: the next command goes through. The trial's end is computed from the clock, so a trial ends the day after its last day without any event, and seats granted or bought end the read-only state as soon as they are recorded.

## Commands a read-only tenant may still run

Mark a command `AllowedWhileSuspended` when a read-only tenant must still be able to run it, such as reporting a problem to support, exporting its data or leaving:

```csharp
[Command]
[AllowedWhileSuspended]
public record ReportProblem(string Problem)
{
    public ProblemReported Handle() => new(Problem);
}
```

The marker lifts only the read-only refusal. It does not lift the seat requirement of a command marked `RequiresSeat`; see [Seat licensing](./seats.md).

## Showing the reason

The `CurrentStanding` query returns a `TenantStanding` that tells the frontend whether the tenant is suspended, why the operator suspended it and why it is read-only, so the page can show a banner:

```ts
import { CurrentStanding, ReadOnlyReason } from '@cratis/showrunner.boxoffice';

const [standing] = CurrentStanding.use();
if (standing.data?.readOnlyReason !== ReadOnlyReason.none) {
    // show the banner
}
```

A command that was refused answers with an unauthorized result carrying the reason, which the frontend can also use to explain a refused action.

`SuspensionBanner` and `NoLicense` in [React components](./react.md#banners) show this to the user, and the components refuse nothing themselves: the backend does.

