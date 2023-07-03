using System;
using System.Collections.Generic;
using System.IO;
using SuperNova.Events.ServerEvents;
using SuperNova.Modules.Relay2;
using SuperNova.Modules.Relay2.Discord2;
using SuperNova.Modules.Relay2.IRC2;
namespace SuperNova
{
    public sealed class RelayRoutePlugin2 : Plugin
    {
        public override string SuperNova_Version { get { return "1.9.3.4"; } }
        public override string name { get { return "RelayRoute2"; } }
        List<Route> routes = new List<Route>();

        class Route
        {
            public RelayBot2 srcBot2, dstBot2;
            public string srcChan2, dstChan2;
        }

        public override void Load(bool startup)
        {
            OnChannelMessageEvent.Register(OnMessage2, Priority.Low);
            OnConfigUpdatedEvent.Register(OnConfigUpdated, Priority.Low);
            OnConfigUpdated();
        }

        public override void Unload(bool shutdown)
        {
            OnChannelMessageEvent.Unregister(OnMessage2);
            OnConfigUpdatedEvent.Unregister(OnConfigUpdated);
        }

        void OnMessage2(RelayBot2 bot, string channel, RelayUser2 user, string message, ref bool cancel)
        {
            // ignore messages from relay bots themselves
            if (cancel || user.ID == bot.UserID) return;

            foreach (Route route in routes)
            {
                if (route.srcBot2 != bot) continue;
                if (route.srcChan2 != channel) continue;

                string msg = "(" + bot.RelayName + ") " + user.Nick + ": " + message;
                route.dstBot2.SendMessage(route.dstChan2, msg);
            }
        }
        void OnConfigUpdated() { LoadRoutes(); }


        const string ROUTES_FILE = "text/relayroutes2.txt";
        static string[] default_routes = new string[] {
            "# This file contains a list of routes for relay2 bots",
            "# Each route should be on a separate line",
            "#    Note: Only messages sent by other users are routed",
            "#",
            "# Each route must use the following format: ",
            "#    [source service] [source channel] : [destination service] [destination channel]",
            "#",
            "# Some examples:",
            "# - Route from Discord2 channel 123456789 to IRC2 channel #test",
            "#    Discord2 123456789 : IRC2 #test",
            "# - Route from Discord2 channel 123456789 to Discord2 channel 987654321",
            "#    Discord2 123456789 : Discord2 987654321",
            "# - Route from IRC2 channel #test to Discord2 channel 123456789",
            "#    IRC2 #test : Discord2 123456789",
        };

        void LoadRoutes()
        {
            if (!File.Exists(ROUTES_FILE))
                File.WriteAllLines(ROUTES_FILE, default_routes);

            string[] lines = File.ReadAllLines(ROUTES_FILE);
            List<Route> r = new List<Route>();

            foreach (string line in lines)
            {
                if (line.IsCommentLine()) continue;
                try
                {
                    r.Add(ParseRouteLine(line));
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error parsing route '" + line + "'", ex);
                }
            }
            routes = r;
        }

        Route ParseRouteLine(string line)
        {
            string[] bits = line.Split(':');
            if (bits.Length != 2)
                throw new ArgumentException("Route requires exactly 1 separating :");

            Route r = new Route();
            ParseRouteNode2(bits[0], out r.srcBot2, out r.srcChan2);
            ParseRouteNode2(bits[1], out r.dstBot2, out r.dstChan2);

            return r;
        }

        void ParseRouteNode2(string part, out RelayBot2 bot, out string chan)
        {
            string[] bits = part.Trim().Split(' ');
            if (bits.Length != 2)
                throw new ArgumentException("A space is required between route service and channel");

            bot = GetRouteBot2(bits[0]);
            chan = bits[1];
        }

        RelayBot2 GetRouteBot2(string service)
        {
            if (service.CaselessEq("IRC2")) return IRCPlugin2.Bot;
            if (service.CaselessEq("Discord2")) return DiscordPlugin2.Bot2;
            throw new ArgumentException("Unknown service '" + service + "'");
        }
    }
}