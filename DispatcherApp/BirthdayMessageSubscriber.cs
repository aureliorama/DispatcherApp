using Library.Dispatchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherApp
{
    /// <summary>
    /// Birthday message handler
    /// </summary>
    public class BirthdayMessageSubsciber : ISubscriber<HappyBirthdayEvent, HappyBirthdayMessage>
    {
        /// <summary>
        /// all birthday messages received for potential processing later
        /// </summary>
        private readonly HashSet<HappyBirthdayMessage> _messagesReceived;

        /// <summary>
        /// .ctor
        /// </summary>
        public BirthdayMessageSubsciber()
        {
            _messagesReceived = new HashSet<HappyBirthdayMessage>();
        }

        /// <summary>
        /// Message handler implementation for birthday messages
        /// </summary>
        /// <param name="message"></param>
        public void HandleMessage(HappyBirthdayMessage message)
        {
            _messagesReceived.Add(message);
            Console.WriteLine($"Happy Birthday {message.Name}!! Time: {DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt")}");
        }
    }
}
