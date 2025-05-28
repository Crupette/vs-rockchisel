using System.Linq;
using System.Security.Cryptography.X509Certificates;
using ToolModeLib;
using ToolModeLib.Content;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace RockChisel;
public class ItemRockChisel : Item
{
    public override float OnBlockBreaking(IPlayer player, BlockSelection blockSel, ItemSlot itemslot, float remainingResistance, float dt, int counter)
    {
        EntityAgent byEntity = player.Entity;
        if (byEntity.LeftHandItemSlot?.Itemstack?.Collectible?.Tool != EnumTool.Hammer && player?.WorldData.CurrentGameMode != EnumGameMode.Creative)
        {
            (api as ICoreClientAPI)?.TriggerIngameError(this, "nohammer", Lang.Get("Requires a hammer in the off hand"));
            return remainingResistance;
        }
        return base.OnBlockBreaking(player, blockSel, itemslot, remainingResistance, dt, counter);
    }
}

public abstract class ToolModeRockChisel : ToolMode
{
    public ToolModeRockChisel(AssetLocation group) : base(group)
    { }

    public override bool ShouldDisplay(CollectibleObject collObj, ItemSlot slot, IClientPlayer forPlayer, BlockSelection blockSel)
    {
        if (blockSel == null) return false;
        return true;
    }

    public override bool OnBlockBrokenWith(IWorldAccessor world, Entity byEntity, ItemSlot itemSlot, BlockSelection blockSel, float dropQuantityMultiplier, ref EnumHandling handling)
    {
        IPlayer byPlayer = null;
        if (byEntity is EntityPlayer entityPlayer) byPlayer = byEntity.World.PlayerByUid(entityPlayer.PlayerUID);

        Block block = blockSel.Block ?? world.BlockAccessor.GetBlock(blockSel.Position);
        if (block.Code.FirstCodePart() != "rock") return true;

        int damage = OnStoneBrokenWith(world, byEntity, itemSlot, blockSel, block);
        Item heldItem = itemSlot.Itemstack.Item;

        block.OnBlockBroken(world, blockSel.Position, byPlayer, 0);
        if (heldItem.DamagedBy != null && heldItem.DamagedBy.Contains(EnumItemDamageSource.BlockBreaking))
            heldItem.DamageItem(world, byEntity, itemSlot, damage);

        handling = EnumHandling.PreventSubsequent;
        return true;
    }

    public abstract int OnStoneBrokenWith(IWorldAccessor world, Entity byEntity, ItemSlot itemSlot, BlockSelection blockSel, Block block);
}

public class ToolModeRockChiselRelieve : ToolModeRockChisel
{
    SkillItem skillItem;

    public ToolModeRockChiselRelieve(AssetLocation group) : base(group)
    { }

    public override void OnLoaded(ICoreAPI api)
    {
        skillItem = new()
        {
            Code = Code,
            Name = Lang.Get("Relieve stone (full block)")
        };
        if (api is ICoreClientAPI capi)
        {
            skillItem = skillItem.WithIcon(capi, capi.Gui.LoadSvgWithPadding(new AssetLocation("rockchisel:textures/icons/rockchisel-relieve.svg"), 48, 48, 5, ColorUtil.WhiteArgb));
            skillItem.TexturePremultipliedAlpha = false;
        }
    }

    public override SkillItem GetDisplaySkillItem(ICoreAPI api)
    {
        return skillItem;
    }

    public override int OnStoneBrokenWith(IWorldAccessor world, Entity byEntity, ItemSlot itemSlot, BlockSelection blockSel, Block block)
    {
        world.SpawnItemEntity(new ItemStack(block), blockSel.Position.ToVec3d().Add(0.5, 0.5, 0.5), null);
        return 1;
    }
}

public class ToolModeRockChiselSmooth : ToolModeRockChisel
{
    SkillItem skillItem;

    public ToolModeRockChiselSmooth(AssetLocation group) : base(group)
    { }

