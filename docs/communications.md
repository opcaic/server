# Worload distribution implementation

The actual tournament execution is delegated to standalone worker machines. The
server sends a worker a message with describing which match needs to be
executed, and later, the worker sends back a response. In the following text, we
will refer to the load-balancing part on the web server as a *broker*, and by
*worker* we will mean the part of worker executable which communicates with the
broker

The broker and worker communication is implemented using NetMQ library, which is
a dotnet implementation of the [ZeroMQ](https://github.com/zeromq/netmq)
messaging framework. This messaging framework is very lightweit and lowlevel and
there is an infrastructure built around it to hide it from the rest of the
platform.

## Connectors

The accesses to the NetMQ sockets is encapsulated in *connector* classes. There
are two separate connector implementations: `BrokerConnector` and
`WorkerConnector`. These classes utilize the ZeroMQ *router* and *dealer*
sockets, respectively. The main job of these classes is to manage these sockets,
send heartbeat messages and convert the sent and received messages between
objects and ZeroMQ
frames.

The connectors API allows users to send any serializable object. Receiving
messages is done by registering *handlers* for a particular message object type
by calling `RegisterHandler` and `RegisterHandlerAsync`. The difference between
the two is that the "sync" version gets invoked on the same thread which
monitors the socket and should therefore be used for very short callbacks, while
the async handler gets called on different thread and does not block the
socket. See threading model below for more information. The registered handler
is invoked when a message of given type is received. Handlers have the form of
simple `Action<TMessage>` on worker, and `Action<string, TMessage>` on
broker. The first argument in the broker version is the identifier of the worker
who sent the message.

### Connector classes threading model

Each connector uses two threads to manage the communication. Throughout the
codebase, these threads are called the *socket* thread and the *consumer*
thread. The socket thread is the only thread that is accessing the actual socket
to send or receive a message. It also takes care of sending heartbeat messages
when appropriate. The consumer thread is intended to run handlers which for time
constraints cannot be run on the socket thread (the so-called async handlers).

The communication between these two threads happens through the `NetMQPoller`
objects. This class allows passively waiting on multiple NetMQ socket-like
objects at the same time. It also derives from `TaskScheduler`, so one can
schedule tasks to be executed on the thread serving the poller. Sending a
message therefore involves scheduling a `Task` object on the `NetMQPoller`
instance belonging to the socket thread, and the task can then access the ZeroMQ
socket without further synchronization. Similarly, after a message is received
by the socket thread, a task invoking the responsible handler is scheduled on
the consumer poller instance.

This means that all (async) handlers are executed on the same thread and
therefore there is no further need for synchronizing accesses to data
structures.

The following image illustrates the process of propagating a message from a
broker to a worker:

![connector-example](img/connector-example.svg)

### Heartbeat

In order for the communication to be robust and be able to detect failure and
crash of either broker or (any) worker. The communicators implement a scheme
called Paranoid Pirate Pattern in the official [ZeroMQ
documentation](http://zguide.zeromq.org/php:chapter4#Robust-Reliable-Queuing-Paranoid-Pirate-Pattern). In
Paranoid Pirate, both sides periodically send messages back and forth
essentially saying "I am alive" (these are generally called *heartbeat*
messages). If one side does not receive a message for a certain period of time,
e.g. three times the beat interval, then the other side is considered dead
and/or unreachable.

## Broker

The `Broker` class wraps the `BrokerConnector` and implements simple
load-balancing based on the capabilities of the workers. Through its interface,
the rest of the web backend can schedule jobs to be executed on the worker
machines without needing any more specific knowledge about workers.

### Threading model

All public methods on the `Broker` class should use the `Schedule` helper method
so that the actual code executes on the consumer thread of the worker to avoid
the need for locks and other synchronization. Private methods can be executed
only on the consumer thread by design and no further scheduling is needed.

### Load balancing

The broker keeps a queue of jobs sorted by the time the job was enqueued for
execution by the rest of the platform. When a worker is available, a task is
chosen such that it is the oldest enqueued job for a game that the worker is
capable of executing. In case of a worker crashes while executing a job, the job
is put back to work queue and can be later scheduled for execution on a
different worker.

## Worker

TODO:
