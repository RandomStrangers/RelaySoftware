using System;
using RelaySoftware.Tasks;

namespace RelaySoftware
{
	public class HelloWorld : Plugin
	{
		public override string name { get { return "Saying hello"; } } // to unload, /punload hello
		public override string SuperNova_Version { get { return "0.0.0.1"; } }
		// public override string welcome { get { return "Loaded Message!"; } } You actually don't need to put this in here, it just sends a message when loaded.
		public override string creator { get { return "RandomStrangers & ccatgirl"; } } // Made from StartupNotify, so added you here, if you even decide to look at this.
		public override void Load(bool startup) {
     Server.MainScheduler.QueueOnce(SayHello, null, TimeSpan.FromSeconds(10));
		}
   
  void SayHello(SchedulerTask task) {
    Command.Find("say").Use(Player.Console, "Hello World!");
    Logger.Log(LogType.Warning, "&fHello World!");
  }
	public override void Unload(bool shutdown)
		{
     Server.MainScheduler.QueueOnce(SayGoodbye, null, TimeSpan.FromSeconds(0));
		}
  void SayGoodbye(SchedulerTask task) {
    Command.Find("say").Use(Player.Console, "Goodbye Cruel World!");
        Logger.Log(LogType.Warning, "&fGoodbye Cruel World!");
		}

		public override void Help(Player p)
		{
			p.Message("");
		}
	}
}