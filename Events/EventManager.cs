using SingularityBase.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SingularityCore.Events
{
    public interface IEventManager
    {
    }

    internal  class EventManager : IEventManager
    {
        public static List<UserEvent> ActiveEvents { get; } = new List<UserEvent>();


        public static bool PluginActive
        {
            get {
                return ActiveEvents.Any(t =>
                    t.EventType == EventType.AddinButton || t.EventType == EventType.AddinEventReaction ||
                    t.EventType == EventType.AddinIconState);
            }
        }
    }

    internal class UserEvent : IDisposable
    {
        public EventType EventType { get; }
        public DefinedPlugin PlugIn { get; }
        public ISingleCommandDef Command { get; }
        internal UserEvent(DefinedPlugin plugIn, EventType eventType, ISingleCommandDef command)
        {
            EventType = eventType;
            Command = command;
            PlugIn = plugIn;
        }

        public void Dispose()
        {
            EventManager.ActiveEvents.Remove(this);
        }
    }

    internal enum EventType
    {
        Unknown,
        AddinButton,
        AddinIconState,
        AddinEventReaction,
        User,
        Solidworks
    }

}
