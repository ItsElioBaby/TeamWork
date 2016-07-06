using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Threading;

namespace TeamWork_Server.HTTP
{
    public class HttpContextReceivedEventArgs : EventArgs
    {
        public HttpListenerContext Context;

        public HttpContextReceivedEventArgs(HttpListenerContext c)
        {
            Context = c;
        }
    }

    public class HttpService
    {
        HttpListener listener;

        //private static int num = 0;

        public HttpService(string ip, int port)
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://" + ip + ":" + port.ToString() + "/");
        }

        public void RegisterNewPrefix(string prefix)
        {
            listener.Prefixes.Add(prefix);
            //Log.Info("Registered new prefix: " + prefix);
        }

        public event EventHandler<HttpContextReceivedEventArgs> HttpContextReceived;

        private void Gotcha(IAsyncResult ar)
        {
            HttpListenerContext context = listener.EndGetContext(ar);
            ThreadPool.QueueUserWorkItem(delegate
            {
                if (HttpContextReceived != null)
                {
                    HttpContextReceived(this, new HttpContextReceivedEventArgs(context));
                }
            });
            listener.BeginGetContext(Gotcha, null);
        }

        private void xRun()
        {
            while(true)
            {
                HttpListenerContext context = listener.GetContext();
                /*ThreadPool.QueueUserWorkItem(delegate
                {
                    if (HttpContextReceived != null)
                        HttpContextReceived(this, new HttpContextReceivedEventArgs(context));
                });*/
                Thread t = new Thread(x => { if (HttpContextReceived != null) HttpContextReceived(this, new HttpContextReceivedEventArgs(context)); });
                t.IsBackground = true;
                t.Start(null);
            }
        }

        public void Start()
        {
            listener.Start();
            //THIS IS NOT NEEDED, AS IT'S PART OF MY OTHER PROJECT... CommandsFactory.ExecuteStatusCommand("HTTP", true);
            listener.BeginGetContext(Gotcha, null);
	    // Here we don't wanna create new worker threads. Instead doing an asynchronous send-recieve method we consume way less CPU.
            //new Thread(xRun).Start();
        }

        public void Pause()
        {
            listener.Stop();
            //CommandsFactory.ExecuteStatusCommand("HTTP", false);
        }
    }
}
