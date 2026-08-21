namespace TaskBridge.Core.Constants;

/// <summary>
/// Constants for event types used in audit logging and notifications.
/// </summary>
public static class EventTypes
{
    /// <summary>Project was created.</summary>
    public const string ProjectCreated = "PROJECT_CREATED";

    /// <summary>Project milestone status was updated.</summary>
    public const string MilestoneUpdated = "MILESTONE_UPDATED";

    /// <summary>Project was deleted.</summary>
    public const string ProjectDeleted = "PROJECT_DELETED";

    /// <summary>Project milestone was reopened.</summary>
    public const string MilestoneReopened = "MILESTONE_REOPENED";
}
