using Exiled.API.Features;
using Exiled.API.Interfaces;
using Exiled.Loader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Test
{
    public class PluginConfig : IConfig
    {
        public bool IsEnabled { get; set; } = true;
    }

    public class MainClass : Plugin<PluginConfig>
    {
        public override string Author { get; } = "Killers0992";
        public override string Name { get; } = "Test";
        public override string Prefix { get; } = "test";

        public override void OnEnabled()
        {
            base.OnEnabled();
        }
    }
}
