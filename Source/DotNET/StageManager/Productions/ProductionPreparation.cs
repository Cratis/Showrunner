// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// Gathers what a Production needs before anything is applied, and says plainly what is missing. It changes nothing in the cluster, so a Production that cannot run is never half configured.
/// </summary>
/// <param name="cluster">The cluster.</param>
public class ProductionPreparation(IProductionCluster cluster)
{
    /// <summary>
    /// Prepares the Production.
    /// </summary>
    /// <param name="production">The Production.</param>
    /// <param name="status">The status to record conditions in.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>What is needed to apply it, or null when the Production is blocked, which is then recorded in <paramref name="status"/>.</returns>
    public async Task<PreparedProduction?> PrepareAsync(Production production, ProductionStatusBuilder status, CancellationToken cancellationToken)
    {
        var spec = production.Spec;
        var problems = ValidateSpec(spec, status);
        var sinksValid = await CheckSinksAsync(production, problems, status, cancellationToken);
        var vaultSupported = CheckVault(spec, status);
        var modesUnchanged = CheckModes(production, status);
        var secretsFound = problems.Count == 0 && await CheckSecretsAsync(production, status, cancellationToken);
        var operatorsInstalled = await CheckOperatorsAsync(spec, status, cancellationToken);

        var configurationValid = problems.Count == 0 && sinksValid && vaultSupported;
        var infrastructureReady = modesUnchanged && secretsFound && operatorsInstalled;
        if (!configurationValid || !infrastructureReady)
        {
            status.Block();
            return null;
        }

        return new(ChronicleKernelConfiguration.Build(spec, ProductionNames.For(production)));
    }

    /// <summary>
    /// Gets the database kind and mode a spec asks for, in the form the status keeps it.
    /// </summary>
    /// <param name="spec">The spec.</param>
    /// <returns>The kind and mode, as kind/mode.</returns>
    internal static string DatabaseOf(ProductionSpec spec) => $"{spec.Database.Kind}/{spec.Database.Mode}";

    /// <summary>
    /// Gets the graph mode and edition a spec asks for, in the form the status keeps it.
    /// </summary>
    /// <param name="spec">The spec.</param>
    /// <returns>The mode and edition, as mode/edition.</returns>
    internal static string GraphOf(ProductionSpec spec) => $"{spec.Graph.Mode}/{spec.Graph.Edition}";

