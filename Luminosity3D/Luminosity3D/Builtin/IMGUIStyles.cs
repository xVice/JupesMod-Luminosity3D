﻿using ImGuiNET;
using System.Numerics;

namespace Luminosity3D.Builtin
{
    public static class IMGUIStyles
    {
        public static void SetupVisualStudioImGuiStyle()
        {
            // Rounded Visual Studio styleRedNicStone from ImThemes
            var style = ImGuiNET.ImGui.GetStyle();

            style.Alpha = 1.0f;
            style.WindowPadding = new Vector2(8.0f, 8.0f);
            style.WindowRounding = 4.0f;
            style.WindowBorderSize = 0.0f;
            style.WindowMinSize = new Vector2(32.0f, 32.0f);
            style.WindowTitleAlign = new Vector2(0.0f, 0.5f);
            style.WindowMenuButtonPosition = ImGuiDir.Left;
            style.ChildRounding = 0.0f;
            style.ChildBorderSize = 1.0f;
            style.PopupRounding = 4.0f;
            style.PopupBorderSize = 1.0f;
            style.FramePadding = new Vector2(4.0f, 3.0f);
            style.FrameRounding = 2.5f;
            style.FrameBorderSize = 0.0f;
            style.ItemSpacing = new Vector2(8.0f, 4.0f);
            style.ItemInnerSpacing = new Vector2(4.0f, 4.0f);
            style.IndentSpacing = 21.0f;
            style.ColumnsMinSpacing = 6.0f;
            style.ScrollbarSize = 11.0f;
            style.ScrollbarRounding = 2.5f;
            style.GrabMinSize = 10.0f;
            style.GrabRounding = 2.0f;
            style.TabRounding = 3.5f;
            style.TabBorderSize = 0.0f;
            style.ColorButtonPosition = ImGuiDir.Right;
            style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
            style.SelectableTextAlign = new Vector2(0.0f, 0.0f);

            style.Colors[(int)ImGuiCol.Text] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            style.Colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.5921568870544434f, 0.5921568870544434f, 0.5921568870544434f, 1.0f);
            style.Colors[(int)ImGuiCol.WindowBg] = new Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.ChildBg] = new Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.PopupBg] = new Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.Border] = new Vector4(0.3058823645114899f, 0.3058823645114899f, 0.3058823645114899f, 1.0f);
            style.Colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.3058823645114899f, 0.3058823645114899f, 0.3058823645114899f, 1.0f);
            style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.2000000029802322f, 0.2000000029802322f, 0.2156862765550613f, 1.0f);
            style.Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.1137254908680916f, 0.5921568870544434f, 0.9254902005195618f, 1.0f);
            style.Colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.0f, 0.4666666686534882f, 0.7843137383460999f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBg] = new Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.2000000029802322f, 0.2000000029802322f, 0.2156862765550613f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.2000000029802322f, 0.2000000029802322f, 0.2156862765550613f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.321568638086319f, 0.321568638086319f, 0.3333333432674408f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.3529411852359772f, 0.3529411852359772f, 0.3725490272045135f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.3529411852359772f, 0.3529411852359772f, 0.3725490272045135f, 1.0f);
            style.Colors[(int)ImGuiCol.CheckMark] = new Vector4(0.0f, 0.4666666686534882f, 0.7843137383460999f, 1.0f);
            style.Colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.1137254908680916f, 0.5921568870544434f, 0.9254902005195618f, 1.0f);
            style.Colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.0f, 0.4666666686534882f, 0.7843137383460999f, 1.0f);
            style.Colors[(int)ImGuiCol.Button] = new Vector4(0.2000000029802322f, 0.2000000029802322f, 0.2156862765550613f, 1.0f);
            style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.1137254908680916f, 0.5921568870544434f, 0.9254902005195618f, 1.0f);
            style.Colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.1137254908680916f, 0.5921568870544434f, 0.9254902005195618f, 1.0f);
            style.Colors[(int)ImGuiCol.Header] = new Vector4(0.2000000029802322f, 0.2000000029802322f, 0.2156862765550613f, 1.0f);
            style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.1137254908680916f, 0.5921568870544434f, 0.9254902005195618f, 1.0f);
            style.Colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.0f, 0.4666666686534882f, 0.7843137383460999f, 1.0f);
            style.Colors[(int)ImGuiCol.Separator] = new Vector4(0.3058823645114899f, 0.3058823645114899f, 0.3058823645114899f, 1.0f);
            style.Colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.3058823645114899f, 0.3058823645114899f, 0.3058823645114899f, 1.0f);
            style.Colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.3058823645114899f, 0.3058823645114899f, 0.3058823645114899f, 1.0f);
            style.Colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.2000000029802322f, 0.2000000029802322f, 0.2156862765550613f, 1.0f);
            style.Colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.321568638086319f, 0.321568638086319f, 0.3333333432674408f, 1.0f);
            style.Colors[(int)ImGuiCol.Tab] = new Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.TabHovered] = new Vector4(0.1137254908680916f, 0.5921568870544434f, 0.9254902005195618f, 1.0f);
            style.Colors[(int)ImGuiCol.TabActive] = new Vector4(0.0f, 0.4666666686534882f, 0.7843137383460999f, 1.0f);
            style.Colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.0f, 0.4666666686534882f, 0.7843137383460999f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotLines] = new Vector4(0.0f, 0.4666666686534882f, 0.7843137383460999f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(0.1137254908680916f, 0.5921568870544434f, 0.9254902005195618f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.0f, 0.4666666686534882f, 0.7843137383460999f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(0.1137254908680916f, 0.5921568870544434f, 0.9254902005195618f, 1.0f);
            style.Colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.0f, 0.4666666686534882f, 0.7843137383460999f, 1.0f);
            style.Colors[(int)ImGuiCol.DragDropTarget] = new Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.NavHighlight] = new Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.0f, 1.0f, 1.0f, 0.699999988079071f);
            style.Colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.800000011920929f, 0.800000011920929f, 0.800000011920929f, 0.2000000029802322f);
            style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
        }

        public static void SetupDarkStyle()
        {
            // Deep Dark stylejanekb04 from ImThemes
            var style = ImGuiNET.ImGui.GetStyle();

            style.Alpha = 1.0f;
            style.WindowPadding = new Vector2(8.0f, 8.0f);
            style.WindowRounding = 7.0f;
            style.WindowBorderSize = 1.0f;
            style.WindowMinSize = new Vector2(32.0f, 32.0f);
            style.WindowTitleAlign = new Vector2(0.0f, 0.5f);
            style.WindowMenuButtonPosition = ImGuiDir.Left;
            style.ChildRounding = 4.0f;
            style.ChildBorderSize = 1.0f;
            style.PopupRounding = 4.0f;
            style.PopupBorderSize = 1.0f;
            style.FramePadding = new Vector2(5.0f, 2.0f);
            style.FrameRounding = 3.0f;
            style.FrameBorderSize = 1.0f;
            style.ItemSpacing = new Vector2(6.0f, 6.0f);
            style.ItemInnerSpacing = new Vector2(6.0f, 6.0f);
            style.IndentSpacing = 25.0f;
            style.ColumnsMinSpacing = 6.0f;
            style.ScrollbarSize = 15.0f;
            style.ScrollbarRounding = 9.0f;
            style.GrabMinSize = 10.0f;
            style.GrabRounding = 3.0f;
            style.TabRounding = 4.0f;
            style.TabBorderSize = 1.0f;
            style.ColorButtonPosition = ImGuiDir.Right;
            style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
            style.SelectableTextAlign = new Vector2(0.0f, 0.0f);

            style.Colors[(int)ImGuiCol.Text] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            style.Colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.4980392158031464f, 0.4980392158031464f, 0.4980392158031464f, 1.0f);
            style.Colors[(int)ImGuiCol.WindowBg] = new Vector4(0.09803921729326248f, 0.09803921729326248f, 0.09803921729326248f, 1.0f);
            style.Colors[(int)ImGuiCol.ChildBg] = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            style.Colors[(int)ImGuiCol.PopupBg] = new Vector4(0.1882352977991104f, 0.1882352977991104f, 0.1882352977991104f, 0.9200000166893005f);
            style.Colors[(int)ImGuiCol.Border] = new Vector4(0.1882352977991104f, 0.1882352977991104f, 0.1882352977991104f, 0.2899999916553497f);
            style.Colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.0f, 0.0f, 0.0f, 0.239999994635582f);
            style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.0470588244497776f, 0.0470588244497776f, 0.0470588244497776f, 0.5400000214576721f);
            style.Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.1882352977991104f, 0.1882352977991104f, 0.1882352977991104f, 0.5400000214576721f);
            style.Colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.2000000029802322f, 0.2196078449487686f, 0.2274509817361832f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBg] = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.05882352963089943f, 0.05882352963089943f, 0.05882352963089943f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.1372549086809158f, 0.1372549086809158f, 0.1372549086809158f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.0470588244497776f, 0.0470588244497776f, 0.0470588244497776f, 0.5400000214576721f);
            style.Colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.3372549116611481f, 0.3372549116611481f, 0.3372549116611481f, 0.5400000214576721f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.4000000059604645f, 0.4000000059604645f, 0.4000000059604645f, 0.5400000214576721f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.5568627715110779f, 0.5568627715110779f, 0.5568627715110779f, 0.5400000214576721f);
            style.Colors[(int)ImGuiCol.CheckMark] = new Vector4(0.3294117748737335f, 0.6666666865348816f, 0.8588235378265381f, 1.0f);
            style.Colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.3372549116611481f, 0.3372549116611481f, 0.3372549116611481f, 0.5400000214576721f);
            style.Colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.5568627715110779f, 0.5568627715110779f, 0.5568627715110779f, 0.5400000214576721f);
            style.Colors[(int)ImGuiCol.Button] = new Vector4(0.0470588244497776f, 0.0470588244497776f, 0.0470588244497776f, 0.5400000214576721f);
            style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.1882352977991104f, 0.1882352977991104f, 0.1882352977991104f, 0.5400000214576721f);
            style.Colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.2000000029802322f, 0.2196078449487686f, 0.2274509817361832f, 1.0f);
            style.Colors[(int)ImGuiCol.Header] = new Vector4(0.0f, 0.0f, 0.0f, 0.5199999809265137f);
            style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.0f, 0.0f, 0.0f, 0.3600000143051147f);
            style.Colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.2000000029802322f, 0.2196078449487686f, 0.2274509817361832f, 0.3300000131130219f);
            style.Colors[(int)ImGuiCol.Separator] = new Vector4(0.2784313857555389f, 0.2784313857555389f, 0.2784313857555389f, 0.2899999916553497f);
            style.Colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.4392156898975372f, 0.4392156898975372f, 0.4392156898975372f, 0.2899999916553497f);
            style.Colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.4000000059604645f, 0.4392156898975372f, 0.4666666686534882f, 1.0f);
            style.Colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.2784313857555389f, 0.2784313857555389f, 0.2784313857555389f, 0.2899999916553497f);
            style.Colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.4392156898975372f, 0.4392156898975372f, 0.4392156898975372f, 0.2899999916553497f);
            style.Colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.4000000059604645f, 0.4392156898975372f, 0.4666666686534882f, 1.0f);
            style.Colors[(int)ImGuiCol.Tab] = new Vector4(0.0f, 0.0f, 0.0f, 0.5199999809265137f);
            style.Colors[(int)ImGuiCol.TabHovered] = new Vector4(0.1372549086809158f, 0.1372549086809158f, 0.1372549086809158f, 1.0f);
            style.Colors[(int)ImGuiCol.TabActive] = new Vector4(0.2000000029802322f, 0.2000000029802322f, 0.2000000029802322f, 0.3600000143051147f);
            style.Colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.0f, 0.0f, 0.0f, 0.5199999809265137f);
            style.Colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.1372549086809158f, 0.1372549086809158f, 0.1372549086809158f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotLines] = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotHistogram] = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);

            style.Colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.2000000029802322f, 0.2196078449487686f, 0.2274509817361832f, 1.0f);
            style.Colors[(int)ImGuiCol.DragDropTarget] = new Vector4(0.3294117748737335f, 0.6666666865348816f, 0.8588235378265381f, 1.0f);
            style.Colors[(int)ImGuiCol.NavHighlight] = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.0f, 0.0f, 0.0f, 0.699999988079071f);
            style.Colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(1.0f, 0.0f, 0.0f, 0.2000000029802322f);
            style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(1.0f, 0.0f, 0.0f, 0.3499999940395355f);
        }

        public static void SetupComfyImGuiStyle()
        {
            // Comfy styleGiuseppe from ImThemes
            var style = ImGuiNET.ImGui.GetStyle();

            style.Alpha = 1.0f;
            style.WindowPadding = new Vector2(8.0f, 8.0f);
            style.WindowRounding = 10.0f;
            style.WindowBorderSize = 0.0f;
            style.WindowMinSize = new Vector2(30.0f, 30.0f);
            style.WindowTitleAlign = new Vector2(0.5f, 0.5f);
            style.WindowMenuButtonPosition = ImGuiDir.Right;
            style.ChildRounding = 5.0f;
            style.ChildBorderSize = 1.0f;
            style.PopupRounding = 10.0f;
            style.PopupBorderSize = 0.0f;
            style.FramePadding = new Vector2(5.0f, 3.5f);
            style.FrameRounding = 5.0f;
            style.FrameBorderSize = 0.0f;
            style.ItemSpacing = new Vector2(5.0f, 4.0f);
            style.ItemInnerSpacing = new Vector2(5.0f, 5.0f);
            style.IndentSpacing = 5.0f;
            style.ColumnsMinSpacing = 5.0f;
            style.ScrollbarSize = 15.0f;
            style.ScrollbarRounding = 9.0f;
            style.GrabMinSize = 15.0f;
            style.GrabRounding = 5.0f;
            style.TabRounding = 5.0f;
            style.TabBorderSize = 0.0f;
            style.ColorButtonPosition = ImGuiDir.Right;
            style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
            style.SelectableTextAlign = new Vector2(0.0f, 0.0f);

            style.Colors[(int)ImGuiCol.Text] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            style.Colors[(int)ImGuiCol.TextDisabled] = new Vector4(1.0f, 1.0f, 1.0f, 0.3605149984359741f);
            style.Colors[(int)ImGuiCol.WindowBg] = new Vector4(0.09803921729326248f, 0.09803921729326248f, 0.09803921729326248f, 1.0f);
            style.Colors[(int)ImGuiCol.ChildBg] = new Vector4(1.0f, 0.0f, 0.0f, 0.0f);
            style.Colors[(int)ImGuiCol.PopupBg] = new Vector4(0.09803921729326248f, 0.09803921729326248f, 0.09803921729326248f, 1.0f);
            style.Colors[(int)ImGuiCol.Border] = new Vector4(0.4235294163227081f, 0.3803921639919281f, 0.572549045085907f, 0.54935622215271f);
            style.Colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.1568627506494522f, 0.1568627506494522f, 0.1568627506494522f, 1.0f);
            style.Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.3803921639919281f, 0.4235294163227081f, 0.572549045085907f, 0.5490196347236633f);
            style.Colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.6196078658103943f, 0.5764706134796143f, 0.7686274647712708f, 0.5490196347236633f);
            style.Colors[(int)ImGuiCol.TitleBg] = new Vector4(0.09803921729326248f, 0.09803921729326248f, 0.09803921729326248f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.09803921729326248f, 0.09803921729326248f, 0.09803921729326248f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.2588235437870026f, 0.2588235437870026f, 0.2588235437870026f, 0.0f);
            style.Colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            style.Colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.1568627506494522f, 0.1568627506494522f, 0.1568627506494522f, 0.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.1568627506494522f, 0.1568627506494522f, 0.1568627506494522f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.2352941185235977f, 0.2352941185235977f, 0.2352941185235977f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.294117659330368f, 0.294117659330368f, 0.294117659330368f, 1.0f);
            style.Colors[(int)ImGuiCol.CheckMark] = new Vector4(0.294117659330368f, 0.294117659330368f, 0.294117659330368f, 1.0f);
            style.Colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.6196078658103943f, 0.5764706134796143f, 0.7686274647712708f, 0.5490196347236633f);
            style.Colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.8156862854957581f, 0.772549033164978f, 0.9647058844566345f, 0.5490196347236633f);
            style.Colors[(int)ImGuiCol.Button] = new Vector4(0.6196078658103943f, 0.5764706134796143f, 0.7686274647712708f, 0.5490196347236633f);
            style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.7372549176216125f, 0.6941176652908325f, 0.886274516582489f, 0.5490196347236633f);
            style.Colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.8156862854957581f, 0.772549033164978f, 0.9647058844566345f, 0.5490196347236633f);
            style.Colors[(int)ImGuiCol.Header] = new Vector4(0.6196078658103943f, 0.5764706134796143f, 0.7686274647712708f, 0.5490196347236633f);
            style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.7372549176216125f, 0.6941176652908325f, 0.886274516582489f, 0.5490196347236633f);
            style.Colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.8156862854957581f, 0.772549033164978f, 0.9647058844566345f, 0.5490196347236633f);
            style.Colors[(int)ImGuiCol.Separator] = new Vector4(0.6196078658103943f, 0.5764706134796143f, 0.7686274647712708f, 0.5490196347236633f);
            style.Colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.7372549176216125f, 0.6941176652908325f, 0.886274516582489f, 0.5490196347236633f);
            style.Colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.8156862854957581f, 0.772549033164978f, 0.9647058844566345f, 0.5490196347236633f);
            style.Colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.6196078658103943f, 0.5764706134796143f, 0.7686274647712708f, 0.5490196347236633f);
            style.Colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.7372549176216125f, 0.6941176652908325f, 0.886274516582489f, 0.5490196347236633f);
            style.Colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.8156862854957581f, 0.772549033164978f, 0.9647058844566345f, 0.5490196347236633f);
            style.Colors[(int)ImGuiCol.Tab] = new Vector4(0.6196078658103943f, 0.5764706134796143f, 0.7686274647712708f, 0.5490196347236633f);
            style.Colors[(int)ImGuiCol.TabHovered] = new Vector4(0.7372549176216125f, 0.6941176652908325f, 0.886274516582489f, 0.5490196347236633f);
            style.Colors[(int)ImGuiCol.TabActive] = new Vector4(0.8156862854957581f, 0.772549033164978f, 0.9647058844566345f, 0.5490196347236633f);
            style.Colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.0f, 0.4509803950786591f, 1.0f, 0.0f);
            style.Colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.1333333402872086f, 0.2588235437870026f, 0.4235294163227081f, 0.0f);
            style.Colors[(int)ImGuiCol.PlotLines] = new Vector4(0.294117659330368f, 0.294117659330368f, 0.294117659330368f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(0.7372549176216125f, 0.6941176652908325f, 0.886274516582489f, 0.5490196347236633f);
            style.Colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.6196078658103943f, 0.5764706134796143f, 0.7686274647712708f, 0.5490196347236633f);
            style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(0.7372549176216125f, 0.6941176652908325f, 0.886274516582489f, 0.5490196347236633f);
            style.Colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.7372549176216125f, 0.6941176652908325f, 0.886274516582489f, 0.5490196347236633f);
            style.Colors[(int)ImGuiCol.DragDropTarget] = new Vector4(1.0f, 1.0f, 0.0f, 0.8999999761581421f);
            style.Colors[(int)ImGuiCol.NavHighlight] = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.0f, 1.0f, 1.0f, 0.699999988079071f);
            style.Colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.800000011920929f, 0.800000011920929f, 0.800000011920929f, 0.2000000029802322f);
            style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.800000011920929f, 0.800000011920929f, 0.800000011920929f, 0.3499999940395355f);
        }

        public static void SetupSteamImGuiStyle()
        {
            // Classic Steam stylemetasprite from ImThemes
            var style = ImGuiNET.ImGui.GetStyle();

            style.Alpha = 1.0f;
            style.WindowPadding = new Vector2(8.0f, 8.0f);
            style.WindowRounding = 0.0f;
            style.WindowBorderSize = 1.0f;
            style.WindowMinSize = new Vector2(32.0f, 32.0f);
            style.WindowTitleAlign = new Vector2(0.0f, 0.5f);
            style.WindowMenuButtonPosition = ImGuiDir.Left;
            style.ChildRounding = 0.0f;
            style.ChildBorderSize = 1.0f;
            style.PopupRounding = 0.0f;
            style.PopupBorderSize = 1.0f;
            style.FramePadding = new Vector2(4.0f, 3.0f);
            style.FrameRounding = 0.0f;
            style.FrameBorderSize = 1.0f;
            style.ItemSpacing = new Vector2(8.0f, 4.0f);
            style.ItemInnerSpacing = new Vector2(4.0f, 4.0f);
            style.IndentSpacing = 21.0f;
            style.ColumnsMinSpacing = 6.0f;
            style.ScrollbarSize = 14.0f;
            style.ScrollbarRounding = 0.0f;
            style.GrabMinSize = 10.0f;
            style.GrabRounding = 0.0f;
            style.TabRounding = 0.0f;
            style.TabBorderSize = 0.0f;
            style.ColorButtonPosition = ImGuiDir.Right;
            style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
            style.SelectableTextAlign = new Vector2(0.0f, 0.0f);

            style.Colors[(int)ImGuiCol.Text] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            style.Colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.4980392158031464f, 0.4980392158031464f, 0.4980392158031464f, 1.0f);
            style.Colors[(int)ImGuiCol.WindowBg] = new Vector4(0.2862745225429535f, 0.3372549116611481f, 0.2588235437870026f, 1.0f);
            style.Colors[(int)ImGuiCol.ChildBg] = new Vector4(0.2862745225429535f, 0.3372549116611481f, 0.2588235437870026f, 1.0f);
            style.Colors[(int)ImGuiCol.PopupBg] = new Vector4(0.239215686917305f, 0.2666666805744171f, 0.2000000029802322f, 1.0f);
            style.Colors[(int)ImGuiCol.Border] = new Vector4(0.5372549295425415f, 0.5686274766921997f, 0.5098039507865906f, 0.5f);
            style.Colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.1372549086809158f, 0.1568627506494522f, 0.1098039224743843f, 0.5199999809265137f);
            style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.239215686917305f, 0.2666666805744171f, 0.2000000029802322f, 1.0f);
            style.Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.2666666805744171f, 0.2980392277240753f, 0.2274509817361832f, 1.0f);
            style.Colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.2980392277240753f, 0.3372549116611481f, 0.2588235437870026f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBg] = new Vector4(0.239215686917305f, 0.2666666805744171f, 0.2000000029802322f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.2862745225429535f, 0.3372549116611481f, 0.2588235437870026f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.0f, 0.0f, 0.0f, 0.5099999904632568f);
            style.Colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.239215686917305f, 0.2666666805744171f, 0.2000000029802322f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.3490196168422699f, 0.4196078479290009f, 0.3098039329051971f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.2784313857555389f, 0.3176470696926117f, 0.239215686917305f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.2470588237047195f, 0.2980392277240753f, 0.2196078449487686f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.2274509817361832f, 0.2666666805744171f, 0.2078431397676468f, 1.0f);
            style.Colors[(int)ImGuiCol.CheckMark] = new Vector4(0.5882353186607361f, 0.5372549295425415f, 0.1764705926179886f, 1.0f);
            style.Colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.3490196168422699f, 0.4196078479290009f, 0.3098039329051971f, 1.0f);
            style.Colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.5372549295425415f, 0.5686274766921997f, 0.5098039507865906f, 0.5f);
            style.Colors[(int)ImGuiCol.Button] = new Vector4(0.2862745225429535f, 0.3372549116611481f, 0.2588235437870026f, 0.4000000059604645f);
            style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.3490196168422699f, 0.4196078479290009f, 0.3098039329051971f, 1.0f);
            style.Colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.5372549295425415f, 0.5686274766921997f, 0.5098039507865906f, 0.5f);
            style.Colors[(int)ImGuiCol.Header] = new Vector4(0.3490196168422699f, 0.4196078479290009f, 0.3098039329051971f, 1.0f);
            style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.3490196168422699f, 0.4196078479290009f, 0.3098039329051971f, 0.6000000238418579f);
            style.Colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.5372549295425415f, 0.5686274766921997f, 0.5098039507865906f, 0.5f);
            style.Colors[(int)ImGuiCol.Separator] = new Vector4(0.1372549086809158f, 0.1568627506494522f, 0.1098039224743843f, 1.0f);
            style.Colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.5372549295425415f, 0.5686274766921997f, 0.5098039507865906f, 1.0f);
            style.Colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.5882353186607361f, 0.5372549295425415f, 0.1764705926179886f, 1.0f);
            style.Colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.1882352977991104f, 0.2274509817361832f, 0.1764705926179886f, 0.0f);
            style.Colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.5372549295425415f, 0.5686274766921997f, 0.5098039507865906f, 1.0f);
            style.Colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.5882353186607361f, 0.5372549295425415f, 0.1764705926179886f, 1.0f);
            style.Colors[(int)ImGuiCol.Tab] = new Vector4(0.3490196168422699f, 0.4196078479290009f, 0.3098039329051971f, 1.0f);
            style.Colors[(int)ImGuiCol.TabHovered] = new Vector4(0.5372549295425415f, 0.5686274766921997f, 0.5098039507865906f, 0.7799999713897705f);
            style.Colors[(int)ImGuiCol.TabActive] = new Vector4(0.5882353186607361f, 0.5372549295425415f, 0.1764705926179886f, 1.0f);
            style.Colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.239215686917305f, 0.2666666805744171f, 0.2000000029802322f, 1.0f);
            style.Colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.3490196168422699f, 0.4196078479290009f, 0.3098039329051971f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotLines] = new Vector4(0.6078431606292725f, 0.6078431606292725f, 0.6078431606292725f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(0.5882353186607361f, 0.5372549295425415f, 0.1764705926179886f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotHistogram] = new Vector4(1.0f, 0.7764706015586853f, 0.2784313857555389f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1.0f, 0.6000000238418579f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.5882353186607361f, 0.5372549295425415f, 0.1764705926179886f, 1.0f);
            style.Colors[(int)ImGuiCol.DragDropTarget] = new Vector4(0.729411780834198f, 0.6666666865348816f, 0.239215686917305f, 1.0f);
            style.Colors[(int)ImGuiCol.NavHighlight] = new Vector4(0.5882353186607361f, 0.5372549295425415f, 0.1764705926179886f, 1.0f);
            style.Colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.0f, 1.0f, 1.0f, 0.699999988079071f);
            style.Colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.800000011920929f, 0.800000011920929f, 0.800000011920929f, 0.2000000029802322f);
            style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.800000011920929f, 0.800000011920929f, 0.800000011920929f, 0.3499999940395355f);
        }

        public static void SetupImGuiStyle()
        {
            // Darcula styleice1000 from ImThemes
            var style = ImGuiNET.ImGui.GetStyle();

            style.Alpha = 1.0f;
            style.WindowPadding = new Vector2(8.0f, 8.0f);
            style.WindowRounding = 5.300000190734863f;
            style.WindowBorderSize = 1.0f;
            style.WindowMinSize = new Vector2(32.0f, 32.0f);
            style.WindowTitleAlign = new Vector2(0.0f, 0.5f);
            style.WindowMenuButtonPosition = ImGuiDir.Left;
            style.ChildRounding = 0.0f;
            style.ChildBorderSize = 1.0f;
            style.PopupRounding = 0.0f;
            style.PopupBorderSize = 1.0f;
            style.FramePadding = new Vector2(4.0f, 3.0f);
            style.FrameRounding = 2.299999952316284f;
            style.FrameBorderSize = 1.0f;
            style.ItemSpacing = new Vector2(8.0f, 6.5f);
            style.ItemInnerSpacing = new Vector2(4.0f, 4.0f);
            style.IndentSpacing = 21.0f;
            style.ColumnsMinSpacing = 6.0f;
            style.ScrollbarSize = 14.0f;
            style.ScrollbarRounding = 5.0f;
            style.GrabMinSize = 10.0f;
            style.GrabRounding = 2.299999952316284f;
            style.TabRounding = 4.0f;
            style.TabBorderSize = 0.0f;
            style.ColorButtonPosition = ImGuiDir.Right;
            style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
            style.SelectableTextAlign = new Vector2(0.0f, 0.0f);

            style.Colors[(int)ImGuiCol.Text] = new Vector4(0.7333333492279053f, 0.7333333492279053f, 0.7333333492279053f, 1.0f);
            style.Colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.3450980484485626f, 0.3450980484485626f, 0.3450980484485626f, 1.0f);
            style.Colors[(int)ImGuiCol.WindowBg] = new Vector4(0.2352941185235977f, 0.2470588237047195f, 0.2549019753932953f, 0.9399999976158142f);
            style.Colors[(int)ImGuiCol.ChildBg] = new Vector4(0.2352941185235977f, 0.2470588237047195f, 0.2549019753932953f, 0.0f);
            style.Colors[(int)ImGuiCol.PopupBg] = new Vector4(0.2352941185235977f, 0.2470588237047195f, 0.2549019753932953f, 0.9399999976158142f);
            style.Colors[(int)ImGuiCol.Border] = new Vector4(0.3333333432674408f, 0.3333333432674408f, 0.3333333432674408f, 0.5f);
            style.Colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.1568627506494522f, 0.1568627506494522f, 0.1568627506494522f, 0.0f);
            style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.168627455830574f, 0.168627455830574f, 0.168627455830574f, 0.5400000214576721f);
            style.Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.4509803950786591f, 0.6745098233222961f, 0.9960784316062927f, 0.6700000166893005f);
            style.Colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.4705882370471954f, 0.4705882370471954f, 0.4705882370471954f, 0.6700000166893005f);
            style.Colors[(int)ImGuiCol.TitleBg] = new Vector4(0.03921568766236305f, 0.03921568766236305f, 0.03921568766236305f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.0f, 0.0f, 0.0f, 0.5099999904632568f);
            style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.1568627506494522f, 0.2862745225429535f, 0.47843137383461f, 1.0f);
            style.Colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.2705882489681244f, 0.2862745225429535f, 0.2901960909366608f, 0.800000011920929f);
            style.Colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.2705882489681244f, 0.2862745225429535f, 0.2901960909366608f, 0.6000000238418579f);
            style.Colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.2196078449487686f, 0.3098039329051971f, 0.4196078479290009f, 0.5099999904632568f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.2196078449487686f, 0.3098039329051971f, 0.4196078479290009f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.1372549086809158f, 0.1921568661928177f, 0.2627451121807098f, 0.9100000262260437f);
            style.Colors[(int)ImGuiCol.CheckMark] = new Vector4(0.8980392217636108f, 0.8980392217636108f, 0.8980392217636108f, 0.8299999833106995f);
            style.Colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.6980392336845398f, 0.6980392336845398f, 0.6980392336845398f, 0.6200000047683716f);
            style.Colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.2980392277240753f, 0.2980392277240753f, 0.2980392277240753f, 0.8399999737739563f);
            style.Colors[(int)ImGuiCol.Button] = new Vector4(0.3333333432674408f, 0.3529411852359772f, 0.3607843220233917f, 0.4900000095367432f);
            style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.2196078449487686f, 0.3098039329051971f, 0.4196078479290009f, 1.0f);
            style.Colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.1372549086809158f, 0.1921568661928177f, 0.2627451121807098f, 1.0f);
            style.Colors[(int)ImGuiCol.Header] = new Vector4(0.3333333432674408f, 0.3529411852359772f, 0.3607843220233917f, 0.5299999713897705f);
            style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.4509803950786591f, 0.6745098233222961f, 0.9960784316062927f, 0.6700000166893005f);
            style.Colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.4705882370471954f, 0.4705882370471954f, 0.4705882370471954f, 0.6700000166893005f);
            style.Colors[(int)ImGuiCol.Separator] = new Vector4(0.3137255012989044f, 0.3137255012989044f, 0.3137255012989044f, 1.0f);
            style.Colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.3137255012989044f, 0.3137255012989044f, 0.3137255012989044f, 1.0f);
            style.Colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.3137255012989044f, 0.3137255012989044f, 0.3137255012989044f, 1.0f);
            style.Colors[(int)ImGuiCol.ResizeGrip] = new Vector4(1.0f, 1.0f, 1.0f, 0.8500000238418579f);
            style.Colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(1.0f, 1.0f, 1.0f, 0.6000000238418579f);
            style.Colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(1.0f, 1.0f, 1.0f, 0.8999999761581421f);
            style.Colors[(int)ImGuiCol.Tab] = new Vector4(0.1764705926179886f, 0.3490196168422699f, 0.5764706134796143f, 0.8619999885559082f);
            style.Colors[(int)ImGuiCol.TabHovered] = new Vector4(0.2588235437870026f, 0.5882353186607361f, 0.9764705896377563f, 0.800000011920929f);
            style.Colors[(int)ImGuiCol.TabActive] = new Vector4(0.196078434586525f, 0.407843142747879f, 0.6784313917160034f, 1.0f);
            style.Colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.06666667014360428f, 0.1019607856869698f, 0.1450980454683304f, 0.9724000096321106f);
            style.Colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.1333333402872086f, 0.2588235437870026f, 0.4235294163227081f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotLines] = new Vector4(0.6078431606292725f, 0.6078431606292725f, 0.6078431606292725f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(1.0f, 0.4274509847164154f, 0.3490196168422699f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.8980392217636108f, 0.6980392336845398f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1.0f, 0.6000000238418579f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.1843137294054031f, 0.3960784375667572f, 0.7921568751335144f, 0.8999999761581421f);
            style.Colors[(int)ImGuiCol.DragDropTarget] = new Vector4(1.0f, 1.0f, 0.0f, 0.8999999761581421f);
            style.Colors[(int)ImGuiCol.NavHighlight] = new Vector4(0.2588235437870026f, 0.5882353186607361f, 0.9764705896377563f, 1.0f);
            style.Colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.0f, 1.0f, 1.0f, 0.699999988079071f);
            style.Colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.800000011920929f, 0.800000011920929f, 0.800000011920929f, 0.2000000029802322f);
            style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.800000011920929f, 0.800000011920929f, 0.800000011920929f, 0.3499999940395355f);
        }
    }
}
