using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherApp
{

    /// <summary>
    /// Base Message Event Type class
    /// </summary>
    public class MessageEvent : IEventType
    {
        public int messageId { get; set; }
    }

    /// <summary>
    /// Event Type for the TestMessage type
    /// </summary>
    public class ForLoopEvent : MessageEvent
    {
    }


    /// <summary>
    /// Event Type for HappyBirthdayMessage type
    /// </summary>
    public class HappyBirthdayEvent : MessageEvent
    { 

    }

    /// <summary>
    /// Test Message class for testing the dispatcher
    /// </summary>
    public class TestMessage : IMessage
    {
        public string Message { get; set; }
        public DateTime TimeEntered { get; set; }

    }

    /// <summary>
    /// Happy Birthday Message
    /// </summary>
    public class HappyBirthdayMessage : IMessage
    {
        public string Name { get; set; }
    }
}
