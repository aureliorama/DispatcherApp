using System.Collections.Concurrent;
using NLog;

namespace Library.Dispatchers
{
    public interface IEventProcessor
    {
        void StartProcessor();// object param);
        void StopProcessor();
    }

    /// <summary>
    /// Base for Any Dispatch message
    /// </summary>
    public class DispatchMessage<TKeyType, TDispatchMessageType>
    {
        public TKeyType Key { get; set; }
        public TDispatchMessageType? TheDispatchMessage { get; set; }
    }

    

    /// <summary>
    /// Generic class for a dispatcher 
    /// </summary>
    /// <typeparam name="TKeyType"></typeparam>
    /// <typeparam name="TDispatchMessageType"></typeparam>
    public class DispatchMessageProcessor<TKeyType, TDispatchMessageType> : IEventProcessor
    {
        public delegate void OnErrorShutdown();
        public event OnErrorShutdown OnErrorShutdownEvent;

        /// <summary>
        /// Threadsafe queue "wraps" a concurrentqueue
        /// </summary>
        protected BlockingCollection<DispatchMessage<TKeyType, TDispatchMessageType>> Queue;
        protected volatile bool IsRunning;
        /// <summary>
        /// main processor thread
        /// </summary>
        protected Task Processor;

        // threading objects
        protected readonly object StateLock = new object();
        protected readonly CancellationTokenSource TokenSource = new CancellationTokenSource();

        protected readonly ILogger Logger = LogManager.GetLogger(String.Format("RequestHandlerProcessor_{0}", typeof(TDispatchMessageType)));
        
        // Request handlers for the dispatcher
        protected readonly Dictionary<TKeyType, List<Action<TDispatchMessageType>>> RequestHandlers;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="startThread">starts the thread on craetion</param>
        public DispatchMessageProcessor(bool startThread)
        {
            Queue = new BlockingCollection<DispatchMessage<TKeyType, TDispatchMessageType>>();
            RequestHandlers = new Dictionary<TKeyType, List<Action<TDispatchMessageType>>>();
            if (!startThread) return;
            // start a new thread here for the application
            var messageDispatcherThread = new Thread(StartProcessor);
            messageDispatcherThread.Start();
        }

        /// <summary>
        /// Method to queue a new request/message
        /// </summary>
        /// <param name="msg"></param>
        public void QueueRequest(DispatchMessage<TKeyType, TDispatchMessageType> msg)
        {
            Queue.Add(msg);
        }

        /// <summary>
        /// Subscribe a callback action for a specific message type
        /// </summary>
        /// <param name="callbackAction"></param>
        /// <param name="eventType"></param>
        public void Subscribe(Action<TDispatchMessageType> callbackAction, TKeyType eventType)
        {
            if (!RequestHandlers.ContainsKey(eventType))
            {
                RequestHandlers.Add(eventType, new List<Action<TDispatchMessageType>>());
            }

            if (!RequestHandlers[eventType].Contains(callbackAction))
            {
                RequestHandlers[eventType].Add(callbackAction);
            }
        }

        /// <summary>
        /// Remove handler from message type
        /// </summary>
        /// <param name="callbackAction"></param>
        /// <param name="eventType"></param>
        public void Unsubscribe(Action<TDispatchMessageType> callbackAction, TKeyType eventType)
        {
            if (!RequestHandlers.ContainsKey(eventType)) return;
            if (RequestHandlers[eventType].Contains(callbackAction))
            {
                RequestHandlers[eventType].Remove(callbackAction);
            }
        }

        /// <summary>
        /// Processing thread function
        /// </summary>
        /// <param name="param"></param>
        public void StartProcessor()//object param)
        {
            try
            {
                Logger.Info(String.Format("Starting RequestHandlerProcessor Thread of type {0}", typeof(TDispatchMessageType)));

                IsRunning = true;
                Processor = Task.Factory.StartNew(() =>
                {
                    try
                    {

                        while (IsRunning)
                        {
                            DispatchMessage<TKeyType, TDispatchMessageType> req;
                            if (Queue.TryTake(out req, Timeout.Infinite,
                                TokenSource.Token))
                            {
                                try
                                {
                                    HandleRequest(req);
                                }
                                catch (Exception ex)
                                {
                                    // catch excpetions to allow further processing
                                    Logger.Error(
                                        String.Format(
                                            "Error in handling RequestHandlerProcessor of type {0} request. Exception: {1}",
                                            typeof(TDispatchMessageType),
                                            ex.ToString()));
                                }

                            }
                            else
                            {
                                Logger.Error(String.Format("Pull from RequestHandlerProcessor of type {0} queue failed.", typeof(TDispatchMessageType)));
                            }
                        }
                    }

                    catch (OperationCanceledException)
                    {
                        Logger.Info(String.Format("RequestHandlerProcessor of type {0} canceled.", typeof(TDispatchMessageType)));
                    }
                });

            }
            catch (Exception ex)
            {
                Logger.Error(String.Format("Exception in RequestHandlerProcessor of type {0} stopped. {1} ", typeof(TDispatchMessageType), ex.ToString()));
                OnError();
            }
        }

        /// <summary>
        /// Calls whatever Shutdown functions need to be called to cleanup
        /// </summary>
        protected void OnError()
        {
            if (ReferenceEquals(null, OnErrorShutdownEvent)) return;
            OnErrorShutdownEvent();
            OnErrorShutdownEvent -= OnErrorShutdownEvent;
        }

        /// <summary>
        /// Stops handler
        /// </summary>
        public void StopProcessor()
        {
            lock (StateLock)
            {
                if (!IsRunning) return;
                IsRunning = false;
                TokenSource.Cancel(false);
                Processor.Wait();
                Logger.Info(String.Format("RequestHandlerProcessor of type {0} stopped.", typeof(TDispatchMessageType)));
            }
        }

        /// <summary>
        /// Main Request Handler, inherited classes can implement this in their own way if desired
        /// </summary>
        /// <param name="request"></param>
        protected virtual void HandleRequest(DispatchMessage<TKeyType, TDispatchMessageType> request)
        {
            if (!RequestHandlers.ContainsKey(request.Key))
            {
                Logger.Info("Request handlers does not have key - " + request.Key);
                return;
            }
            RequestHandlers[request.Key].ForEach(cb => cb(request.TheDispatchMessage));
        }
    }
}