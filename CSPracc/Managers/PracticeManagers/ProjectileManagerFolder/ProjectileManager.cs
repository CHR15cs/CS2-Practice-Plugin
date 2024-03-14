using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.DataModules;
using CSPracc.CommandHandler;
using CounterStrikeSharp.API.Modules.Entities;
using System.Xml.Linq;
using CSPracc.Extensions;
using CSPracc.DataModules.Constants;
using CSPracc.Modes;
using CSPracc.DataStorages.JsonStorages;
using System.Reflection;
using System.Numerics;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;
using System.Globalization;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using Microsoft.Extensions.Logging;
using static System.Formats.Asn1.AsnWriter;
using CounterStrikeSharp.API.Modules.Memory;
using System.ComponentModel;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using System.Reflection.Metadata;
using CSPracc.Managers;
using static CounterStrikeSharp.API.Core.BasePlugin;
using CounterStrikeSharp.API.Modules.Commands;
using CSPracc.Managers.BaseManagers;
using CSPracc.Managers.BaseManagers.CommandManagerFolder;

namespace CSPracc
{
    public class ProjectileManager : BaseManager
    {
        /// <summary>
        /// Save history of thrown grenades
        /// </summary>
        public Dictionary<ulong, List<ProjectileSnapshot>> LastThrownGrenade
        {
            get; set;
        } = new Dictionary<ulong, List<ProjectileSnapshot>>();
        /// <summary>
        /// List of plugin re-thrown grenades
        /// </summary>
        public List<CBaseCSGrenadeProjectile?> SelfThrownGrenade
        {
            get; set;
        } = new List<CBaseCSGrenadeProjectile?>();
        /// <summary>
        /// Last thrown smokes for printing timings
        /// </summary>
        public Dictionary<int, DateTime> LastThrownSmoke
        {
            get; set;
        } = new Dictionary<int, DateTime>();
        /// <summary>
        /// Saved positions for .flash command
        /// </summary>
        Dictionary<ulong, Position> FlashPosition
        {
            get; set;
        } = new Dictionary<ulong, Position>();
        /// <summary>
        /// List of players to remove blinding effect from
        /// </summary>
        public List<ulong> NoFlashList
        {
            get; set;
        } = new List<ulong>();
        /// <summary>
        /// Last nade a player added
        /// </summary>
        Dictionary<ulong, int> lastSavedNade
        {
            get; set;
        } = new Dictionary<ulong, int>();

        /// <summary>
        /// When using .back or .forward the int is set to the position where the player currently is in the last thrown grenades
        /// </summary>
        Dictionary<ulong, int> playerGrenadeHistorePosition
        {
            get; set;
        } = new Dictionary<ulong, int>();


        /// <summary>
        /// Stored nades
        /// </summary>
        protected Dictionary<string, ProjectileStorage> projectileStorages { get; init; } = new Dictionary<string, ProjectileStorage>();
        /// <summary>
        /// Gets Projectile Storage for current map
        /// </summary>
        protected ProjectileStorage CurrentProjectileStorage
        {
            get
            {
                return GetOrAddProjectileStorage(Server.MapName);
            }
        }
        GuiManager GuiManager;
        CSPraccPlugin Plugin;
        public ProjectileManager(ref CommandManager commandManager,ref GuiManager guiManager, ref CSPraccPlugin plugin) : base(ref commandManager)
        {
            Plugin = plugin;
            GuiManager = guiManager;
            plugin.RegisterListener<Listeners.OnEntitySpawned>(OnEntitySpawned);
            plugin.RegisterEventHandler<EventSmokegrenadeDetonate>(OnSmokeDetonate, hookMode: HookMode.Post);
            plugin.AddCommand("css_pracc_smokecolor_enabled", "Enable smoke coloring", SmokeColoring);
            Commands.Add(PROJECTILE_COMMAND.NADES,new PlayerCommand(PROJECTILE_COMMAND.NADES, "Show nade menu", NadesCommandHandler, null,null));
            Commands.Add(PROJECTILE_COMMAND.SAVE, new PlayerCommand(PROJECTILE_COMMAND.SAVE, "Save last thrown nade", SaveSnapshotCommandHandler, null, null));
            Commands.Add(PROJECTILE_COMMAND.Rename, new PlayerCommand(PROJECTILE_COMMAND.Rename, "Rename last loaded grenade", RenameLastSnapshotCommandHandler, null));
            Commands.Add(PROJECTILE_COMMAND.Delete, new PlayerCommand(PROJECTILE_COMMAND.Delete, "Delete last nade", CommandHandlerRemoveSnapshot, null));
            Commands.Add(PROJECTILE_COMMAND.rethrow, new PlayerCommand(PROJECTILE_COMMAND.rethrow, "Rethrow last grenade or with given id/tag", ReThrowCommandHandler, null));
            Commands.Add(PROJECTILE_COMMAND.Last, new PlayerCommand(PROJECTILE_COMMAND.Last, "Go back to last thrown grenade spot", RestorePlayersLastThrownGrenadeCommandHandler, null));
            Commands.Add(PROJECTILE_COMMAND.BACK, new PlayerCommand(PROJECTILE_COMMAND.BACK, "Go back in grenade history", RestorePlayersLastThrownGrenadeCommandHandler, null));
            Commands.Add(PROJECTILE_COMMAND.forward, new PlayerCommand(PROJECTILE_COMMAND.forward, "Go forward in grenade history", RestoreNextPlayersLastThrownGrenadeCommandHandler, null));
            Commands.Add(PROJECTILE_COMMAND.AddTag, new PlayerCommand(PROJECTILE_COMMAND.AddTag, "Add tag to last loaded grenade", AddTagToLastGrenadeCommandHandler, null));
            Commands.Add(PROJECTILE_COMMAND.RemoveTag, new PlayerCommand(PROJECTILE_COMMAND.RemoveTag, "Remove tag from last loaded grenade", RemoveTagFromLastGrenadeCommandHandler, null));
            Commands.Add(PROJECTILE_COMMAND.showtags, new PlayerCommand(PROJECTILE_COMMAND.showtags, "Show all tags", ShowAllAvailableTagsCommandHandler, null));
            Commands.Add(PROJECTILE_COMMAND.DeleteTag, new PlayerCommand(PROJECTILE_COMMAND.DeleteTag, "Delete tag from all grenades", DeleteTagFromAllNadesCommandHandler, null));
            Commands.Add(PROJECTILE_COMMAND.ClearTags, new PlayerCommand(PROJECTILE_COMMAND.ClearTags, "Clear grenade from tags", ClearTagsFromLastGrenadeCommandHandler, null));
            Commands.Add(PROJECTILE_COMMAND.Description, new PlayerCommand(PROJECTILE_COMMAND.Description, "Add description to last loaded grenade", AddDescriptionCommandHandler, null));
            Commands.Add(PROJECTILE_COMMAND.delay, new PlayerCommand(PROJECTILE_COMMAND.delay, "Add delay to last loaded grenade", SetDelayCommandHandler, null));
            Commands.Add(PROJECTILE_COMMAND.find, new PlayerCommand(PROJECTILE_COMMAND.find, "Search grenade menu for string", FindCommandHandler, null));
            Commands.Add(PROJECTILE_COMMAND.UpdatePos, new PlayerCommand(PROJECTILE_COMMAND.UpdatePos, "Update player starting position", UpdatePositionCommandHandler, null));
            Commands.Add(PROJECTILE_COMMAND.flash, new PlayerCommand(PROJECTILE_COMMAND.flash, "Enable flash testing mode", FlashCommandHandler, null));
            Commands.Add(PROJECTILE_COMMAND.stop, new PlayerCommand(PROJECTILE_COMMAND.stop, "Stop flash testing mode", StopCommandHandler, null));
            Commands.Add(PROJECTILE_COMMAND.noflash, new PlayerCommand(PROJECTILE_COMMAND.noflash, "Toggle noflash", NoFlashCommandHandler, null));
            Commands.Add(PROJECTILE_COMMAND.CLEAR, new PlayerCommand(PROJECTILE_COMMAND.CLEAR, "Clear player grenade projectiles", ClearPersonalNadesCommandHandler, null));
            Commands.Add(PROJECTILE_COMMAND.ClearAll, new PlayerCommand(PROJECTILE_COMMAND.ClearAll, "Clear all player grenade projectiles", ClearAllNadesCommandHandler, null));
        }

