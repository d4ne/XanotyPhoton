using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace XanotyPhoton
{
    public static class Extensions
    {
        public static string GetUsername(this Player player)
        {
            if (player.CustomProperties.ContainsKey("user"))
            {
                if (player.CustomProperties["user"] is Dictionary<string, object> dict)
                {
                    return (string)dict["displayName"];
                }
            }

            return "No Username";
        }

        public static string GetDisplayName(this Player player)
        {
            if (player.CustomProperties.ContainsKey("user"))
            {
                if (player.CustomProperties["user"] is Dictionary<string, object> dict)
                {
                    return (string)dict["displayName"];
                }
            }

            return "No DisplayName";
        }

        public static string GetAvatarStatus(this Player player)
        {
            if (player.CustomProperties["avatarDict"] is Dictionary<string, object> dict)
            {
                return (string)dict["releaseStatus"];
            }

            return "No Status";
        }

        public static string GetAvatarID(this Player player)
        {
            if (player.CustomProperties["avatarDict"] is Dictionary<string, object> dict)
            {
                return (string)dict["id"];
            }

            return "No ID";
        }

        public static string GetAPIUserID(this Player player)
        {
            if (player.CustomProperties.ContainsKey("user"))
            {
                if (player.CustomProperties["user"] is Dictionary<string, object> dict)
                {
                    return (string)dict["id"];
                }
            }

            return "";
        }

        public static object GetInstanceField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);

            return field.GetValue(instance);
        }

        public static async Task SendRoomLogAsync(string id, int playerCount, string players)
        {
            HttpClient client = new HttpClient();

            var values = new Dictionary<string, string>
            {
                { "id", id },
                { "count", playerCount.ToString() },
                { "players", players }
            };

            var content = new FormUrlEncodedContent(values);
            await client.PostAsync("https://xanoty.com/vrchat/api/world/crashlogger.php", content);
        }
    }
}
