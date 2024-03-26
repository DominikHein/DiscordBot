using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Database
{
    public class DUser
    {

        public string userName { get; set; }
        public string serverName { get; set; }
        public ulong serverID { get; set; }
        public string avatarURL { get; set; }
        public double XP { get; set; }
        public int level { get; set; }
        public int xplimit { get; set; }
    }
}
