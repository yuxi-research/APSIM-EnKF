﻿// -----------------------------------------------------------------------
// <copyright file="Events.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------
namespace Models.Core
{
    using APSIM.Shared.Utilities;
    using Models.Core;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    class Events
    {
        private List<Events.Publisher> publishers = null;
        private List<Events.Subscriber> subscribers = null;
        private Dictionary<IModel, List<Subscriber>> cache = new Dictionary<IModel, List<Subscriber>>();
        private Simulation simulation;

        /// <summary>Default constructor</summary>
        public Events() { }

        /// <summary>Constructor</summary>
        /// <param name="simulation">Parent simulation</param>
        public Events(Simulation simulation)
        {
            this.simulation = simulation;
        }
        /// <summary>Connect all events in the specified simulation.</summary>
        internal void ConnectEvents()
        {
            // Get a complete list of all models in simulation (including the simulation itself).
            List<IModel> allModels = new List<IModel>();
            allModels.Add(simulation);
            allModels.AddRange(Apsim.ChildrenRecursively(simulation));

            if (publishers == null)
                publishers = Events.Publisher.FindAll(allModels);
            if (subscribers == null)
                subscribers = Events.Subscriber.FindAll(allModels);

            // Connect publishers to subscribers.
            foreach (Events.Publisher publisher in publishers)
                ConnectPublisherToScriber(publisher, FilterSubscribersInScope(publisher));
        }

        /// <summary>Connect all events in the specified simulation.</summary>
        /// <param name="model"></param>
        internal void DisconnectEvents(IModel model)
        {
            foreach (Events.Publisher publisher in publishers)
                publisher.DisconnectAll();
        }

        /// <summary>
        /// Scan a model and all child model for events and add the found publishers and subscribers
        /// to the list of known events and handlers.
        /// </summary>
        /// <param name="model">The model to scan</param>
        internal void AddModelEvents(IModel model)
        {
            // Get a complete list of all models in simulation (including the simulation itself).
            List<IModel> allModels = new List<IModel>();
            allModels.Add(model);
            allModels.AddRange(Apsim.ChildrenRecursively(model));
            if (publishers == null)
            {
                publishers = new List<Core.Events.Publisher>();
                subscribers = new List<Core.Events.Subscriber>();
            }
            publishers.AddRange(Events.Publisher.FindAll(allModels));
            subscribers.AddRange(Events.Subscriber.FindAll(allModels));
        }

        /// <summary>
        /// Remove a model and all child model events and handlers from the list.
        /// </summary>
        /// <param name="model">The model to scan</param>
        internal void RemoveModelEvents(IModel model)
        {
            // Get a complete list of all models in simulation (including the simulation itself).
            List<IModel> allModels = new List<IModel>();
            allModels.Add(model);
            allModels.AddRange(Apsim.ChildrenRecursively(model));

            publishers.RemoveAll(publisher => allModels.Contains(publisher.Model as IModel));
            subscribers.RemoveAll(subscriber => allModels.Contains(subscriber.Model as IModel));
        }

        /// <summary>
        /// Call the specified event on the specified model and all child models.
        /// </summary>
        /// <param name="model">The model to call the event on</param>
        /// <param name="eventName">The name of the event</param>
        /// <param name="args">The event arguments. Can be null</param>
        internal void CallEventHandler(IModel model, string eventName, object[] args)
        {
            List<IModel> allModels = new List<IModel>();
            allModels.Add(model);
            allModels.AddRange(Apsim.ChildrenRecursively(model));

            List<Subscriber> matches = subscribers.FindAll(subscriber => subscriber.Name == eventName &&
                                                                         allModels.Contains(subscriber.Model as IModel));

            foreach (Subscriber subscriber in matches)
                subscriber.Invoke(args);
        }

        /// <summary>Connect the specified publisher to all subscribers in scope</summary>
        /// <param name="publisher">Publisher to connect.</param>
        /// <param name="subscribers">All subscribers</param>
        private static void ConnectPublisherToScriber(Events.Publisher publisher, List<Events.Subscriber> subscribers)
        {
            // Find all publishers with the same name.
            List<Events.Subscriber> matchingSubscribers = subscribers.FindAll(subscriber => subscriber.Name == publisher.Name);

            // Connect subscriber to all matching publishers.
            matchingSubscribers.ForEach(subscriber => publisher.ConnectSubscriber(subscriber));
        }

        /// <summary>
        /// Return a list of subscribers that are in scope.
        /// </summary>
        /// <param name="relativeTo">Model to base scoping rules on.</param>
        private List<Subscriber> FilterSubscribersInScope(Publisher relativeTo)
        {
            // Try cache
            List<Subscriber> subscribersInScope;
            if (cache.TryGetValue(relativeTo.Model as IModel, out subscribersInScope))
                return subscribersInScope;

            List<IModel> modelsInScope = new List<IModel>(simulation.Scope.FindAll(relativeTo.Model as IModel));
            subscribersInScope = new List<Subscriber>();
            subscribersInScope = subscribers.FindAll(subscriber => modelsInScope.Contains(subscriber.Model as IModel));
            cache.Add(relativeTo.Model as IModel, subscribersInScope);
            return subscribersInScope;
        }



        /// <summary>A wrapper around an event subscriber MethodInfo.</summary>
        internal class Subscriber
        {
            /// <summary>The model instance containing the event hander.</summary>
            public object Model { get; set; }

            /// <summary>The method info for the event handler.</summary>
            private MethodInfo methodInfo { get; set; }

            /// <summary>Gets or sets the name of the event.</summary>
            public string Name { get; private set; }

            /// <summary>Find all event subscribers in the specified models.</summary>
            /// <param name="models">The models to scan for event handlers.</param>
            /// <returns>The list of event subscribers</returns>
            internal static List<Subscriber> FindAll(List<IModel> models)
            {
                List<Subscriber> subscribers = new List<Subscriber>();
                foreach (IModel modelNode in models)
                {
                    foreach (MethodInfo method in modelNode.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy))
                    {
                        EventSubscribeAttribute subscriberAttribute = (EventSubscribeAttribute)ReflectionUtilities.GetAttribute(method, typeof(EventSubscribeAttribute), false);
                        if (subscriberAttribute != null)
                            subscribers.Add(new Subscriber()
                            {
                                Name = subscriberAttribute.ToString(),
                                methodInfo = method,
                                Model = modelNode
                            });
                    }
                }

                return subscribers;
            }

            /// <summary>Creates and returns a delegate for the event handler.</summary>
            /// <param name="handlerType">The corresponding event publisher event handler type.</param>
            /// <returns>The delegate. Never returns null.</returns>
            internal virtual Delegate CreateDelegate(Type handlerType)
            {
                return Delegate.CreateDelegate(handlerType, Model, methodInfo);
            }

            /// <summary>
            /// Call the event handler.
            /// </summary>
            /// <param name="args"></param>
            internal void Invoke(object[] args)
            {
                methodInfo.Invoke(Model, args);
            }

        }

