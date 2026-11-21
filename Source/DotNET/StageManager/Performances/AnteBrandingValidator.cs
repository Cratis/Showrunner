// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Finds what is wrong with the Ante settings that are not part of the contract itself: branding and reserved names.
/// </summary>
public static class AnteBrandingValidator
{
    /// <summary>
    /// Adds the problems with the settings.
    /// </summary>
    /// <param name="onboarding">The onboarding spec.</param>
    /// <param name="problems">The list to add to.</param>
    public static void Validate(OnboardingSpec onboarding, ICollection<string> problems)
    {
        if (onboarding.ReservedOrganizationNames.Any(string.IsNullOrWhiteSpace))
        {
            problems.Add("onboarding.reservedOrganizationNames may not hold an empty name.");
        }

        if (onboarding.Branding is not { } branding)
        {
            return;
        }

        ValidateAddress("onboarding.branding.logoUrl", branding.LogoUrl, problems);
        ValidateAddress("onboarding.branding.customCssUrl", branding.CustomCssUrl, problems);
        ValidateAssets(branding.Assets, problems);
    }

    static void ValidateAddress(string field, string? value, ICollection<string> problems)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        var isPath = value.StartsWith('/') && !value.StartsWith("//", StringComparison.Ordinal);
        var isHttps = Uri.TryCreate(value, UriKind.Absolute, out var url) && url.Scheme == Uri.UriSchemeHttps;
        if (!isPath && !isHttps)
        {
            problems.Add($"{field} must be a path on the host of Ante, such as /brand/logo.svg, or an https URL.");
        }
    }

    static void ValidateAssets(AnteBrandingAssets? assets, ICollection<string> problems)
    {
        if (assets is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(assets.ConfigMapRef.Name))
        {
            problems.Add("onboarding.branding.assets.configMapRef.name is required.");
        }

        if (assets.Files.Count == 0)
        {
            problems.Add("onboarding.branding.assets.files needs at least one key to serve.");
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var file in assets.Files)
        {
            if (string.IsNullOrWhiteSpace(file) || file.Contains('/', StringComparison.Ordinal) || string.Equals(file, ".", StringComparison.Ordinal) || string.Equals(file, "..", StringComparison.Ordinal))
            {
                problems.Add($"onboarding.branding.assets.files holds '{file}', which is not a plain file name.");
            }
            else if (!seen.Add(file))
            {
                problems.Add($"onboarding.branding.assets.files holds '{file}' twice.");
            }
        }
    }
}
