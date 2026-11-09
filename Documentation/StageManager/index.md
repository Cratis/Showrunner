---
title: Stage Manager
description: Run Cratis applications with Showrunner's Stage Manager Kubernetes operator, using Performance resources for environment, AuthProxy and Ante deployment.
---

Stage Manager is the Kubernetes operator of Showrunner. Unlike the other parts it is not a library an application hosts: it is a service you install in a cluster, and it owns the custom resources of the `showrunner.cratis.io` API group, version `v1alpha1`.

| Resource | What it is | State |
| --- | --- | --- |
| `Performance` | One application running in one Cratis environment, with its AuthProxy and, when it has onboarding, its own Ante. The application can be a published Stage application, and can have a lifetime | Reconciled |
| `Production` | [Chronicle for one or more applications](./production.md): TLS, an external or managed database, an optional Neo4j graph, named sinks and a connection Secret | Reconciled |
| `Understudy` | How a Production is backed up | Schema only |
| `Restore` | Brings a backup back | Schema only |
| `Rehearsal` | Proves that backups restore | Schema only |
| `BackstagePass` | Who may use the Stage Manager API | Schema only |
| `ControlRoom` | An outbound connection to a control plane | Schema only |

The custom resource definitions are generated from the C# types in `Source/DotNET/StageManager`; there is no hand-written CRD. Schema-only resources can be created and read, and nothing acts on them yet.

Start by [installing the operator](./install.md), then prepare the [environment and secrets](./environments-and-secrets.md) it will resolve for your application. With those inputs ready, choose one application path:

- [Run an application with a Performance](./performance.md) when you have an application image and a Chronicle connection. This guide also explains the resources, status, AuthProxy and Ante configuration shared by Performances.
- [Run Chronicle with a Production](./production.md) when your applications need a Chronicle in the cluster. A Performance then connects to it through `productionRef`.
- [Run a published Stage application](./published-stage-applications.md) when you have an exported Stage package to run on the generic Stage runtime. This path covers its storage and sign-in requirements, releases and rollbacks, and the lifetime and usage reference used by both paths.
