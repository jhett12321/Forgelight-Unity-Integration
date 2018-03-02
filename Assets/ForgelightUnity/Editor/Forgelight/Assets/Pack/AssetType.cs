namespace ForgelightUnity.Editor.Forgelight.Assets.Pack
{
    public enum AssetType
    {
        ADR,    //Actor Definition - https://github.com/psemu/ps2ls/wiki/Adr
        CNK0,   //Terrain Data - https://github.com/psemu/ps2ls/wiki/CNK0-and-CNK1-to-CNKn
        CNK1,   //Terrain Data - https://github.com/psemu/ps2ls/wiki/CNK0-and-CNK1-to-CNKn
        CNK2,   //Terrain Data - https://github.com/psemu/ps2ls/wiki/CNK0-and-CNK1-to-CNKn
        CNK3,   //Terrain Data - https://github.com/psemu/ps2ls/wiki/CNK0-and-CNK1-to-CNKn
        CNK4,   //Terrain Data - https://github.com/psemu/ps2ls/wiki/CNK0-and-CNK1-to-CNKn
        CNK5,   //Terrain Data - https://github.com/psemu/ps2ls/wiki/CNK0-and-CNK1-to-CNKn
        DDS,    //Texture Format
        PNG,    //Image Format
        JPG,    //Image Format
        TGA,    //Image Format
        DMA,    //Material Definition - https://github.com/psemu/ps2ls/wiki/Dma
        DME,    //Mesh Data - https://github.com/psemu/ps2ls/wiki/Dme
        DMV,    //Mesh Data/Occlusion - https://github.com/psemu/ps2ls/wiki/Dmv
        ECO,    //Environment clutter, flora, etc. https://github.com/psemu/ps2ls/wiki/ECO
        FSB,    //FMod Sound Banks - https://github.com/psemu/ps2ls/wiki/FSB
        WAV,    //Audio Format
        FXO,    //Compiled DX Shaders.
        GFX,    //Scaleform - https://github.com/psemu/ps2ls/wiki/Gfx
        LST,    //Scaleform, referenced by GFX - https://github.com/psemu/ps2ls/wiki/Lst
        NSA,    //Morpheme animation file - https://github.com/psemu/ps2ls/wiki/Nsa
        TXT,    //Text file.
        INI,    //Text/Configuration File.
        XML,    //XML Document.
        ZONE,   //Object, Light and other placement data - https://github.com/psemu/ps2ls/wiki/Zone
        AGR,    //Model Group
        CDT,    //Collision (non-vehicle)
        CRC,    //Scaleform
        DSK,
        TOME,   //Occlusion
        DEF,    //File name is Color?
        FXD,
        AGS,
        APX,    //Collision (dynamic)
        MRN,    //Animations
        PSSB,   //Shader?
        PRSB,   //Shader?
        APB,    //Apex databases
        VNFO,   //Occlusion
        DB,
        XSSB,   //Shader?
        XRSB,   //Shader?
        PLAYERSTUDIO,
        CUR,    //Cursors
        GNF,    //Images?
        AMR,    //Model Related
        PSD,    //Photoshop?
        Unknown
    }
}