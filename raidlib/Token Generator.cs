using Leaf.xNet;
using Newtonsoft.Json.Linq;
using System;
using System.Net;

namespace LithiumSpammer.raidlib
{
	class Token_Generator
	{
		DefaultHttp defHttp = new DefaultHttp();
		public string GenerateToken(string username, string password, string email, bool autojoin, string invitecode)
		{
			string token = this.SendCreationRequest(username, password, email);
			if (autojoin)
			{
				Client client = new Client(token);
				if (autojoin) client.joinGuild(invitecode);
			}
			return token;
		}
		private string SendCreationRequest(string username, string password, string email)
		{
			HttpRequest httpRequest = defHttp.Request();
			httpRequest.AddHeader("Content-Type", "application/json");
			httpRequest.AddHeader("guilded-client-id", "12b6d22d-6eec-4df1-9382-274f8520a823");
			httpRequest.AddHeader("guilded-device-id", "537b5c96c662685a3c6a9cf97b15fafe68ca4eb5141254ac908d08f4460218cc");
			httpRequest.AddHeader("guilded-stag", "ca9fdbbdd5297a0cba08d8932c17c758");
			JObject jObject = new JObject();
			jObject["name"] = username;
			jObject["password"] = password;
			jObject["email"] = email;
			jObject["fullName"] = username;
			JObject jObject2 = new JObject();
			jObject2["platform"] = "desktop";
			jObject["extraInfo"] = jObject2;
			httpRequest.Post("https://www.guilded.gg/api/users?type=email", jObject.ToString(), "application/json");

			if (httpRequest.Response.IsOK)
			{
				HttpRequest httpRequest2 = defHttp.Request();
				string str = "{\"email\":\"" + email + "\", \"password\":\"" + password + "\", \"getMe\": true}";
				httpRequest2.AddHeader("Content-Type", "application/json");
				httpRequest2.Post("https://www.guilded.gg/api/login", str, "application/json");
				foreach (Cookie cookie in httpRequest2.Response.Cookies.GetCookies(new Uri("https://www.guilded.gg/api/login")))
				{
					if (cookie.Name == "hmac_signed_session")
					{
						return cookie.Value.ToString();
					}
				}
			}
            else
            {
				return httpRequest.Response.ToString();
			}

			return "";
		}
	}
}