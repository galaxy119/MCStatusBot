/*
 * MineStat.cs - A Minecraft server status checker
 * Copyright (C) 2014 Lloyd Dilley
 * http://www.dilley.me/
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along
 * with this program; if not, write to the Free Software Foundation, Inc.,
 * 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;

public class MineStat
{
  private const ushort dataSize = 512;  // this will hopefully suffice since the MotD should be <=59 characters
  private const ushort numFields = 6;   // number of values expected from server
  private const int defaultTimeout = 5; // default TCP timeout in seconds

  private string Address { get; set; }
  private ushort Port { get; set; }
  private int Timeout { get; set; }
  private string Motd { get; set; }
  private string Version { get; set; }
  public string CurrentPlayers { get; set; }
  public string MaximumPlayers { get; set; }
  public bool ServerUp { get; set; }
  public long Latency { get; set; }

  public MineStat(string address, ushort port, int timeout = defaultTimeout)
  {
    byte[] rawServerData = new byte[dataSize];

    Address = address;
    Port = port;
    Timeout = timeout * 1000;   // milliseconds

    try
    {
      Stopwatch stopWatch = new Stopwatch();
      TcpClient tcpClient = new TcpClient { ReceiveTimeout = Timeout };
      stopWatch.Start();
      tcpClient.Connect(address, port);
      stopWatch.Stop();
      Latency = stopWatch.ElapsedMilliseconds;
      NetworkStream stream = tcpClient.GetStream();
      byte[] payload = new byte[] { 0xFE, 0x01 };
      stream.Write(payload, 0, payload.Length);
      stream.Read(rawServerData, 0, dataSize);
      tcpClient.Close();
    }
    catch(Exception)
    {
      ServerUp = false;
      return;
    }

    if(rawServerData.Length == 0)
      ServerUp = false;
    else
    {
      string[] serverData = Encoding.Unicode.GetString(rawServerData).Split("\u0000\u0000\u0000".ToCharArray());
      if(serverData != null && serverData.Length >= numFields)
      {
        ServerUp = true;
        Version = serverData[2];
        Motd = serverData[3];
        CurrentPlayers = serverData[4];
        MaximumPlayers = serverData[5];
      }
      else
        ServerUp = false;
    }
  }
}