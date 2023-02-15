using Library.Dispatchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherApp
{
    /// <summary>
    /// Subscriber interface for a given event type
    /// </summary>
    public interface ISubscriber<IEventType, IMessage>
    {
        /// <summary>
        /// Message handler with whatever your implementation is 
        /// </summary>
        /// <param name="message"></param>
        public void HandleMessage(IMessage message);

    }
}
