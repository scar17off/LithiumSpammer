using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LithiumSpammer.Utilities
{
    class Utils
    {
        public static string GetInviteCodeByInviteLink(string inviteLink)
        {
            try
            {
                if (inviteLink.EndsWith("/"))
                {
                    inviteLink = inviteLink.Substring(0, inviteLink.Length - 1);
                }

                if (inviteLink.Contains("guilded") && inviteLink.Contains("/") && inviteLink.Contains("http"))
                {
                    string[] splitter = inviteLink.Split('/');

                    return splitter[splitter.Length - 1];
                }
            }
            catch
            {

            }

            return inviteLink;
        }
    }
}
