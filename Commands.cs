using System;
using System.Collections.Generic;

namespace BeppyServer {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CommandAttribute : Attribute
    {
        public string name;
        public string description;
        public string permission;

        public CommandAttribute(string name, string permission, string description)
        {
            this.name = name;
            this.permission = permission;
            this.description = description;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CommandAliasesAttribute : Attribute
    {
        public string[] aliases;
        public CommandAliasesAttribute(params string[] aliases)
        {
            this.aliases = aliases;
        }
    }

    public class CommandArgsException : Exception
    {
        public CommandArgsException() : base("Invalid arguments.") { }
    }

    public class InvalidCommandException : Exception
    {
        public InvalidCommandException() : base("Invalid command.") { }
    }

    public abstract class PlayerCommand
    {
        public string name;
        public string description;
        public string permission;
        public List<string> aliases;

        public PlayerCommand()
        {
            CommandAttribute attr = (CommandAttribute)Attribute.GetCustomAttribute(GetType(), typeof(CommandAttribute));
            if (attr == null)
                throw new Exception("Player command must include command attribute.");

            name = attr.name;
            permission = attr.permission;
            description = attr.description;
            aliases = new List<string>();

            CommandAliasesAttribute aliasAttr = (CommandAliasesAttribute)Attribute.GetCustomAttribute(GetType(), typeof(CommandAliasesAttribute));
            if (aliasAttr != null)
                aliases.AddRange(aliasAttr.aliases);
        }

        public abstract void Execute(ClientInfo sender, List<string> args);
    }

    public class Commands
    {
        static List<PlayerCommand> commands = new List<PlayerCommand>()
        {
            new BanCommand(),
            new KickCommand(),
            new BalanceCommand(),
            new CallAdminCommand(),
            new ClaimCommand(),
            new GimmeCommand(),
            new HelpCommand(),
            new ListTeleCommand(),
            new PayCommand(),
            new RemoveTeleCommand(),
            new RenameTeleCommand(),
            new SeenCommand(),
            new SetTeleCommand(),
            new ShopCommand(),
            new TeleCommand(),
            new TelePublicCommand(),
            new TelePrivateCommand(),
            new VoteCommand(),
            new WhoCommand(),
            new OutputGameObjectsCommand(),
            new OutputComponentsCommand(),
        };

        public static PlayerCommand GetCommand(string cmd)
        {
            foreach (PlayerCommand command in commands)
            {
                if (command.name.EqualsCaseInsensitive(cmd))
                    return command;

                if (command.aliases.ContainsCaseInsensitive(cmd))
                    return command;
            }

            throw new InvalidCommandException();
        }
    }

    [Command("ban", "cmdBan", "Permanently bans a player from the server.")]
    public class BanCommand : PlayerCommand
    {
        public override void Execute(ClientInfo sender, List<string> args)
        {
            if (args.Count < 1)
                throw new CommandArgsException();

            var user = args[0];

            GameUtils.KickPlayerData kickData = new GameUtils.KickPlayerData()
            {
                reason = GameUtils.EKickReason.Banned,
                banUntil = DateTime.MaxValue
            };

            if (args.Count > 1)
                kickData.customReason = args[1];

            GameUtils.KickPlayerForClientInfo(sender, kickData);
        }
    }

    [Command("kick", "cmdKick", "Forecfully disconnects a player from the server.")]
    public class KickCommand : PlayerCommand
    {
        public override void Execute(ClientInfo sender, List<string> args)
        {
            if (args.Count < 1)
                throw new CommandArgsException();

            var user = args[0];

            GameUtils.KickPlayerData kickData = new GameUtils.KickPlayerData()
            {
                reason = GameUtils.EKickReason.ManualKick
            };

            if (args.Count >= 2)
                kickData.customReason = args[1];

            GameUtils.KickPlayerForClientInfo(sender, kickData);
        }
    }

    [Command("balance", "cmdBalance", "See your current balance")]
    [CommandAliases("bal", "wallet")]
    public class BalanceCommand : PlayerCommand
    {
        public override void Execute(ClientInfo sender, List<string> args)
        {
            throw new NotImplementedException();
        }
    }

    [Command("calladmin", "cmdCallAdmin", "Make a support ticket")]
    [CommandAliases("admin", "admins", "support")]
    public class CallAdminCommand : PlayerCommand
    {
        public override void Execute(ClientInfo sender, List<string> args)
        {
            throw new NotImplementedException();
        }
    }

    [Command("claim", "cmdClaim", "Claim items you have bought in the shop")]
    [CommandAliases("claimitems")]
    public class ClaimCommand : PlayerCommand
    {
        public override void Execute(ClientInfo sender, List<string> args)
        {
            throw new NotImplementedException();
        }
    }

    [Command("gimme", "cmdGimme", "Get a random item, command or entity.")]
    [CommandAliases("gimmie")]
    public class GimmeCommand : PlayerCommand
    {
        public override void Execute(ClientInfo sender, List<string> args)
        {
            throw new NotImplementedException();
        }
    }

    [Command("help", "cmdHelp", "Get some help")]
    public class HelpCommand : PlayerCommand
    {
        public override void Execute(ClientInfo sender, List<string> args)
        {
            throw new NotImplementedException();
        }
    }

    [Command("listtele", "cmdListTele", "List teleport locations")]
    [CommandAliases("telelist", "teleslist", "listteles")]
    public class ListTeleCommand : PlayerCommand
    {
        public override void Execute(ClientInfo sender, List<string> args)
        {
            throw new NotImplementedException();
        }
    }

    [Command("pay", "cmdPay", "Send some currency to another player.")]
    public class PayCommand : PlayerCommand
    {
        public override void Execute(ClientInfo sender, List<string> args)
        {
            throw new NotImplementedException();
        }
    }

    [Command("removetele", "cmdRemoveTele", "Remove a teleport location")]
    [CommandAliases("deltele", "deletetele", "teledelete", "teleremove")]
    public class RemoveTeleCommand : PlayerCommand
    {
        public override void Execute(ClientInfo sender, List<string> args)
        {
            throw new NotImplementedException();
        }
    }

    [Command("renametele", "cmdRenameTele", "Rename a teleport location")]
    [CommandAliases("telerename")]
    public class RenameTeleCommand : PlayerCommand
    {
        public override void Execute(ClientInfo sender, List<string> args)
        {
            throw new NotImplementedException();
        }
    }

    [Command("seen", "cmdSeen", "Check when a player was last online")]
    [CommandAliases("lastseen", "lastonline")]
    public class SeenCommand : PlayerCommand
    {
        public override void Execute(ClientInfo sender, List<string> args)
        {
            throw new NotImplementedException();
        }
    }

    [Command("settele", "cmdSetTele", "Create a teleport location")]
    [CommandAliases("teleset", "telecreate", "createtele")]
    public class SetTeleCommand : PlayerCommand
    {
        public override void Execute(ClientInfo sender, List<string> args)
        {
            throw new NotImplementedException();
        }
    }

    [Command("shop", "cmdShop", "Ingame shop")]
    [CommandAliases("store")]
    public class ShopCommand : PlayerCommand
    {
        public override void Execute(ClientInfo sender, List<string> args)
        {
            throw new NotImplementedException();
        }
    }

    [Command("tele", "cmdTele", "Teleport to a set location.")]
    [CommandAliases("tp", "teleport")]
    public class TeleCommand : PlayerCommand
    {
        public override void Execute(ClientInfo sender, List<string> args)
        {
            throw new NotImplementedException();
        }
    }

    [Command("telepublic", "cmdTelePublic", "Make a teleport public")]
    [CommandAliases("telepub", "pubtele", "publictele")]
    public class TelePublicCommand : PlayerCommand
    {
        public override void Execute(ClientInfo sender, List<string> args)
        {
            throw new NotImplementedException();
        }
    }

    [Command("teleprivate", "cmdTelePrivate", "Make a teleport private")]
    [CommandAliases("privatetele", "privtele", "telepriv")]
    public class TelePrivateCommand : PlayerCommand
    {
        public override void Execute(ClientInfo sender, List<string> args)
        {
            throw new NotImplementedException();
        }
    }

    [Command("vote", "cmdVote", "Claim vote rewards")]
    public class VoteCommand : PlayerCommand
    {
        public override void Execute(ClientInfo sender, List<string> args)
        {
            throw new NotImplementedException();
        }
    }

    [Command("who", "cmdWho", "See who was in your area")]
    [CommandAliases("track", "search")]
    public class WhoCommand : PlayerCommand
    {
        public override void Execute(ClientInfo sender, List<string> args)
        {
            throw new NotImplementedException();
        }
    }

    [Command("outputgameobjects", "cmdOutputGameobjects", "Outputs the game objects to console and log.")]
    public class OutputGameObjectsCommand : PlayerCommand
    {
        public override void Execute(ClientInfo sender, List<string> args)
        {
            UnityUtility.OutputGameObjects();
        }
    }

    [Command("outputcomponents", "cmdOutputComponents", "Outputs the components attached to objects?")]
    public class OutputComponentsCommand : PlayerCommand
    {
        public override void Execute(ClientInfo sender, List<string> args)
        {
            if (args.Count < 1)
            {
                BeppyServer.Log("Need argument <gameobject>");
                return;
            }

            UnityUtility.OutputComponents(args[0]);
        }
    } 
}