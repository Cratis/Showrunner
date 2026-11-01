---
title: Security boundary
description: What the Stage operator exposes of a published application, what it never exposes, and how that is enforced in layers.
---

A published application is reachable by the public, so its internals must not be. The boundary is enforced in layers; each is independent of the next.

## What is never exposed

| Internal | Where it is kept |
| --- | --- |
| Stage, port 9090 | Bound to the loopback interface of the pod. The container declares no port for it. |
| Chronicle, port 35000 | Inside the Stage process. Nothing declares or routes it. |
| MongoDB, port 27017 | A headless `Service` inside the namespace, reachable only from the application pod by `NetworkPolicy`. |

The `Service` of an application has exactly one port: the sign-in proxy. The `Ingress` routes to that `Service` and to nothing else. Specifications of the generated objects check that neither 9090 nor 35000 appears on a container, a `Service` or an `Ingress`.

## The layers

1. **Sign-in proxy.** Every request enters through the proxy container in the same pod, which authenticates the user and forwards to Stage on `localhost:9090` with `x-ms-client-principal*` headers. Stage runs in its published mode with `Stage__Published__Authentication__Mode=TrustedProxy`. An application is refused without `spec.authentication`.
2. **Stage published mode.** Stage itself does not serve its API explorer, Chronicle Workbench, read-model browser, loading or semantic routes. This is the primary enforcement of the denied paths.
3. **Ingress path denial.** `/scalar`, `/openapi`, `/workbench`, `/read-models`, `/stage/load` and `/stage/semantic` are routed to a `Service` without endpoints (`<name>-denied`). Ingress path matching prefers the longest prefix, so these paths never reach the proxy, and this works with every ingress controller. Setting `DenyPathsWithSnippet` additionally adds an `ingress-nginx` `server-snippet` that answers `404`; it is off by default because many clusters disable snippet annotations.
4. **NetworkPolicy.** The application pod accepts traffic only on the proxy port (plus the Notes collector port when configured); the database pod accepts traffic only from the application pod on 27017. The policy restricts ingress only; egress from the application pod, including to the Notes collector, is left to the cluster's own egress policy. This needs a network plugin that enforces `NetworkPolicy`.

The pod does not mount a service account token, drops all capabilities and disallows privilege escalation.

## What the operator itself can do

The operator runs with a least-privilege service account: it manages the objects it generates and reads pod metrics, and nothing else. It does not read secrets. The chart creates a `ClusterRole` for the cluster-wide mode or a `Role` per watched namespace.
