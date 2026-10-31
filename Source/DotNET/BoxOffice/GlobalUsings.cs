// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

global using Cratis.Arc.Authorization;
global using Cratis.Arc.Commands;
global using Cratis.Arc.Commands.ModelBound;
global using Cratis.Chronicle.Events;
global using Cratis.Showrunner.BoxOffice.Contracts;
global using Cratis.Showrunner.BoxOffice.Contracts.Compute;
global using Cratis.Showrunner.BoxOffice.Contracts.Entitlements;
global using Cratis.Showrunner.BoxOffice.Contracts.People;
global using Cratis.Showrunner.BoxOffice.Contracts.Seats;
global using Cratis.Showrunner.BoxOffice.Contracts.Tenants;
global using Cratis.Showrunner.BoxOffice.Contracts.Trials;
global using FluentValidation;

#if DEBUG
global using Cratis.Arc.Chronicle.Testing.Commands;
global using Cratis.Arc.Testing.Commands;
global using Cratis.Arc.Validation;
global using Cratis.Chronicle.Testing;
global using Cratis.Chronicle.Testing.EventSequences;
global using Cratis.Chronicle.Testing.ReadModels;
global using Cratis.Concepts;
global using Cratis.Specifications;
global using NSubstitute;
global using Xunit;
#endif
