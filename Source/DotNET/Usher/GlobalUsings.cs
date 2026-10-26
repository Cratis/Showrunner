// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Production global usings are added alongside the first slice that needs them - Release builds treat
// an unused using (IDE0005) as an error. The spec usings below are compiled into Debug builds only.
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