    static IReadOnlyList<ProductionProblem> ValidateSpec(ProductionSpec spec, ProductionStatusBuilder status)
    {
        var problems = ProductionValidator.ValidateEach(spec);
        var configured = spec.Sinks.Configurations.Select(sink => sink.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Problems with a named sink are reported on the sink's own condition.
        var general = problems.Where(problem => problem.Sink is null || !configured.Contains(problem.Sink)).Select(problem => problem.Message).ToList();
        status.Set(ProductionConditions.SpecValid, general.Count == 0, general.Count == 0 ? "Valid" : "InvalidSpec", general.Count == 0 ? "The spec is valid." : string.Join(' ', general));
        return problems;
    }

    static bool CheckVault(ProductionSpec spec, ProductionStatusBuilder status)
    {
        var supported = string.Equals(spec.Vault.Mode, ProductionModes.None, StringComparison.Ordinal);
        status.Set(
            ProductionConditions.VaultSupported,
            supported,
            supported ? "None" : "VaultNotSupported",
            supported ? "Chronicle keeps its keys in its database." : $"vault.mode {spec.Vault.Mode} is not supported yet; only None is. See https://github.com/Cratis/Showrunner/issues/17.");
        return supported;
    }

    static bool CheckModes(Production production, ProductionStatusBuilder status)
    {
        var spec = production.Spec;
        var databaseHolds = Unchanged(production.Status.Database, DatabaseOf(spec), "database.kind", "database.mode", out var databaseMessage);
        status.Set(ProductionConditions.DatabaseModeUnchanged, databaseHolds, databaseHolds ? "Unchanged" : "ModeChangeRefused", databaseMessage);

        var graphHolds = Unchanged(production.Status.Graph, GraphOf(spec), "graph.mode", "graph.edition", out var graphMessage);
        status.Set(ProductionConditions.GraphModeUnchanged, graphHolds, graphHolds ? "Unchanged" : "ModeChangeRefused", graphMessage);
        return databaseHolds && graphHolds;
    }

    static bool Unchanged(string running, string asked, string first, string second, out string message)
    {
        if (string.IsNullOrEmpty(running) || string.Equals(running, asked, StringComparison.Ordinal))
        {
            message = $"{first} and {second} are {asked}.";
            return true;
        }

        var fields = ChangedFields(running, asked, first, second);
        message = $"Changing {string.Join(" and ", fields)} on a Production that runs is refused: the data stays where it is. Set it back to {running}, or create a new Production.";
        return false;
    }

    static List<string> ChangedFields(string running, string asked, string first, string second)
    {
        var before = running.Split('/');
        var after = asked.Split('/');
        var fields = new List<string>();
        if (!string.Equals(before[0], after[0], StringComparison.Ordinal))
        {
            fields.Add($"{first} from {before[0]} to {after[0]}");
        }

        if (!string.Equals(before.ElementAtOrDefault(1), after.ElementAtOrDefault(1), StringComparison.Ordinal))
        {
            fields.Add($"{second} from {before.ElementAtOrDefault(1)} to {after.ElementAtOrDefault(1)}");
        }

        return fields;
    }

    async Task<bool> CheckSinksAsync(Production production, IReadOnlyList<ProductionProblem> problems, ProductionStatusBuilder status, CancellationToken cancellationToken)
    {
        var valid = true;
        foreach (var sink in production.Spec.Sinks.Configurations.DistinctBy(sink => sink.Name, StringComparer.OrdinalIgnoreCase))
        {
            var messages = problems.Where(problem => string.Equals(problem.Sink, sink.Name, StringComparison.OrdinalIgnoreCase)).Select(problem => problem.Message).ToList();
            var reason = messages.Count > 0 ? "InvalidSink" : "Configured";
            if (messages.Count == 0 && sink.ConnectionSecretRef is { } reference)
            {
                var key = reference.KeyOrDefault();
                if (await cluster.GetSecretValueAsync(production.Metadata.NamespaceProperty, reference.Name, key, cancellationToken) is null)
                {
                    messages.Add($"The sink '{sink.Name}' names the Secret '{reference.Name}', which does not exist or has no key '{key}'.");
                    reason = "SecretNotFound";
                }
            }

            var holds = messages.Count == 0;
            valid &= holds;
            status.Set(ProductionConditions.SinkConfigured(sink.Name), holds, reason, holds ? $"The sink '{sink.Name}' is {sink.Kind}." : string.Join(' ', messages));
        }

        return valid;
    }

    async Task<bool> CheckSecretsAsync(Production production, ProductionStatusBuilder status, CancellationToken cancellationToken)
    {
        var databaseFound = await CheckDatabaseSecretAsync(production, status, cancellationToken);
        var graphFound = await CheckGraphSecretAsync(production, status, cancellationToken);
        return databaseFound && graphFound;
    }

    async Task<bool> CheckDatabaseSecretAsync(Production production, ProductionStatusBuilder status, CancellationToken cancellationToken)
    {
        var @namespace = production.Metadata.NamespaceProperty;
        var spec = production.Spec;
        if (spec.Database.IsExternal())
        {
            var name = spec.Database.ConnectionSecretName;
            var exists = await cluster.GetSecretValueAsync(@namespace, name, ProductionSecretReference.DefaultKey, cancellationToken) is not null;
            if (!exists)
            {
                status.Set(ProductionConditions.DatabaseReady, false, "SecretNotFound", $"The database connection Secret '{name}' does not exist or has no key '{ProductionSecretReference.DefaultKey}'.");
            }

            return exists;
        }

        return true;
    }

    async Task<bool> CheckGraphSecretAsync(Production production, ProductionStatusBuilder status, CancellationToken cancellationToken)
    {
        var @namespace = production.Metadata.NamespaceProperty;
        var spec = production.Spec;
        if (spec.Graph.Mode == ProductionModes.External)
        {
            var name = spec.Graph.CredentialsSecretName;
            var missing = new List<string>();
            foreach (var key in new[] { ProductionGraph.ServerKey, ProductionGraph.PasswordKey })
            {
                if (await cluster.GetSecretValueAsync(@namespace, name, key, cancellationToken) is null)
                {
                    missing.Add(key);
                }
            }

            if (missing.Count > 0)
            {
                status.Set(ProductionConditions.GraphReady, false, "SecretNotFound", $"The graph credentials Secret '{name}' does not exist or has no key {string.Join(" or ", missing.Select(key => $"'{key}'"))}.");
            }

            return missing.Count == 0;
        }

        return true;
    }

    async Task<bool> CheckOperatorsAsync(ProductionSpec spec, ProductionStatusBuilder status, CancellationToken cancellationToken)
    {
        var certManager = await cluster.HasDefinitionAsync(CertManagerCertificate.DefinitionName, cancellationToken)
            && await cluster.HasDefinitionAsync(CertManagerIssuer.DefinitionName, cancellationToken);
        status.Set(
            ProductionConditions.CertManagerInstalled,
            certManager,
            certManager ? "Installed" : "CertManagerNotInstalled",
            certManager ? "cert-manager is installed." : "cert-manager is not installed: Chronicle's TLS certificate comes from it. Install cert-manager in the cluster.");

        var percona = true;
        if (!spec.Database.IsExternal())
        {
            percona = await cluster.HasDefinitionAsync(PerconaServerMongoDB.DefinitionName, cancellationToken);
            if (!percona)
            {
                status.Set(ProductionConditions.DatabaseReady, false, "PerconaOperatorNotInstalled", "The Percona Operator for MongoDB is not installed: a Managed database is run by it. Install it, or use an External database.");
            }
        }

        return certManager && percona;
    }
}
