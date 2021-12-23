using System.Collections.Generic;
using Dalamud.Game.Gui;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace RaidBuffTracker.Utils
{
    public sealed class PartyListHUD
    {
        private readonly GameGui _gameGui;

        public PartyListHUD(GameGui gameGui)
        {
            _gameGui = gameGui;
        }

        public unsafe IEnumerable<uint> GetPartyObjectIDs()
        {
            var hud = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentHUD();
            var list = (HudPartyMember*)hud->PartyMemberList;

            var result = new List<uint>();
            for (var i = 0; i < hud->PartyMemberCount; i++)
            {
                result.Add(list[i].ObjectId);
            }

            return result;
        }
    }
}