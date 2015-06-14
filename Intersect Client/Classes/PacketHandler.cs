﻿using System;

namespace Intersect_Client.Classes
{
    public static class PacketHandler
    {
        public static void HandlePacket(byte[] packet)
        {
            var bf = new ByteBuffer();
            bf.WriteBytes(packet);
            var packetHeader = (Enums.ServerPackets)bf.ReadLong();
            switch (packetHeader)
            {
                case Enums.ServerPackets.RequestPing:
                    PacketSender.SendPing();
                    break;
                case Enums.ServerPackets.JoinGame:
                    HandleJoinGame(bf.ReadBytes(bf.Length()));
                    break;
                case Enums.ServerPackets.MapData:
                    HandleMapData(bf.ReadBytes(bf.Length()));
                    break;
                case Enums.ServerPackets.EntityData:
                    HandleEntityData(bf.ReadBytes(bf.Length()));
                    break;
                case Enums.ServerPackets.EntityPosition:
                    HandlePositionInfo(bf.ReadBytes(bf.Length()));
                    break;
                case Enums.ServerPackets.EntityLeave:
                    HandleLeave(bf.ReadBytes(bf.Length()));
                    break;
                case Enums.ServerPackets.ChatMessage:
                    HandleMsg(bf.ReadBytes(bf.Length()));
                    break;
                case Enums.ServerPackets.GameData:
                    HandleGameData(bf.ReadBytes(bf.Length()));
                    break;
                case Enums.ServerPackets.TilesetArray:
                    HandleTilesets(bf.ReadBytes(bf.Length()));
                    break;
                case Enums.ServerPackets.EnterMap:
                    HandleEnterMap(bf.ReadBytes(bf.Length()));
                    break;
                case Enums.ServerPackets.EntityMove:
                    HandleEntityMove(bf.ReadBytes(bf.Length()));
                    break;
                case Enums.ServerPackets.EntityVitals:
                    HandleVitals(bf.ReadBytes(bf.Length()));
                    break;
                case Enums.ServerPackets.EntityStats:
                    HandleStats(bf.ReadBytes(bf.Length()));
                    break;
                case Enums.ServerPackets.EntityDir:
                    HandleEntityDir(bf.ReadBytes(bf.Length()));
                    break;
                case Enums.ServerPackets.EventDialog:
                    HandleEventDialog(bf.ReadBytes(bf.Length()));
                    break;
                case Enums.ServerPackets.LoginError:
                    HandleLoginError(bf.ReadBytes(bf.Length()));
                    break;
                case Enums.ServerPackets.GameTime:
                    HandleGameTime(bf.ReadBytes(bf.Length()));
                    break;
                default:
                    Console.WriteLine(@"Non implemented packet received: " + packetHeader);
                    break;
            }
        }

        private static void HandleJoinGame(byte[] packet)
        {
            var bf = new ByteBuffer();
            bf.WriteBytes(packet);
            Globals.MyIndex = (int)bf.ReadLong();
            EntityManager.JoinGame();
            Globals.JoiningGame = true;
        }

        private static void HandleMapData(byte[] packet)
        {
            var bf = new ByteBuffer();
            bf.WriteBytes(packet);
            var mapNum = bf.ReadLong();
            var mapLength = bf.ReadLong();
            var mapData = bf.ReadBytes((int)mapLength);
            Globals.GameMaps[mapNum] = new MapStruct((int)mapNum, mapData);


        }

        private static void HandleEntityData(byte[] packet)
        {
            var bf = new ByteBuffer();
            bf.WriteBytes(packet);
            var i = (int)bf.ReadLong();
            var entityType = bf.ReadInteger();
            if (entityType == 0)
            {
                if (i == Globals.MyIndex)
                {
                    EntityManager.AddPlayer(i, bf.ReadString(), bf.ReadString(), true);
                }
                else
                {
                    EntityManager.AddPlayer(i, bf.ReadString(), bf.ReadString(), false);
                }
            }
            else
            {
                EntityManager.AddEvent(i, bf.ReadString(), bf.ReadString(), false);
            }

        }

