using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace MapKnifeFix
{
    public class MapKnifeFix : BasePlugin
    {
        private readonly Dictionary<CCSPlayerController, DateTime> _playerLastCommandTime = new();
        private static readonly string ChatPrefix = $"{ChatColors.Lime}𝗖𝗦𝗞𝗢.𝗡𝗘𝗧 ● {ChatColors.Default}";
        private const int CommandCooldownSeconds = 10;

        public override string ModuleAuthor => "TICHOJEBEC";
        public override string ModuleName => "Map Knife Fix";
        public override string ModuleVersion => "v1.1";

        public override void Load(bool hotReload)
        {
            RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
            RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
            AddCommandListener("jointeam", OnPlayerChangeTeam);
            AddCommandListener("knife", OnPlayerCommandKnife);

            if (hotReload)
            {
                _playerLastCommandTime.Clear();
            }
        }

        private static bool HasWeapon(CCSPlayerController? player, string weaponName)
        {
            if (player == null || !player.IsValid || !player.PawnIsAlive) return false;
            var pawn = player.PlayerPawn.Value;
            return pawn?.WeaponServices?.MyWeapons
                .Any(weapon => weapon.Value?.IsValid == true && weapon.Value.DesignerName.Contains(weaponName)) ?? false;
        }

        private void EnsurePlayerHasKnife(CCSPlayerController? player)
        {
            if (IsInvalidPlayer(player)) return;

            if (HasWeapon(player, "weapon_knife")) return;
            player!.GiveNamedItem("weapon_knife");
            player.PrintToChat($"{ChatPrefix}If you lost your knife, use {ChatColors.Red}!knife{ChatColors.Default} to get a new one.");
        }

        private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
        {
            EnsurePlayerHasKnife(@event.Userid);
            return HookResult.Continue;
        }

        private HookResult OnPlayerChangeTeam(CCSPlayerController? player, CommandInfo command)
        {
            EnsurePlayerHasKnife(player);
            return HookResult.Continue;
        }

        private HookResult OnPlayerCommandKnife(CCSPlayerController? player, CommandInfo command)
        {
            if (IsInvalidPlayer(player)) return HookResult.Continue;

            var nonNullPlayer = player!;

            if (_playerLastCommandTime.TryGetValue(nonNullPlayer, out var lastCommandTime) &&
                (DateTime.Now - lastCommandTime).TotalSeconds < CommandCooldownSeconds)
            {
                nonNullPlayer.PrintToChat($"{ChatPrefix}You can only use this command once every {ChatColors.Red}{CommandCooldownSeconds} {ChatColors.Default}seconds.");
                return HookResult.Continue;
            }

            _playerLastCommandTime[nonNullPlayer] = DateTime.Now;
            if (!HasWeapon(nonNullPlayer, "weapon_knife"))
            {
                nonNullPlayer.GiveNamedItem("weapon_knife");
            }
            else
            {
                nonNullPlayer.PrintToChat($"{ChatPrefix}{ChatColors.Red}You already have a knife!");
            }
            return HookResult.Continue;
        }

        private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
        {
            if (@event.Userid != null)
            {
                _playerLastCommandTime.Remove(@event.Userid);
            }
            return HookResult.Continue;
        }

        private static bool IsInvalidPlayer(CCSPlayerController? player)
        {
            return player == null || !player.IsValid || player.IsBot || player.IsHLTV;
        }
    }
}