// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Cratis.Chronicle.Registrations;
using Cratis.Showrunner.Herald.Sending;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cratis.Showrunner.Herald.Integration.given;

/// <summary>
/// An application sending email through Herald in a generic host, with its own event store and Herald's delivery running.
/// </summary>
/// <param name="host">The running host.</param>
/// <param name="name">The name of the application store.</param>
public sealed class HeraldApplication(IHost host, EventStoreName name) : IAsyncDisposable
{
    /// <summary>
    /// The password the application signs in to the mail server with. The specifications look for it in what Herald records.
    /// </summary>
    public const string SmtpPassword = "integration-smtp-password";

    /// <summary>
    /// Starts an application with a unique store name, sending through a mail server.
    /// </summary>
    /// <param name="infrastructure">The kernel.</param>
    /// <param name="templates">The folder with the templates.</param>
    /// <param name="images">The folder with the images.</param>
    /// <param name="smtpPort">The port of the mail server on this machine.</param>
    /// <returns>The running application.</returns>
    /// <exception cref="InvalidOperationException">The application store did not register.</exception>
    public static async Task<HeraldApplication> Start(ChronicleInfrastructure infrastructure, string templates, string images, int smtpPort)
    {
        var name = new EventStoreName($"Ledger-{Guid.NewGuid():N}");
        var builder = Host.CreateApplicationBuilder();
        builder.AddCratisChronicle(options =>
        {
            options.ConnectionString = new(infrastructure.ChronicleConnectionString);
            options.EventStore = name;
        });
        builder.AddHerald(herald =>
        {
            herald.TemplatesFolder = templates;
            herald.ImagesFolder = images;
            herald.Sender = new() { Address = "noreply@ledger.example.com", Name = "Ledger" };
            herald.Look = new() { BrandColor = "#0b5fff", Footer = "Ledger AS, Oslo" };
            herald.Smtp = new() { Host = "127.0.0.1", Port = smtpPort, Security = SmtpSecurity.None, Password = SmtpPassword };
            herald.Retry = new() { MaxAttempts = 5, InitialDelay = TimeSpan.FromSeconds(1), MaxDelay = TimeSpan.FromSeconds(2) };
            herald.PollingInterval = TimeSpan.FromMilliseconds(300);
        });

        var host = builder.Build();
        await host.StartAsync();
        var application = new HeraldApplication(host, name);
        var store = await application.Store();
        var timeout = TimeSpan.FromSeconds(30);
        var wait = Stopwatch.StartNew();
        var registration = await store.WaitForRegistration(timeout);
        wait.Stop();
        if (!registration.IsSuccess)
        {
            var diagnostics = await infrastructure.RegistrationDiagnostics($"Application store {name} did not register.", wait.Elapsed, timeout);
            throw new InvalidOperationException(diagnostics, registration.Failure);
        }

        return application;
    }

    /// <summary>
    /// Gets what the application sends email with.
    /// </summary>
    public IEmails Emails => host.Services.GetRequiredService<IEmails>();

    /// <summary>
    /// Gets the application's event store.
    /// </summary>
    /// <returns>The event store.</returns>
    public Task<IEventStore> Store() =>
        host.Services.GetRequiredService<IChronicleClient>().GetEventStore(name, EventStoreNamespaceName.Default);

    /// <summary>
    /// Waits for a fact about an email in the event log.
    /// </summary>
    /// <typeparam name="TEvent">The fact.</typeparam>
    /// <param name="about">The event source the email is recorded against.</param>
    /// <param name="emailId">The email.</param>
    /// <param name="matches">Narrows to the fact wanted; null for any.</param>
    /// <returns>The fact as the event log holds it.</returns>
    public async Task<TEvent> WaitFor<TEvent>(EventSourceId about, EmailId emailId, Func<TEvent, bool>? matches = null)
        where TEvent : class
    {
        var store = await Store();
        var appended = await Eventually.Get(
            async () => (await store.EventLog.GetFromSequenceNumber(EventSequenceNumber.First, about))
                .FirstOrDefault(item => item.Content is TEvent fact && matches?.Invoke(fact) != false && FactIsAbout(fact, emailId)),
            $"{typeof(TEvent).Name} for {emailId} in {name}'s event log");
        return (TEvent)appended.Content;
    }

    /// <summary>
    /// Waits until no email is waiting to be delivered, according to the read model of queued emails.
    /// </summary>
    /// <returns>An awaitable task.</returns>
    public async Task WaitForEmptyQueue()
    {
        var store = await Store();
        await Eventually.Get(
            async () => (await store.ReadModels.GetInstances<QueuedEmail>()).Any() ? null : new object(),
            "the queue of emails to be empty");
    }

    /// <summary>
    /// Gets every fact recorded against something.
    /// </summary>
    /// <param name="about">The event source the emails are about, which holds their facts.</param>
    /// <returns>The facts.</returns>
    public async Task<IReadOnlyList<object>> Facts(EventSourceId about)
    {
        var store = await Store();
        return [.. (await store.EventLog.GetFromSequenceNumber(EventSequenceNumber.First, about)).Select(item => item.Content)];
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await host.StopAsync();
        host.Dispose();
    }

    static bool FactIsAbout(object fact, EmailId emailId) => fact switch
    {
        EmailSent sent => sent.EmailId == emailId,
        EmailFailed failed => failed.EmailId == emailId,
        EmailAttemptFailed attempt => attempt.EmailId == emailId,
        _ => false
    };
}