        private static void HandlePositionInfo(byte[] packet)
        {
            var bf = new ByteBuffer();
            bf.WriteBytes(packet);
            var index = (int)bf.ReadLong();
            var isEvent = bf.ReadInteger();
            if (isEvent == 0)
            {
                if (index >= Globals.Entities.Count) { return; }
                if (Globals.Entities[index] == null) { return; }
                Globals.Entities[index].CurrentMap = bf.ReadInteger();
                Globals.Entities[index].CurrentX = bf.ReadInteger();
                Globals.Entities[index].CurrentY = bf.ReadInteger();
                Globals.Entities[index].Dir = bf.ReadInteger();
                Globals.Entities[index].Passable = bf.ReadInteger();
                Globals.Entities[index].HideName = bf.ReadInteger();
                if (index != Globals.MyIndex) return;
                if (Globals.CurrentMap == Globals.Entities[index].CurrentMap) return;
                Globals.CurrentMap = Globals.Entities[index].CurrentMap;
                //Initiate loading screen, we got probz
                Graphics.FadeStage = 2;
                Graphics.FadeAmt = 255.0f;
                Globals.GameLoaded = false;
                Globals.LocalMaps[4] = -1;
            }
            else
            {
                if (index >= Globals.Events.Count) { return; }
                if (Globals.Events[index] == null) { return; }
                Globals.Events[index].CurrentMap = bf.ReadInteger();
                Globals.Events[index].CurrentX = bf.ReadInteger();
                Globals.Events[index].CurrentY = bf.ReadInteger();
                Globals.Events[index].Dir = bf.ReadInteger();
                Globals.Events[index].Passable = bf.ReadInteger();
                Globals.Events[index].HideName = bf.ReadInteger();
            }
        }

        private static void HandleLeave(byte[] packet)
        {
            var bf = new ByteBuffer();
            bf.WriteBytes(packet);
            var index = (int)bf.ReadLong();
            var isEvent = bf.ReadInteger();
            EntityManager.RemoveEntity(index, isEvent);

        }

        private static void HandleMsg(byte[] packet)
        {
            var bf = new ByteBuffer();
            bf.WriteBytes(packet);
            Globals.ChatboxContent.Add(bf.ReadString());

        }

        private static void HandleGameData(byte[] packet)
        {
            var bf = new ByteBuffer();
            bf.WriteBytes(packet);
            var mapCount = (int)bf.ReadLong();
            Globals.GameMaps = new MapStruct[mapCount];
            Globals.MapCount = mapCount;
            //Database.LoadMapRevisions();
        }

        private static void HandleTilesets(byte[] packet)
        {
            var bf = new ByteBuffer();
            bf.WriteBytes(packet);
            var tilesetCount = bf.ReadLong();
            if (tilesetCount > 0)
            {
                Globals.Tilesets = new string[tilesetCount];
                for (var i = 0; i < tilesetCount; i++)
                {
                    Globals.Tilesets[i] = bf.ReadString();
                }
                Graphics.LoadTilesets(Globals.Tilesets);
            }
        }

        private static void HandleEnterMap(byte[] packet)
        {
            var bf = new ByteBuffer();
            bf.WriteBytes(packet);
            var mapNum = (int)bf.ReadLong();
            if (Globals.CurrentMap != mapNum && Globals.CurrentMap != -1) return;
            for (var i = 0; i < 9; i++)
            {
                Globals.LocalMaps[i] = (int)bf.ReadLong();
                if (Globals.LocalMaps[i] <= -1) continue;
                if (Globals.GameMaps[Globals.LocalMaps[i]] == null)
                {
                    PacketSender.SendNeedMap(Globals.LocalMaps[i]);
                }
            }
            for (var i = 0; i < Globals.GameMaps.Length; i++)
            {
                if (Globals.GameMaps[i] == null) continue;
                if (Globals.GameMaps[i].CacheCleared) continue;
                for (var x = 0; x < 9; x++)
                {
                    if (Globals.LocalMaps[x] == i)
                    {
                        break;
                    }
                    if (x == 8)
                    {
                        Globals.GameMaps[i].ClearCache();
                    }
                }
            }
        }

        private static void HandleEntityMove(byte[] packet)
        {
            var bf = new ByteBuffer();
            bf.WriteBytes(packet);
            var index = (int)bf.ReadLong();
            var isEvent = bf.ReadInteger();
            if (isEvent == 0)
            {
                if (index >= Globals.Entities.Count) { return; }
                if (Globals.Entities[index] == null) { return; }
                Globals.Entities[index].CurrentMap = bf.ReadInteger();
                Globals.Entities[index].CurrentX = bf.ReadInteger();
                Globals.Entities[index].CurrentY = bf.ReadInteger();
                Globals.Entities[index].Dir = bf.ReadInteger();
                Globals.Entities[index].IsMoving = true;
                switch (Globals.Entities[index].Dir)
                {
                    case 0:
                        Globals.Entities[index].OffsetY = 32;
                        Globals.Entities[index].OffsetX = 0;
                        break;
                    case 1:
                        Globals.Entities[index].OffsetY = -32;
                        Globals.Entities[index].OffsetX = 0;
                        break;
                    case 2:
                        Globals.Entities[index].OffsetY = 0;
                        Globals.Entities[index].OffsetX = 32;
                        break;
                    case 3:
                        Globals.Entities[index].OffsetY = 0;
                        Globals.Entities[index].OffsetX = -32;
                        break;
                }
            }
            else
            {
                if (index >= Globals.Events.Count) { return; }
                if (Globals.Events[index] == null) { return; }
                Globals.Events[index].CurrentMap = bf.ReadInteger();
                Globals.Events[index].CurrentX = bf.ReadInteger();
                Globals.Events[index].CurrentY = bf.ReadInteger();
                Globals.Events[index].Dir = bf.ReadInteger();
                Globals.Events[index].IsMoving = true;
                switch (Globals.Events[index].Dir)
                {
                    case 0:
                        Globals.Events[index].OffsetY = 32;
                        Globals.Events[index].OffsetX = 0;
                        break;
                    case 1:
                        Globals.Events[index].OffsetY = -32;
                        Globals.Events[index].OffsetX = 0;
                        break;
                    case 2:
                        Globals.Events[index].OffsetY = 0;
                        Globals.Events[index].OffsetX = 32;
                        break;
                    case 3:
                        Globals.Events[index].OffsetY = 0;
                        Globals.Events[index].OffsetX = -32;
                        break;
                }
            }
        }

