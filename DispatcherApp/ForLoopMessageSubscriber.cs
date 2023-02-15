using Library.Dispatchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherApp
{
    /// <summary>
    /// Subscriber handler for TestMessage type
    /// </summary>
    public class ForLoopMessageSubscriber :ISubscriber<ForLoopEvent, TestMessage>
    {
        /// <summary>
        /// all the testmessages received for potential processing later
        /// used a Hashset instead of a list for better performance, but a List would
        /// do in this case as well
        /// </summary>
        private readonly HashSet<TestMessage> _messagesReceived;

        /// <summary>
        /// .ctor
        /// </summary>
        public ForLoopMessageSubscriber()
        {
            _messagesReceived = new HashSet<TestMessage>();
        }

        /// <summary>
        /// Handle a TestMessage
        /// </summary>
        /// <param name="message"></param>
        public void HandleMessage(TestMessage message)
        {
            // release dispatcher handler as quickly as possible
            _messagesReceived.Add(message);
            Console.WriteLine($"Received message {message.Message} at {DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt")}");
        }
    }
}
