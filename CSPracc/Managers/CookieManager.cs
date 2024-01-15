using CounterStrikeSharp.API.Core;
using CSPracc.DataStorages.JsonStorages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers
{
    public static class CookieManager
    {
        private static UserCookieStorage? _storage = null;
        private static UserCookieStorage UserCookieStorage
        {
            get
            {
                if (_storage == null)
                {
                    DirectoryInfo UserCookieDir = new DirectoryInfo(Path.Combine(CSPraccPlugin.Instance!.ModuleDirectory, "UserCookieDir"));
                    if (!UserCookieDir.Exists)
                    {
                        UserCookieDir.Create();
                    }
                    _storage = new UserCookieStorage(UserCookieDir);
                }
                return _storage;
            }
        }

        public static bool GetValueOfCookie(CCSPlayerController playerController, string cookieName, out string? value)
        {
            return UserCookieStorage.GetValueOfCookie(playerController.SteamID, cookieName, out value);
        }

        public static bool GetValueOfCookie(ulong userId, string cookieName, out string? value)
        {
            return UserCookieStorage.GetValueOfCookie(userId, cookieName, out value);
        }

        public static bool AddOrSetValueOfCookie(CCSPlayerController playerController, string cookieName, string value)
        {
            return UserCookieStorage.SetOrAddValueOfCookie(playerController.SteamID, cookieName, value);
        }

        public static bool AddOrSetValueOfCookie(ulong userId, string cookieName,  string value)
        {
            return UserCookieStorage.SetOrAddValueOfCookie(userId, cookieName, value);
        }

        public static void RemoveCookie(ulong userId, string cookieName)
        {
           UserCookieStorage.RemoveCookie(userId, cookieName);
        }

    }
}
