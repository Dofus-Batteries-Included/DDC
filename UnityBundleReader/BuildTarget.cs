﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public enum BuildTarget
    {
        NoTarget = -2,
        AnyPlayer = -1,
        ValidPlayer = 1,
        StandaloneOSX = 2,
        StandaloneOsxppc = 3,
        StandaloneOSXIntel = 4,
        StandaloneWindows,
        WebPlayer,
        WebPlayerStreamed,
        Wii = 8,
        IOS = 9,
        PS3,
        Xbox360,
        Broadcom = 12,
        Android = 13,
        StandaloneGlesEmu = 14,
        StandaloneGles20Emu = 15,
        NaCl = 16,
        StandaloneLinux = 17,
        FlashPlayer = 18,
        StandaloneWindows64 = 19,
        WebGL,
        WSAPlayer,
        StandaloneLinux64 = 24,
        StandaloneLinuxUniversal,
        Wp8Player,
        StandaloneOSXIntel64,
        BlackBerry,
        Tizen,
        Psp2,
        PS4,
        Psm,
        XboxOne,
        SamsungTV,
        N3Ds,
        WiiU,
        TVOS,
        Switch,
        Lumin,
        Stadia,
        CloudRendering,
        GameCoreXboxSeries,
        GameCoreXboxOne,
        PS5,
        EmbeddedLinux,
        Qnx,
        UnknownPlatform = 9999
    }
}
