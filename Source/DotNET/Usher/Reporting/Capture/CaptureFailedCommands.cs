// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Cratis.Showrunner.Usher.Contracts.Reporting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Usher.Reporting.Capture;

/// <summary>
/// Captures the exception of a failing command in <see cref="ICapturedErrors"/>, for the signed-in user in the current
/// tenant, and leaves the failure itself to Arc.
/// </summary>
/// <remarks>
/// It is an <see cref="ICommandExceptionMapper"/> that never maps: it always declines, so the command fails exactly as
/// it would without Usher, and another mapper or Arc's default conversion decides the result. A command whose exception
/// an earlier mapper accepts is not captured. Capturing is best effort and never changes the outcome: when it fails, it
/// is logged and the command fails with its own exception. Nothing is captured for anonymous requests, or when
/// <see cref="ErrorCaptureOptions.Enabled"/> is off or Usher is not added.
/// </remarks>
/// <param name="logger">The <see cref="ILogger"/> capture failures are logged to.</param>
public class CaptureFailedCommands(ILogger<CaptureFailedCommands> logger) : ICommandExceptionMapper
{
    /// <inheritdoc/>
    public bool TryMap(CommandContext context, Exception exception, [NotNullWhen(true)] out CommandResult? result)
    {
        result = null;
        try
        {
            var services = context.ServiceProvider;
            if (services?.GetService<IOptions<UsherOptions>>()?.Value is not { ErrorCapture.Enabled: true } options ||
                services.GetService<ICapturedErrors>() is not { } errors ||
                services.GetService<ICurrentErrorOwner>() is not { } owners ||
                !owners.TryGet(out var owner))
            {
                return false;
            }

            var location = new SliceLocation(context.SliceLocation.Module.Value, context.SliceLocation.Feature.Value, context.SliceLocation.Slice.Value);
            var error = ExceptionCapture.Capture(exception, location, ApplicationVersions.Resolve(options), options.ErrorCapture.MaximumStackFrames);
            errors.Capture(owner, context.CorrelationId.Value, error);
        }
        catch (Exception captureFailure)
        {
            logger.CaptureFailed(context.CorrelationId.Value, captureFailure);
        }

        return false;
    }
}
