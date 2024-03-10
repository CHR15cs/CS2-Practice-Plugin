using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CounterStrikeSharp.API;
using CSPracc.DataModules.Constants;
using CSPracc.DataModules;
using CSPracc.Managers;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using CSPracc.Modes;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace CSPracc.CommandHandler
{
    public class BaseCommandHandler : IDisposable
    {
        BaseMode BaseMode;
        public BaseCommandHandler(BaseMode mode) 
        {
            CSPraccPlugin.Instance!.AddCommand("css_rcon_password", "gain temporary admin access", RconPassword);
            BaseMode = mode;
        }

        private void RconPassword(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null)
            {
                return;
            }
            if (!player.IsValid)
            {
                return;
            }
            string input = command.ArgString.Trim();
            if(input == CSPraccPlugin.Instance!.Config!.RconPassword)
            {
                AdminManager.AddPlayerPermissions(player,AdminFlags.Standard);
                player.HtmlMessage("Granted temporary admin permissions.");
            }
            else
            {
                player.PrintToCenter("incorrect admin password!");
            }
        }

        protected string getCommand(string input)
        {
            string command = null;
            try
            {
                //detect arguments
                if (input.Contains(' '))
                {
                    command = input.Substring(0, input.IndexOf(" "));                
                }
                else
                {
                    command = input;
                }
            }
            catch (Exception e)
            {
                return "";
            }
            return command;
        }

        protected string getArgs(string input)
        {
            string command, args = "";
            try
            {
                //detect arguments
                if (input.Contains(' '))
                {
                    command = input.Substring(0, input.IndexOf(" "));
                    if (command.IndexOf(' ') != input.Length - 1)
                    {
                        args = input.Substring(input.IndexOf(' ') + 1);
                    }
                    else
                    {
                        args = "";
                    }
                }
            }
            catch (Exception e)
            {
                return "";
            }
            return args;
        }

        protected bool CheckAndGetCommand(int userid, string commandWithArgs,out string command, out string args, out CCSPlayerController player)
        {
            Logging.LogMessage($"Inside CheckAndGetCommand");
            Logging.LogMessage($"{nameof(userid)}: {userid}");
            Logging.LogMessage($"{nameof(commandWithArgs)}: {userid}");
            command = string.Empty;
            args = string.Empty;
            player = null;
            if(commandWithArgs.StartsWith('!'))
            {
                commandWithArgs = "." + commandWithArgs.Substring(1);
            }
            if (!commandWithArgs.StartsWith("."))
            {
                Logging.LogMessage("String is no command. Leaving now.");
                return false;
            }
            player = new CCSPlayerController(NativeAPI.GetEntityFromIndex(userid+1));
            
            Logging.LogMessage($"Created {nameof(player)} ({player.PlayerName}) from userId {userid}");
            if (!IsPlayerValid(player))
            {
                Logging.LogMessage("EventPlayerChat invalid entity");
                return false;
            }
            Logging.LogMessage($"Player {player.PlayerName} is valid");
            command = getCommand(commandWithArgs);
            Logging.LogMessage($"Extracted command. Command now looks like this: \"{command}\"");
            if(CommandAliasManager.Instance.ReplaceAlias(player, command, out string replacedCommand))
            {
                Logging.LogMessage($"Replaced command to {replacedCommand}");
                command = replacedCommand;
            }
            args = getArgs(commandWithArgs);
            Logging.LogMessage($"Extracted args. Args now look like this: \"{args}\"");
            return true;
        }

        public virtual bool PlayerChat(EventPlayerChat @event, GameEventInfo info)
        {
            if(!CheckAndGetCommand(@event.Userid,@event.Text,out string command,out string args,out CCSPlayerController player))
            {
                Logging.LogMessage("Returning after CheckAndGetCommand");
                return false;
            }
            
            switch (command)
            {
                case BASE_COMMAND.HELP:
                    {
                        PrintHelp(player, args.Trim());
                        break;
                    }
                case BASE_COMMAND.MODE:
                    {
                        BaseMode.ShowModeMenu(player);
                        break;
                    }
                case BASE_COMMAND.FAKERCON:
                    {
                        OnFakeRcon(player, args);
                        break;
                    }
                case BASE_COMMAND.MAP:
                    {
                        BaseMode.ChangeMap(player, args);
                        break;
                    }
                case BASE_COMMAND.Unload:
                    {
                        if (!player.IsAdmin())
                        {
                            player.PrintToCenter("Only admins can execute this command!");
                            return false;
                        }
                        CSPraccPlugin.SwitchMode(Enums.PluginMode.Base);
                        break;
                    }
                case BASE_COMMAND.PRAC:
                case BASE_COMMAND.PRACC:
                    {
                        if (!player.IsAdmin())
                        {
                            player.PrintToCenter("Only admins can execute this command!");
                            return false;
                        }
                        CSPraccPlugin.SwitchMode(Enums.PluginMode.Pracc);
                        break;
                    }
                case BASE_COMMAND.MATCH:
                    {
                        if (!player.IsAdmin())
                        {
                            player.PrintToCenter("Only admins can execute this command!");
                            return false;
                        }
                        RoundRestoreManager.CleanupOldFiles();
                        CSPraccPlugin.SwitchMode(Enums.PluginMode.Match);
                        break;
                    }
                case BASE_COMMAND.DryRun:
                    {
                        if (!player.IsAdmin())
                        {
                            player.PrintToCenter("Only admins can execute this command!");
                            return false;
                        }
                        RoundRestoreManager.CleanupOldFiles();
                        CSPraccPlugin.SwitchMode(Enums.PluginMode.DryRun);
                        break;
                    }
                //case BASE_COMMAND.Retake:
                //    {
                //        if (!player.IsAdmin())
                //        {
                //            player.PrintToCenter("Only admins can execute this command!");
                //            return false;
                //        }
                //        CSPraccPlugin.SwitchMode(Enums.PluginMode.Retake);
                //        break;
                //    }
                //case BASE_COMMAND.Prefire:
                //    {
                //        if (!player.IsAdmin())
                //        {
                //            player.PrintToCenter("Only admins can execute this command!");
                //            return false;
                //        }
                //        CSPraccPlugin.SwitchMode(Enums.PluginMode.Prefire);
                //        break;
                //    }
                case BASE_COMMAND.SWAP:
                    {
                        if (!player.IsAdmin())
                        {
                            player.PrintToCenter("Only admins can execute this command!");
                            return false;
                        }
                        Server.ExecuteCommand(COMMANDS.SWAP_TEAMS);
                        break;
                    }
                case BASE_COMMAND.ALIAS:
                    {
                        if (!player.IsAdmin())
                        {
                            player.PrintToCenter("Only admins can execute this command!");
                            return false;
                        }
                        if (!GetArgumentList(args, out List<string> ArgumentList))
                        {
                            player.PrintToCenter("Invalid amout of parameters. Command need to be used .alias <newAlias> <commandTobeExecuted>");
                        }
                        if (ArgumentList.Count != 2)
                        {
                            player.PrintToCenter("Invalid amout of parameters. Command need to be used .alias <newAlias> <commandTobeExecuted>");
                        }
                        CommandAliasManager.Instance.CreateAlias(player, ArgumentList[0], ArgumentList[1]);
                        break;
                    }
                case BASE_COMMAND.REMOVEALIAS:
                    {
                        if (!player.IsAdmin())
                        {
                            player.PrintToCenter("Only admins can execute this command!");
                            return false;
                        }
                        args = args.Trim();
                        if (args.Length == 0)
                        {
                            player.PrintToCenter("Invalid command arguments");
                        }
                        CommandAliasManager.Instance.RemoveAlias(player, args);
                        break;
                    }
                case BASE_COMMAND.GOT:
                    {
                        player.ChangeTeam(CsTeam.Terrorist);
                        break;
                    }
                case BASE_COMMAND.GOCT:
                    {
                        player.ChangeTeam(CsTeam.CounterTerrorist);
                        break;
                    }
                case BASE_COMMAND.GOSPEC:
                    {
                        player.ChangeTeam(CsTeam.Spectator);
                        break;
                    }

                default:
                    {
                        Logging.LogMessage("Could not find base command");
                        break;
                    }
            }

            return true;
        }

        protected bool GetArgumentList(string Argument, out List<string> arguments)
        {
            Logging.LogMessage($"Getting command arguments of string {Argument}");
            arguments = new List<string>();
            if (String.IsNullOrEmpty(Argument))
            {
                return false;
            }
            do
            {
                //Remove Leading or Trailing whitespaces
                Argument = Argument.Trim();
                int index = Argument.IndexOf(' ');
                if (index == -1)
                {
                    arguments.Add(Argument);
                    Argument = string.Empty;
                    break;
                }
                string foundArgument = Argument.Substring(0, index);
                arguments.Add(foundArgument);
                Logging.LogMessage($"Adding argument {foundArgument}");
                Argument = Argument.Substring(index);
            } while (Argument.Length > 0 && Argument != String.Empty);
            return true;
        }

        protected bool IsPlayerValid(CCSPlayerController player)
        {
            if (player == null)
            {
                return false;
            }
            if (!player.IsValid)
            {
                Logging.LogMessage("EventPlayerChat invalid entity");
                return false;
            }
            return true;
        }
        protected bool IsPlayerValidAdmin(CCSPlayerController player)
        {
            if(IsPlayerValid(player))
            {
                if(player.IsAdmin())
                {
                    return true;
                }
            }
            return false;
        }

        public void ShowModeMenu(CCSPlayerController? player)
        {
            if (player == null)
            {
                Server.ExecuteCommand("say player is null");
                return;
            }
            if (!player.PlayerPawn.IsValid)
            {
                Server.ExecuteCommand("say player not valid");
                return;
            }
            if (!player.IsAdmin())
            {
                player.PrintToCenter("Only admins can execute this command!");
                return;
            }
            
        }

        public void OnFakeRcon(CCSPlayerController? player, string args)
        {
            if (player == null) return;
            if (!player.PlayerPawn.IsValid) return;
            if (!player.IsAdmin())
            {
                player.PrintToCenter("Only admins can execute this command!");
                return;
            }
            Server.ExecuteCommand(args);
        }

        public virtual void PrintHelp(CCSPlayerController? player, string args = "")
        {
            List<string> message = new List<string>();
            message.Add($" {CSPraccPlugin.Instance!.Config.ChatPrefix} Command list:");
            message.Add($" {ChatColors.Green}{BASE_COMMAND.MODE}{ChatColors.White} Opening menu to switch modes.");
            message.Add($" {ChatColors.Green}{BASE_COMMAND.MAP}{ChatColors.Red} 'mapname'{ChatColors.White}. Switch to given map.");
            message.Add($" {ChatColors.Green}{BASE_COMMAND.MATCH}{ChatColors.White} Switching to match mode.");
            message.Add($" {ChatColors.Green}{BASE_COMMAND.PRACC}{ChatColors.White} Switching to practice mode.");
            //message.Add($" {ChatColors.Green}{BASE_COMMAND.Prefire}{ChatColors.White} Switching to prefire mode.");
            message.Add($" {ChatColors.Green}{BASE_COMMAND.DryRun}{ChatColors.White} Switching to dry run mode.");
            message.Add($" {ChatColors.Green}{BASE_COMMAND.ALIAS}{ChatColors.Red} '.new alias' '.old alias'{ChatColors.White}. Creates a personal alias for a command.");
            message.Add($" {ChatColors.Green}{BASE_COMMAND.REMOVEALIAS}{ChatColors.Red} '.aliasToRemove'{ChatColors.White}. Removes given alias.");
            message.Add($" {ChatColors.Green}{BASE_COMMAND.FAKERCON}{ChatColors.Red} 'server command'{ChatColors.White}. Execute given server command.");
            message.Add($" {ChatColors.Green}{BASE_COMMAND.SWAP}{ChatColors.White} Swap teams.");
            message.Add($" {ChatColors.Green}{BASE_COMMAND.GOCT}{ChatColors.White} Switch to ct.");
            message.Add($" {ChatColors.Green}{BASE_COMMAND.GOT}{ChatColors.White} Switch to t.");
            message.Add($" {ChatColors.Green}{BASE_COMMAND.GOSPEC}{ChatColors.White} Switch to spectator.");
            message.Add($" {ChatColors.Green}{BASE_COMMAND.HELP}{ChatColors.White} This command.");
            foreach (string s in message)
            {
                if( args == "" || s.ToLower().Contains(args.ToLower()))
                {
                    player?.PrintToChat(s);
                }                
            }
        }

        public virtual void Dispose()
        {
            CommandInfo.CommandCallback rconCommandCallback = RconPassword;
            CSPraccPlugin.Instance!.RemoveCommand("css_rcon_password", rconCommandCallback);
        }
    }
}
