// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration;

/// <summary>
/// Describes one module while the product declares its offering.
/// </summary>
/// <param name="id">The module's key.</param>
/// <param name="name">The module's name.</param>
public class ModuleBuilder(ModuleId id, ModuleName name)
{
    readonly List<ModuleId> _requires = [];
    readonly List<string> _namespaces = [];
    int _trialDays;
    ModuleAvailability _availability = ModuleAvailability.Paid;

    /// <summary>
    /// States that a tenant needs the given modules as well to use this one.
    /// </summary>
    /// <param name="modules">The keys of the modules.</param>
    /// <returns>The builder for continuation.</returns>
    public ModuleBuilder Requires(params ModuleId[] modules)
    {
        _requires.AddRange(modules);
        return this;
    }

    /// <summary>
    /// States that the module owns the code in the given namespaces, and everything inside them that no other module owns more closely.
    /// </summary>
    /// <param name="namespaces">The namespaces.</param>
    /// <returns>The builder for continuation.</returns>
    public ModuleBuilder Owns(params string[] namespaces)
    {
        _namespaces.AddRange(namespaces);
        return this;
    }

    /// <summary>
    /// Sets the length of the module's trial unless the operator decides otherwise.
    /// </summary>
    /// <param name="days">The number of days.</param>
    /// <returns>The builder for continuation.</returns>
    public ModuleBuilder TrialDays(int days)
    {
        _trialDays = days;
        return this;
    }

    /// <summary>
    /// Sets how the module can be had. A module is <see cref="ModuleAvailability.Paid"/> unless said otherwise.
    /// </summary>
    /// <param name="availability">The availability.</param>
    /// <returns>The builder for continuation.</returns>
    public ModuleBuilder Availability(ModuleAvailability availability)
    {
        _availability = availability;
        return this;
    }

    /// <summary>
    /// Creates the definition of the module.
    /// </summary>
    /// <returns>The <see cref="ModuleDefinition"/>.</returns>
    internal ModuleDefinition Build() => new(id, name, [.. _requires], [.. _namespaces], _trialDays, _availability);
}
