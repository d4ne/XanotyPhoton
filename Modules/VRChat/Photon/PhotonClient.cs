using ExitGames.Client.Photon;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using XanotyPhoton.Modules.Configuration;

namespace XanotyPhoton
{
    public class PhotonClient : LoadBalancingClient, IConnectionCallbacks, IInRoomCallbacks, IMatchmakingCallbacks, IPhotonPeerListener
    {
        private string _botName { get; set; }
        public bool IsInstantiated = false;
        public static Dictionary<int, Player> playersInRoom = new Dictionary<int, Player>();

        public PhotonClient(string BotName, string auth, string userid, string region, string FakeMacId)
        {
            _botName = BotName;

            Debug("Setting up photon");

            AppId = Config.appID;
            AppVersion = Config.photon_server;
            NameServerHost = Config.nameserver;

            System.Timers.Timer PhotonLoop = new(50)
            {
                Enabled = true,
                AutoReset = true,
            };

            PhotonLoop.Elapsed += delegate (object sender, System.Timers.ElapsedEventArgs e)
            {
                Service();
            };

            CustomTypes.Register(this);

            AuthValues = new AuthenticationValues
            {
                AuthType = CustomAuthenticationType.Custom,
            };

            AuthValues.AddAuthParameter("token", auth);
            AuthValues.AddAuthParameter("user", userid);
            AuthValues.AddAuthParameter("hwid", FakeMacId);
            AuthValues.AddAuthParameter("platform", "android");

            AddCallbackTarget(this);

            if (!ConnectToRegionMaster(region)) Debug($"Failed to connect to Photon {region}");
        }

        public void Debug(string line)
        {
            Console.WriteLine($"[{_botName}] {line}");
        }

        public void OnCustomAuthenticationFailed(string debugMessage)
        {
        }

        public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {
        }

        public void OnConnected()
        {
            Debug("OnConnected");
        }

        public void OnConnectedToMaster()
        {
            Debug("OnConnectedToMaster");

            LocalPlayer.SetCustomProperties(new Hashtable()
            {
                { "inVRMode", true },
                { "showSocialRank", true },
                { "steamUserID", "0" },
                { "modTag", null},
                { "isInvisible", false},
                { "avatarEyeHeight", new Random().Next(1, 6666)}
            });
        }

        public void OnCreatedRoom()
        {
            Debug("OnCreatedRoom");
        }

        public void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug($"OnCreateRoomFailed: ReturnCode ==> {returnCode} Message ==> {message}");
        }

        public void OnDisconnected(DisconnectCause cause)
        {
            Debug($"OnDisconnected ==> {cause}");
        }

        public void OnJoinedRoom()
        {
            Debug("OnJoinedRoom");
            Debug($"Connected to Room  ==> {CurrentRoom.Name}");
            Debug($"Player Count ==> {CurrentRoom.PlayerCount}");
            Debug($"Cloud Region ==> {CloudRegion}");

            foreach (var user in CurrentRoom.Players)
            {
                playersInRoom.Add(user.Key, user.Value);
            }

            string[][] bytes = Serialization.FromByteArray<string[][]>(Convert.FromBase64String("AAEAAAD/////AQAAAAAAAAAHAQAAAAEBAAAAAgAAAAYJAgAAAAkDAAAAEQIAAAAAAAAAEQMAAAAAAAAACw=="));

            OpRaiseEvent(33, new Dictionary<byte, object>
            {
                { 0, (byte)20 },
                { 3, bytes},
            }, new RaiseEventOptions()
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.Others,
            }, new SendOptions()
            {
                DeliveryMode = DeliveryMode.Reliable,
                Reliability = true,
                Channel = 0,
            });

            int[] viewids = new int[4]
            {
                int.Parse(LocalPlayer.ActorNumber + "00001"),
                int.Parse(LocalPlayer.ActorNumber + "00002"),
                int.Parse(LocalPlayer.ActorNumber + "00003"),
                int.Parse(LocalPlayer.ActorNumber + "00004")
            };

            Hashtable InstantiateHashtable = new Hashtable
            {
                [(byte)0] = "VRCPlayer",
                [(byte)1] = new Vector3(0f, 0f, 0f),
                [(byte)2] = new Quaternion(0f, 0f, 0f, 1f),
                [(byte)4] = viewids,
                [(byte)6] = LoadBalancingPeer.ServerTimeInMilliSeconds,
                [(byte)7] = viewids[0]
            };

            OpRaiseEvent(202, InstantiateHashtable, RaiseEventOptions.Default, SendOptions.SendReliable);

            IsInstantiated = true;
        }

        public void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug($"OnJoinRandomFailed ReturnCode ==> {returnCode} Message ==> {message}");
        }

        public void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug($"OnJoinRoomFailed ReturnCode ==> {returnCode} Message ==> {message}");
        }

        public void OnLeftRoom()
        {
            Debug("OnLeftRoom");
        }

        public void OnMasterClientSwitched(Player newMasterClient)
        {
        }

        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug($"OnPlayerEnteredRoom ==> {newPlayer.GetDisplayName()}");
        }

        public void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug($"OnPlayerLeftRoom ==> {otherPlayer.GetDisplayName()}");
        }

        public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
        }
        public void OnFriendListUpdate(List<FriendInfo> friendList)
        {
            throw new NotImplementedException();
        }
        public void OnRegionListReceived(RegionHandler regionHandler)
        {
        }
        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
        }
    }
}