    public override void OnLoaded(ICoreAPI api)
    {
        skillItem = new()
        {
            Code = Code,
            Name = Lang.Get("Polish stone (full block)")
        };
        if (api is ICoreClientAPI capi)
        {
            skillItem = skillItem.WithIcon(capi, capi.Gui.LoadSvgWithPadding(new AssetLocation("rockchisel:textures/icons/rockchisel-polish.svg"), 48, 48, 5, ColorUtil.WhiteArgb));
            skillItem.TexturePremultipliedAlpha = false;
        }
    }

    public override SkillItem GetDisplaySkillItem(ICoreAPI api)
    {
        return skillItem;
    }

    public override int OnStoneBrokenWith(IWorldAccessor world, Entity byEntity, ItemSlot itemSlot, BlockSelection blockSel, Block block)
    {
        string rockType = block.Variant["rock"];
        AssetLocation polishedRockCode = new AssetLocation("rockpolished").WithPathAppendix("-" + rockType);
        Block smoothRock = byEntity.World.GetBlock(polishedRockCode);
        if (smoothRock == null)
            return 1;

        world.SpawnItemEntity(new ItemStack(smoothRock), blockSel.Position.ToVec3d().Add(0.5, 0.5, 0.5), null);
        return 2;
    }
}

public class ToolModeRockChiselSplit : ToolModeRockChisel
{
    SkillItem skillItem;

    public ToolModeRockChiselSplit(AssetLocation group) : base(group)
    { }

    public override void OnLoaded(ICoreAPI api)
    {
        skillItem = new()
        {
            Code = Code,
            Name = Lang.Get("Split stone (polished slab)")
        };
        if (api is ICoreClientAPI capi)
        {
            skillItem = skillItem.WithIcon(capi, capi.Gui.LoadSvgWithPadding(new AssetLocation("rockchisel:textures/icons/rockchisel-split.svg"), 48, 48, 5, ColorUtil.WhiteArgb));
            skillItem.TexturePremultipliedAlpha = false;
        }
    }

    public override SkillItem GetDisplaySkillItem(ICoreAPI api)
    {
        return skillItem;
    }

    public override int OnStoneBrokenWith(IWorldAccessor world, Entity byEntity, ItemSlot itemSlot, BlockSelection blockSel, Block block)
    {
        string rockType = block.Variant["rock"];
        AssetLocation slabCode = new AssetLocation("polishedrockslab").WithPathAppendix("-" + rockType + "-down-free");
        Block slab = byEntity.World.GetBlock(slabCode);
        if (slab == null)
            return 1;

        for (int i = 0; i < 2; i++)
            world.SpawnItemEntity(new ItemStack(slab), blockSel.Position.ToVec3d().Add(0.5, 0.5, 0.5), null);
        return 3;
    }
}

public class ToolModeRockChiselBrick : ToolModeRockChisel
{
    SkillItem skillItem;

    public ToolModeRockChiselBrick(AssetLocation group) : base(group)
    { }

    public override void OnLoaded(ICoreAPI api)
    {
        skillItem = new()
        {
            Code = Code,
            Name = Lang.Get("Make bricks from stone")
        };
        if (api is ICoreClientAPI capi)
        {
            skillItem = skillItem.WithIcon(capi, capi.Gui.LoadSvgWithPadding(new AssetLocation("rockchisel:textures/icons/rockchisel-brick.svg"), 48, 48, 5, ColorUtil.WhiteArgb));
            skillItem.TexturePremultipliedAlpha = false;
        }
    }

    public override SkillItem GetDisplaySkillItem(ICoreAPI api)
    {
        return skillItem;
    }

    public override int OnStoneBrokenWith(IWorldAccessor world, Entity byEntity, ItemSlot itemSlot, BlockSelection blockSel, Block block)
    {
        string rockType = block.Variant["rock"];
        AssetLocation brickCode = new AssetLocation("stonebrick").WithPathAppendix("-" + rockType);
        Item brick = world.GetItem(brickCode);
        if (brick == null)
            return 1;
        
        for(int i = 0; i < 8; i++)
            world.SpawnItemEntity(new ItemStack(brick), blockSel.Position.ToVec3d().Add(0.5, 0.5, 0.5), null);
        return 5;
    }
}