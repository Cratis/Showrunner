// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Integration.given;

/// <summary>
/// The artifacts a desk client registers: only the contract event types. The default provider scans every loaded
/// assembly, which in this process includes Usher; a desk using it would register Usher's reactors in the desk store.
/// </summary>
public sealed class ContractArtifacts : IClientArtifactsProvider
{
    static readonly System.Reflection.Assembly _contracts = typeof(IssueId).Assembly;
    readonly IClientArtifactsProvider _default = DefaultClientArtifactsProvider.Default;

    /// <inheritdoc/>
    public IEnumerable<Type> EventTypes => _default.EventTypes.Where(type => type.Assembly == _contracts);

    /// <inheritdoc/>
    public IEnumerable<Type> EventTypeMigrators => [];

    /// <inheritdoc/>
    public IEnumerable<Type> ComplianceForTypesProviders => _default.ComplianceForTypesProviders;

    /// <inheritdoc/>
    public IEnumerable<Type> ComplianceForPropertiesProviders => _default.ComplianceForPropertiesProviders;

    /// <inheritdoc/>
    public IEnumerable<Type> Projections => [];

    /// <inheritdoc/>
    public IEnumerable<Type> ModelBoundProjections => [];

    /// <inheritdoc/>
    public IEnumerable<Type> Reactors => [];

    /// <inheritdoc/>
    public IEnumerable<Type> ReadModelReactors => [];

    /// <inheritdoc/>
    public IEnumerable<Type> Reducers => [];

    /// <inheritdoc/>
    public IEnumerable<Type> ReactorMiddlewares => [];

    /// <inheritdoc/>
    public IEnumerable<Type> AdditionalEventInformationProviders => [];

    /// <inheritdoc/>
    public IEnumerable<Type> ConstraintTypes => [];

    /// <inheritdoc/>
    public IEnumerable<Type> UniqueConstraints => [];

    /// <inheritdoc/>
    public IEnumerable<Type> UniqueEventTypeConstraints => [];

    /// <inheritdoc/>
    public IEnumerable<Type> RemoveConstraintEventTypes => [];

    /// <inheritdoc/>
    public IEnumerable<Type> EventSeeders => [];
}
