// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Text;

namespace Cratis.Showrunner.StageManager.Understudies.Reporting;

/// <summary>
/// Renders the metrics of <see cref="UnderstudyMetrics"/> in the Prometheus text format. It listens to the meter by name, so it shows what the instruments report and nothing else.
/// </summary>
public sealed class PrometheusExporter : IDisposable
{
    readonly MeterListener _listener = new();
    readonly ConcurrentDictionary<string, (bool Counter, string Help, ConcurrentDictionary<string, double> Series)> _instruments = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes a new instance of the <see cref="PrometheusExporter"/> class.
    /// </summary>
    public PrometheusExporter()
    {
        _listener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == UnderstudyMetrics.MeterName)
            {
                _instruments[instrument.Name] = (instrument is Counter<long>, instrument.Description ?? string.Empty, new(StringComparer.Ordinal));
                listener.EnableMeasurementEvents(instrument);
            }
        };
        _listener.SetMeasurementEventCallback<long>((instrument, value, tags, _) => Record(instrument, value, tags));
        _listener.SetMeasurementEventCallback<int>((instrument, value, tags, _) => Record(instrument, value, tags));
        _listener.Start();
    }

    /// <summary>
    /// Renders the current values.
    /// </summary>
    /// <returns>The text.</returns>
    public string Render()
    {
        // Observable gauges are read afresh on every scrape, so a series that is gone disappears.
        foreach (var instrument in _instruments.Values.Where(instrument => !instrument.Counter))
        {
            instrument.Series.Clear();
        }

        _listener.RecordObservableInstruments();

        var text = new StringBuilder();
        foreach (var (name, instrument) in _instruments.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            text.Append("# HELP ").Append(name).Append(' ').Append(instrument.Help).Append('\n')
                .Append("# TYPE ").Append(name).Append(instrument.Counter ? " counter\n" : " gauge\n");
            foreach (var (labels, value) in instrument.Series.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                text.Append(name).Append(labels).Append(' ').Append(value.ToString("R", CultureInfo.InvariantCulture)).Append('\n');
            }
        }

        return text.ToString();
    }

    /// <inheritdoc/>
    public void Dispose() => _listener.Dispose();

    static string Labels(ReadOnlySpan<KeyValuePair<string, object?>> tags)
    {
        if (tags.IsEmpty)
        {
            return string.Empty;
        }

        var pairs = tags.ToArray().OrderBy(tag => tag.Key, StringComparer.Ordinal).Select(tag => $"{tag.Key}=\"{Escape(Convert.ToString(tag.Value, CultureInfo.InvariantCulture) ?? string.Empty)}\"");
        return "{" + string.Join(',', pairs) + "}";
    }

    static string Escape(string value) => value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal).Replace("\n", "\\n", StringComparison.Ordinal);

    void Record(Instrument instrument, double value, ReadOnlySpan<KeyValuePair<string, object?>> tags)
    {
        if (!_instruments.TryGetValue(instrument.Name, out var known))
        {
            return;
        }

        var labels = Labels(tags);
        if (known.Counter)
        {
            known.Series.AddOrUpdate(labels, static (_, added) => added, static (_, current, added) => current + added, value);
        }
        else
        {
            known.Series[labels] = value;
        }
    }
}
