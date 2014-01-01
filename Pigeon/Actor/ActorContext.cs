﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pigeon.Actor
{
    public class ActorContext : ActorRefFactory
    {             
        public LocalActorRef Self { get;  set; }
        public ActorRefFactory Parent { get; set; }

        protected ConcurrentBag<ActorRef> Children = new ConcurrentBag<ActorRef>();

        public override ActorRef ActorOf<TActor>(string name = null) 
        {
            if (name == null)
            {
                name = typeof(TActor).Name;
                if (name.EndsWith("Actor"))
                    name = name.Substring(0, name.Length - 5);

                name = name + "#" + Guid.NewGuid();
            }

            var existing = Child(name);
            if (existing != null)
                return existing;

            var context = new ActorContext
            {
                System = this.System,
                Self = new LocalActorRef(new ActorPath(name)),
                Parent = this,
            };
            Children.Add(context.Self);
            var actor = (ActorBase)Activator.CreateInstance(typeof(TActor), new object[] {context});
            return context.Self;
        }
        public override ActorRef Child(string name)
        {
            return Children.Where(actorRef => actorRef.Path.Name == name).FirstOrDefault();
        }

        public override ActorRef ActorSelection(string remoteActorPath)
        {
            var actorRef = new RemoteActorRef(this, remoteActorPath);
            return actorRef;
        }
    }    
}