        #region CommandHandlers

        public bool ClearAllNadesCommandHandler(CCSPlayerController player, PlayerCommandArgument args)
        {
            return ClearNades(player, true);
        }

        public bool ClearPersonalNadesCommandHandler(CCSPlayerController player, PlayerCommandArgument args)
        {
            return ClearNades(player, false);
        }

        private bool SmokeColoringOn { get; set; } = true;
        private void SmokeColoring(CCSPlayerController? playerController,CommandInfo info)
        {
            string input = info.ArgString.ToLower();
            if(input == "1" || input == "true")
            {
                SmokeColoringOn = true;
            }
            else
            {
                SmokeColoringOn = false;
            }
        }

        public bool NadesCommandHandler(CCSPlayerController playerController, PlayerCommandArgument args)
        {
            if (args.ArgumentString.Trim().ToLower() == "all")
            {
                GuiManager.AddMenu(playerController.SteamID, GetNadeMenu(playerController));

            }
            else if (int.TryParse(args.ArgumentString.Trim(), out int id))
            {
                RestoreSnapshot(playerController, id);
                SetLastAddedProjectileSnapshot(playerController.SteamID, id);
            }
            else
            {
                GuiManager.AddMenu(playerController.SteamID, GetPlayerBasedNadeMenu(playerController, args.ArgumentString.ToLower(), ""));
            }
            return true;
        }

        public bool FindCommandHandler(CCSPlayerController playerController, PlayerCommandArgument args)
        {
            if (args.ArgumentCount < 1)
            {
                playerController.ChatMessage($"No query passed.");
                return false;
            }
            GuiManager.AddMenu(playerController.SteamID, GetPlayerBasedNadeMenu(playerController, args.ToString(), ""));
            return true;
        }

        /// <summary>
        /// Add grenade to the list
        /// </summary>
        /// <param name="player">player who issued the command</param>
        /// <param name="args">Arguments shall look like <Name> <Description></param>
        public bool SaveSnapshotCommandHandler(CCSPlayerController playerController, PlayerCommandArgument args)
        {
            if (args.ArgumentCount < 1)
            {
                playerController.ChatMessage("You need to pass a name for the grenade");
            }
            string grenadeName = args.ArgumentString;
            ProjectileSnapshot? snapshotToAdd = getLatestProjectileSnapshot(playerController.SteamID);
            if (snapshotToAdd == null)
            {

                playerController.ChatMessage("Could not save last thrown projectile.");
                return false;
            }
            snapshotToAdd.Title = grenadeName;
            int newid = CurrentProjectileStorage.Add(snapshotToAdd);
            lastSavedNade.SetOrAdd(playerController.SteamID, newid);
            playerController.ChatMessage($"Successfully added grenade {ChatColors.Blue}\"{grenadeName}\"{ChatColors.White} at id: {ChatColors.Green}{newid}");
            return true;
        }

        /// <summary>
        /// removing grenade
        /// </summary>
        /// <param name="player">player who issued the command</param>
        /// <param name="args">Arguments shall look like <Name> <Description></param>
        public bool CommandHandlerRemoveSnapshot(CCSPlayerController player, List<string> args)
        {
            if(args.Count == 0)
            {
                KeyValuePair<int, ProjectileSnapshot> snapshot = getLastAddedProjectileSnapshot(player.SteamID);
                CurrentProjectileStorage.RemoveKey(snapshot.Key);
                CurrentProjectileStorage.Save();
                Utils.ClientChatMessage($"Removed the last added grenade: {snapshot.Value.Title}", player);
            }
            int id = -1;
            try
            {
                id = Convert.ToInt32(args[1]);
            }
            catch
            {
                player.PrintToCenter("invalid argument, needs to be a number");
                return false;
            }
            if (!CurrentProjectileStorage.ContainsKey(id))
            {
                player.PrintToCenter($"Projectile with id {id} does not exist on current map");
                return false;
            }
            if (CurrentProjectileStorage.RemoveKey(id))
            {
                player.PrintToCenter($"Successfully removed projectile with id {id}");
            }
            else
            {
                player.PrintToCenter($"Failed to remove projectile with id {id}");
                return false;
            }
            return true;

        }

