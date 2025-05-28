using ToolModeLib;
using Vintagestory.API.Common;

namespace RockChisel {

    public class Core : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterItemClass("rockchisel.ItemRockChisel", typeof(ItemRockChisel));
            api.RegisterToolMode("rockchisel:relieve", typeof(ToolModeRockChiselRelieve));
            api.RegisterToolMode("rockchisel:smooth", typeof(ToolModeRockChiselSmooth));
            api.RegisterToolMode("rockchisel:split", typeof(ToolModeRockChiselSplit));
            api.RegisterToolMode("rockchisel:brick", typeof(ToolModeRockChiselBrick));
        }
    }
}