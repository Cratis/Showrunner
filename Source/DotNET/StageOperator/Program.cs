// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.Hosting;
using Cratis.Showrunner.StageOperator.Kubernetes;
using Cratis.Showrunner.StageOperator.Reconciling;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<StageOperatorOptions>(builder.Configuration.GetSection(StageOperatorOptions.SectionName));
builder.Services.AddSingleton(TimeProvider.System);

// In a pod the in-cluster service account is used; on a developer machine the current kubeconfig context.
builder.Services.AddSingleton<IKubernetes>(_ => new Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig()));
builder.Services.AddSingleton<IKubernetesGateway, KubernetesGateway>();
builder.Services.AddSingleton<Reconciler>();
builder.Services.AddHostedService<ReconcileService>();
builder.Services.AddHostedService<UsageSamplerService>();

await builder.Build().RunAsync();
