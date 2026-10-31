// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using MongoDB.Driver;

namespace Cratis.Showrunner.BoxOffice.Integration.given;

/// <summary>
/// A MongoDB server in a container, shared by the specifications of a run.
/// </summary>
public sealed class MongoInfrastructure : IAsyncLifetime
{
    /// <summary>
    /// The image used unless <c>BOXOFFICE_MONGO_IMAGE</c> names another.
    /// </summary>
    public const string DefaultImage = "mongo:8.2.6";

    const ushort MongoDBPort = 27017;
    const int MaxStartAttempts = 3;

    IContainer? _container;

    /// <summary>
    /// Gets the server started for <see cref="MongoCollection"/>. Specifications reach it statically: the shared
    /// specification runner only runs Establish and Because once for classes with a parameterless constructor.
    /// </summary>
    /// <exception cref="InvalidOperationException">The specification is not in the collection.</exception>
    public static MongoInfrastructure Current
    {
        get => field ?? throw new InvalidOperationException($"Specifications using MongoDB must be in [Collection({nameof(MongoCollection)}.{nameof(MongoCollection.Name)})].");
        private set;
    }

    /// <summary>
    /// Gets the image to run.
    /// </summary>
    public static string Image => Environment.GetEnvironmentVariable("BOXOFFICE_MONGO_IMAGE") is { Length: > 0 } image ? image : DefaultImage;

    /// <summary>
    /// Gets a client of the server.
    /// </summary>
    public IMongoClient Client { get; private set; } = null!;

    /// <summary>
    /// Creates a database of its own, so specifications do not see each other's buckets.
    /// </summary>
    /// <returns>The database.</returns>
    public IMongoDatabase NewDatabase() => Client.GetDatabase($"boxoffice-{Guid.NewGuid():N}");

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        // The image pull that Testcontainers performs as part of starting the container can lose a race against
        // the Docker daemon on a GitHub-hosted runner, surfacing as "No such image" even though the image exists.
        for (var attempt = 1; ; attempt++)
        {
            var container = CreateContainer();
            try
            {
                await container.StartAsync();
                _container = container;
                break;
            }
            catch (Exception exception) when (attempt < MaxStartAttempts && IsImagePullFailure(exception))
            {
                await container.DisposeAsync();
            }
        }

        Client = new MongoClient($"mongodb://{_container.Hostname}:{_container.GetMappedPublicPort(MongoDBPort)}/?directConnection=true");
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                await Client.GetDatabase("admin").RunCommandAsync<MongoDB.Bson.BsonDocument>("{ ping: 1 }");
                break;
            }
            catch (Exception) when (attempt < 30)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        Current = this;
    }

    /// <inheritdoc/>
    public async Task DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }

    static bool IsImagePullFailure(Exception exception)
    {
        var description = exception.ToString();
        return description.Contains("No such image", StringComparison.OrdinalIgnoreCase) ||
            description.Contains("DockerImageNotFoundException", StringComparison.OrdinalIgnoreCase);
    }

    IContainer CreateContainer() =>
        new ContainerBuilder(Image)
            .WithPortBinding(MongoDBPort, true)
            .WithLabel("cratis.showrunner.integration", "true")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(MongoDBPort))
            .Build();
}
