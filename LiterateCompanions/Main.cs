using HarmonyLib;
using Kingmaker.Blueprints.JsonSystem;
using System;
using System.Reflection;
using UnityModManagerNet;

namespace LiterateCompanions;

public static class Main
{
    internal static Harmony HarmonyInstance;
    internal static Guid ModNamespaceGuid;
    internal static UnityModManager.ModEntry.ModLogger log;

    public static bool Load(UnityModManager.ModEntry modEntry)
    {
        log = modEntry.Logger;
        ModNamespaceGuid = NamespaceGuidUtils.CreateV5Guid(NamespaceGuidUtils.UrlNamespace, modEntry.Info.Id);
        HarmonyInstance = new Harmony(modEntry.Info.Id);
        HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        return true;
    }
}

[HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
static class AfterBlueprintInitPatcher
{
    [HarmonyPostfix]
    static void Postfix()
    {
        BookTransformer.ConvertBooks();
    }
}