        /// <summary>
        /// A wrapper around an event publisher EventInfo.
        /// </summary>
        internal class Publisher
        {
            /// <summary>The model instance containing the event hander.</summary>
            public object Model { get; set; }

            /// <summary>The reflection event info instance.</summary>
            private EventInfo eventInfo;

            /// <summary>Return the event name.</summary>
            public string Name {  get { return eventInfo.Name; } }

            internal void ConnectSubscriber(Subscriber subscriber)
            {
                // connect subscriber to the event.
                Delegate eventDelegate = subscriber.CreateDelegate(eventInfo.EventHandlerType);
                eventInfo.AddEventHandler(Model, eventDelegate);
            }

            internal void DisconnectAll()
            {
                FieldInfo eventAsField = Model.GetType().GetField(Name, BindingFlags.Instance | BindingFlags.NonPublic);
                eventAsField.SetValue(Model, null);
            }

            /// <summary>Find all event publishers in the specified models.</summary>
            /// <param name="models">The models to scan for event publishers</param>
            /// <returns>The list of event publishers</returns>
            internal static List<Publisher> FindAll(List<IModel> models)
            {
                List<Publisher> publishers = new List<Publisher>();
                foreach (IModel modelNode in models)
                {
                    foreach (EventInfo eventInfo in modelNode.GetType().GetEvents(BindingFlags.Instance | BindingFlags.Public))
                        publishers.Add(new Publisher() { eventInfo = eventInfo, Model = modelNode });
                }

                return publishers;
            }
        }

    }
}
