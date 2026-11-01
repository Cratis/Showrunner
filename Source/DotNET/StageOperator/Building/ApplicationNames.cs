// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageOperator.Resources;

namespace Cratis.Showrunner.StageOperator.Building;

/// <summary>The names, labels and ports of the objects the operator generates for an application.</summary>
public static class ApplicationNames
{
    /// <summary>The label that names the application instance.</summary>
    public const string InstanceLabel = "showrunner.cratis.io/instance";

    /// <summary>The label that names the part of the application an object belongs to.</summary>
    public const string ComponentLabel = "showrunner.cratis.io/component";

    /// <summary>The label the publisher sets with the publication id.</summary>
    public const string PublicationLabel = "showrunner.cratis.io/publication-id";

    /// <summary>The label the publisher sets with the organization.</summary>
    public const string OrganizationLabel = "showrunner.cratis.io/organization";

    /// <summary>The component label value of the application pod.</summary>
    public const string AppComponent = "app";

    /// <summary>The component label value of the MongoDB pod.</summary>
    public const string MongoComponent = "mongo";

    /// <summary>The name of the Stage container.</summary>
    public const string StageContainer = "stage";

    /// <summary>The name of the sign-in proxy container.</summary>
    public const string ProxyContainer = "auth-proxy";

    /// <summary>The port Stage listens on. It is reachable only inside the pod.</summary>
    public const int StagePort = 9090;

    /// <summary>The port Chronicle listens on. It is reachable only inside the pod.</summary>
    public const int ChroniclePort = 35000;

    /// <summary>The MongoDB port.</summary>
    public const int MongoPort = 27017;

    /// <summary>The name of the proxy port on the pod, the Service and the Ingress backend.</summary>
    public const string ProxyPortName = "http";

    /// <summary>The port the Service listens on.</summary>
    public const int ServicePort = 80;

    /// <summary>The paths that must never be reachable through the ingress.</summary>
    public static readonly string[] DeniedPaths =
    [
        "/scalar",
        "/openapi",
        "/workbench",
        "/read-models",
        "/stage/load",
        "/stage/semantic"
    ];

    /// <summary>Gets the name of the Mongo StatefulSet and its headless Service.</summary>
    /// <param name="application">The application.</param>
    /// <returns>The name.</returns>
    public static string Mongo(StageApplication application) => $"{application.Name}-mongo";

    /// <summary>Gets the name of the Service that has no endpoints and answers the denied paths.</summary>
    /// <param name="application">The application.</param>
    /// <returns>The name.</returns>
    public static string Denied(StageApplication application) => $"{application.Name}-denied";

    /// <summary>Gets the host of the application.</summary>
    /// <param name="application">The application.</param>
    /// <param name="baseDomain">The base domain.</param>
    /// <returns>The host.</returns>
    public static string Host(StageApplication application, string baseDomain) => $"{application.Spec.PublishedName}.{baseDomain}";

    /// <summary>Makes a value usable as a label value: valid characters only, at most 63 characters, starting and ending alphanumeric.</summary>
    /// <param name="value">The value.</param>
    /// <returns>The label value.</returns>
    public static string LabelValue(string value)
    {
        var chars = value.Select(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.' ? c : '_').ToArray();
        var text = new string(chars);
        if (text.Length > 63)
        {
            text = text[..63];
        }

        return text.Trim('-', '_', '.');
    }
}
