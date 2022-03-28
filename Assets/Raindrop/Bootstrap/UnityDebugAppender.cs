using log4net.Appender;
using log4net.Core;
using UnityEngine;

public class UnityDebugAppender : AppenderSkeleton
{
    protected override void Append(LoggingEvent loggingEvent)
    {
        var level = loggingEvent.Level;

        if (level == Level.Warn)
        {
            Debug.LogWarning(RenderLoggingEvent(loggingEvent));
            return;
        }
        if (level == Level.Error)
        {
            Debug.LogError(RenderLoggingEvent(loggingEvent));
            return;
        }

        Debug.Log(RenderLoggingEvent(loggingEvent));
    }
}