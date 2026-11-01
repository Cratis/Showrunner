// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Herald;

/// <summary>
/// Represents how emails leave the application.
/// </summary>
public enum EmailTransportKind
{
    /// <summary>
    /// Emails are sent to a mail server over SMTP.
    /// </summary>
    Smtp = 0,

    /// <summary>
    /// Emails are written as <c>.eml</c> files to <see cref="HeraldOptions.PickupFolder"/>, for development: open them in
    /// any mail client, or point a mail catcher at the folder.
    /// </summary>
    Folder = 1
}