        /// <summary>
        /// Rethrowing last grenade
        /// Smokes are currently not detonating, that why they are disabled for now.
        /// </summary>
        /// <param name="player">player who issued the command</param>
        public bool ReThrowCommandHandler(CCSPlayerController playerController,List<string> args)
        {
            List<KeyValuePair<int, ProjectileSnapshot>> nades = getCurrentPlayerNades(playerController);

            if(args.Count == 0)
            {
                ProjectileSnapshot? grenade = getLatestProjectileSnapshot(playerController.SteamID);
                if (grenade == null)
                {
                    playerController.PrintToCenter("Could not get last thrown nade");
                    return false;
                }
                if (grenade == null)
                {
                    playerController.PrintToCenter("Could not get last thrown nade");
                    return false;
                }
                if (!ThrowGrenadePojectile(grenade, playerController))
                {
                    playerController.ChatMessage("Encountered error while throwing your last grenade.");
                    return false;
                }
                playerController.ChatMessage("Rethrowing your last grenade.");
                return true;
            }
            string tag = String.Join(" ", args);
            if (tag.StartsWith("id:"))
            {
                tag = tag[3..];
                if (!int.TryParse(tag, out int id))
                {
                    playerController.ChatMessage("Could not parse id");
                    return false;
                }

                var foundNade = nades.FirstOrDefault(x => x.Key == id).Value;
                if (foundNade == null)
                {
                    playerController.ChatMessage($"Could not find grenade with id {ChatColors.Red}{id}");
                    return false;
                }

                CSPraccPlugin.Instance!.AddTimer(foundNade.Delay, () => ThrowGrenadePojectile(foundNade, playerController));
                playerController.ChatMessage($"Threw your grenade {ChatColors.Green}{foundNade.Title}");

                return false;
            }
            else if (int.TryParse(tag, out int id))
            {
                var foundNade = nades.FirstOrDefault(x => x.Key == id).Value;
                if (foundNade != null)
                {
                    CSPraccPlugin.Instance!.AddTimer(foundNade.Delay, () => ThrowGrenadePojectile(foundNade, playerController));
                    playerController.ChatMessage($"Threw your grenade {ChatColors.Green}{foundNade.Title}");
                    return true;
                }
            }

            Utils.ClientChatMessage($"Throwing all grenades containing tag: {ChatColors.Green}{tag}", playerController);

            foreach (var kvp in nades)
            {
                if (snapshotContainTag(kvp.Value, tag))
                {
                    CSPraccPlugin.Instance!.AddTimer(kvp.Value.Delay, () => ThrowGrenadePojectile(kvp.Value, playerController));
                }
            }
            return true;
        }

        /// <summary>
        /// Save current pos, restore it when flash thrown.
        /// </summary>
        /// <param name="player">player who issued the command</param>
        public bool FlashCommandHandler(CCSPlayerController player, List<string> args)
        {
            if (!FlashPosition.ContainsKey(player.SteamID))
            {
                FlashPosition.Add(player.SteamID, new Position(player.PlayerPawn.Value.CBodyComponent.SceneNode.AbsOrigin.Copy(), player.PlayerPawn.Value.EyeAngles.Copy()));
            }
            else
            {
                FlashPosition[player.SteamID] = new Position(player.PlayerPawn.Value.CBodyComponent.SceneNode.AbsOrigin.Copy(), player.PlayerPawn.Value.EyeAngles.Copy());
            }
            player.PrintToCenter("In flashing mode. Use .stop to disable flashing mode.");
            return true;
        }


        public bool StopCommandHandler(CCSPlayerController player, List<string> args)
        {
            if (FlashPosition.ContainsKey(player.SteamID))
            {
                FlashPosition.Remove(player.SteamID);
                player.PrintToCenter("Stopped flashing mode.");
            }
            return true;
        }

        /// <summary>
        /// Disabling flash effect
        /// </summary>
        /// <param name="player">player who issued the command</param>
        public bool NoFlashCommandHandler(CCSPlayerController player, List<string> args)
        {

            if (!NoFlashList.Contains(player.SteamID))
            {
                NoFlashList.Add(player.SteamID);
                Server.NextFrame(() => player.PlayerPawn.Value.FlashMaxAlpha = 0.5f);
                player.HtmlMessage($"No flash: <font color='#008000'>enabled</font>", 2);
            }
            else
            {
                NoFlashList.Remove(player.SteamID);
                player.HtmlMessage("No flash: <font color='#008000'>disabled</font>", 2);
            }
            return true;
        }



