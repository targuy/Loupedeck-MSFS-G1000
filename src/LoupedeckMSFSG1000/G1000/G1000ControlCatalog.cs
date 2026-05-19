namespace LoupedeckMSFSG1000.G1000;

using Loupedeck;

public static class G1000ControlCatalog
{
    public static IReadOnlyList<G1000CommandDefinition> Commands { get; } =
    new G1000CommandDefinition[]
    {
        new("fixed.directto", "D->", H("AS1000_PFD_DIRECTTO"), G1000ControlPage.Fixed),
        new("fixed.menu", "MENU", H("AS1000_PFD_MENU_Push"), G1000ControlPage.Fixed),
        new("fixed.fpl", "FPL", H("AS1000_PFD_FPL_Push"), G1000ControlPage.Fixed),
        new("fixed.proc", "PROC", H("AS1000_PFD_PROC_Push"), G1000ControlPage.Fixed),
        new("fixed.clr", "CLR", H("AS1000_PFD_CLR"), G1000ControlPage.Fixed),
        new("fixed.ent", "ENT", H("AS1000_PFD_ENT_Push"), G1000ControlPage.Fixed),

        new("ap.fd", "FD", K("TOGGLE_FLIGHT_DIRECTOR"), G1000ControlPage.Autopilot, "ap.fd"),
        new("ap.hdg", "HDG", K("AP_HDG_HOLD"), G1000ControlPage.Autopilot, "ap.hdg"),
        new("ap.nav", "NAV", K("AP_NAV1_HOLD"), G1000ControlPage.Autopilot, "ap.nav"),
        new("ap.alt", "ALT", K("AP_ALT_HOLD"), G1000ControlPage.Autopilot, "ap.alt"),
        new("ap.vs", "VS", K("AP_VS_HOLD"), G1000ControlPage.Autopilot, "ap.vs"),
        new("ap.flc", "FLC", K("FLIGHT_LEVEL_CHANGE"), G1000ControlPage.Autopilot, "ap.flc"),
        new("ap.vnv", "VNV", K("AP_VNV_TOGGLE"), G1000ControlPage.Autopilot, "ap.vnv"),
        new("ap.apr", "APR", K("AP_APR_HOLD"), G1000ControlPage.Autopilot, "ap.apr"),
        new("ap.bc", "BC", K("AP_BC_HOLD"), G1000ControlPage.Autopilot, "ap.bc"),

        new("comnav.com1.swap", "COM1 Swap", H("AS1000_PFD_COM_Radio_1_PUSH"), G1000ControlPage.ComNav),
        new("comnav.com2.swap", "COM2 Swap", H("AS1000_PFD_COM_Radio_2_PUSH"), G1000ControlPage.ComNav),
        new("comnav.nav1.swap", "NAV1 Swap", H("AS1000_PFD_NAV_Radio_1_PUSH"), G1000ControlPage.ComNav),
        new("comnav.nav2.swap", "NAV2 Swap", H("AS1000_PFD_NAV_Radio_2_PUSH"), G1000ControlPage.ComNav),
    }
    .Concat(CreateSoftkeys("pfd.softkey", "PFD SK", "AS1000_PFD_SOFTKEYS_", G1000ControlPage.Pfd))
    .Concat(CreateSoftkeys("mfd.softkey", "MFD SK", "AS1000_MFD_SOFTKEYS_", G1000ControlPage.Mfd))
    .ToArray();

