# Implementation notes

## Prerequisites

This project assumes that the target project (the project
that will be using the ``UnityTutorialSystem`` library) exposes
relevant ``UnityEvent``s or can be modified to invoke the event
triggers on the UnityTutorialSystem when the relevant 
program state has changed.

The ``UnityTutorialSystem`` works best if your project has a 
clear set of states or conditions that can act as triggers
for tutorial event transitions. Natural examples of these 
states or conditions are tasks the player has completed or
events that have been triggered in the game world (ie
an enemy attacks, or supplies run low, etc).

## Defining Events

The ``UnityTutorialSystem`` is driven by a set of event streams.
The ``BasicEventStream`` class is a ``ScriptableObject`` that 
manages a predefined set of ``BasicEventStreamMessages``.
You can sub-class the ``BasicEventStream`` to generate more
specialised ``BasicEventStreamMessage`` types. As every
``BasicEventStreamMessage`` is an ``ScriptableObject`` itself you
can define additional properties and methods on these 
objects if necessary. (By using a scriptable object as 
basis of this implementation you can also reference those
events in prefabs and other scriptable object assets; and
it completely avoids the use of Singletons or other scene
based crutches to manage the flow of events.)

Each ``BasicEventStreamMessage`` carries a reference to its
declaring BasicEventStream. This means you can trigger a
message by simply calling '``BasicEventStreamMessage#Publish``'
at any time.

``BasicEventStream`` accepts event listeners that will be
called whenever a message managed by this stream has been 
published. As such the ``BasicEventStream`` acts as a simple
event bus for its defined messages. You can have multiple
event streams in your project and I recommend that you
create one ``BasicEventStream`` instance for each distinct
sequence of tasks you want to track.

The stream can handle a limited amount of reentrant events. 
This means that a message that is published via the stream 
can generate new events that are published via the same 
stream, and all of those messages will be sent out to all 
listeners.

To avoid infinite loops from configuration errors where
a message sends out new messages in an infinite loop, the
stream will stop processing after 250 messages have been
processed in the current frame.

The ``UnityTutorialSystem`` provides a ``PublishStreamEvent``
mono-behaviour that can be used to publish messages 
in response to other ``UnityEvent`` invokations.

``BasicEventStreamMessage`` objects are defined in the Unity-Editor
by editing the ``BasicEventStream`` itself. Each message entry
will generate a new ``BasicEventStreamMessage`` object as 
sub-asset of the ``BasicEventStream``. Each stream will only
process messages that it defined itself. However
``EventStreamMessageAggregator``s can combine messages from many
``BasicEventStream`` instances into higher level tracking events.

## Event Message Aggregators

An event message aggregator is a component that analyses
the events it has received to match predefined sequences
of events.

Each aggregator maintains a list of ``BasicEventStreamMessage``
objects it expects to see. During start up, the stream
will attempt to register itself with the ``BasicEventStream``
that publishes those messages.

The ``EventMessageAggregators`` implemented here are stateful
trackers that attempt to maintain only minimal state during
the matching process. Each time a new event is received,
the ``EventMessageAggregator`` will update its internal state
and will fire events to notify any listener of its eventual
state change.

Due to the structure of the matching done the ``EventMessageAggregator``
can tell which ``BasicEventStreamMessage`` would need to be 
received next to move the state closer to a succesful 
match. (This is very similar to a stream based pattern matcher
or regular expression matching.) Internally the ``EventMessageAggregator``
implementations use a state machine that can be in one of 
three states: Waiting for data, success, or failure. 

The ``EventMessageAggregator`` can provide detailed information
about its internal state, including which of the messages
have been seen, which will be (hopefully) seen next, and which
are not yet matched. 

All of this is implemented via the ``EventMessageAggregator#ListEvents``
method. This method accepts a buffer of ``EventMessageState`` 
data objects so that all calls can be completely non-allocating.

This information will be used by both the predictor components
and the ``TreeModelBuilder``.

