using Guna.UI2.WinForms;
using Leaf.xNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace LithiumSpammer.raidlib
{
    public delegate void GotUserId(object sender, EventArgs e);
    public class VoiceConnection
    {
        public string endpoint { get; set; }
        public string region { get; set; }
        public int bitrate { get; set; }
        public string token { get; set; }
    }

    public class Client
    {
        public event EventHandler<object> GotUID;
        public static string Token { get; private set; }
        public object Id { get; private set; }
        private object _Profile { get; set; }
		public dynamic Profile = null;
        private DefaultHttp defHttp;

        private class ClientData
        {
            public string Id { get; set; }
            public object Profile { get; set; }
        }

        private class ProfileData
        {
            public ClientData user { get; set; }
        }

        public Client(string token)
        {
			Token = token;
            defHttp = new DefaultHttp(Token);
            Task task = Task.Run(() =>
            {
                _Profile = this.getProfile();
            });
            Task.WhenAll(task).ContinueWith(_ =>
            {
				if (_Profile.ToString() != "{}")
				{
                    dynamic profileObject = JsonConvert.DeserializeObject(_Profile.ToString());

					Profile = profileObject;

                    string userId = profileObject?.user?.id;

                    if (!string.IsNullOrEmpty(userId))
                    {
						GotUID?.Invoke(this, Profile);
					}
                }
			});
        }
		
		public void setUsername(string username)
        {
            HttpRequest request = defHttp.Request();

            JObject jObject = new JObject();
			jObject["userId"] = this.Profile.user.Id;
			jObject["name"] = username;

            request.Put("https://www.guilded.gg/api/users/" + this.Profile.user.Id + "/profilev2", jObject.ToString(), "application/json");

			if (request.Response.IsOK)
			{
				
			}
        }

        public void setPresence(string presence)
		{
            Dictionary<string, string> presences = new Dictionary<string, string>
			{
				{ "Online", "1" },
				{ "Idle", "2" },
				{ "DND", "3" },
				{ "Offline", "4" }
			};

            HttpRequest request = defHttp.Request();
			JObject jObject = new JObject();
			jObject["status"] = Int32.Parse(presences[presence]);
			if (presence == "1") jObject["status"] = 1;

            request.Post("https://www.guilded.gg/api/users/me/presence", jObject.ToString(), "application/json");

            Console.WriteLine(request.Response.ToString());
        }
		public void setStatus(string text, int customReactionID)
        {
			HttpRequest request = defHttp.Request();

            JObject jObject = new JObject(
                new JProperty("content", new JObject(
                    new JProperty("object", "value"),
                    new JProperty("document", new JObject(
                        new JProperty("object", "document"),
                        new JProperty("data", new JObject()),
                        new JProperty("nodes", new JArray(
                            new JObject(
                                new JProperty("object", "block"),
                                new JProperty("type", "paragraph"),
                                new JProperty("data", new JObject()),
                                new JProperty("nodes", new JArray(
                                    new JObject(
                                        new JProperty("object", "text"),
                                        new JProperty("leaves", new JArray(
                                            new JObject(
                                                new JProperty("object", "leaf"),
                                                new JProperty("text", text),
                                                new JProperty("marks", new JArray())
                                            )
                                        ))
                                    )
                                ))
                            )
                        ))
                    ))
                )),
                new JProperty("customReactionId", customReactionID),
                new JProperty("expireInMs", 0)
            );

            request.Post("https://www.guilded.gg/api/users/me/status", jObject.ToString(), "application/json");
        }
        public void clearStatus() {
            HttpRequest request = defHttp.Request();

            request.Post("https://www.guilded.gg/api/users/me/status", "{}", "application/json");
        }
        public void joinGuild(string invite)
        {
			invite = Utilities.Utils.GetInviteCodeByInviteLink(invite);

			if (invite != "")
			{
				HttpRequest request = defHttp.Request();
				request.Put("https://www.guilded.gg/api/invites/" + invite);

                if (request.Response.IsOK)
                {
                    Console.WriteLine(request.Response.ToString());
                }
            }
		}

		public void leaveGuild(string sid)
		{
			HttpRequest request = defHttp.Request();
			request.Delete("https://www.guilded.gg/api/teams/" + sid + "/members/" + this.Id);
		}

		public void voiceCall(string UID)
		{
			HttpRequest request = defHttp.Request();
			JObject jObject = new JObject();
			jObject["callType"] = "voice";
			request.Post("https://www.guilded.gg/api/channels/" + UID + "/call", jObject.ToString(), "application/json");
		}
		
		public void addFriend(string uid)
		{
			HttpRequest request = defHttp.Request();
			JObject jObject = new JObject();
			jObject["friendUserIds"] = new JArray(uid);
			request.Post("https://www.guilded.gg/api/users/me/friendrequests", jObject.ToString(), "application/json");
		}

		public void unFriend(string uid)
		{
			HttpRequest request = defHttp.Request();
			JObject jObject = new JObject();
			jObject["friendUserIds"] = new JArray(uid);
			request.Delete("https://www.guilded.gg/api/users/me/friendrequests");
		}

		public string getProfile()
		{
			HttpRequest request = defHttp.Request();
			request.Get("https://www.guilded.gg/api/me?isLogin=false&v2=true");

            if (request.Response.IsOK)
            {
                return request.Response.ToString();
			}
			return null;
		}

		public void addReaction(string cid, string mid, string rid)
		{
            HttpRequest request = defHttp.Request();

            request.Post("https://www.guilded.gg/api/channels/" + cid + "/messages/" + mid + "/reactions/" + rid);
        }

		public void removeReaction(string cid, string mid, string rid)
		{
			HttpRequest request = defHttp.Request();

			request.Delete("https://www.guilded.gg/api/channels/" + cid + "/messages/" + mid + "/reactions/" + rid);
        }

		public void joinVC(string cid)
		{
            HttpRequest connection = defHttp.Request();

            connection.Get("https://www.guilded.gg/api/channels/" + cid + "/connection?preferredRegion=");

            if (connection.Response.IsOK)
            {
                VoiceConnection server = JsonConvert.DeserializeObject<VoiceConnection>(connection.Response.ToString());

                HttpRequest rtc = defHttp.Request();

                rtc.AddHeader("guilded-rtc-token", server.token);

                JObject jObject = new JObject(
                    new JProperty("rtpCapabilities", new JObject(
                        new JProperty("codecs", new JArray(
                            new JObject(
                                new JProperty("mimeType", "audio/opus"),
                                new JProperty("kind", "audio"),
                                new JProperty("preferredPayloadType", 100),
                                new JProperty("clockRate", 48000),
                                new JProperty("channels", 2),
                                new JProperty("parameters", new JObject(
                                    new JProperty("minptime", 10),
                                    new JProperty("useinbandfec", 1)
                                )),
                                new JProperty("rtcpFeedback", new JArray(
                                    new JObject(
                                        new JProperty("type", "transport-cc"),
                                        new JProperty("parameter", "")
                                    ),
                                    new JObject(
                                        new JProperty("type", "nack"),
                                        new JProperty("parameter", "")
                                    )
                                ))
                            ),
                            new JObject(
                                new JProperty("mimeType", "video/H264"),
                                new JProperty("kind", "video"),
                                new JProperty("preferredPayloadType", 101),
                                new JProperty("clockRate", 90000),
                                new JProperty("parameters", new JObject(
                                    new JProperty("level-asymmetry-allowed", 1),
                                    new JProperty("packetization-mode", 1),
                                    new JProperty("profile-level-id", "42e01f")
                                )),
                                new JProperty("rtcpFeedback", new JArray(
                                    new JObject(
                                        new JProperty("type", "goog-remb"),
                                        new JProperty("parameter", "")
                                    ),
                                    new JObject(
                                        new JProperty("type", "transport-cc"),
                                        new JProperty("parameter", "")
                                    ),
                                    new JObject(
                                        new JProperty("type", "ccm"),
                                        new JProperty("parameter", "fir")
                                    ),
                                    new JObject(
                                        new JProperty("type", "nack"),
                                        new JProperty("parameter", "")
                                    ),
                                    new JObject(
                                        new JProperty("type", "nack"),
                                        new JProperty("parameter", "pli")
                                    )
                                ))
                            ),
                            new JObject(
                                new JProperty("mimeType", "video/rtx"),
                                new JProperty("kind", "video"),
                                new JProperty("preferredPayloadType", 102),
                                new JProperty("clockRate", 90000),
                                new JProperty("parameters", new JObject(
                                    new JProperty("apt", 101)
                                )),
                                new JProperty("rtcpFeedback", new JArray())
                            )
                        )),
                        new JProperty("headerExtensions", new JArray(
                            new JObject(
                                new JProperty("kind", "audio"),
                                new JProperty("uri", "urn:ietf:params:rtp-hdrext:sdes:mid"),
                                new JProperty("preferredId", 1),
                                new JProperty("preferredEncrypt", false),
                                new JProperty("direction", "sendrecv")
                            ),
                            new JObject(
                                new JProperty("kind", "video"),
                                new JProperty("uri", "urn:ietf:params:rtp-hdrext:toffset"),
                                new JProperty("preferredId", 12),
                                new JProperty("preferredEncrypt", false),
                                new JProperty("direction", "sendrecv")
                            )
                        ))
                    )),
                    new JProperty("wasMoved", false),
                    new JProperty("supportsVideo", false),
                    new JProperty("appType", "Web"),
                    new JProperty("isRestarting", false),
                    new JProperty("channelIdFromPreviousConnection", null)
                );

                rtc.Post("https://" + server.endpoint + "/channels/" + cid + "/voicegroups/lobby/connect", jObject.ToString(), "application/json");

                if (rtc.Response.IsOK)
                {
                    HttpRequest transport = defHttp.Request();

                    transport.AddHeader("guilded-rtc-token", server.token);

                    JObject jObject_2 = new JObject(
                        new JProperty("transportId", "7437b534-c230-41bb-9589-6c4d32a090c3"),
                        new JProperty("dtlsParameters",
                            new JObject(
                                new JProperty("role", "client"),
                                new JProperty("fingerprints",
                                    new JArray(
                                        new JObject(
                                            new JProperty("algorithm", "sha-256"),
                                            new JProperty("value", "64:48:0D:4A:CD:BE:55:32:7B:00:93:C3:2E:C4:E2:1F:1B:63:79:98:16:FB:EA:19:79:67:BB:23:4D:42:2D:D2")
                                        )
                                    )
                                )
                            )
                        )
                    );

                    transport.Post("https://" + server.endpoint + "/channels/" + cid + "/voicegroups/lobby/transport", jObject_2.ToString(), "application/json");
                }
            }
        }
		
		public void leaveVC(string cid)
		{
            HttpRequest request = defHttp.Request();

            request.Post("https://rtc-us-west-2-a2c865af-prod.guilded.gg/channels/" + cid + "/voicegroups/lobby/leave", "{}", "application/json");
        }

		public void sendMessage(string text, string cid)
		{
			HttpRequest request = defHttp.Request();

			JObject jObject = new JObject(
				new JProperty("messageId", Guid.NewGuid().ToString()),
				new JProperty("content", new JObject(
					new JProperty("object", "value"),
					new JProperty("document", new JObject(
						new JProperty("object", "document"),
						new JProperty("data", new JObject()),
						new JProperty("nodes", new JArray(
							new JObject(
								new JProperty("object", "block"),
								new JProperty("type", "paragraph"),
								new JProperty("data", new JObject()),
								new JProperty("nodes", new JArray(
									new JObject(
										new JProperty("object", "text"),
										new JProperty("leaves", new JArray(
											new JObject(
												new JProperty("object", "leaf"),
												new JProperty("text", text),
												new JProperty("marks", new JArray())
											)
										))
									)
								))
							)
						))
					))
				)),
				new JProperty("repliesToIds", new JArray()),
				new JProperty("confirmed", false),
				new JProperty("isSilent", false),
				new JProperty("isPrivate", false)
			);

			request.Post("https://www.guilded.gg/api/channels/" + cid + "/messages", jObject.ToString(), "application/json");

            Console.WriteLine(request.Response.ToString());
        }
	}
}