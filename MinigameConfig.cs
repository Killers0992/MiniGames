using Exiled.API.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGames
{
    public class MinigameConfig : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool VotingEnabled { get; set; } = true;
    }
}
