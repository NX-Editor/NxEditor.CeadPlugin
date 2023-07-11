﻿using CommunityToolkit.Mvvm.ComponentModel;
using ConfigFactory.Core;
using ConfigFactory.Core.Attributes;

namespace NxEditor.CeadPlugin;

public partial class CeadConfig : ConfigModule<CeadConfig>
{
    [ObservableProperty]
    [property: Config(
        Header = "RESTBL Strings",
        Description = "",
        Category = "EAD Plugin")]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFile,
        Title = "RESTBL Strings File",
        InstanceBrowserKey = "epdplugin-config-restble-strings")]
    private string _restblStrings = string.Empty;

    partial void OnRestblStringsChanged(string value)
    {
        SetValidation(() => RestblStrings,
            value => !string.IsNullOrEmpty(value));
    }
}
