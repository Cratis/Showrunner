// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Applications;

/// <summary>
/// The application a note was given in, and the publication of it the receiving side finds the organization, application
/// and event model by.
/// </summary>
/// <param name="ApplicationName">The name of the application; Usher's concept, so both parts name an application the same way.</param>
/// <param name="ApplicationVersion">The version of the application the user was running.</param>
/// <param name="PublicationId">The publication the application runs as.</param>
public record PublishedApplication(ApplicationName ApplicationName, ApplicationVersion ApplicationVersion, PublicationId PublicationId);
