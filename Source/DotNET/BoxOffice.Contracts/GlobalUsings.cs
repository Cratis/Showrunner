// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

global using Cratis.Chronicle.Events;
global using Cratis.Concepts;
global using Cratis.Showrunner.BoxOffice.Contracts.Compute;
global using Cratis.Showrunner.BoxOffice.Contracts.Entitlements;
global using Cratis.Showrunner.BoxOffice.Contracts.People;
global using Cratis.Showrunner.BoxOffice.Contracts.Seats;
global using Cratis.Showrunner.BoxOffice.Contracts.Trials;

// The spec usings below are compiled into Debug builds only.
#if DEBUG
global using Cratis.Chronicle.Testing;
global using Cratis.Chronicle.Testing.EventSequences;
global using Cratis.Showrunner.BoxOffice.Contracts.Offering;
global using Cratis.Showrunner.BoxOffice.Contracts.Offers;
global using Cratis.Showrunner.BoxOffice.Contracts.Tenants;
global using Cratis.Specifications;
global using NSubstitute;
global using Xunit;
#endif
