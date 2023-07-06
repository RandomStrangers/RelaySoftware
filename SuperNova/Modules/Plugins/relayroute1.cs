using System;
using System.Collections.Generic;
using System.IO;
using SuperNova.Events.ServerEvents;
using SuperNova.Modules.Relay1;
using SuperNova.Modules.Relay1.Discord1;
using SuperNova.Modules.Relay1.IRC1;
namespace SuperNova
{
	public sealed class RelayRoutePlugin1 : Plugin
	{
        public override string creator { get { return Server.SoftwareName + " team"; } }
        public override string SuperNova_Version { get { return Server.Version; } }
        public override string name { get { return "RelayRoute1"; } }
		List<Route> routes = new List<Route>();
		
		class Route {
            public RelayBot1 srcBot1, dstBot1;
            public string srcChan1, dstChan1;
        }

		public override void Load(bool startup) {
			OnChannelMessageEvent.Register(OnMessage1, Priority.Low);
            OnConfigUpdatedEvent.Register(OnConfigUpdated, Priority.Low);
			OnConfigUpdated();
		}
		
		public override void Unload(bool shutdown) {
			OnChannelMessageEvent.Unregister(OnMessage1);
            OnConfigUpdatedEvent.Unregister(OnConfigUpdated);
		}
		
        void OnMessage1(RelayBot1 bot, string channel, RelayUser1 user, string message, ref bool cancel)
        {
            // ignore messages from relay bots themselves
            if (cancel || user.ID == bot.UserID) return;

            foreach (Route route in routes)
            {
                if (route.srcBot1 != bot) continue;
                if (route.srcChan1 != channel) continue;

                string msg = "(" + bot.RelayName + ") " + user.Nick + ": " + message;
                route.dstBot1.SendMessage(route.dstChan1, msg);
            }
        }
        void OnConfigUpdated() { LoadRoutes(); }
		
		
		const string ROUTES_FILE = "text/relayroutes1.txt";
		static string[] default_routes = new string[] {
			"# This file contains a list of routes for relay1 bots",
			"# Each route should be on a separate line",
			"#    Note: Only messages sent by other users are routed",
			"#",
			"# Each route must use the following format: ",
			"#    [source service] [source channel] : [destination service] [destination channel]",
			"#",
			"# Some examples:",
			"# - Route from Discord1 channel 123456789 to IRC1 channel #test",
            "#    Discord1 123456789 : IRC1 #test",
            "# - Route from Discord1 channel 123456789 to Discord1 channel 987654321",
            "#    Discord1 123456789 : Discord1 987654321",
            "# - Route from IRC1 channel #test to Discord1 channel 123456789",
            "#    IRC1 #test : Discord1 123456789",
        };
		
		void LoadRoutes() {
			if (!File.Exists(ROUTES_FILE))
				File.WriteAllLines(ROUTES_FILE, default_routes);
			
			string[] lines = File.ReadAllLines(ROUTES_FILE);
			List<Route> r  = new List<Route>();
			
			foreach (string line in lines)
			{
				if (line.IsCommentLine()) continue;
				try {
					r.Add(ParseRouteLine(line));
				} catch (Exception ex) {
					Logger.LogError("Error parsing route '" + line + "'", ex);
				}
			}
			routes = r;
		}
		
		Route ParseRouteLine(string line) {
			string[] bits = line.Split(':');
			if (bits.Length != 2)
				throw new ArgumentException("Route requires exactly 1 separating :");
			
			Route r = new Route();
            ParseRouteNode1(bits[0], out r.srcBot1, out r.srcChan1);
            ParseRouteNode1(bits[1], out r.dstBot1, out r.dstChan1);

            return r;
		}

         void ParseRouteNode1(string part, out RelayBot1 bot, out string chan)
        {
            string[] bits = part.Trim().Split(' ');
            if (bits.Length != 2)
                throw new ArgumentException("A space is required between route service and channel");

            bot = GetRouteBot1(bits[0]);
            chan = bits[1];
        }

		RelayBot1 GetRouteBot1(string service)
		{
			if (service.CaselessEq("IRC1")) return IRCPlugin1.Bot;
			if (service.CaselessEq("Discord1")) return DiscordPlugin1.Bot1;
			throw new ArgumentException("Unknown service '" + service + "'");
		}
    }
}