using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CSPracc.DataModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.DataStorages.JsonStorages
{
    public class UserCookieStorage : JsonStorage<ulong, Dictionary<string,string>>
    {
        public UserCookieStorage(DirectoryInfo cookieStorageDir) : base(new FileInfo(Path.Combine(cookieStorageDir.FullName, $"CookieStorage.json")))
        {
        }


        public void RemoveCookie(ulong userId,string cookieName) 
        {
            if (userId == 0 || cookieName == "")
            {
                return;
            }
            if(!Storage.TryGetValue(userId, out Dictionary<string,string>? userCookieDict))
            {
                return;
            }
            if(userCookieDict == null)
            {
                return;
            }
            userCookieDict.Remove(cookieName);
            Storage[userId] = userCookieDict;
            Save();
            return;
        }

        public bool GetValueOfCookie(ulong userId,string cookieName, out string? cookieValue)
        {
            cookieValue = null;
            if (userId == 0 || cookieName == "")
            {
                return false;
            }         
            if(!Storage.TryGetValue(userId,out Dictionary<string,string>? userCookies))
            {
                return false;
            }
            if(userCookies == null)
            {
                return false;
            }
            if(!userCookies.TryGetValue(cookieName,out cookieValue))
            {
                return false;
            }
            if(cookieValue == null)
            {
                return false;
            }
            return true;
        }

        public bool SetOrAddValueOfCookie(ulong userId, string cookieName, string cookieValue)
        {
            if(userId == 0 || cookieName == "" || cookieValue == "")
            {
                return false;
            }
            if (!Storage.TryGetValue(userId, out Dictionary<string, string>? userCookies))
            {
                Dictionary<string, string> userCookieDict = new Dictionary<string, string>();
                userCookieDict.Add(cookieName, cookieValue);
                if (!Storage.ContainsKey(userId))
                {                   
                    Storage.Add(userId, userCookieDict);
                }
                Storage[userId] = userCookieDict;
            }
            else
            {
                if (userCookies == null)
                {
                    userCookies = new Dictionary<string, string>();
                }
                if (userCookies.ContainsKey(cookieName))
                {
                    userCookies[cookieName] = cookieValue;
                }
                Storage[userId] = userCookies;
            }
            Save();
            return true;
        }

        public override bool Get(ulong key, out Dictionary<string, string> value)
        {
            if (!Storage.TryGetValue(key, out value))
            {
                return false;
            }
            return true;
        }
    }
}
