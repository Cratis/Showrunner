// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Environments;
using Cratis.Environments.Secrets;

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Resolves environments with the <c>Cratis.Environments</c> library, the same way the Cratis command line does. Passphrase environments use the passphrase the Performance names; Azure Key Vault uses the workload identity of the operator (DefaultAzureCredential); HashiCorp Vault uses the operator's VAULT_* settings and Kubernetes auth.
/// </summary>
/// <param name="logger">The logger.</param>
public class CratisEnvironmentResolver(ILogger<CratisEnvironmentResolver> logger) : IEnvironmentResolver
{
    /// <inheritdoc/>
    public async Task<EnvironmentResolution> ResolveAsync(string name, string environmentFile, string? passphrase, CancellationToken cancellationToken)
    {
        // The library reads environments from <root>/.cratis/environments, so the file is placed in a private temporary root.
        var root = Directory.CreateTempSubdirectory("stage-manager-");
        try
        {
            var environments = ProjectEnvironments.ForProject(root.FullName);
            Directory.CreateDirectory(environments.EnvironmentsDirectory);
            await File.WriteAllTextAsync(environments.PathFor(name), environmentFile, cancellationToken);

            var source = passphrase is null ? new MissingPassphraseSource() : PassphraseSources.Fixed(passphrase);
            var environment = ProjectEnvironment.Open(environments, name, source);
            var values = await environment.GetAllAsync(cancellationToken);
            return EnvironmentResolution.Resolved(new(name, values));
        }
        catch (EnvironmentException exception)
        {
            // The library's messages name the key and the provider; they never carry a value.
            logger.EnvironmentNotResolved(name, exception.GetType().Name);
            return EnvironmentResolution.Failed(exception.Message);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    sealed class MissingPassphraseSource : IPassphraseSource
    {
        public string GetPassphrase() => throw new EnvironmentException("The environment needs a passphrase and the Performance names no environment.passphraseSecretRef.");

        public string GetNewPassphrase() => GetPassphrase();
    }
}
