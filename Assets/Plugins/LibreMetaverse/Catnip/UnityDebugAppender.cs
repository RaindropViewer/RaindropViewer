using log4net.Appender;
using log4net.Core;
using UnityEngine;

public class UnityDebugAppender : AppenderSkeleton
{
    protected override void Append(LoggingEvent loggingEvent)
    {
        if (loggingEvent.Level == Level.Info || loggingEvent.Level == Level.Debug)
        {
            var message = RenderLoggingEvent(loggingEvent);
            Debug.Log(message);
        }
        else if (loggingEvent.Level == Level.Warn)
        {
            var message = RenderLoggingEvent(loggingEvent);
            Debug.LogWarning(message);
        }
        else if (loggingEvent.Level == Level.Error || loggingEvent.Level == Level.Fatal)
        {
            var message = RenderLoggingEvent(loggingEvent);
            Debug.LogError(message);
        }
        else 
        {
            var message = RenderLoggingEvent(loggingEvent);
            Debug.Log(message);
        }


    }
}