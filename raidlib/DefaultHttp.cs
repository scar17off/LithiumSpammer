using Leaf.xNet;

namespace LithiumSpammer.raidlib
{
    class DefaultHttp
    {
        public string token;
        public DefaultHttp(string token = "")
        {
            this.token = token;
        }
        public HttpRequest Request()
        {
            HttpRequest request = new HttpRequest();
            request.IgnoreInvalidCookie = true;
            request.IgnoreProtocolErrors = true;
            request.Reconnect = true;
            request.ReconnectDelay = 800;
            request.UserAgentRandomize();
            request.KeepAlive = false;
            if (token != "")
            {
                request.AddHeader("Cookie", "hmac_signed_session=" + this.token);
            }
            return request;
        }
    }
}