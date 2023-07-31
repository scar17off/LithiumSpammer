using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Leaf.xNet;
using System.Text;
using System.Threading.Tasks;

namespace LithiumSpammer.raidlib
{
    class Token_Checker
    {
        DefaultHttp defHttp = new DefaultHttp();

        public bool isValidAccount(string email, string password)
        {
            HttpRequest httpRequest2 = defHttp.Request();
            string str = "{\"email\":\"" + email + "\", \"password\":\"" + password + "\", \"getMe\": true}";
            httpRequest2.AddHeader("Content-Type", "application/json");
            httpRequest2.Post("https://www.guilded.gg/api/login", str, "application/json");
            foreach (Cookie cookie in httpRequest2.Response.Cookies.GetCookies(new Uri("https://www.guilded.gg/api/login")))
            {
                if (cookie.Name == "hmac_signed_session")
                {
                    return true;
                }
                else
                    return false;
            }
            return false;
        }
        public string getAccountToken(string email, string password)
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
                else
                    return null;
            }
            return null;
        }
    }
}
