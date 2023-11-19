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

namespace CSPracc.CommandHandler
{
    public class BaseCommandHandler : IDisposable
    {
        public BaseCommandHandler() {  }

        ChatMenu? _modeMenu = null;
        ChatMenu ModeMenu
        {
            get
            {
                if (_modeMenu == null)
                {
                    _modeMenu = new ChatMenu("Mode Menu");
                    var handleGive = (CCSPlayerController player, ChatMenuOption option) => ModeMenuOption(player, option.Text);
                    _modeMenu.AddMenuOption("Pracc", handleGive);
                    _modeMenu.AddMenuOption("Match", handleGive);
                    _modeMenu.AddMenuOption("Help", handleGive);
                }
                return _modeMenu;
            }
        }

        private void ModeMenuOption(CCSPlayerController player, string optionText)
        {
            switch (optionText)
            {
                case "Pracc":
                    CSPraccPlugin.SwitchMode(Enums.PluginMode.Pracc);
                    break;
                case "Match":
                    RoundRestoreManager.CleanupOldFiles();
                    CSPraccPlugin.SwitchMode(Enums.PluginMode.Match);
                    break;
                case "Help":
                    PrintHelp(player);
                    break;
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
                case PRACC_COMMAND.HELP:
                    {
                        PrintHelp(player);
                        break;
                    }
                case PRACC_COMMAND.MODE:
                    {
                        ShowModeMenu(player);
                        break;
                    }
                case PRACC_COMMAND.FAKERCON:
                    {
                        OnFakeRcon(player, args);
                        break;
                    }
                case PRACC_COMMAND.MAP:
                    {
                        BaseMode.ChangeMap(player, args);
                        break;
                    }
                case PRACC_COMMAND.PRACC:
                    {
                        if (!player.IsAdmin())
                        {
                            player.PrintToCenter("Only admins can execute this command!");
                            return false;
                        }
                        CSPraccPlugin.SwitchMode(Enums.PluginMode.Pracc);
                        break;
                    }
                case PRACC_COMMAND.MATCH:
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
                case PRACC_COMMAND.SWAP:
                    {
                        if (!player.IsAdmin())
                        {
                            player.PrintToCenter("Only admins can execute this command!");
                            return false;
                        }
                        Server.ExecuteCommand(COMMANDS.SWAP_TEAMS);
                        break;
                    }
                case PRACC_COMMAND.ALIAS:
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
                case PRACC_COMMAND.REMOVEALIAS:
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
                case PRACC_COMMAND.DEMO:
                    {
                        DemoManager.OpenDemoManagerMenu(player);
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
            ChatMenus.OpenMenu(player, ModeMenu);
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

        public virtual void PrintHelp(CCSPlayerController? player)
        {
            List<string> message = new List<string>();
            message.Add($" {CSPracc.DataModules.Constants.Strings.ChatTag} Command list:");
            message.Add($" {ChatColors.Green} {PRACC_COMMAND.PAUSE} {ChatColors.White} - Switching mode. Available modes: standard - unloading changes, pracc - loading practice config, match - loading match config");
            message.Add($" {ChatColors.Green} {PRACC_COMMAND.UNPAUSE} {ChatColors.White} - Starting the match. Works only in the warmup during match mode.");
            message.Add($" {ChatColors.Green} {PRACC_COMMAND.STOP}  {ChatColors.White} - Stopping the match.");
            message.Add($" {ChatColors.Green} {PRACC_COMMAND.WARMUP} {ChatColors.White} - (Re)starting warmup. Works only during match.");
            message.Add($" {ChatColors.Green} {PRACC_COMMAND.RESTART}  {ChatColors.White} - Restarting the match. Works only during a live match.");
            message.Add($" {ChatColors.Green} {PRACC_COMMAND.READY}  {ChatColors.White} - Ready up as a player. Works only during a warmup of a  match.");
            message.Add($" {ChatColors.Green} {PRACC_COMMAND.FORCEREADY}  {ChatColors.White} - Forcing all players to ready up. Works only during a warmup of a  match.");
            message.Add($" {ChatColors.Green} {PRACC_COMMAND.SPAWN}  {ChatColors.White} - Works only in practice mode. Teleports you to spawn number X");
            message.Add($" {ChatColors.Green} {PRACC_COMMAND.HELP}  {ChatColors.White} - Prints the help command.");
            foreach (string s in message)
            {
                player?.PrintToChat(s);
            }
        }

        public virtual void Dispose()
        {
            
        }
    }
}
