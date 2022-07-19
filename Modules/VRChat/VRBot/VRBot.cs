using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Photon.Realtime;
using System;
using System.Collections;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using XanotyPhoton.Modules.Configuration;

namespace XanotyPhoton
{
    public class VRBot
    {
        public HttpClient Client;
        public PhotonClient photonclient;
        public string Auth { get; set; }
        public string userid { get; set; }
        public string FakeMacId { get; set; }
        public string username { get; set; }
        public string CurrentWorldInstance { get; private set; }

        public VRBot(string usernamepassword)
        {
            SetNewMacID();
            Task.Run(SetUpClientAsync(usernamepassword).GetAwaiter().GetResult).Wait();
            photonclient = new PhotonClient(username, Auth, userid, Config.region, FakeMacId);

            System.Timers.Timer VisitPing = new System.Timers.Timer(30000)
            {
                AutoReset = true,
                Enabled = true
            };

            VisitPing.Elapsed += async delegate (object sender, System.Timers.ElapsedEventArgs e)
            {
                if (string.IsNullOrEmpty(CurrentWorldInstance)) return;

                HttpRequestMessage visitPayload = new HttpRequestMessage(HttpMethod.Put, "https://api.vrchat.cloud/api/1/visits?apiKey=JlE5Jldo5Jibnk5O5hTx6XVqsJu4WJ26&organization=vrchat");
                string visitBody = JsonConvert.SerializeObject(new { userId = userid, worldId = CurrentWorldInstance });
                visitPayload.Content = new StringContent(visitBody, Encoding.UTF8, "application/json");
                HttpResponseMessage visitResp = await Client.SendAsync(visitPayload);
                string visitRespBody = await visitResp.Content.ReadAsStringAsync();
                JObject visitObjResp = JObject.Parse(visitRespBody);
                
                if (visitObjResp.ContainsKey("success"))
                {
                    Console.WriteLine(visitObjResp["success"]?["message"]);
                }
            };
        }

        private void SetNewMacID()
        {
            byte[] bytes = new byte[20];
            new Random(Environment.TickCount).NextBytes(bytes);

            FakeMacId = string.Join("", bytes.Select(x => x.ToString("x2")));
        }

        public void JoinRoom(string worldinstance, int cap = 0)
        {
            CurrentWorldInstance = worldinstance;
            string worldid = worldinstance.Split(':')[0];

            if (cap == 0)
            {
                cap = int.Parse(Task.Run(async () => await Get($"worlds/{worldid}")).Result["capacity"].ToString());
            }

            Task.Run(async () =>
            {
                HttpRequestMessage joinWorldPayload = new HttpRequestMessage(HttpMethod.Put, "https://api.vrchat.cloud/api/1/joins?apiKey=JlE5Jldo5Jibnk5O5hTx6XVqsJu4WJ26&organization=vrchat");
                string joinWorldBody = JsonConvert.SerializeObject(new { userId = userid, worldId = CurrentWorldInstance });
                joinWorldPayload.Content = new StringContent(joinWorldBody, Encoding.UTF8, "application/json");
                HttpResponseMessage joinWorldResp = await Client.SendAsync(joinWorldPayload);
                string joinWorldRespBody = await joinWorldResp.Content.ReadAsStringAsync();
            }).Wait();

            string token = Task.Run(async () => await Get("instances/" + CurrentWorldInstance + "/join")).Result["token"].ToString();

            if (token == null || cap == 0)
            {
                photonclient.Debug("No Roomtoken / Capacity");
                return;
            }

            EnterRoomParams enterRoomParams = new EnterRoomParams
            {
                CreateIfNotExists = true,
                RejoinOnly = false,
                RoomName = worldinstance,
                RoomOptions = new RoomOptions
                {
                    IsOpen = true,
                    IsVisible = false,
                    MaxPlayers = Convert.ToByte(cap * 2),
                    CustomRoomProperties = new Hashtable
                    {
                        { (byte)3, 1},
                        { (byte)2, token}
                    },
                    EmptyRoomTtl = 0,
                    PublishUserId = false
                }
            };

            photonclient.OpJoinRoom(enterRoomParams);
        }

        public async Task<JObject> Get(string endpoint)
        {
            string hasQuery = endpoint.IndexOf('?') != -1 ? "&" : "?";
            string body = await Client.GetStringAsync($"https://api.vrchat.cloud/api/1/{endpoint}{hasQuery}apiKey=JlE5Jldo5Jibnk5O5hTx6XVqsJu4WJ26&organization=vrchat");
            
            return JObject.Parse(body);
        }

        private async Task SetUpClientAsync(string usernamepassword)
        {
            var httpClientHandler = new HttpClientHandler
            {
                UseCookies = false,
            };

            Console.WriteLine("Setting client headers");
            
            Client = new HttpClient(httpClientHandler);
            Client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            Client.DefaultRequestHeaders.Add("X-MacAddress", FakeMacId);
            Client.DefaultRequestHeaders.Add("X-Client-Version", Config.x_client_version);
            Client.DefaultRequestHeaders.Add("X-Platform", "standalonewindows");
            Client.DefaultRequestHeaders.Add("Origin", "vrchat.com");
            Client.DefaultRequestHeaders.Add("Host", "api.vrchat.cloud");
            Client.DefaultRequestHeaders.Add("Connection", "Keep-Alive, TE");
            Client.DefaultRequestHeaders.Add("TE", "identity");
            Client.DefaultRequestHeaders.Add("User-Agent", "VRC.Core.BestHTTP");

            HttpRequestMessage loginPayLoad = new HttpRequestMessage(HttpMethod.Get, "https://api.vrchat.cloud/api/1/auth/user?apiKey=JlE5Jldo5Jibnk5O5hTx6XVqsJu4WJ26&organization=vrchat");
            loginPayLoad.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(usernamepassword)));
            HttpResponseMessage loginResp = await Client.SendAsync(loginPayLoad);
            loginResp.EnsureSuccessStatusCode();
            string loginBody = await loginResp.Content.ReadAsStringAsync();

            Console.WriteLine(loginResp.StatusCode);

            Client.DefaultRequestHeaders.Add("cookie", loginResp.Headers.GetValues("set-cookie"));

            Auth = loginResp.Headers.GetValues("set-cookie").First().Split('=')[1].Split(';')[0];
            userid = JObject.Parse(loginBody)["id"].ToString();
            username = JObject.Parse(loginBody)["username"].ToString();
        }
    }
}
