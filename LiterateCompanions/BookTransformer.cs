using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using System.Collections.Generic;
using System.Linq;

namespace LiterateCompanions;
internal static class BookTransformer
{
    private static readonly LocalizedString EmptyStr = new();
    private static BlueprintItemEquipmentUsable TomeOfClearThoughtPlus2;
    internal static void ConvertBooks()
    {
        TomeOfClearThoughtPlus2 = Utils.GetBlueprint<BlueprintItemEquipmentUsable>("3584c2a2f8b5b1b43ae11128f0ff1583");

        List<string> books = [
            "944bc8dce4e80a144b4434402f6d11fb", // Xanthir Vang's Notes
            "75b3f3672d8c4da7a1ca37a5bf5625e3", // "The Acts of Iomedae"
            "027188bc60a84c14adca15b58d91db34", // "The History of the Worldwound"
            "f244426222d6450a98dae742ad6c85c3", // "Baphomet the Uncaged"
            "7d0106b7c70a4630b474494be1c0ea75", // "Baphomet the Imprisoned"
            "d15d7384f3ddf804883c543badc776ab", // "Tome of the Minotaur: Sermons from the Labyrinth"
            "6d82798ebe6a4ec985ba8ecbd2fc802d", // "Crusade Chronicles"
            "95c3a3bee7de4538b20c4e159e3acf8c", // "Know Thy Enemy! A Crusader's Brief"
            "6e09b855929d419893eb2fbaeec80e51", // "Temples of Iomedae: Father Lorian's Guide for Neophytes"
            "c0697a86bc2442d0ae9a382caf8623b9", // "Terendelev, the Guardian of Kenabres"
            "c856cd988c844fadb8da723d8c1dc13e", // Guino Pollen, "Notes from My Travels in Northern Avistan"
            "fa1f67444ec844508ea2eb6549581d5d", // "The Keen Edge of Truth: Recollections and Admonitions of an Inquisitor of Asmodeus"
            "ec4184f7b7db4805b837fd314ee8dbce", // A Letter in Small Handwriting
            "9d62838089e4420689a1e230a8186258", // An Unfinished Draft of a Letter
            "e9fe24ad25c54b6b8f288ef822c549ce", // Diary of a Student of the Great Xanthir the Plagued One
            "194a0fc9a69f278499b2f81fbf9fd3ab", // Mutasafen's Journal
            "a40e092665744b8c801fb7fe4c402de7", // Laboratory Journal of Xanthir Vang, 4712
            "2669932ca67e47d9a97e6582db0301d5", // Xanthir Vang, "On the Nature of Demons"
            "de12840a4662481f937ff9542a6beb6b", // Zacharius, "Toward Eternity"
            "beaa7a8b2f5bfd8459042db7f81867d7", // "Grimoire of the Beast", Excerpts
        ];

        foreach (var book in books)
        {
            ConvertBook(book);
        }
        TomeOfClearThoughtPlus2 = null;
    }

    internal static void ConvertBook(string bookGuid)
    {
        try
        {
            var origBook = Utils.GetBlueprint<BlueprintItem>(bookGuid);
            var ability = CreateBookAbility(origBook);
            if (ability == null)
            {
                Main.log.Log($"Couldn't find bonus for book: {origBook.m_DisplayNameText}, guid: {bookGuid}");
                return;
            }
            Utils.ReplaceBlueprint<BlueprintItemEquipmentUsable, BlueprintItem>(bookGuid, (a, orig) =>
            {
                a.m_DisplayNameText = orig.m_DisplayNameText;
                a.m_DescriptionText = orig.m_DescriptionText;
                a.m_Ability = ability;
                a.m_FlavorText = orig.m_DescriptionText;
                a.m_NonIdentifiedNameText = EmptyStr;
                a.m_NonIdentifiedDescriptionText = EmptyStr;
                a.m_Icon = orig.m_Icon;
                a.m_Cost = orig.m_Cost;
                a.m_Weight = orig.m_Weight;
                a.m_IsNotable = false;
                a.m_Destructible = true;
                a.m_ShardItem = TomeOfClearThoughtPlus2.m_ShardItem;
                a.m_MiscellaneousType = TomeOfClearThoughtPlus2.m_MiscellaneousType;
                a.m_InventoryPutSound = TomeOfClearThoughtPlus2.m_InventoryPutSound;
                a.m_InventoryTakeSound = TomeOfClearThoughtPlus2.m_InventoryTakeSound;
                a.SpendCharges = true;
                a.Charges = 1;
                a.RestoreChargesOnRest = false;
                a.CasterLevel = 20;
                a.SpellLevel = 17;
                a.DC = 0;
                a.IsNonRemovable = false;
                a.m_EquipmentEntity = TomeOfClearThoughtPlus2.m_EquipmentEntity;
                a.m_EquipmentEntityAlternatives = TomeOfClearThoughtPlus2.m_EquipmentEntityAlternatives;
                a.m_ForcedRampColorPresetIndex = 0;
                a.Type = UsableItemType.Other;
                a.m_IdentifyDC = 0;
                a.m_InventoryEquipSound = TomeOfClearThoughtPlus2.m_InventoryEquipSound;
                a.m_BeltItemPrefab = TomeOfClearThoughtPlus2.m_BeltItemPrefab;
                a.m_Enchantments = TomeOfClearThoughtPlus2.m_Enchantments;
            });
            Main.log.Log($"Converted book: {origBook.m_DisplayNameText}, guid: {bookGuid}");
        }
        catch (System.Exception e)
        {
            Main.log.Error($"Exception when converting book guid: {bookGuid} : {e}");
        }
    }

    private static BlueprintAbilityReference CreateBookAbility(BlueprintItem book)
    {
        var comp = book.GetComponent<AddItemShowInfoCallback>();
        var addAction = comp.Action?.Actions.OfType<ContextActionAddFeature>().FirstOrDefault();
        if (addAction != null)
        {
            var feature = addAction.PermanentFeature;
            if (feature != null)
            {
                var ability = Utils.CreateBlueprint<BlueprintAbility>($"{book.name}Ability", bp =>
                {
                    bp.AddComponent<AbilityEffectRunAction>(c =>
                    {
                        c.Actions = new()
                        {
                            Actions =
                            [
                                new ContextActionAddFeature() {
                                    m_PermanentFeature = addAction.m_PermanentFeature
                                }
                            ]
                        };
                    });
                    bp.m_DisplayName = feature.m_DisplayName;
                    bp.m_Description = feature.m_Description;
                    bp.m_DescriptionShort = EmptyStr;
                    bp.m_Icon = book.m_Icon;
                    bp.Type = AbilityType.Special;
                    bp.Range = AbilityRange.Personal;
                    bp.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Potion;
                    bp.CanTargetEnemies = true;
                    bp.CanTargetFriends = true;
                    bp.CanTargetSelf = true;
                    bp.ActionType = Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard;
                    bp.LocalizedDuration = EmptyStr;
                    bp.LocalizedSavingThrow = EmptyStr;
                    bp.ResourceAssetIds = TomeOfClearThoughtPlus2.Ability.ResourceAssetIds;
                });
                return ability.ToReference<BlueprintAbilityReference>();
            }
        }
        return null;
    }
}
