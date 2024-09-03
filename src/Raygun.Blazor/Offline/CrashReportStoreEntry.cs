using System;

namespace Raygun.Blazor.Offline;

public sealed class CrashReportStoreEntry
{
  /// <summary>
  /// Unique ID for the record
  /// </summary>
  public Guid Id { get; set; }

  /// <summary>
  /// The application api key that the payload was intended for
  /// </summary>
  /// TODO: Not sure if this is necessary
  public string ApiKey { get; set; }

  /// <summary>
  /// The JSON serialized payload of the error
  /// </summary>
  public string MessagePayload { get; set; }
}