        /// <summary>
        /// Adds description to your last saved nade
        /// </summary>
        /// <param name="steamId">player who issued the command</param>
        /// <param name="description">description</param>
        public bool AddDescriptionCommandHandler(CCSPlayerController playerController, List<string> args)
        {
            if(args.Count == 0)
            {
                playerController.ChatMessage("Cannot add empty description");
                return false;
            }
            string description = String.Join(" ", args);
            KeyValuePair<int, ProjectileSnapshot> lastSnapshot = getLastAddedProjectileSnapshot(playerController.SteamID);
            if (lastSnapshot.Key != 0)
            {
                if (lastSnapshot.Value != null)
                {
                    lastSnapshot.Value.Description = description;
                    CurrentProjectileStorage.SetOrAdd(lastSnapshot.Key, lastSnapshot.Value);
                    playerController.ChatMessage($"Updating grenade description to {description}");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Adds description to your last saved nade
        /// </summary>
        /// <param name="steamId">player who issued the command</param>
        /// <param name="description">description</param>
        public bool SetDelayCommandHandler(CCSPlayerController playerController, List<string> args)
        {
            if (args.Count == 0)
            {
                playerController.ChatMessage("Cannot add empty delay");
                return false;
            }          
            KeyValuePair<int, ProjectileSnapshot> lastSnapshot = getLastAddedProjectileSnapshot(playerController.SteamID);
            if (lastSnapshot.Key != 0)
            {
                if (lastSnapshot.Value != null)
                {
                    if (!float.TryParse(args[0], out float delayInSeconds))
                    {
                        playerController.ChatMessage("Could not parse delay");
                        return false;
                    }
                    lastSnapshot.Value.Delay = delayInSeconds;
                    CurrentProjectileStorage.SetOrAdd(lastSnapshot.Key, lastSnapshot.Value);
                    playerController.ChatMessage($"Updating grenade delay to {delayInSeconds}");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Adds description to your last saved nade
        /// </summary>
        /// <param name="steamId">player who issued the command</param>
        /// <param name="title">description</param>
        public bool RenameLastSnapshotCommandHandler(CCSPlayerController playerController, List<string> args)
        {
            if (args.Count == 0)
            {
                playerController.ChatMessage("Cannot set empty name");
                return false;
            }
            string title = String.Join(" ", args);
            KeyValuePair<int, ProjectileSnapshot> lastSnapshot = getLastAddedProjectileSnapshot(playerController.SteamID);
            if (lastSnapshot.Key != 0)
            {
                if (lastSnapshot.Value != null)
                {
                    lastSnapshot.Value.Title = title;
                    CurrentProjectileStorage.SetOrAdd(lastSnapshot.Key, lastSnapshot.Value);
                    playerController.ChatMessage($"Updating grenade name to {title}");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Adds description to your last saved nade
        /// </summary>
        /// <param name="steamId">player who issued the command</param>
        /// <param name="title">description</param>
        public bool UpdatePositionCommandHandler(CCSPlayerController playerController, List<string> args)
        {
            KeyValuePair<int, ProjectileSnapshot> lastSnapshot = getLastAddedProjectileSnapshot(playerController.SteamID);
            if (lastSnapshot.Key != 0)
            {
                if (lastSnapshot.Value != null)
                {
                    lastSnapshot.Value.PlayerPosition = playerController.GetCurrentPosition()!.PlayerPosition.ToVector3();
                    CurrentProjectileStorage.SetOrAdd(lastSnapshot.Key, lastSnapshot.Value);
                    Utils.ClientChatMessage($"Updating player position for your current nade.", playerController);
                    return true;
                }
            }
            return false;
        }

        public bool AddTagToLastGrenadeCommandHandler(CCSPlayerController playerController, List<string> args)
        {
            if(args.Count == 0)
            {
                playerController.ChatMessage("Cannot add empty tag to grenade");
                return false;
            }
            string tag = String.Join(" ", args);
            if (int.TryParse(tag, out int _))
            {
                playerController.ChatMessage("Cannot use a number as tag");
                return false;
            }
            KeyValuePair<int, ProjectileSnapshot> lastSnapshot = getLastAddedProjectileSnapshot(playerController.SteamID);
            if (lastSnapshot.Key != 0)
            {
                if (lastSnapshot.Value != null)
                {
                    if (snapshotContainTag(lastSnapshot.Value, tag))
                    {
                        playerController.ChatMessage($"Grenade already contains tag {tag}");
                        return false;
                    }
                    lastSnapshot.Value.Tags.Add(tag);
                    CurrentProjectileStorage.SetOrAdd(lastSnapshot.Key, lastSnapshot.Value);
                    playerController.ChatMessage($"Added tag {tag}  to {lastSnapshot.Value.Title}");
                    return true;
                }
            }
            return false;
        }

        public bool RemoveTagFromLastGrenadeCommandHandler(CCSPlayerController playerController, List<string> args)
        {
            if (args.Count == 0)
            {
                playerController.ChatMessage("Cannot add empty tag to grenade");
                return false;
            }
            string tag = String.Join(" ", args);
            KeyValuePair<int, ProjectileSnapshot> lastSnapshot = getLastAddedProjectileSnapshot(playerController.SteamID);
            if (lastSnapshot.Key != 0)
            {
                if (lastSnapshot.Value != null)
                {
                    bool foundTag = false;
                    foreach (string tagToDelete in lastSnapshot.Value.Tags)
                    {
                        if (tagToDelete.ToLower() == tag.ToLower())
                        {
                            foundTag = true;
                            lastSnapshot.Value.Tags.Remove(tagToDelete);
                            break;
                        }
                    }
                    if (foundTag)
                    {
                        CurrentProjectileStorage.SetOrAdd(lastSnapshot.Key, lastSnapshot.Value);
                        playerController.ChatMessage($"Removed tag {tag} from {lastSnapshot.Value.Title}");
                        return true;
                    }

                }
            }
            return false;
        }

        public bool ClearTagsFromLastGrenadeCommandHandler(CCSPlayerController playerController, List<string> args)
        {
            if (args.Count == 0)
            {
                playerController.ChatMessage("Cannot add empty tag to grenade");
                return false;
            }
            KeyValuePair<int, ProjectileSnapshot> lastSnapshot = getLastAddedProjectileSnapshot(playerController.SteamID);
            if (lastSnapshot.Key != 0)
            {
                if (lastSnapshot.Value != null)
                {
                    lastSnapshot.Value.Tags.Clear();
                    CurrentProjectileStorage.SetOrAdd(lastSnapshot.Key, lastSnapshot.Value);
                    playerController.ChatMessage($"Removed alls tags from {lastSnapshot.Value.Title}");
                    return true;
                }
            }
            return false;
        }

        public bool DeleteTagFromAllNadesCommandHandler(CCSPlayerController playerController, List<string> args)
        {
            if (args.Count == 0)
            {
                playerController.ChatMessage("Cannot add empty tag to grenade");
                return false;
            }
            string tag = String.Join(" ", args);
            List<KeyValuePair<int, ProjectileSnapshot>> playerSnapshots = getAllNadesFromPlayer(playerController.SteamID);
            foreach (KeyValuePair<int, ProjectileSnapshot> kvp in playerSnapshots)
            {
                kvp.Value.Tags.Remove(tag);
                CurrentProjectileStorage.SetOrAdd(kvp.Key, kvp.Value);
            }
            playerController.ChatMessage($"Removed tag {tag} from all your grenades");
            return true;
        }


        /// <summary>
        /// Restoring the last thrown smoke
        /// </summary>
        /// <param name="player"></param>
        public bool RestorePlayersLastThrownGrenadeCommandHandler(CCSPlayerController player,List<string> args)
        {
            int count = -1;
            if(args.Count > 0)
            {
                int.TryParse(args[0], out count);
            }
            if (!LastThrownGrenade.TryGetValue(player.SteamID, out List<ProjectileSnapshot>? snapshots))
            {
                Utils.ClientChatMessage($"{ChatColors.Red}Failed to get your last grenades", player);
                return false;
            }
            if (snapshots == null)
            {
                Utils.ClientChatMessage($"{ChatColors.Red}You did not throw any grenades yet.", player);
                return false;
            }

            if (!playerGrenadeHistorePosition.TryGetValue(player.SteamID, out int pos))
            {
                pos = -1;
                playerGrenadeHistorePosition.SetOrAdd(player.SteamID, pos);
            }
            if (count == -1)
            {
                snapshots[0].Restore(player);
                return false;
            }
            pos += count;
            if (pos >= snapshots.Count)
            {
                pos--;
                Utils.ClientChatMessage($"Reached the end of your grenade history, teleporting to the last grenade.", player);
            }
            ProjectileSnapshot? snapshot = snapshots[pos];
            if (snapshot != null)
            {
                playerGrenadeHistorePosition.SetOrAdd(player.SteamID, pos);
                snapshot.Restore(player);
                return false;
            }
            player.PrintToCenter("You did not throw a projectile yet!");
            return true;
        }

        /// <summary>
        /// Restoring the last thrown smoke
        /// </summary>
        /// <param name="player"></param>
        public bool RestoreNextPlayersLastThrownGrenadeCommandHandler(CCSPlayerController player,List<string> args)
        {
            int count = 1;
            if (args.Count > 0)
            {
                int.TryParse(args[0], out count);
            }
            if (!LastThrownGrenade.TryGetValue(player.SteamID, out List<ProjectileSnapshot>? snapshots))
            {
                Utils.ClientChatMessage($"{ChatColors.Red}Failed to get your last grenades", player);
                return false;
            }
            if (snapshots == null)
            {
                Utils.ClientChatMessage($"{ChatColors.Red}You did not throw any grenades yet.", player);
                return false;
            }
            if (!playerGrenadeHistorePosition.TryGetValue(player.SteamID, out int pos))
            {
                pos = 1;
                playerGrenadeHistorePosition.SetOrAdd(player.SteamID, pos);
            }
            pos -= count;
            if (pos < 0)
            {
                pos = 0;
                Utils.ClientChatMessage($"You are at your latest grenade.", player);
                return false;
            }
            ProjectileSnapshot? snapshot = snapshots[pos];
            if (snapshot != null)
            {
                playerGrenadeHistorePosition.SetOrAdd(player.SteamID, pos);
                snapshot.Restore(player);
                return true;
            }
            player.PrintToCenter("You did not throw a projectile yet!");
            return false;
        }

        /// <summary>
        /// Get the last projectilesnapshot a player added
        /// </summary>
        /// <param name="steamId">player</param>
        /// <returns>snapshot</returns>
        public bool ShowAllAvailableTagsCommandHandler(CCSPlayerController playerController, List<string> args)
        {
            List<string> tags = new List<string>();
            playerController.GetValueOfCookie("PersonalizedNadeMenu", out string? value);
            string MenuTitle = string.Empty;
            List<KeyValuePair<int, ProjectileSnapshot>> nadeList = new List<KeyValuePair<int, ProjectileSnapshot>>();
            if (value == null || value == "yes")
            {
                nadeList = getAllNadesFromPlayer(playerController.SteamID);
            }
            else
            {
                nadeList = CurrentProjectileStorage.GetAll();
            }

            foreach (KeyValuePair<int, ProjectileSnapshot> nade in nadeList)
            {
                foreach (string tag in nade.Value.Tags)
                {
                    if (!tags.Contains(tag))
                    {
                        tags.Add(tag);
                    }
                }
            }
            playerController.ChatMessage($"Your currently available tags: {ChatColors.Green}{String.Join(", ", tags)}");
            return true;
        }

        #endregion
        public new void Dispose()
        {
            Listeners.OnEntitySpawned onEntitySpawned = new Listeners.OnEntitySpawned(OnEntitySpawned);
            Plugin.RemoveListener("OnEntitySpawned", onEntitySpawned);
            GameEventHandler<EventSmokegrenadeDetonate> smokegrenadedetonate = OnSmokeDetonate;
            Plugin.DeregisterEventHandler("smokegrenade_detonate", smokegrenadedetonate, true);
            base.Dispose();
        }
        /// <summary>
        /// Gets or Adds Projectile Storage for given map
        /// </summary>
        /// <param name="mapName">Map name</param>
        /// <returns>Projectile Storage for given map</returns>
        protected ProjectileStorage GetOrAddProjectileStorage(string mapName)
        {
            if (!projectileStorages.ContainsKey(mapName))
            {
                //TODO: Get Directory from config.
                projectileStorages.Add(mapName, new ProjectileStorage(new DirectoryInfo(Path.Combine(CSPraccPlugin.ModuleDir.FullName, "Projectiles"))));
            }
            return projectileStorages[mapName];
        }

        /// <summary>
        /// Create nade menu
        /// </summary>
        /// <param name="player">player who called the nade menu</param>
        /// <returns></returns>
        public HtmlMenu GetNadeMenu(CCSPlayerController player)
        {
            List<KeyValuePair<string, Action>> nadeOptions = new List<KeyValuePair<string, Action>>();
            ProjectileSnapshot? latestSnapshot = getLatestProjectileSnapshot(player.SteamID);
            if (latestSnapshot != null)
            {
                nadeOptions.Add(new KeyValuePair<string, Action>($"Your last thrown projectile", new Action(() => RestorePlayersLastThrownGrenadeCommandHandler(player, new List<string>()))));
            }
            foreach (KeyValuePair<int, ProjectileSnapshot> entry in CurrentProjectileStorage.GetAll())
            {
                nadeOptions.Add(new KeyValuePair<string, Action>($"{entry.Value.Title} ID:{entry.Key}", new Action(() =>
                {
                    RestoreSnapshot(player, entry.Key);
                    SetLastAddedProjectileSnapshot(player.SteamID, entry.Key);
                    })));
            }
            HtmlMenu htmlNadeMenu = new HtmlMenu("Nade Menu", nadeOptions, false); ;
            return htmlNadeMenu;
        }

        /// <summary>
        /// Create nade menu
        /// </summary>
        /// <param name="player">player who called the nade menu</param>
        /// <returns></returns>
        public HtmlMenu GetPlayerBasedNadeMenu(CCSPlayerController player, string tag, string name = "")
        {
            tag = tag.ToLower();
            List<KeyValuePair<string, Action>> nadeOptions = new List<KeyValuePair<string, Action>>();

            player.GetValueOfCookie("PersonalizedNadeMenu", out string? value);
            string MenuTitle = string.Empty;
            bool usePersonalNadeMenu = (value == "yes") || (value == null && CSPraccPlugin.Instance!.Config!.UsePersonalNadeMenu) ? true : false;
            if (usePersonalNadeMenu)
            {
                MenuTitle = "Personal Nade Menu";
                foreach (KeyValuePair<int, ProjectileSnapshot> entry in getAllNadesFromPlayer(player.SteamID))
                {
                    if (snapshotContainTag(entry.Value,tag) || tag == "" || entry.Value.Title.Contains(tag) && entry.Value.Title.Contains(name))
                    {
                        nadeOptions.Add(new KeyValuePair<string, Action>($"{entry.Value.Title} ID:{entry.Key}", new Action(() => {

                            RestoreSnapshot(player, entry.Key);
                            SetLastAddedProjectileSnapshot(player.SteamID, entry.Key);
                        })));
                    }
                }
            }
            else
            {
                MenuTitle = "Global Nade Menu";
                foreach (KeyValuePair<int, ProjectileSnapshot> entry in CurrentProjectileStorage.GetAll())
                {
                    if (snapshotContainTag(entry.Value, tag) || tag == "" && entry.Value.Title.Contains(name))
                        nadeOptions.Add(new KeyValuePair<string, Action>($"{entry.Value.Title} ID:{entry.Key}", new Action(() => {

                            RestoreSnapshot(player, entry.Key);
                            SetLastAddedProjectileSnapshot(player.SteamID, entry.Key);
                        })));
                }
            }
            HtmlMenu htmlNadeMenu;
            if (tag == "")
            {
                htmlNadeMenu = new HtmlMenu($"{MenuTitle}", nadeOptions, false);
            }
            else
            {
                htmlNadeMenu = new HtmlMenu($"{MenuTitle} [{tag}]", nadeOptions, false);
            }
            return htmlNadeMenu;
        }     

        private bool snapshotContainTag(ProjectileSnapshot snapshot, string tagToSearch)
        {
            foreach(string tag in snapshot.Tags)
            {
                if(tag.ToLower() ==tagToSearch.ToLower()) return true;
            }
            return false;
        }

        private List<KeyValuePair<int, ProjectileSnapshot>> getCurrentPlayerNades(CCSPlayerController player)
        {
            player.GetValueOfCookie("PersonalizedNadeMenu", out string? value);
            string MenuTitle = string.Empty;
            List<KeyValuePair<int, ProjectileSnapshot>> nadeList = new List<KeyValuePair<int, ProjectileSnapshot>>();
            if (value == null)
            {
                Server.PrintToConsole("Could not get cookie");
                if(CSPraccPlugin.Instance.Config.UsePersonalNadeMenu)
                {
                    Server.PrintToConsole("personal nade menu");
                    nadeList = getAllNadesFromPlayer(player.SteamID);
                }
                else
                {
                    Server.PrintToConsole("global");
                    nadeList = CurrentProjectileStorage.GetAll();
                }
                return nadeList;
            }
            if(value == "yes")
            {
                Server.PrintToConsole("Could get cookie");
                nadeList = getAllNadesFromPlayer(player.SteamID);
            }
            else
            {
                nadeList = CurrentProjectileStorage.GetAll();
            }
            return nadeList;
        }


        /// <summary>
        /// Set the last projectilesnapshot a player added
        /// </summary>
        /// <param name="steamId">player</param>
        /// <returns>snapshot</returns>
        public void SetLastAddedProjectileSnapshot(ulong steamId, int snapshotid)
        {
            if(CurrentProjectileStorage.Get(snapshotid, out ProjectileSnapshot? snapshot))
            {
                if(snapshot != null)
                {
                    if(snapshot.initialThrower == 0 || snapshot.initialThrower == steamId)
                    {
                        lastSavedNade.SetOrAdd(steamId, snapshotid);
                        Utils.ClientChatMessage($"You can now edit {snapshot.Title}",steamId);
                    }
                }               
            }
        }
                
        /// <summary>
        /// Get the last projectilesnapshot a player added
        /// </summary>
        /// <param name="steamId">player</param>
        /// <returns>snapshot</returns>
        private KeyValuePair<int, ProjectileSnapshot> getLastAddedProjectileSnapshot(ulong steamId)
        {
            if(!lastSavedNade.TryGetValue(steamId,out int snapshotid))
            {
                Server.PrintToChatAll("could not get snapshotid");
                return new KeyValuePair<int, ProjectileSnapshot>();           
            }
            if(snapshotid == 0) return new KeyValuePair<int, ProjectileSnapshot>();

            CurrentProjectileStorage.Get(snapshotid, out ProjectileSnapshot? snapshot);

            if(snapshot == null) return new KeyValuePair<int, ProjectileSnapshot>();
            if (snapshot.initialThrower != steamId && snapshot.initialThrower != 0)
            {
                return new KeyValuePair<int, ProjectileSnapshot>();
            }
            return new KeyValuePair<int, ProjectileSnapshot>(snapshotid,snapshot);
        }

        private List<KeyValuePair<int,ProjectileSnapshot>> getAllNadesFromPlayer(ulong steamId)
        {
            List<KeyValuePair<int, ProjectileSnapshot>> grenadeList = new List<KeyValuePair<int, ProjectileSnapshot>>();
            foreach(KeyValuePair<int,ProjectileSnapshot> kvp in CurrentProjectileStorage.GetAll())
            {
                if(kvp.Value.initialThrower == steamId)
                {
                    grenadeList.Add(kvp);
                }
            }
            return grenadeList;
        }

        private ProjectileSnapshot? getLatestProjectileSnapshot(ulong steamId)
        {
            if (LastThrownGrenade.TryGetValue(steamId, out List<ProjectileSnapshot>? savedNades))
            {
                if (savedNades != null)
                {
                    ProjectileSnapshot? projectileSnapshot = savedNades.FirstOrDefault();
                    if (projectileSnapshot != null)
                    {
                        return projectileSnapshot;
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// Teleport player to grenade position
        /// </summary>
        /// <param name="player">player to teleport</param>
        /// <param name="grenadeName">grenade destination</param>
        private void RestoreSnapshot(CCSPlayerController player, string grenadeName)
        {
            int index = grenadeName.IndexOf(":");
            if(index == -1)
            {
                //: not found in string
                ProjectileSnapshot? projectileSnapshot = getLatestProjectileSnapshot(player.SteamID);
                if(projectileSnapshot != null)
                {
                    projectileSnapshot.Restore(player);
                }
                player.PrintToCenter($"Could not find id in grenade name {grenadeName}");
                return;
            }
            string idofNade = grenadeName.Substring(index + 1);
            if (!int.TryParse(idofNade, out int snapshotId))
            {
                player.PrintToCenter($"Failed to parse protectile id from {idofNade}");
                return;
            }
            RestoreSnapshot(player, snapshotId);
        }
        internal void RestoreSnapshot(CCSPlayerController player, int snapshotId)
        {
            if (CurrentProjectileStorage.Get(snapshotId, out ProjectileSnapshot snapshot))
            {
                snapshot.Restore(player);
                return;
            }
            else
            {
                player.PrintToCenter($"No projectile found with id {snapshotId}");
                return;
            }
        }
      
        public static MemoryFunctionWithReturn<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, int> CSmokeGrenadeProjectile_CreateFuncWindows = new(@"\x48\x89\x5C\x24\x08\x48\x89\x6C\x24\x10\x48\x89\x74\x24\x18\x57\x41\x56\x41\x57\x48\x83\xEC\x50\x4C\x8B\xB4\x24\x90\x00\x00\x00\x49\x8B\xF8");

        public static MemoryFunctionWithReturn<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, int> CSmokeGrenadeProjectile_CreateFuncLinux = new(@"\x55\x4c\x89\xc1\x48\x89\xe5\x41\x57\x41\x56\x49\x89\xd6\x48\x89\xf2\x48\x89\xfe\x41\x55\x45\x89\xcd\x41\x54\x4d\x89\xc4\x53\x48\x83\xec\x28\x48\x89\x7d\xb8\x48");
        public void OnEntitySpawned(CEntityInstance entity)
        {
            if(entity == null) return;
            if (!entity.IsProjectile())
            {
                return;
            }
            CBaseCSGrenadeProjectile projectile;

            switch (entity.Entity!.DesignerName){
                case (DesignerNames.ProjectileSmoke):
                    {
                        projectile = new CSmokeGrenadeProjectile(entity.Handle);
                        break;
                    }
                default:
                    {
                        projectile = new CBaseCSGrenadeProjectile(entity.Handle);
                        break;
                    }
            };

                Server.NextFrame(() =>
                {
                    CCSPlayerController player = new CCSPlayerController(projectile.Thrower.Value.Controller.Value.Handle);
                    CounterStrikeSharp.API.Modules.Utils.Vector playerPosition = player.PlayerPawn.Value.CBodyComponent!.SceneNode!.AbsOrigin;
                    //TODO provide actual projectile Position
                    CounterStrikeSharp.API.Modules.Utils.Vector projectilePosition = new CounterStrikeSharp.API.Modules.Utils.Vector();
                    QAngle playerAngle = player.PlayerPawn.Value.EyeAngles;
                    string name = "LastThrown";
                    //TODO parse actual description if provided
                    string description = "";
                    GrenadeType_t type = GrenadeType_t.GRENADE_TYPE_SMOKE;
                    switch(projectile.DesignerName)
                    {
                        case DesignerNames.ProjectileSmoke:
                            {
                               
                                type = GrenadeType_t.GRENADE_TYPE_SMOKE;
                                break;
                            }
                        case DesignerNames.ProjectileFlashbang:
                            {
                                type = GrenadeType_t.GRENADE_TYPE_FLASH;
                                TpToFlashPos(player);
                                break;
                            }
                        case DesignerNames.ProjectileHE:
                            {
                                type = GrenadeType_t.GRENADE_TYPE_EXPLOSIVE;
                                break;
                            }
                        case DesignerNames.ProjectileMolotov:
                            {
                                type = GrenadeType_t.GRENADE_TYPE_FIRE;
                                break;
                            }
                        case DesignerNames.ProjectileDecoy:
                            {
                                type = GrenadeType_t.GRENADE_TYPE_DECOY;
                                break;
                            }
                        default :
                            {
                                type = GrenadeType_t.GRENADE_TYPE_SMOKE;
                                break;
                            }
                            
                    }                                   
                    if ( projectile.Globalname != "custom")
                    {                       
                        ProjectileSnapshot tmpSnapshot = new ProjectileSnapshot(playerPosition.ToVector3(), projectile.InitialPosition.ToVector3(), playerAngle.ToVector3(), projectile.InitialVelocity.ToVector3(), name, description, type,player.SteamID);
                        List<ProjectileSnapshot>? projectileSnapshots = new List<ProjectileSnapshot>();                        
                        if (LastThrownGrenade.ContainsKey((player.SteamID)) && LastThrownGrenade.TryGetValue(player.SteamID, out projectileSnapshots))
                        {
                            if(projectileSnapshots== null)
                            {
                                projectileSnapshots = new List<ProjectileSnapshot>();
                            }
                            ProjectileSnapshot? projectileSnapshot = projectileSnapshots.FirstOrDefault();
                            if(projectileSnapshot != null)
                            {
                                if (projectileSnapshot.ProjectilePosition !=projectile.InitialPosition.ToVector3())
                                {

                                    projectileSnapshots.Insert(0, tmpSnapshot);
                                }
                                else
                                {
                                    projectile.Thrower.Raw = player.PlayerPawn.Raw;
                                    projectile.OriginalThrower.Raw = player.PlayerPawn.Raw;
                                }
                            }
                            else
                            {
                                projectileSnapshots.Insert(0, tmpSnapshot);
                            }                                              
                        }
                        else
                        {
                            LastThrownGrenade.SetOrAdd(player.SteamID, new List<ProjectileSnapshot>() { tmpSnapshot });
                        }                      
                    }

                });

            if (!SmokeColoringOn) return;
            if (projectile is CSmokeGrenadeProjectile)
            {
                Server.NextFrame(() =>
                {
                    CSmokeGrenadeProjectile smokeProjectile = (CSmokeGrenadeProjectile)projectile;
                    CCSPlayerController player = new CCSPlayerController(projectile.Thrower.Value.Controller.Value.Handle);
                    smokeProjectile.SmokeColor.X = (float)Utils.GetTeamColor(player).R;
                    smokeProjectile.SmokeColor.Y = (float)Utils.GetTeamColor(player).G;
                    if(LastThrownSmoke.ContainsKey(((int)projectile.Index)))
                    {
                        LastThrownSmoke[(int)projectile.Index] = DateTime.Now;
                    }
                    else
                    {
                        LastThrownSmoke.Add((int)projectile.Index, DateTime.Now);
                    }                    
                });
            }           
        }

        public HookResult OnSmokeDetonate(EventSmokegrenadeDetonate @event, GameEventInfo info)
        {
            if(LastThrownSmoke.TryGetValue(@event.Entityid, out var result)) 
            {
                
               CSmokeGrenadeProjectile projectile =  Utilities.GetEntityFromIndex<CSmokeGrenadeProjectile>(@event.Entityid);
                if(projectile != null)
                {
                    Utils.ServerMessage($"Smoke thrown by {ChatColors.Blue}{@event.Userid.PlayerName}{ChatColors.White} took {ChatColors.Green}{(DateTime.Now - result).TotalSeconds.ToString("0.00")}{ChatColors.White}s and {ChatColors.Green}{projectile.Bounces}{ChatColors.White} bounces to detonate.");
                }
                else
                {
                    Utils.ServerMessage($"Smoke thrown by {@event.Userid.PlayerName} took {(DateTime.Now - result).TotalSeconds.ToString("0.00")}s to detonate");
                }
                
            }
            return HookResult.Continue;
        }


        public void SaveLastGrenade(CCSPlayerController playerController, string name)
        {
            ProjectileSnapshot? snapshot = getLatestProjectileSnapshot(playerController.SteamID);
            if(snapshot == null)
            {
                return;
            }
            snapshot.Title = name;
            CurrentProjectileStorage.Add(snapshot);
            playerController.PrintToCenter($"Successfully added grenade {name}");           
        }

        public bool ThrowGrenadePojectile(ProjectileSnapshot projectile, CCSPlayerController player)
        {
            CBaseCSGrenadeProjectile? cGrenade = null;
            switch (projectile.GrenadeType_T)
            {
                case GrenadeType_t.GRENADE_TYPE_EXPLOSIVE:
                    {
                        cGrenade = Utilities.CreateEntityByName<CHEGrenadeProjectile>(DesignerNames.ProjectileHE);
                        break;
                    }
                case GrenadeType_t.GRENADE_TYPE_FLASH:
                    {
                        cGrenade = Utilities.CreateEntityByName<CFlashbangProjectile>(DesignerNames.ProjectileFlashbang);
                        break;
                    }
                case GrenadeType_t.GRENADE_TYPE_SMOKE:
                    {
                        cGrenade = Utilities.CreateEntityByName<CSmokeGrenadeProjectile>(DesignerNames.ProjectileSmoke);
                        cGrenade!.IsSmokeGrenade = true;
                        if(OperatingSystem.IsLinux())
                        {
                            CSmokeGrenadeProjectile_CreateFuncLinux.Invoke(
                                projectile.ProjectilePosition.ToCSVector().Handle,
                                projectile.ProjectilePosition.ToCSVector().Handle,
                                projectile.Velocity.ToCSVector().Handle,
                                projectile.Velocity.ToCSVector().Handle,
                                player.Pawn.Value.Handle,
                                45,
                                player.TeamNum
                            );

                        }
                        else if(OperatingSystem.IsWindows())
                        {
                            CSmokeGrenadeProjectile_CreateFuncWindows.Invoke(
                                projectile.ProjectilePosition.ToCSVector().Handle,
                                projectile.ProjectilePosition.ToCSVector().Handle,
                                projectile.Velocity.ToCSVector().Handle,
                                projectile.Velocity.ToCSVector().Handle,
                                player.Pawn.Value.Handle,
                                45,
                                player.TeamNum
                            );
                        }
                        else
                        {
                            Utils.ServerMessage($"{ChatColors.Red}Unknown operating system");
                            return false;
                        }          
                        return true;
                    }
                case GrenadeType_t.GRENADE_TYPE_FIRE:
                    {
                        cGrenade = Utilities.CreateEntityByName<CMolotovProjectile>(DesignerNames.ProjectileMolotov);
                        if(cGrenade != null)
                        cGrenade.SetModel("weapons/models/grenade/incendiary/weapon_incendiarygrenade.vmdl");
                        break;
                    }
                case GrenadeType_t.GRENADE_TYPE_DECOY:
                    {
                        cGrenade = Utilities.CreateEntityByName<CDecoyProjectile>(DesignerNames.ProjectileDecoy);
                        break;
                    }
                default:
                    {
                        cGrenade = Utilities.CreateEntityByName<CSmokeGrenadeProjectile>(DesignerNames.ProjectileSmoke);
                        break;
                    }
            }
            if (cGrenade == null)
            {
                CSPraccPlugin.Instance!.Logger.LogError("grenade entity is  null");
                return false;
            }
            cGrenade.Elasticity = 0.33f;
            cGrenade.IsLive = false;
            cGrenade.DmgRadius = 350.0f;
            cGrenade.Damage = 99.0f;
            cGrenade.InitialPosition.X = projectile.ProjectilePosition.X;
            cGrenade.InitialPosition.Y = projectile.ProjectilePosition.Y;
            cGrenade.InitialPosition.Z = projectile.ProjectilePosition.Z;
            cGrenade.InitialVelocity.X = projectile.Velocity.X;
            cGrenade.InitialVelocity.Y = projectile.Velocity.Y;
            cGrenade.InitialVelocity.Z = projectile.Velocity.Z;
            cGrenade.Teleport(projectile.ProjectilePosition.ToCSVector(), projectile.PlayerAngle.ToCSQAngle(), projectile.Velocity.ToCSVector());

            cGrenade.DispatchSpawn();
            cGrenade.Globalname = "custom";
            cGrenade.AcceptInput("FireUser1", player, player, "");
            cGrenade.AcceptInput("InitializeSpawnFromWorld", null, null, "");
            cGrenade.TeamNum = player.TeamNum;
            cGrenade.Thrower.Raw = player.PlayerPawn.Raw;
            cGrenade.OriginalThrower.Raw = player.PlayerPawn.Raw;
            cGrenade.OwnerEntity.Raw = player.PlayerPawn.Raw;
            SelfThrownGrenade.Add(cGrenade);
            return true;
        }


        private void TpToFlashPos(CCSPlayerController? player)
        {
            if (player == null || !player.IsValid)
            {
                return;
            }

            if(!FlashPosition.TryGetValue(player.SteamID,out Position? pos))
            {
                return;
            }
            if (pos == null) return;

            player.PlayerPawn.Value!.Teleport(pos.PlayerPosition, pos.PlayerAngle, new Vector(0,0,0));
        }
        public bool ClearNades(CCSPlayerController player, bool all = false)
        {
            var smokes = Utilities.FindAllEntitiesByDesignerName<CSmokeGrenadeProjectile>("smokegrenade_projectile");
            foreach (var entity in smokes)
            {
                if (entity != null)
                {                  
                    if (entity.Thrower.Value!.Handle == 0)
                    {
                        List<ProjectileSnapshot>? projectileSnapshots = new List<ProjectileSnapshot>();
                        if (LastThrownGrenade.ContainsKey((player.SteamID)) && LastThrownGrenade.TryGetValue(player.SteamID, out projectileSnapshots))
                        {
                            ProjectileSnapshot? projectileSnapshot = projectileSnapshots.FirstOrDefault();
                            if (projectileSnapshot != null)
                            {
                                if (projectileSnapshot.ProjectilePosition != entity.InitialPosition.ToVector3())
                                {
                                    entity.Remove();
                                    continue;
                                }
                            }
                            continue;
                        }
                        continue;
                    }
                    CCSPlayerController? thrower = new CCSPlayerController(entity.Thrower!.Value!.Controller!.Value!.Handle);
                    if (thrower.Handle == player.Handle || all)
                    {
                        entity.Remove();
                    }
                }
            }
                var mollys = Utilities.FindAllEntitiesByDesignerName<CSmokeGrenadeProjectile>("molotov_projectile");
                foreach (var entity in mollys)
                {
                    if (entity != null)
                    {
                        CCSPlayerController thrower = new CCSPlayerController(entity.Thrower.Value.Controller.Value.Handle);
                        if (thrower.Handle == player.Handle || all)
                        {
                            entity.Remove();
                        }
                    }
                }
                var inferno = Utilities.FindAllEntitiesByDesignerName<CSmokeGrenadeProjectile>("inferno");
                foreach (var entity in inferno)
                {
                    CCSPlayerController? thrower = new CCSPlayerController(entity.Thrower.Value.Controller.Value.Handle);
                    if (entity != null)
                    {
                        if (thrower.Handle == player.Handle || all)
                        {
                            entity.Remove();
                        }
                    }
                }
            return true;
            }
    }
    }
