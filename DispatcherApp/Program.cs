// See https://aka.ms/new-console-template for more information
using DispatcherApp;
using Library.Dispatchers;

/*
 * This application uses a generic dispatcher that can be used in different ways. THe generic uses a threadsafe
 * BlockingCollection to manage messages in and dequeuing messages
 * 
 * You can use the dispatcher in various ways. For example:
 * One dispatcher for all message types we want to handle (the example in this application)
 * One dispatcher per message type 
 *     Potentially you have the dispatcher connected to a guaranteed queue to handle a single type of message (i.e., orders)
 * 
 * 
 * In this example, we have our main program that is has subscribers for different message types
 * One dispatcher thread to handle all messages in 
 * 2 message "sending" threads adding messages to the single dispatcher thread
 *    TestMessage sender
 *    BirthdayMessage sender
 *    
 */

// this dispatcher is a single thread to handle all messages of one type
var dispatcher = new DispatchMessageProcessor<IEventType, IMessage>(false);

// subscriber event types
var forLoopSubscribeMessageEventType = new ForLoopEvent();
var birthdayEventType = new HappyBirthdayEvent();

// subscribers 
var subscriber = new ForLoopMessageSubscriber();
var birthdaySubscriber = new BirthdayMessageSubsciber();

// callback for subscriptions
var subscriberCallback = (IMessage message) => {
    subscriber.HandleMessage(message as TestMessage); };
var birthdaySubscriberCallback = (IMessage message) => { 
    birthdaySubscriber.HandleMessage(message as HappyBirthdayMessage); 
}; 

// subscribe for two different message types in one single dispatcher 
dispatcher.Subscribe(subscriberCallback, forLoopSubscribeMessageEventType);
dispatcher.Subscribe(birthdaySubscriberCallback, birthdayEventType);

// start the dispatcher
dispatcher.StartProcessor();

// this will act as the "sending thread" for TestMessage types
var task = Task.Factory.StartNew(() =>
{
    for (int i = 0; i < 100; i++)
    {

        Console.WriteLine($"Adding Test message {i} at {DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt")}");

        dispatcher.QueueRequest(new DispatchMessage<IEventType, IMessage>
        {
            Key =forLoopSubscribeMessageEventType,
            TheDispatchMessage = new TestMessage { Message = $"Message number {i}!", TimeEntered = DateTime.Now }
        });
    }
});

// sending thread for birthday messages
var birthdayTask = Task.Factory.StartNew(() =>
{
    for (int i = 0; i < 100; i++)
    {

        Console.WriteLine($"Adding Birthday message {i} at {DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt")}");

        dispatcher.QueueRequest(new DispatchMessage<IEventType, IMessage>
        {
            Key = birthdayEventType,
            TheDispatchMessage = new HappyBirthdayMessage {Name = $"Child number {i}"}
        });
    }
});

// wait for the senders
task.Wait();
birthdayTask.Wait();

// trigger shutdown
Console.ReadLine();
dispatcher.Unsubscribe(subscriberCallback, forLoopSubscribeMessageEventType);
dispatcher.Unsubscribe(birthdaySubscriberCallback, birthdayEventType);
dispatcher.StopProcessor();
