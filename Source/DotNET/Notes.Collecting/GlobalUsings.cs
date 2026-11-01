// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

global using Cratis.Arc.Authorization;
global using Cratis.Arc.Commands;
global using Cratis.Arc.Commands.ModelBound;
global using Cratis.Chronicle.Events;
global using Cratis.Concepts;
global using Cratis.Showrunner.Notes.Contracts;
global using Cratis.Showrunner.Notes.Contracts.Applications;
global using Cratis.Showrunner.Notes.Contracts.Components;
global using Cratis.Showrunner.Notes.Contracts.Context;
global using Cratis.Showrunner.Notes.Contracts.Giving;
global using Cratis.Showrunner.Notes.Contracts.People;
global using Cratis.Showrunner.Notes.Contracts.Responses;
global using Cratis.Showrunner.Usher.Contracts.Reporting;
global using FluentValidation;

// The spec usings below are compiled into Debug builds only.
#if DEBUG
global using Cratis.Arc.Chronicle.Testing.Commands;
global using Cratis.Arc.Testing.Commands;
global using Cratis.Chronicle.Testing;
global using Cratis.Chronicle.Testing.EventSequences;
global using Cratis.Chronicle.Testing.ReadModels;
global using Cratis.Specifications;
global using NSubstitute;
global using Xunit;
#endif
