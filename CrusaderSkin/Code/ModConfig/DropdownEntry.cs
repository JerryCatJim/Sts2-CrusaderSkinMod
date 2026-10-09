using System.Collections.Generic;

namespace CrusaderSkin.Code.ModConfig;

public struct DropdownEntry
{
    public string id { get; set; } = null;

    public string type { get; } = "choice";

    public string presentation { get; set; } = "dropdown";

    public string key { get; set; } = null;

    public string label { get; set; } = null;

    public string defaultValue { get; set; } = "Original";

    public string selected { get; set; } = null;  //当前选中的

    public Dictionary<string, string> options { get; set; }  //<value, label>

    public RLMCScope scope { get; set; } = RLMCScope.global;

    public DropdownEntry()
    {
    }
}
