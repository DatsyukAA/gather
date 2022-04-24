using SteamKit2;
using SteamKit2.GC;
using SteamKit2.GC.Dota.Internal;
using SteamKit2.Internal;
using System.Security.Cryptography;

namespace DotaKit
{
    class DotaClient
    {
        public SteamClient client;

        public SteamUser user;
        public SteamGameCoordinator gameCoordinator;

        CallbackManager callbackMgr;
        public string username;
        public string password;
        string authCode = null;
        string twoFactorAuth = null;

        // dota2's appid
        const int APPID = 570;




        public DotaClient(string username,
                          string password)
        {

            this.username = username;
            this.password = password;
            client = new SteamClient();

            // get our handlers
            user = client.GetHandler<SteamUser>();
            gameCoordinator = client.GetHandler<SteamGameCoordinator>();

            // setup callbacks
            callbackMgr = new CallbackManager(client);
            callbackMgr.Subscribe<SteamClient.CMListCallback>(OnCMList);
            callbackMgr.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            callbackMgr.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);

            callbackMgr.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            callbackMgr.Subscribe<SteamGameCoordinator.MessageCallback>(OnGCMessage);

            callbackMgr.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);
            callbackMgr.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth);
        }


        public void Connect()
        {
            Console.WriteLine("Connecting to Steam...");
            client.Connect();
        }

        public void Wait()
        {
            while (true)
            {
                callbackMgr.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }
        }

        #region CLIENT CALLBACKS
        public delegate void onConnectedhandler(SteamClient.ConnectedCallback callback);
        public event onConnectedhandler onConnected;
        void OnConnected(SteamClient.ConnectedCallback callback)
        {
            onConnected(callback);

            byte[]? sentryHash = null;
            if (File.Exists($"{username}/sentry.bin"))
            {
                // if we have a saved sentry file, read and sha-1 hash it
                byte[] sentryFile = File.ReadAllBytes($"{username}/sentry.bin");
                sentryHash = CryptoHelper.SHAHash(sentryFile);
            }

            user.LogOn(new SteamUser.LogOnDetails
            {
                Username = username,
                Password = password,
                AuthCode = authCode,
                TwoFactorCode = twoFactorAuth,
                SentryFileHash = sentryHash,
            });

        }

        public delegate void onDisconnectedhandler(SteamClient.DisconnectedCallback callback);
        public event onDisconnectedhandler onDisconnected;
        void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            // after recieving an AccountLogonDenied, we'll be disconnected from steam
            // so after we read an authcode from the user, we need to reconnect to begin the logon flow again
            onDisconnected(callback);

            Thread.Sleep(TimeSpan.FromSeconds(5));

            client.Connect();

        }
        public delegate void onCMListHandler(SteamClient.CMListCallback callback);
        public event onCMListHandler onCMList;
        void OnCMList(SteamClient.CMListCallback callback)
        {
            //onCMList(callback);
        }
        #endregion
        #region STEAM USER CALLBACK
        void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            Console.WriteLine("Logged off of Steam: {0}", callback.Result);
        }

        void OnMachineAuth(SteamUser.UpdateMachineAuthCallback callback)
        {
            Console.WriteLine("Updating sentryfile...");

            // write out our sentry file
            // ideally we'd want to write to the filename specified in the callback
            // but then this sample would require more code to find the correct sentry file to read during logon
            // for the sake of simplicity, we'll just use "sentry.bin"

            int fileSize;
            byte[] sentryHash;
            using (var fs = File.Open($"{username}/sentry.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                fs.Seek(callback.Offset, SeekOrigin.Begin);
                fs.Write(callback.Data, 0, callback.BytesToWrite);
                fileSize = (int)fs.Length;

                fs.Seek(0, SeekOrigin.Begin);
                using var sha = SHA1.Create();
                sentryHash = sha.ComputeHash(fs);
            }

            // inform the steam servers that we're accepting this sentry file
            user.SendMachineAuthResponse(new SteamUser.MachineAuthDetails
            {
                JobID = callback.JobID,

                FileName = callback.FileName,

                BytesWritten = callback.BytesToWrite,
                FileSize = fileSize,
                Offset = callback.Offset,

                Result = EResult.OK,
                LastError = 0,

                OneTimePassword = callback.OneTimePassword,

                SentryFileHash = sentryHash,
            });

            Console.WriteLine("Done!");
        }

        // called when the client successfully (or unsuccessfully) logs onto an account
        void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {

            bool isSteamGuard = callback.Result == EResult.AccountLogonDenied;
            bool is2FA = callback.Result == EResult.AccountLoginDeniedNeedTwoFactor;

            if (isSteamGuard || is2FA)
            {
                Console.WriteLine("This account is SteamGuard protected!");

                if (is2FA)
                {
                    Console.Write("Please enter your 2 factor auth code from your authenticator app: ");
                    twoFactorAuth = Console.ReadLine();
                }
                else
                {
                    Console.Write("Please enter the auth code sent to the email at {0}: ", callback.EmailDomain);
                    authCode = Console.ReadLine();
                }

                return;
            }

            if (callback.Result != EResult.OK)
            {
                Console.WriteLine("Unable to logon to Steam: {0}", callback.Result);
                return;
            }

            Console.WriteLine("Logged in! Launching DOTA...");
            var playGame = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);

            playGame.Body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed
            {
                game_id = new GameID(APPID), // or game_id = APPID,
            });

            client.Send(playGame);

            Thread.Sleep(5000);

            var clientHello = new ClientGCMsgProtobuf<CMsgClientHello>((uint)EGCBaseClientMsg.k_EMsgGCClientHello);
            clientHello.Body.engine = ESourceEngine.k_ESE_Source2;
            gameCoordinator.Send(clientHello, APPID);
        }
        #endregion
        void OnGCMessage(SteamGameCoordinator.MessageCallback callback)
        {
            // setup our dispatch table for messages
            // this makes the code cleaner and easier to maintain
            var messageMap = new Dictionary<uint, Action<IPacketGCMsg>>
            {
                { ( uint )EGCBaseClientMsg.k_EMsgGCClientWelcome, OnClientWelcome },
            };

            Action<IPacketGCMsg> func;
            if (!messageMap.TryGetValue(callback.EMsg, out func))
            {
                return;
            }

            func(callback.Message);
        }

        // this message arrives when the GC welcomes a client
        // this happens after telling steam that we launched dota (with the ClientGamesPlayed message)
        // this can also happen after the GC has restarted (due to a crash or new version)
        void OnClientWelcome(IPacketGCMsg packetMsg)
        {
            // in order to get at the contents of the message, we need to create a ClientGCMsgProtobuf from the packet message we recieve
            // note here the difference between ClientGCMsgProtobuf and the ClientMsgProtobuf used when sending ClientGamesPlayed
            // this message is used for the GC, while the other is used for general steam messages
            var msg = new ClientGCMsgProtobuf<CMsgClientWelcome>(packetMsg);

            Console.WriteLine("GC is welcoming us. Version: {0}", msg.Body.version);

            Console.WriteLine("===================================");
            Console.WriteLine("=                                 =");
            Console.WriteLine("=     Trying to create lobby      =");
            Console.WriteLine("=                                 =");
            Console.WriteLine("===================================");
            CreateLobby(customGameId: 2087457643, mapName: "horde_5p", maxPlayers: 10);
            Console.WriteLine("===================================");
            Console.WriteLine("=                                 =");
            Console.WriteLine("=            Created              =");
            Console.WriteLine("=                                 =");
            Console.WriteLine("===================================");
            InviteLobby();
            Console.WriteLine("===================================");
            Console.WriteLine("=                                 =");
            Console.WriteLine("=           Intive sent           =");
            Console.WriteLine("=                                 =");
            Console.WriteLine("===================================");
            Console.ReadKey();
            Console.WriteLine("===================================");
            Console.WriteLine("=                                 =");
            Console.WriteLine("=             Leave               =");
            Console.WriteLine("=                                 =");
            Console.WriteLine("===================================");
            LaunchLobby();
            LeaveLobby();
        }

        void CreateLobby(ulong customGameId,
                         string mapName,
                         string? password = null,
                         DOTALobbyVisibility visibility = DOTALobbyVisibility.DOTALobbyVisibility_Unlisted,
                         uint maxPlayers = 0)
        {
            var createLobby =
                new ClientGCMsgProtobuf<CMsgPracticeLobbyCreate>((uint)EDOTAGCMsg.k_EMsgGCPracticeLobbyCreate);

            createLobby.Body.search_key = "";
            createLobby.Body.lobby_details = new CMsgPracticeLobbySetDetails
            {
                game_mode = (uint)DOTA_GameMode.DOTA_GAMEMODE_CUSTOM,
                game_name = "",
                visibility = visibility,
                game_version = DOTAGameVersion.GAME_VERSION_STABLE,
                custom_max_players = maxPlayers,
                custom_game_mode = customGameId.ToString(),
                custom_game_crc = 9139261913836182630,
                custom_game_id = customGameId,
                custom_map_name = mapName,
                pass_key = password,
                intro_mode = false,
                lan = false
            };


            gameCoordinator.Send(createLobby, APPID);
        }

        public void LaunchLobby()
        {
            var launchLobby = new ClientGCMsgProtobuf<CMsgPracticeLobbyLaunch>((uint)EDOTAGCMsg.k_EMsgGCPracticeLobbyLaunch);
            gameCoordinator.Send(launchLobby, APPID);
        }
        public void InviteParty()
        {
            var inviteParty = new ClientGCMsgProtobuf<CMsgInviteToParty>((uint)EGCBaseMsg.k_EMsgGCInviteToParty);
            inviteParty.Body.steam_id = 76561198275381288;
            gameCoordinator.Send(inviteParty, APPID);
        }

        public void InviteLobby()
        {
            var inviteParty = new ClientGCMsgProtobuf<CMsgInviteToLobby>((uint)EGCBaseMsg.k_EMsgGCInviteToLobby);
            inviteParty.Body.steam_id = 76561198275381288;
            gameCoordinator.Send(inviteParty, APPID);
        }

        public void LeaveLobby()
        {
            var leaveLobby = new ClientGCMsgProtobuf<CMsgPracticeLobbyLeave>((uint)EDOTAGCMsg.k_EMsgGCPracticeLobbyLeave);
            gameCoordinator.Send(leaveLobby, APPID);
        }
    }
}
