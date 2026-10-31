// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Gating;

/// <summary>
/// Decides whether the current tenant may use what a module owns. A module owns the code namespaces the product declared
/// for it; a command, a query or a reactor in one of them is admitted only when the tenant has the module.
/// </summary>
public interface IModuleGate
{
    /// <summary>
    /// Gets the module that owns a code namespace when the tenant has not got it.
    /// </summary>
    /// <param name="namespace">The namespace of the command or the full name of the query.</param>
    /// <returns>The module the tenant lacks, or <see langword="null"/> when the namespace is admitted: no module owns it, the
    /// module is included in the product, the tenant has it, or enforcement is turned off.</returns>
    Task<ModuleId?> RefusedModuleOf(string? @namespace);

    /// <summary>
    /// Gets whether the tenant may use an artifact, such as a reactor: whether the module owning its namespace is one the
    /// tenant has. A gated reactor asks with <c>Admits(this)</c> and skips the event when it does not.
    /// </summary>
    /// <param name="artifact">The artifact, or its <see cref="Type"/>.</param>
    /// <returns><see langword="true"/> when the artifact is admitted.</returns>
    Task<bool> Admits(object artifact);
}