        private static void HandleVitals(byte[] packet)
        {
            var bf = new ByteBuffer();
            bf.WriteBytes(packet);
            var index = (int)bf.ReadLong();
            var isEvent = bf.ReadInteger();
            if (isEvent == 0)
            {
                if (index >= Globals.Entities.Count) { return; }
                if (Globals.Entities[index] == null) { return; }
                for (var i = 0; i < (int) Enums.Vitals.VitalCount; i++)
                {
                    Globals.Entities[index].MaxVital[i] = bf.ReadInteger();
                    Globals.Entities[index].Vital[i] = bf.ReadInteger();
                }
            }
            else
            {
                if (index >= Globals.Events.Count) { return; }
                if (Globals.Events[index] == null) { return; }
                for (var i = 0; i < (int)Enums.Vitals.VitalCount; i++)
                {
                    Globals.Events[index].MaxVital[i] = bf.ReadInteger();
                    Globals.Events[index].Vital[i] = bf.ReadInteger();
                }
            }
        }

        private static void HandleStats(byte[] packet)
        {
            var bf = new ByteBuffer();
            bf.WriteBytes(packet);
            var index = (int)bf.ReadLong();
            var isEvent = bf.ReadInteger();
            if (isEvent == 0)
            {
                if (index >= Globals.Entities.Count) { return; }
                if (Globals.Entities[index] == null) { return; }
                for (var i = 0; i < (int)Enums.Stats.StatCount; i++)
                {
                    Globals.Entities[index].Stat[i] = bf.ReadInteger();
                }
            }
            else
            {
                if (index >= Globals.Events.Count) { return; }
                if (Globals.Events[index] == null) { return; }
                for (var i = 0; i < (int)Enums.Stats.StatCount; i++)
                {
                    Globals.Events[index].Stat[i] = bf.ReadInteger();
                }
            }
        }

        private static void HandleEntityDir(byte[] packet)
        {
            var bf = new ByteBuffer();
            bf.WriteBytes(packet);
            var index = (int)bf.ReadLong();
            var isEvent = bf.ReadInteger();
            if (isEvent == 0)
            {
                if (index >= Globals.Entities.Count) { return; }
                if (Globals.Entities[index] == null) { return; }
                Globals.Entities[index].Dir = bf.ReadInteger();
            }
            else
            {
                if (index >= Globals.Events.Count) { return; }
                if (Globals.Events[index] == null) { return; }
                Globals.Events[index].Dir = bf.ReadInteger();
            }

        }

        private static void HandleEventDialog(byte[] packet)
        {
            var bf = new ByteBuffer();
            var ed = new EventDialog();
            bf.WriteBytes(packet);
            ed.Prompt = bf.ReadString();
            ed.Type = bf.ReadInteger();
            if (ed.Type == 0)
            {

            }
            else
            {
                ed.Opt1 = bf.ReadString();
                ed.Opt2 = bf.ReadString();
                ed.Opt3 = bf.ReadString();
                ed.Opt4 = bf.ReadString();
            }
            ed.EventIndex = bf.ReadInteger();
            Globals.EventDialogs.Add(ed);
        }

        private static void HandleLoginError(byte[] packet)
        {
            var bf = new ByteBuffer();
            bf.WriteBytes(packet);
            var error = bf.ReadString();
            Graphics.FadeStage = 1;
            Graphics.FadeAmt = 250;
            Globals.WaitingOnServer = false;
            Gui.MsgboxErrors.Add(error);
            Gui._MenuGui.Reset();
        }

        private static void HandleGameTime(byte[] packet)
        {
            var bf = new ByteBuffer();
            bf.WriteBytes(packet);
            Globals.GameTime = bf.ReadInteger();
        }

    }
}