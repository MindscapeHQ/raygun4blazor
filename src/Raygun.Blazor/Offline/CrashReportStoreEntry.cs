using System;
using Raygun.Blazor.Models;

namespace Raygun.Blazor.Offline;

/// <summary>
/// 
/// </summary>
public sealed class CrashReportStoreEntry
{
  /// <summary>
  /// 
  /// </summary>
  /// <param name="id"></param>
  /// <param name="messagePayload"></param>
  public CrashReportStoreEntry(Guid id, RaygunRequest messagePayload)
  {
    Id = id;
    MessagePayload = messagePayload;
  }

  /// <summary>
  /// Unique ID for the record
  /// </summary>
  public Guid Id { get; init; }

  /// <summary>
  /// The JSON serialized payload of the error
  /// </summary>
  public RaygunRequest MessagePayload { get; init; }
}