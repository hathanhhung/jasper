using System;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Subscriptions;
using Microsoft.Extensions.Logging;

namespace Jasper.Messaging.Tracking
{
    /// <summary>
    /// Useful for automated testing scenarios against the service bus to "know"
    /// when all outstanding messages are completed. DO NOT USE IN PRODUCTION!!!
    /// </summary>
    public class MessageTrackingLogger : IMessageLogger
    {
        public static readonly string Envelope = "Envelope";
        public static readonly string Execution = "Execution";

        private readonly MessageHistory _history;
        private readonly IMessageLogger _trackingInnerLogger;


        public MessageTrackingLogger(MessageHistory history, IMessageLogger trackingInnerLogger)
        {
            _history = history;
            _trackingInnerLogger = trackingInnerLogger;
        }

        public void LogException(Exception ex, Guid correlationId = default(Guid), string message = "Exception detected:")
        {
           _trackingInnerLogger.LogException(ex, correlationId, message);
        }

        public void Sent(Envelope envelope)
        {
            _history.Start(envelope, Envelope);
            _trackingInnerLogger.Sent(envelope);
        }

        public void Received(Envelope envelope)
        {
            _trackingInnerLogger.Received(envelope);
        }

        public void ExecutionStarted(Envelope envelope)
        {
            _history.Start(envelope, Execution);
            _trackingInnerLogger.ExecutionStarted(envelope);
        }

        public void ExecutionFinished(Envelope envelope)
        {
            _history.Complete(envelope, Execution);
            _trackingInnerLogger.ExecutionFinished(envelope);
        }

        public void MessageSucceeded(Envelope envelope)
        {
            _history.Complete(envelope, Envelope);
            _trackingInnerLogger.MessageSucceeded(envelope);
        }

        public void MessageFailed(Envelope envelope, Exception ex)
        {
            _history.Complete(envelope, Envelope, ex);
            _trackingInnerLogger.MessageFailed(envelope, ex);
        }

        public void NoHandlerFor(Envelope envelope)
        {
            _trackingInnerLogger.NoHandlerFor(envelope);
        }

        public void NoRoutesFor(Envelope envelope)
        {
            _trackingInnerLogger.NoRoutesFor(envelope);
        }

        public void SubscriptionMismatch(PublisherSubscriberMismatch mismatch)
        {
            _trackingInnerLogger.SubscriptionMismatch(mismatch);
        }

        public void MovedToErrorQueue(Envelope envelope, Exception ex)
        {
            _trackingInnerLogger.MovedToErrorQueue(envelope, ex);
        }

        public void DiscardedEnvelope(Envelope envelope)
        {
            _trackingInnerLogger.DiscardedEnvelope(envelope);
        }

    }


}
