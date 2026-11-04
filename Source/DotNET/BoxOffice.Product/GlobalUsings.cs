// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

global using Cratis.Chronicle.Events;
global using Cratis.Showrunner.BoxOffice.Contracts;
global using Cratis.Showrunner.BoxOffice.Contracts.Activity;
global using Cratis.Showrunner.BoxOffice.Contracts.People;
global using Cratis.Showrunner.BoxOffice.Contracts.Tenants;

// The spec usings below are compiled into Debug builds only.
#if DEBUG
global using Cratis.Chronicle.Testing;
global using Cratis.Showrunner.BoxOffice.Contracts.Entitlements;
global using Cratis.Showrunner.BoxOffice.Contracts.Seats;
global using Cratis.Showrunner.BoxOffice.Contracts.Trials;
global using Cratis.Chronicle.Testing.EventSequences;
global using Cratis.Specifications;
global using Microsoft.AspNetCore.Http;
global using NSubstitute;
global using Xunit;
#endif