    public static IReadOnlyList<G1000AdjustmentDefinition> Adjustments { get; } =
    new G1000AdjustmentDefinition[]
    {
        new("pfd.baro", "PFD BARO", H("AS1000_PFD_BARO_INC"), H("AS1000_PFD_BARO_DEC"), G1000ControlPage.Pfd, ResetCode: H("AS1000_PFD_BARO_PUSH")),
        new("pfd.hdg", "PFD HDG", K("HEADING_BUG_INC"), K("HEADING_BUG_DEC"), G1000ControlPage.Pfd, "bug.hdg", K("HEADING_BUG_SET")),
        new("pfd.crs", "PFD CRS", K("VOR1_OBI_INC"), K("VOR1_OBI_DEC"), G1000ControlPage.Pfd, "crs1"),
        new("pfd.nav1.whole", "NAV1 MHz", H("AS1000_PFD_NAV_Radio_1_Whole_INC"), H("AS1000_PFD_NAV_Radio_1_Whole_DEC"), G1000ControlPage.Pfd, "nav1.stby"),
        new("pfd.nav1.fract", "NAV1 kHz", H("AS1000_PFD_NAV_Radio_1_Fract_INC"), H("AS1000_PFD_NAV_Radio_1_Fract_DEC"), G1000ControlPage.Pfd, "nav1.stby"),
        new("pfd.com1.whole", "COM1 MHz", H("AS1000_PFD_COM_Radio_1_Whole_INC"), H("AS1000_PFD_COM_Radio_1_Whole_DEC"), G1000ControlPage.Pfd, "com1.stby"),
        new("pfd.com1.fract", "COM1 kHz", H("AS1000_PFD_COM_Radio_1_Fract_INC"), H("AS1000_PFD_COM_Radio_1_Fract_DEC"), G1000ControlPage.Pfd, "com1.stby"),
        new("pfd.range", "PFD Range", H("AS1000_MFD_RANGE_INC"), H("AS1000_MFD_RANGE_DEC"), G1000ControlPage.Pfd),

        new("mfd.fms.outer", "MFD FMS Out", H("AS1000_MFD_FMS_Upper_INC"), H("AS1000_MFD_FMS_Upper_DEC"), G1000ControlPage.Mfd),
        new("mfd.fms.inner", "MFD FMS In", H("AS1000_MFD_FMS_Lower_INC"), H("AS1000_MFD_FMS_Lower_DEC"), G1000ControlPage.Mfd),
        new("mfd.nav2.whole", "NAV2 MHz", H("AS1000_PFD_NAV_Radio_2_Whole_INC"), H("AS1000_PFD_NAV_Radio_2_Whole_DEC"), G1000ControlPage.Mfd, "nav2.stby"),
        new("mfd.nav2.fract", "NAV2 kHz", H("AS1000_PFD_NAV_Radio_2_Fract_INC"), H("AS1000_PFD_NAV_Radio_2_Fract_DEC"), G1000ControlPage.Mfd, "nav2.stby"),
        new("mfd.com2.whole", "COM2 MHz", H("AS1000_PFD_COM_Radio_2_Whole_INC"), H("AS1000_PFD_COM_Radio_2_Whole_DEC"), G1000ControlPage.Mfd, "com2.stby"),
        new("mfd.com2.fract", "COM2 kHz", H("AS1000_PFD_COM_Radio_2_Fract_INC"), H("AS1000_PFD_COM_Radio_2_Fract_DEC"), G1000ControlPage.Mfd, "com2.stby"),
        new("mfd.alt", "ALT Sel", K("AP_ALT_VAR_INC"), K("AP_ALT_VAR_DEC"), G1000ControlPage.Mfd, "bug.alt"),
        new("mfd.vs", "VS Sel", K("AP_VS_VAR_INC"), K("AP_VS_VAR_DEC"), G1000ControlPage.Mfd, "bug.vs"),

        new("ap.hdg", "AP HDG", K("HEADING_BUG_INC"), K("HEADING_BUG_DEC"), G1000ControlPage.Autopilot, "bug.hdg"),
        new("ap.alt.100", "AP ALT 100", K("AP_ALT_VAR_INC"), K("AP_ALT_VAR_DEC"), G1000ControlPage.Autopilot, "bug.alt"),
        new("ap.alt.1000", "AP ALT 1000", K("AP_ALT_VAR_INC"), K("AP_ALT_VAR_DEC"), G1000ControlPage.Autopilot, "bug.alt"),
        new("ap.vs", "AP VS", K("AP_VS_VAR_INC"), K("AP_VS_VAR_DEC"), G1000ControlPage.Autopilot, "bug.vs"),
        new("ap.flc.speed", "FLC Speed", K("AP_SPD_VAR_INC"), K("AP_SPD_VAR_DEC"), G1000ControlPage.Autopilot, "bug.ias"),
        new("ap.course", "AP CRS", K("VOR1_OBI_INC"), K("VOR1_OBI_DEC"), G1000ControlPage.Autopilot, "crs1"),
        new("ap.baro", "AP BARO", H("AS1000_PFD_BARO_INC"), H("AS1000_PFD_BARO_DEC"), G1000ControlPage.Autopilot),

        new("comnav.com1.whole", "COM1 MHz", H("AS1000_PFD_COM_Radio_1_Whole_INC"), H("AS1000_PFD_COM_Radio_1_Whole_DEC"), G1000ControlPage.ComNav, "com1.stby"),
        new("comnav.com1.fract", "COM1 kHz", H("AS1000_PFD_COM_Radio_1_Fract_INC"), H("AS1000_PFD_COM_Radio_1_Fract_DEC"), G1000ControlPage.ComNav, "com1.stby"),
        new("comnav.nav1.whole", "NAV1 MHz", H("AS1000_PFD_NAV_Radio_1_Whole_INC"), H("AS1000_PFD_NAV_Radio_1_Whole_DEC"), G1000ControlPage.ComNav, "nav1.stby"),
        new("comnav.nav1.fract", "NAV1 kHz", H("AS1000_PFD_NAV_Radio_1_Fract_INC"), H("AS1000_PFD_NAV_Radio_1_Fract_DEC"), G1000ControlPage.ComNav, "nav1.stby"),
        new("comnav.com2.whole", "COM2 MHz", H("AS1000_PFD_COM_Radio_2_Whole_INC"), H("AS1000_PFD_COM_Radio_2_Whole_DEC"), G1000ControlPage.ComNav, "com2.stby"),
        new("comnav.com2.fract", "COM2 kHz", H("AS1000_PFD_COM_Radio_2_Fract_INC"), H("AS1000_PFD_COM_Radio_2_Fract_DEC"), G1000ControlPage.ComNav, "com2.stby"),
        new("comnav.nav2.whole", "NAV2 MHz", H("AS1000_PFD_NAV_Radio_2_Whole_INC"), H("AS1000_PFD_NAV_Radio_2_Whole_DEC"), G1000ControlPage.ComNav, "nav2.stby"),
        new("comnav.nav2.fract", "NAV2 kHz", H("AS1000_PFD_NAV_Radio_2_Fract_INC"), H("AS1000_PFD_NAV_Radio_2_Fract_DEC"), G1000ControlPage.ComNav, "nav2.stby"),
    };

