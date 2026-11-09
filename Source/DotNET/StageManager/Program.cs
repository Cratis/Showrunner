// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Performances;
using Cratis.Showrunner.StageManager.Productions;
using KubeOps.Operator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddKubernetesOperator()
    .RegisterComponents();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IEnvironmentResolver, CratisEnvironmentResolver>();
builder.Services.AddSingleton<IClusterResources, ClusterResources>();
builder.Services.AddSingleton<PerformancePreparation>();
builder.Services.AddSingleton<PerformanceReconciler>();
builder.Services.AddSingleton<IProductionCluster, ProductionCluster>();
builder.Services.AddSingleton<ProductionPreparation>();
builder.Services.AddSingleton<ProductionReconciler>();

await builder.Build().RunAsync();
