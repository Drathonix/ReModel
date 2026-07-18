using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace ReModel.Client;

public class ReModelClient
{

    public static ICoreClientAPI capi;

    internal static bool DidSelect;

    public static void Init(ICoreClientAPI api)
    {
        capi = api;
    }
}