    public static G1000CommandDefinition? FindCommand(String id) =>
        Commands.FirstOrDefault(command => command.Id == id);

    public static G1000AdjustmentDefinition? FindAdjustment(String id) =>
        Adjustments.FirstOrDefault(adjustment => adjustment.Id == id);

    public static BitmapColor PageColor(G1000ControlPage page) => page switch
    {
        G1000ControlPage.Pfd => new BitmapColor(0, 85, 255),
        G1000ControlPage.Mfd => new BitmapColor(0, 204, 68),
        G1000ControlPage.Autopilot => new BitmapColor(255, 179, 0),
        G1000ControlPage.ComNav => new BitmapColor(0, 204, 255),
        _ => new BitmapColor(42, 47, 55),
    };

    private static IEnumerable<G1000CommandDefinition> CreateSoftkeys(String idPrefix, String labelPrefix, String eventPrefix, G1000ControlPage page)
    {
        for (var i = 1; i <= 12; i++)
        {
            yield return new G1000CommandDefinition($"{idPrefix}.{i}", $"{labelPrefix} {i}", H($"{eventPrefix}{i}"), page, $"{idPrefix}.{i}.label");
        }
    }

    private static String H(String name) => $"(>H:{name})";

    private static String K(String name) => $"(>K:{name})";
}
