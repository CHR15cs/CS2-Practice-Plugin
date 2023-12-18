using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.DataModules;
using CSPracc.DataStorages.JsonStorages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.Managers
{
    public class GunManager
    {
        WeaponKitStorage WeaponKitStorage { get; set; }
        HtmlMenu GunMenu { get; set; }
        GuiManager GuiManager { get; set; }

        private UserPlayerEquipStorage playerEquipment;
        public GunManager(GuiManager gm) 
        { 
            GuiManager = gm;
            WeaponKitStorage = new WeaponKitStorage(new FileInfo(Path.Combine(CSPraccPlugin.Instance!.ModuleDirectory, "Retake", "WeaponKits.json")));
            WeaponKitStorage.SetOrAdd((int)CsTeam.Terrorist, new DataModules.WeaponKit(new List<string>() { "weapon_ak47", "weapon_awp", "weapon_galilar" }, new List<string>() { "weapon_p250", "weapon_p250", "weapon_deagle" }, true, true));
            WeaponKitStorage.SetOrAdd((int)CsTeam.CounterTerrorist, new DataModules.WeaponKit(new List<string>() { "weapon_m4a1", "weapon_m4a1_silencer", "weapon_awp", "weapon_famas" }, new List<string>() { "weapon_p250", "weapon_p250", "weapon_deagle" }, true, true));
            playerEquipment = new UserPlayerEquipStorage(new FileInfo(Path.Combine(CSPraccPlugin.Instance!.ModuleDirectory, "Retake", "UserEquipStorage.json")));
        }

        public void ShowGunMenu(CCSPlayerController player)
        {
            List<KeyValuePair<string, Task>> GunMenuOptions = new List<KeyValuePair<string, Task>>();
            GunMenuOptions.Add(new KeyValuePair<string, Task>($"Rifle Menu", new Task(() => ShowRifleMenu(player))));
            GunMenuOptions.Add(new KeyValuePair<string, Task>($"Pistol Menu", new Task(() => ShowPistolMenu(player))));
            GunMenu = new HtmlMenu("Gun Menu", GunMenuOptions);
            GuiManager.AddMenu(player.SteamID, GunMenu);
        }

        private void InitWeaponEquip(CCSPlayerController player)
        {
            if (!playerEquipment.ContainsKey(player.SteamID))
            {
                playerEquipment.Add(player.SteamID, new UserSelectedEquipment());
                WeaponKitStorage.Get((int)CsTeam.Terrorist, out WeaponKit kitT);
                if (!playerEquipment.Get(player.SteamID, out UserSelectedEquipment selectedEquiptment))
                {
                    selectedEquiptment = new UserSelectedEquipment();
                }
                for (int i = 0; i < selectedEquiptment.SelectedEquipment.Count; i++)
                {
                    if (!selectedEquiptment.SelectedEquipment.ContainsKey(CsTeam.Terrorist))
                    {
                        selectedEquiptment.SelectedEquipment.Add(CsTeam.Terrorist,new PlayerEquipment());
                        selectedEquiptment.SelectedEquipment[CsTeam.Terrorist].Secondary = kitT!.Secondary!.FirstOrDefault()!;
                        break;
                    }
                }

                WeaponKitStorage.Get((int)CsTeam.CounterTerrorist, out WeaponKit kitCT);
                for (int i = 0; i < selectedEquiptment.SelectedEquipment.Count; i++)
                {
                    if (!selectedEquiptment.SelectedEquipment.ContainsKey(CsTeam.Terrorist))
                    {
                        selectedEquiptment.SelectedEquipment.Add(CsTeam.CounterTerrorist, new PlayerEquipment());
                        selectedEquiptment.SelectedEquipment[CsTeam.CounterTerrorist].Secondary = kitCT!.Secondary!.FirstOrDefault()!;
                        break;
                    }
                }
            }
        }


        private void SelectPrimaryWeapon(CCSPlayerController player, string primaryWeapon)
        {
            if (player == null) return;
            if (!player.IsValid) return;

            if (!this.playerEquipment.ContainsKey(player.SteamID))
            {
                InitWeaponEquip(player);
            }

            if (!this.playerEquipment.Get(player.SteamID, out UserSelectedEquipment selectedEquiptment))
            {
                Utils.ServerMessage("Could not get u.s.e");
                selectedEquiptment = new UserSelectedEquipment();
            }
            if(!WeaponKitStorage.Get((int)player.GetCsTeam(),out WeaponKit weaponKit))
            {
                weaponKit = new WeaponKit();
            }
            if (!selectedEquiptment.SelectedEquipment.ContainsKey(player.GetCsTeam()))
            {
                selectedEquiptment.SelectedEquipment.Add(player.GetCsTeam(), new PlayerEquipment());
                selectedEquiptment.SelectedEquipment[player.GetCsTeam()].Secondary = weaponKit.Secondary!.FirstOrDefault()!;
            }
            selectedEquiptment.SelectedEquipment[player.GetCsTeam()].Primary = primaryWeapon;
            playerEquipment.SetOrAdd(player.SteamID, selectedEquiptment);
            EquipPlayer(player);
        }

        private void SelectSecondaryWeapon(CCSPlayerController player, string secondaryWeapon)
        {
            if (player == null) return;
            if (!player.IsValid) return;
            if (!this.playerEquipment.ContainsKey(player.SteamID))
            {
                Utils.ServerMessage("init weapon_equip");
                InitWeaponEquip(player);
            }

            if (!this.playerEquipment.Get(player.SteamID, out UserSelectedEquipment selectedEquiptment))
            {
                Utils.ServerMessage("Could not get u.s.e");
                selectedEquiptment = new UserSelectedEquipment();
            }
            if (!selectedEquiptment.SelectedEquipment.ContainsKey(player.GetCsTeam()))
            {
                selectedEquiptment.SelectedEquipment.Add(player.GetCsTeam(), new PlayerEquipment());
            }
            selectedEquiptment.SelectedEquipment[player.GetCsTeam()].Secondary = secondaryWeapon;
            playerEquipment.SetOrAdd(player.SteamID, selectedEquiptment);
            EquipPlayer(player);
        }

        public void EquipPlayer(CCSPlayerController player)
        {
            if (player == null) return;
            if (!player.IsValid) return;

            if (!playerEquipment.ContainsKey(player.SteamID)) return;

            player.RemoveWeapons();

            if (!playerEquipment.Get(player.SteamID, out UserSelectedEquipment selectedEquiptment))
            {
                selectedEquiptment = new UserSelectedEquipment();
            }

            if (!selectedEquiptment.SelectedEquipment.ContainsKey(player.GetCsTeam()))
            {
                selectedEquiptment.SelectedEquipment.Add(player.GetCsTeam(), new PlayerEquipment());
            }
            player.GiveNamedItem(selectedEquiptment.SelectedEquipment[player.GetCsTeam()].Primary);
            player.GiveNamedItem(selectedEquiptment.SelectedEquipment[player.GetCsTeam()].Secondary);
            foreach (string equip in selectedEquiptment.SelectedEquipment[player.GetCsTeam()].Equipment)
            {
                player.GiveNamedItem(equip);
            }
            player.GiveNamedItem("weapon_knife");
            player.GiveNamedItem(CsItem.Kevlar);
            player.GiveNamedItem(CsItem.KevlarHelmet);

        }

        private void ShowRifleMenu(CCSPlayerController player)
        {
            if (player == null)
            {
                return;
            }

            if (!player.IsValid)
            {
                return;
            }
            List<KeyValuePair<string, Task>> rifleOptions = new List<KeyValuePair<string, Task>>();
            if (!WeaponKitStorage.Get((int)player.GetCsTeam(), out WeaponKit kit))
            {
                return;
            }
            foreach (string primary in kit.Primary)
            {
                rifleOptions.Add(new KeyValuePair<string, Task>(primary.Substring(7), new Task(() => {

                    SelectPrimaryWeapon(player, primary);
                })
                    ));
            }
            rifleOptions.Add(new KeyValuePair<string, Task>("None", new Task(() => SelectPrimaryWeapon(player, ""))));
            HtmlMenu RifleMenu = new HtmlMenu("Rifle Menu", rifleOptions);
            GuiManager.AddMenu(player.SteamID, RifleMenu);
        }


        private void ShowPistolMenu(CCSPlayerController player)
        {
            if (player == null) return;
            if (!player.IsValid) return;

            List<KeyValuePair<string, Task>> pistolOptions = new List<KeyValuePair<string, Task>>();
            if (!WeaponKitStorage.Get((int)player.GetCsTeam(), out WeaponKit kit))
            {
                return;
            }
            foreach (string secondary in kit.Secondary)
            {
                pistolOptions.Add(new KeyValuePair<string, Task>(secondary.Substring(7), new Task(() => SelectSecondaryWeapon(player, secondary))));
            }
            HtmlMenu pistolMenu = new HtmlMenu("Pistol Menu", pistolOptions);
            GuiManager.AddMenu(player.SteamID, pistolMenu);

        }

    }
}
