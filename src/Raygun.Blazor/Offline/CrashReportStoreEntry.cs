using System;
using Raygun.Blazor.Models;

namespace Raygun.Blazor.Offline;

public sealed class CrashReportStoreEntry
{
  /// <summary>
  /// Unique ID for the record
  /// </summary>
  public Guid Id { get; set; }

  /// <summary>
  /// The JSON serialized payload of the error
  /// </summary>
  public RaygunRequest MessagePayload { get; set; }
}