// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Alerts;

/// <summary>
/// One thing a check found wrong. The key identifies the condition across runs: it is the fingerprint the cooldown, the
/// first-seen time and Direct's de-duplication are all keyed on, so it must not change while the condition lasts.
/// </summary>
/// <param name="Key">The stable key of the condition, for example <c>pod:shop/web-1:CrashLoopBackOff</c>.</param>
/// <param name="Text">What is wrong and what to inspect. It must never contain a credential.</param>
/// <param name="Severity">How serious it is.</param>
public sealed record Finding(string Key, string Text, AlertSeverity Severity = AlertSeverity.Warning);
