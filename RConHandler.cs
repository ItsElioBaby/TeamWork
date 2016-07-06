// RConHandler is now obsolete. There's no need for a RCon tool from now on as I will be implementing web-based admin CP.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using twlib;

namespace TeamWork_Server
{
    public class RConHandler
    {
        Socket s;
        long s_key;

        TWServerConfiguration cfg;

        public RConHandler(long mkey, TWServerConfiguration config)
        {
            s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s_key = mkey;
            cfg = config;
        }

        private void xRun()
        {
            while (true)
            {
                Socket sc = s.Accept();
                byte[] bts = new byte[1024];
                sc.Receive(bts);
                Q1.RRR.BasicQ1BinaryFormation bfmt = new Q1.RRR.BasicQ1BinaryFormation();
                Task.Run(delegate {
                    try
                    {
                        bool response_k;
                        string response_s = "";
                        byte[] bts_d = bfmt.DePatch(bts);
                        UserCommand cmd = (UserCommand)Serialization.BinaryDeserialize(bts_d);
                        if (cmd.Key != s_key)
                        {
                            Log.Error("Command Key is different from the server. Packet has been discarted.");
                            response_k = false;
                            response_s = "Error: Command Key is different from the server.";
                        }
                        if (cmd.Command == "/set")
                        {
                            response_k = CommandsFactory.ExecuteSetCommand(cmd.Arguments[0], cmd.Arguments[1]);
                        }
                        else if (cmd.Command == "/lang?")
                        {
                            response_k = true;
                            response_s = cfg.Language;
                        }
                        else if (cmd.Command == "/setstate")
                        {
                            int status = int.Parse(cmd.Arguments[1]);
                            response_k = CommandsFactory.ExecuteStatusCommand(cmd.Arguments[0], (status == 1 ? true : false));
                        }
                        else if (cmd.Command == "/updatesub")
                        {
                            Subscriptions.Subscriptions.RemoveExpiredSubscriptions();
                            response_k = true;
                        }
                        else if (cmd.Command == "/get")
                        {
                            if (CommandsFactory.Variables.ContainsKey(cmd.Arguments[0]))
                            {
                                response_k = true;
                                response_s = cmd.Arguments[0] + " current value is: " + CommandsFactory.Variables[cmd.Arguments[0]] + ".";
                            }
                            else
                            {
                                response_k = false;
                                response_s = "Error: No variable associated with that name.";
                            }
                        }

                        else
                        {
                            Log.Error("Command '" + cmd.Command + "' has not been recognized. Packet has been discarted.");
                            response_k = false;
                            response_s = "Server Error: Command has not been recognized.";
                        }

                        ServerResponse response = new ServerResponse(response_k, s_key, response_s, null);
                        byte[] bts_res_d = Serialization.BinarySerialize(response);
                        byte[] bb = bfmt.Patch(bts_res_d);
                        sc.Send(bb);
                    }
                    catch (Exception ex)
                    {
                        //string response_str = "";
                        if (ex is Q1.RRR.BasicQ1BinaryFormationDePatchException)
                        {
                            Log.Error("Error during the packet decryption. Packet was probably invalid.");
                        }
                        else if (ex is Q1.RRR.BasicQ1BinaryFormationPatchException)
                        {
                            Log.Error("Error during the ecryption of the response packet.");
                        }
                        else if (ex is System.Runtime.Serialization.SerializationException)
                        {
                            Log.Error("Error during the packet deserialization. Packet was probably invalid.");
                        }
                        else
                        {
                            Log.Error("Unexpected error during the RCON CMD Handler operation.");
                        }
                        ServerResponse response = new ServerResponse(false, 0x00, "", null);
                        byte[] bts_res_d = Serialization.BinarySerialize(response);
                        byte[] bb = bfmt.Patch(bts_res_d);
                        sc.Send(bb);
                    }
                });
            }
        }

        public void Start()
        {
            s.Bind((EndPoint)(new IPEndPoint(IPAddress.Any, 5520)));
            s.Listen(3);
            new Thread(xRun).Start();
            Log.Debug("RCON Handler Is Now Operating");
        }
    }
}
