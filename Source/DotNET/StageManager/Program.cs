// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using Cratis.Showrunner.StageManager.Performances;
using Cratis.Showrunner.StageManager.Productions;
using Cratis.Showrunner.StageManager.Understudies;
using Cratis.Showrunner.StageManager.Understudies.Jobs;
using Cratis.Showrunner.StageManager.Understudies.Reporting;
using Cratis.Showrunner.StageManager.Understudies.Storage;
using KubeOps.Operator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// The Understudy image runs the same assembly as a job: `understudy backup|keys|vault-snapshot`. It reads its work from the environment and exits non-zero when it fails.
if (args.Length > 1 && args[0] == UnderstudyJobKinds.Command)
{
    using var loggers = LoggerFactory.Create(logging => logging.AddSimpleConsole());
    var environment = Environment.GetEnvironmentVariables().Cast<DictionaryEntry>()
        .ToDictionary(entry => (string)entry.Key, entry => entry.Value?.ToString() ?? string.Empty, StringComparer.Ordinal);
    return await UnderstudyJobRunner.RunAsync(args[1], environment, loggers, CancellationToken.None);
}

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
builder.Services.AddSingleton<IObjectStorageFactory, ObjectStorageFactory>();
builder.Services.AddSingleton<IUnderstudyCluster, UnderstudyCluster>();
builder.Services.AddSingleton(new UnderstudyOptions(builder.Configuration[UnderstudyOptions.ImageKey] ?? UnderstudyOptions.FallbackImage));
builder.Services.AddSingleton<UnderstudyMetrics>();
builder.Services.AddSingleton<PrometheusExporter>();
builder.Services.AddHostedService<PrometheusEndpoint>();
builder.Services.AddSingleton<IUnderstudyEventSink, KubernetesEventSink>();
builder.Services.AddSingleton<UnderstudyReporter>();
builder.Services.AddSingleton<UnderstudyReconciler>();

await builder.Build().RunAsync();
return 0;
