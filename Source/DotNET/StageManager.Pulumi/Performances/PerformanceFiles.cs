// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;
using Pulumi.Kubernetes.Core.V1;
using Pulumi.Kubernetes.Types.Inputs.Core.V1;
using Pulumi.Kubernetes.Types.Inputs.Meta.V1;

namespace Cratis.Showrunner.StageManager.Pulumi.Performances;

/// <summary>
/// Turns inline files or a directory of text files into the ConfigMap an AuthProxy or Ante setting refers to.
/// </summary>
internal static class PerformanceFiles
{
    /// <summary>
    /// Reads the files directly in a directory, ordered by name.
    /// </summary>
    /// <param name="directory">The directory.</param>
    /// <returns>The content of each file by file name.</returns>
    /// <exception cref="DirectoryNotFoundException">Thrown when the directory does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the directory holds no files.</exception>
    public static Dictionary<string, Input<string>> FromDirectory(string directory)
    {
        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"The directory '{directory}' does not exist. Run the Pulumi program from the folder the path is relative to, or give an absolute path.");
        }

        var files = Directory.GetFiles(directory).Order(StringComparer.Ordinal).ToArray();
        if (files.Length == 0)
        {
            throw new InvalidOperationException($"The directory '{directory}' holds no files.");
        }

        return files.ToDictionary(file => Path.GetFileName(file), file => (Input<string>)File.ReadAllText(file), StringComparer.Ordinal);
    }

    /// <summary>
    /// Declares a ConfigMap that holds the given files, one per key.
    /// </summary>
    /// <param name="context">The context of the Performance.</param>
    /// <param name="suffix">What distinguishes the ConfigMap among those of the Performance.</param>
    /// <param name="files">The content of each file by file name.</param>
    /// <returns>The name of the ConfigMap.</returns>
    public static string Declare(PerformanceContext context, string suffix, IReadOnlyDictionary<string, Input<string>> files)
    {
        var name = $"{context.KubernetesName}-{suffix}";
        var metadata = new ObjectMetaArgs { Name = name, Namespace = context.Namespace };
        foreach (var (label, value) in context.Labels)
        {
            metadata.Labels.Add(label, value);
        }

        var data = new InputMap<string>();
        foreach (var (file, content) in files)
        {
            data[file] = content;
        }

        context.Declared.Add(new ConfigMap(
            $"{context.Name}-{suffix}",
            new ConfigMapArgs { Metadata = metadata, Data = data },
            new CustomResourceOptions { Parent = context.Parent, Provider = context.Provider, DependsOn = [.. context.DependsOn] }));
        return name;
    }
}
