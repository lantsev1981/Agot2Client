using Agot2Client;
using System.Collections.Concurrent;

namespace GamePortal
{
    public class PortalSettings
    {
        public ConcurrentDictionary<string, GPUser> GamePortal { get; set; }

        public PortalSettings()
        {
            GamePortal = new ConcurrentDictionary<string, GPUser>();
        }
    }
}
