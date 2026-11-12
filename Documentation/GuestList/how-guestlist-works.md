---
title: How GuestList works
description: The three levels an audience is granted at, how the newest decision wins, where a product records it and what it answers when the operator is unreachable.
---

An operator decides, a product records, and the product answers from what it recorded.

```text
operator store                          product store
--------------                          -------------
GrantAudience / WithdrawAudience
  -> AudienceGrantDecided                 tenant namespace "acme":
  -> outbox: AudienceGranted  ---------->   tenant stream, user streams
                                          default namespace:
                                            system stream
                                          queries answer from these
```

## Three levels

| Level | Held in | Applies to |
| --- | --- | --- |
| Tenant | the tenant's own namespace, stream `tenant` | everyone in that tenant |
| User | the user's tenant namespace, stream `user:<id>` | one user of that tenant |
| System | the product's default namespace, stream `system` | the system and every tenant: automation and callers that are not a person |

The audiences of a request are the three added together: its tenant's, its user's and the system's. A grant and a withdrawal touch one level. If `Plus` is granted to the tenant and to one user, withdrawing it from the tenant leaves that user in it.

## The newest decision wins

Each level folds its decisions into the last one per audience, ordered by when the operator decided. A withdrawal is kept as a decision too, so an older grant that is delivered late cannot undo it. Two decisions at the same instant resolve to the one recorded last. A grant that arrives twice, for example after a retry, is recorded once: the product derives an idempotency key from the event itself.

The commands help from the operator side. Granting an audience that is already held, or withdrawing one that is not, records nothing, so the outbox does not publish the same decision again.

## When the link is down

The product never calls the operator when it answers. Its read models are in its own store, so the last known state stays while the operator is down, restarting or has not been deployed yet. When the link is back, Chronicle delivers what was missed and the product catches up.

## Granted but not declared

An application declares the audiences its model has. A grant of any other name is not refused, because a decision is a fact, and declaring the audience later should make the old grant live. It never matches a gate in the meantime, and both sides can list it: the product with the `UndeclaredGrant` query and the operator with the `UndeclaredAudienceGrant` query.

## A tenant the product does not have

A decision never creates a tenant. The operator's command refuses a tenant its `IKnownTenants` does not know, and the product ignores a decision whose tenant namespace it does not have, logging a warning. Once the tenant exists, the operator repeats the decision.
