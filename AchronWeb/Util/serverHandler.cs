﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Networking
{
    class serverHandler
    {
        TcpClient socket;
        IPEndPoint endPoint;
        long startTime = DateTime.UtcNow.Ticks / 1000;

        //create a new client handler
        public serverHandler(TcpClient socket)
        {
            this.socket = socket;
            endPoint = (IPEndPoint)socket.Client.RemoteEndPoint;
            Console.WriteLine("new achron client connected. [" + endPoint.ToString() + "]");
        }

        //handle this client
        public void start()
        {
            NetworkStream ns = socket.GetStream();

            while (socket.Connected)
            {

                //we have data to handle!
                if (socket.Available != 0)
                {
                    byte[] data = new byte[socket.Available];
                    ns.Read(data, 0, socket.Available);
                    string decode = UTF8Encoding.UTF8.GetString(data, 0, data.Length);
                    string[] DataPackets = decode.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                    string[] args = DataPackets[0].Split(new char[] {' ', '?', '&' });

                    Dictionary<string, string> argList = new Dictionary<string, string>();

                    foreach (string arg in args)
                    {
                        //it's an arguement!
                        if (arg.Contains("="))
                        {
                            string[] values = arg.Split('=');
                            argList.Add(values[0], values[1]);
                        }
                    }

                    //registration packet A (OxO02O & Ox7c37)
                    if (argList.ContainsKey("OxO02O") && argList.ContainsKey("Ox7c37"))
                    {
                        Console.WriteLine("User " + argList["OxO02O"] + " connecting..");
                        byte[] reply = AchronWeb.packets.registerPacketA.Handle(argList["OxO02O"], argList["Ox7c37"]);
                        ns.Write(reply, 0, reply.Length);
                        ns.Flush();
                        ns.Close();
                        socket.Close();
                    }

                    //registration packet B (OxO02O & OxO02a & OxO04O)
                    else if (argList.ContainsKey("OxO02O") && argList.ContainsKey("OxO02a") && argList.ContainsKey("OxO04O"))
                    {
                        Console.WriteLine("User " + argList["OxO02O"] + " connected.");
                        byte[] reply = AchronWeb.packets.registerPacketB.Handle(argList["OxO02O"], argList["OxO02a"], argList["OxO04O"]);
                        ns.Write(reply, 0, reply.Length);
                        ns.Flush();
                        ns.Close();
                        socket.Close();
                    }

                    //registration packet B (OxO02O & OxO02a & OxO04O)
                    else if (argList.ContainsKey("OxO04O") && argList.Count == 1)
                    {
                        AchronWeb.features.achronClient user = AchronWeb.features.consts.getUser(argList["OxO04O"]);

                        if (user != null)
                        {
                            Console.WriteLine(user.username + " checks the game list..");
                        }
                        else
                        {
                            Console.WriteLine("Check game list.. [unknown user]");
                        }

                        byte[] reply = AchronWeb.packets.viewGamesPacket.Handle(argList["OxO04O"]);
                        ns.Write(reply, 0, reply.Length);
                        ns.Flush();
                        ns.Close();
                        socket.Close();
                    }

                    //createGamePacket string OxO181, string OxO2O1, string OxO21O, string OxO39O, string OxO04O
                    else if (argList.ContainsKey("OxO181") && argList.ContainsKey("OxO2O1") && argList.ContainsKey("OxO21O") && argList.ContainsKey("OxO39O") && argList.ContainsKey("OxO04O"))
                    {
                        AchronWeb.features.achronClient user = AchronWeb.features.consts.getUser(argList["OxO04O"]);

                        if (user != null)
                        {
                            Console.WriteLine(user.username + " creates a game..");
                        }
                        else
                        {
                            Console.WriteLine("create game.. [unknown user]");
                        }

                        byte[] reply = AchronWeb.packets.createGamePacket.Handle(argList["OxO181"], argList["OxO2O1"], argList["OxO21O"], argList["OxO39O"], argList["OxO04O"], endPoint.Address.ToString());
                        ns.Write(reply, 0, reply.Length);
                        ns.Flush();
                        ns.Close();
                        socket.Close();
                    }

                    else if (argList.ContainsKey("Ox910O") && argList.ContainsKey("OxO04O"))
                    {
                        byte[] reply = AchronWeb.packets.okPacket.Handle();
                        ns.Write(reply, 0, reply.Length);
                        ns.Flush();
                        ns.Close();
                        socket.Close();
                    }

                    //This code doesn't work as expected at all.
                    //Ox910O & OxO04O
                    //Ox910O = gameID
                    //Game is starting or ending (probably) - note: also fires when game is joined =/
                    /*
                    else if (argList.ContainsKey("Ox910O") && argList.ContainsKey("OxO04O"))
                    {
                        AchronWeb.features.achronClient user = AchronWeb.features.consts.getUser(argList["OxO04O"]);

                        

                        if (user == null) { Console.WriteLine("nullUSER: " + decode); break; }
                        else
                        {
                            Console.WriteLine(user.username + ": " + decode);

                            lock (AchronWeb.features.consts.gameList)
                            {
                                Queue<long> endedGames = new Queue<long>();
                                
                                foreach (KeyValuePair<long, AchronWeb.features.achronGame> game in AchronWeb.features.consts.gameList)
                                {
                                    if (game.Value.ownerSESSID == user.SESSID && game.Key.ToString() == argList["Ox910O"])
                                    {
                                        //Game is ending/starting
                                        Console.WriteLine("GAME " + game.Key + " has ended!");
                                        Console.WriteLine("CONTENT: " + decode);
                                        game.Value.lastUpdate = AchronWeb.features.consts.GetTime();
                                        game.Value.Progress = 1;
                                        endedGames.Enqueue(game.Key);
                                    }
                                }

                                if (endedGames.Count != 0)
                                {
                                    while (endedGames.Count != 0)
                                    {
                                        long id = endedGames.Dequeue();
                                        AchronWeb.features.consts.gameList.Remove(id);
                                    }
                                }
                            }
                        }
                    }
                    */

                    ///Game status update.
                    ///Ox910O - appears to be the gameID
                    ///Ox411c - list of players in the game
                    ///Ox50fa - no idea?
                    ///OxO04O - SSID / Key
                    /* For some reason, our server never causes this to be sent - so it is completely unhandled.
                    else if (argList.ContainsKey("Ox910O") && argList.ContainsKey("Ox411c") && argList.ContainsKey("Ox50fa") && argList.ContainsKey("OxO04O"))
                    {
                        AchronWeb.features.achronClient user = AchronWeb.features.consts.getUser(argList["OxO04O"]);

                        if (user == null) { break; }
                        else
                        {
                            lock (AchronWeb.features.consts.gameList)
                            {
                                foreach (KeyValuePair<long, AchronWeb.features.achronGame> game in AchronWeb.features.consts.gameList)
                                {
                                    if (game.Value.ownerSESSID == user.SESSID && game.Key.ToString() == argList["Ox910O"])
                                    {
                                        //Game status update.
                                        game.Value.lastUpdate = AchronWeb.features.consts.GetTime();

                                        string[] players = argList["Ox411c"].Split(',');
                                        int maxPlayers = players.Length;
                                        int currentPlayers = 0;
                                        foreach (string player in players)
                                        {
                                            if (player != " ")
                                            {
                                                currentPlayers++;
                                            }
                                        }

                                        game.Value.maxPlayers = maxPlayers;
                                        game.Value.currentPlayers = currentPlayers;
                                        Console.WriteLine("GAME " + game.Key + " updated. (" + game.Value.currentPlayers + "/" + game.Value.maxPlayers + ")");
                                    }
                                }
                            }
                        }
                    }
                    */

                    else
                    {
                        try
                        {
                            string packetName = "";
                            foreach (KeyValuePair<string, string> key in argList)
                            {
                                packetName += key.Key + "&";
                            }
                            packetName = packetName.Substring(0, packetName.Length - 1);
                            Console.WriteLine("Unhandled request: " + packetName);

                            Console.WriteLine("=============");
                            Console.WriteLine(decode);
                            Console.WriteLine("=============");

                            ns.Close();
                            socket.Close();
                        }
                        catch
                        {
                            Console.WriteLine("EXCEPTION THROWN [" + endPoint.ToString() + " ]: " + Environment.NewLine + decode);
                        }
                        finally
                        {
                            //probably a browser
                            if (!decode.Contains("GET /achron_games.php"))
                            {
                                string reply =
                                    "HTTP/1.1 200 OK" + Environment.NewLine + //OK, we have a valid time
                                    "Date: Now" + Environment.NewLine + //current datetime
                                    "Server: AchronWeb/0.0.1 (DocileDanny)" + Environment.NewLine + //server info
                                    "X-Powered-By: C#/" + Environment.Version.ToString() + Environment.NewLine + //php info
                                    "Cache-Control: no-store, no-cache, must-revalidate, post-check=0, pre-check=0" + Environment.NewLine + //various info about caching.
                                    "Pragma: no-cache" + Environment.NewLine + //pragma values
                                    "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">" + Environment.NewLine +
                                    AchronWeb.features.consts.errorPage + Environment.NewLine; //the content itself.
                                byte[] toSend = UTF8Encoding.UTF8.GetBytes(reply);
                                ns.Write(toSend, 0, toSend.Length);
                                ns.Flush();
                                ns.Close();
                                socket.Close();
                                Console.WriteLine("error page sent to " + endPoint.ToString() + ".");
                            }
                        }
                    }
                }

            }

            Console.WriteLine("[Socket " + endPoint.ToString() + " closed]");
        }
    }
}