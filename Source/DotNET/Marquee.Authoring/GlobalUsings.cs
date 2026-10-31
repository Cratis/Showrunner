// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

global using Cratis.Arc.Authorization;
global using Cratis.Arc.Commands;
global using Cratis.Arc.Commands.ModelBound;
global using Cratis.Chronicle.Events;
global using Cratis.Showrunner.Marquee.Authoring.Announcements;
global using Cratis.Showrunner.Marquee.Contracts.Announcements;
global using Cratis.Showrunner.Marquee.Contracts.Audiences;
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
