# CS2-Practice-Plugin
Open Source Plugin for Counterstrike 2 based on [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp)

## Important
Currently in an early stage of development.
Plugin might be unstable or not working.

## Files
        configmanager.xml
                Stores list of admins, rcon password, list of saved grenades etc.
        Logging.txt
                Output which mitght be usefull for debugging purposes
## Configs
        Needs to be stores in cfg/CSPRACC/
## General Chat Commands
        .menu - Opens small chat menu, current options pracc, match or help
        .help - Prints information about plugin usage
        .rcon <Command> - Executing server sided commands 
        .map <mapname>

## Chat Commands available in match mode       
        .pause 
        .unpause 
        .forceready
        .warmup
        .backup - opens backup menu to restore a round
        
## Chat Commands available in practice mode
        .spawn <number> 
        .nades - Opens a menu of saved grenade lineups
        .save <name> <description> - Stores the current position and viewangle as grenade lineup used in .nades

