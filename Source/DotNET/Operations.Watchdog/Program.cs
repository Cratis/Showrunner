// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Runtime.InteropServices;
using Cratis.Showrunner.Operations.Channels;
using Cratis.Showrunner.Operations.Configuration;
using Cratis.Showrunner.Operations.Kubernetes;
using Cratis.Showrunner.Operations.Watchdog;
using k8s;

var command = WatchdogCommands.Parse(args);
if (command == WatchdogCommand.Unknown)
{
    await Console.Error.WriteLineAsync(WatchdogCommands.Usage);
    return WatchdogCommands.UsageExitCode;
}

using var loggers = LoggerFactory.Create(builder => builder.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "HH:mm:ss ";
}));
var logger = loggers.CreateLogger("watchdog");

using var cancellation = new CancellationTokenSource();
using var termination = PosixSignalRegistration.Create(PosixSignal.SIGTERM, context =>
{
    context.Cancel = true;
    cancellation.Cancel();
});

WatchdogSettings settings;
try
{
    var environment = Environment.GetEnvironmentVariables()
        .Cast<DictionaryEntry>()
        .ToDictionary(entry => (string)entry.Key, entry => entry.Value?.ToString() ?? string.Empty, StringComparer.Ordinal);
    settings = WatchdogSettingsReader.FromEnvironment(environment, CurrentNamespace());
}
catch (WatchdogConfigurationException error)
{
    // The problems name variables, never their values.
    foreach (var problem in error.Problems)
    {
        logger.ConfigurationProblem(problem);
    }

    return 1;
}

using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
using var kubernetes = new Kubernetes(KubernetesClientConfiguration.IsInCluster() ? KubernetesClientConfiguration.InClusterConfig() : KubernetesClientConfiguration.BuildDefaultConfig());
var factory = new WatchdogFactory(
    settings,
    new KubernetesClusterReader(kubernetes),
    new KubernetesConfigMapStore(kubernetes, settings.Options.Namespace),
    http,
    settings.Smtp is { } smtp ? new MailKitEmailSender(smtp) : null,
    TimeProvider.System,
    loggers);

return await WatchdogCommands.Run(command, factory, cancellation.Token);

static string CurrentNamespace()
{
    const string ServiceAccountNamespace = "/var/run/secrets/kubernetes.io/serviceaccount/namespace";
    return Environment.GetEnvironmentVariable("POD_NAMESPACE")
        ?? (File.Exists(ServiceAccountNamespace) ? File.ReadAllText(ServiceAccountNamespace).Trim() : "default");
}
