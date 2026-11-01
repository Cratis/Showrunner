// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

global using Cratis.Chronicle.Events;
global using Cratis.Concepts;
global using Cratis.Showrunner.Notes.Contracts.Applications;
global using Cratis.Showrunner.Notes.Contracts.Components;
global using Cratis.Showrunner.Notes.Contracts.Context;
global using Cratis.Showrunner.Notes.Contracts.People;
global using Cratis.Showrunner.Usher.Contracts.Reporting;

// The spec usings below are compiled into Debug builds only.
#if DEBUG
global using Cratis.Chronicle.Testing;
global using Cratis.Chronicle.Testing.EventSequences;
global using Cratis.Showrunner.Notes.Contracts.Giving;
global using Cratis.Specifications;
global using NSubstitute;
global using Xunit;
#endif
