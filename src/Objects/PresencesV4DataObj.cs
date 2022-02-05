using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssistBackgroundClient.Objects
{
    internal class PresencesV4DataObj
    {
        public Data data { get; set; }
        public string eventType { get; set; }
        public string uri { get; set; }
    }
        public class Presence
        {
            public object actor { get; set; }
            public string basic { get; set; }
            public object details { get; set; }
            public string game_name { get; set; }
            public string game_tag { get; set; }
            public object location { get; set; }
            public object msg { get; set; }
            public string name { get; set; }
            public object patchline { get; set; }
            public string pid { get; set; }
            public object platform { get; set; }
            public string @private { get; set; }
            public object privateJwt { get; set; }
            public string product { get; set; }
            public string puuid { get; set; }
            public string region { get; set; }
            public string resource { get; set; }
            public string state { get; set; }
            public string summary { get; set; }
            public long time { get; set; }
        }

        public class Data
        {
            public List<Presence> presences { get; set; }
        }

    }
