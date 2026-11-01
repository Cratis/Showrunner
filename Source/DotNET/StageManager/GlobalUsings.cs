// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

global using System.Text.Json;
global using System.Text.Json.Serialization;
global using k8s;
global using k8s.Models;
global using KubeOps.Abstractions.Entities;
global using KubeOps.Abstractions.Entities.Attributes;
global using KubeOps.Abstractions.Reconciliation;
global using KubeOps.Abstractions.Reconciliation.Controller;
global using KubeOps.KubernetesClient;
global using Microsoft.Extensions.Logging;

// The spec usings below are compiled into Debug builds only.
#if DEBUG
global using Cratis.Specifications;
global using NSubstitute;
global using Xunit;
#endif
