using MiniGames.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGames.Models
{
    public class VoteEvent
    {
        public string EventName { get; set; }
        public int Votes { get; set; }
        public int EventID { get; set; }
        public IEvent iEvent { get; set; }
    }
}
