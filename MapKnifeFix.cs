using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace MapKnifeFix
{
    public partial class MapKnifeFix : BasePlugin
    {
        private readonly Dictionary<CCSPlayerController, DateTime> playerLastCommandTime = new Dictionary<CCSPlayerController, DateTime>();
        public override string ModuleAuthor => "TICHOJEBEC";
        public override string ModuleName => "Map Knife Fix";
        public override string ModuleVersion => "v1.0";

        public override void Load(bool hotReload)
        {
            RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
            AddCommandListener("jointeam", OnPlayerChangeTeam);
            AddCommandListener("knife", OnPlayerCommandKnife);
        }

        private static bool HasWeapon(CCSPlayerController player, string weaponName)
        {
            if (player == null || !player.IsValid || !player.PawnIsAlive) return false;
            var pawn = player.PlayerPawn.Value;
            return pawn?.WeaponServices?.MyWeapons
                .Any(weapon => weapon?.Value?.IsValid == true && weapon.Value.DesignerName?.Contains(weaponName) == true) ?? false;
        }

        private void EnsurePlayerHasKnife(CCSPlayerController? player)
        {
            if (player == null || !player.IsValid) return;

            if (!HasWeapon(player, "weapon_knife"))
            {
                AddTimer(0, () => player.GiveNamedItem("weapon_knife"));
            }
            player.PrintToChat($" {ChatColors.Lime}𝗖𝗦𝗞𝗢.𝗡𝗘𝗧 ● {ChatColors.Default}If you lost your knife, use {ChatColors.Red}!knife{ChatColors.Default} and you'll get a new one.");
        }

        private HookResult OnPlayerChangeTeam(CCSPlayerController? player, CommandInfo command)
        {
            if (IsInvalidPlayer(player)) return HookResult.Continue;
            EnsurePlayerHasKnife(player);
            return HookResult.Continue;
        }

        private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
        {
            CCSPlayerController? player = @event.Userid;
            if (IsInvalidPlayer(player)) return HookResult.Continue;
            EnsurePlayerHasKnife(player);
            return HookResult.Continue;
        }

        private HookResult OnPlayerCommandKnife(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null || !player.IsValid || player.IsBot || player.IsHLTV)
            {
                return HookResult.Continue;
            }

            if (playerLastCommandTime.TryGetValue(player, out DateTime lastCommandTime))
            {
                if ((DateTime.Now - lastCommandTime).TotalSeconds < 10)
                {
                    player.PrintToChat($" {ChatColors.Lime}𝗖𝗦𝗞𝗢.𝗡𝗘𝗧 ● {ChatColors.Default}You can only use this command once every {ChatColors.Red}10 {ChatColors.Default}seconds.");
                    return HookResult.Continue;
                }
            }

            playerLastCommandTime[player] = DateTime.Now;
            if (!HasWeapon(player, "weapon_knife"))
            {
                player.GiveNamedItem("weapon_knife");
            }
            else
            {
                player.PrintToChat($" {ChatColors.Lime}𝗖𝗦𝗞𝗢.𝗡𝗘𝗧 ● {ChatColors.Red}You already have a knife in hand!");
            }
            return HookResult.Continue;
        }

        private static bool IsInvalidPlayer(CCSPlayerController? player)
        {
            return player == null || !player.IsValid || player.IsBot || player.IsHLTV;
        }
    }
}
