using System;
using Raygun.Blazor.Models;

namespace Raygun.Blazor.Offline;

/// <summary>
/// Data type for storing crash reports in the local application data folder.
/// </summary>
public sealed class CrashReportStoreEntry
{
    /// <summary>
    /// Creates a new instance of the <see cref="CrashReportStoreEntry"/> class.
    /// </summary>
    /// <param name="id">Unique id for the report</param>
    /// <param name="raygunRequest">Raygun request</param>
    public CrashReportStoreEntry(Guid id, RaygunRequest raygunRequest)
    {
        Id = id;
        RaygunRequest = raygunRequest;
    }

    /// <summary>
    /// Unique ID for the record
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Request to store
    /// </summary>
    public RaygunRequest RaygunRequest { get; init; }
}