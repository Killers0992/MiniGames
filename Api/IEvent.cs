using Exiled.API.Features;
using Exiled.Events.EventArgs;
using System.Collections.Generic;

namespace MiniGames.Api
{
    public interface IEvent
    {
        string EventName { get; }
        string Description { get; }
        int EventStatus { get; set; }
        void Start();
        void OnDeath(DiedEventArgs ev);
        void OnJoin(VerifiedEventArgs ev);
        void OnLeave(DestroyingEventArgs ev);
        void OnHurt(HurtingEventArgs ev);
    }
}
