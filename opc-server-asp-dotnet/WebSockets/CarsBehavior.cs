using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace opc_server_asp_dotnet.WebSockets
{
    public class CarsBehavior : WebSocketBehavior
    {
        private string _name;
        private static int _number = 0;
        private string _prefix;
        private Thread _sendThread;
        private bool _keepSend = false;
        OPCMonitor opcMonitor;

        public CarsBehavior() : this(null)
        {
            opcMonitor = OPCMonitor.Instance;
        }
        public CarsBehavior(string prefix)
        {
            _prefix = !prefix.IsNullOrEmpty() ? prefix : "anon#";
        }
        private string getName()
        {
            var name = Context.QueryString["name"];
            return !name.IsNullOrEmpty() ? name : _prefix + getNumber();
        }
        private static int getNumber()
        {
            return Interlocked.Increment(ref _number);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            Sessions.Broadcast(String.Format("{0} got logged off...", _name));
            _keepSend = false;
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            Send(String.Format("{0}: {1}", _name, e.Data));
        }

        protected override void OnOpen()
        {
            _name = getName();
            _sendThread = new Thread(SendMethod);
            _sendThread.IsBackground = true;
            _keepSend = true;
            _sendThread.Start();
            //opcMonitor.WriteCarStatus(0, "true");
        }

        private void SendMethod(object obj)
        {
            while (_keepSend)
            {
                opcMonitor.Read();

                Send(Newtonsoft.Json.JsonConvert.SerializeObject(opcMonitor.CarOpcModels));
                Thread.Sleep(3000);
            }
        }
    }
}