// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;

namespace Cratis.Showrunner.StageManager.Pulumi.Performances;

/// <summary>
/// The legal terms Ante asks people to accept. Give an existing ConfigMap with <c>terms.md</c> and <c>privacy.md</c>, or the two texts and the component writes it.
/// </summary>
public sealed class LegalTermsArgs
{
    /// <summary>
    /// Gets the name of an existing ConfigMap that holds <c>terms.md</c> and <c>privacy.md</c>.
    /// </summary>
    public string? ConfigMapName { get; init; }

    /// <summary>
    /// Gets the terms and conditions, as Markdown.
    /// </summary>
    public Input<string>? TermsAndConditions { get; init; }

    /// <summary>
    /// Gets the privacy policy, as Markdown.
    /// </summary>
    public Input<string>? PrivacyPolicy { get; init; }

    /// <summary>
    /// Builds the spec of the legal terms, writing the ConfigMap when the texts are given as values.
    /// </summary>
    /// <param name="context">The context of the Performance.</param>
    /// <returns>The spec.</returns>
    /// <exception cref="ArgumentException">Thrown when the terms are not given as a ConfigMap or as both texts.</exception>
    internal Dictionary<string, object> ToSpec(PerformanceContext context)
    {
        var existing = ConfigMapName is not null && TermsAndConditions is null && PrivacyPolicy is null;
        var inline = ConfigMapName is null && TermsAndConditions is not null && PrivacyPolicy is not null;
        if (!existing && !inline)
        {
            throw new ArgumentException("Give the legal terms as ConfigMapName, or as both TermsAndConditions and PrivacyPolicy.");
        }

        var name = ConfigMapName ?? PerformanceFiles.Declare(
            context,
            "legal",
            new Dictionary<string, Input<string>>(StringComparer.Ordinal) { ["terms.md"] = TermsAndConditions!, ["privacy.md"] = PrivacyPolicy! });
        return SpecExtensions.NewSpec().Put("configMapRef", SpecExtensions.Reference(name, null));
    }
}