When an ``EventMessageAggregator`` successfully matched all
expected events, it will fire an internal ``success`` event.
You can use an additional ``EventMessageAggregatorStatePublisher``
to publish a ``BasicEventStreamMessage`` when that happens.
The ``EventStreamTreeModelBuilder`` will interpret the fact that
a success of an aggregator caused an message to be published
as a hint that this ``EventMessageAggregator`` is a dependent
aggregator of any ``EventMessageAggregator``that waits for
that message.


## Predictors

The whole point of this library is to point players towards the
next goal in tutorial and other guided sequences. This is 
achieved with the help of the ``predictor`` components.

This library ships with two predictor components:

* NextEventSelector
  
  This is simple class monitors an set of ``EventMessageAggregator``
  instances to wait for a notification that the aggregator expects
  the given ``BasicEventStreamMessage`` as its next received message.
  
  When that happens this NextEventSelector fires a UnityEvent
  that you can use to enable or disable visual indicators or 
  to trigger any other action to guide the player to the next goal
  (or maybe to spawn enemies to prevent the player to get there).
  
* NextEventAggregationActivator

  This is specialized version of the NextEventSelector that 
  simplifies the wiring up of ``EventMessageAggregator`` 
  hierarchies. It is placed next to an 
  ``EventMessageAggregatorStatePublisher`` and will activate
  or deactivate the associated ``EventMessageAggregator`` when
  its ``success`` message is expected to be received next.
  
## User Interface  

The ``UnityTutorialSystem`` library comes with a TreeView component
that can render all ``BasicEventStreamMessage``s known to the
aggregators, their hierarchy and relationship between each other
and their current tracking state. 

The UI package contains the necessary code to render a TreeView
(or a list if you set the 'Indent' property to zero) of all
events using Unity's inbuilt UI system. 

The ``EventStreamTreeModelBuilder`` is responsible for monitoring
all ``EventMessageAggregator`` instances in a scene and produces 
a TreeModel of ``EventStreamTreeModelData`` objects that reflects
the current state of the tracking. The model is updated as soon
as any of the aggregators reports a new state change.

Unity does not like generics in serialized objects in a scene,
so to use the TreeView with your own data, you have to create
a non-generic sub-class of the ``TreeView<TData>`` class.
The ``TutorialEventTreeView`` is such an example.

The ``EventStreamTreeModelBuilder`` requires a list of 
``EventMessageAggregator`` instances to work. If you allow it,
it can fetch all ``EventMessageAggregator`` instances from the 
active scene, which is usually what you'd want anyway.

The Builder then builds up a static model of the events
processed by each of the aggregators and the relationships between
the aggregators. (Note: This happens only once, so any change
you might make to the set of aggregators afterwards, either
by adding more events or new aggregators, will NOT be reflected
in the tree model. Always define your event messages and 
aggregations so that all events are available when the 
scene starts. (And if you really MUST make changes, call
``EventStreamTreeModelBuilder#RebuildModel`` afterwards.)

## Tutorial Events

So far, all messages were pretty much generic. This library's
primary purpose is to make it easier to write tutorial levels
for games. However, no player wants to see 'Kill the Orc' after
they already slain the green humanoid. 

The ``Tutorial`` package contains a specialised ``TutorialEventStream``
that contains ``TutorialEventMessage`` objects. It also nicely
demonstrates how to use customized messages in this library.
A ``TutorialEventMessage`` has three description texts for the
event - one for when the task is not done yey ("Go kill that orc"),
one for when the task was a success ("You've slain the orc!")
and one for when the task failed ("The orc has slain you!").

A specialised TreeView (because Unity really does not like generics
and refuses to save references of such fields) offers some 
additional logic to possibly hide completed tasks.

The ``TutorialTreeItemRenderer`` is responsible for updating the
various UI components with the data from the ``EventStreamTreeModelData``
and its contained ``TutorialEventMessage`` with its three different
messages depending on what state the message is in. 

To connect the ``TutorialEventTreeView`` with the 
``EventStreamTreeModelBuilder`` that supplies the data that is
displayed, we have to utilize a ``TutorialEventTreeBinding``
MonoBehaviour. This class simply takes the model produced by
the ``EventStreamTreeModelBuilder`` and registers it in
the TreeView. Multiple TreeViews can share the same model.

