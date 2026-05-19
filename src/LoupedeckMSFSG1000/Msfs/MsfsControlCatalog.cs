namespace LoupedeckMSFSG1000.Msfs;

using Loupedeck;

public static class MsfsControlCatalog
{
    public static IReadOnlyList<MsfsCommandDefinition> Commands { get; } =
    [
        new("ap.master", "AP Master", K("AP_MASTER"), MsfsControlGroup.Autopilot, "ap.master"),
        new("ap.fd", "Flight Director", K("TOGGLE_FLIGHT_DIRECTOR"), MsfsControlGroup.Autopilot, "ap.fd"),
        new("ap.hdg", "AP HDG", K("AP_HDG_HOLD"), MsfsControlGroup.Autopilot, "ap.hdg"),
        new("ap.nav", "AP NAV", K("AP_NAV1_HOLD"), MsfsControlGroup.Autopilot, "ap.nav"),
        new("ap.alt", "AP ALT", K("AP_ALT_HOLD"), MsfsControlGroup.Autopilot, "ap.alt"),
        new("ap.vs", "AP VS", K("AP_VS_HOLD"), MsfsControlGroup.Autopilot, "ap.vs"),
        new("ap.apr", "AP APR", K("AP_APR_HOLD"), MsfsControlGroup.Autopilot, "ap.apr"),
        new("ap.flc", "AP FLC", K("FLIGHT_LEVEL_CHANGE"), MsfsControlGroup.Autopilot, "ap.flc"),
        new("ap.disconnect", "AP Disconnect", K("AP_DISENGAGE"), MsfsControlGroup.Autopilot),

        new("gear.toggle", "Gear Toggle", K("GEAR_TOGGLE"), MsfsControlGroup.FlightControls),
        new("flaps.inc", "Flaps +", K("FLAPS_INCR"), MsfsControlGroup.FlightControls),
        new("flaps.dec", "Flaps -", K("FLAPS_DECR"), MsfsControlGroup.FlightControls),
        new("spoilers.toggle", "Spoilers", K("SPOILERS_TOGGLE"), MsfsControlGroup.FlightControls),
        new("parking_brake.toggle", "Parking Brake", K("PARKING_BRAKES"), MsfsControlGroup.FlightControls),
        new("brakes.regular", "Brakes", K("BRAKES"), MsfsControlGroup.FlightControls),

        new("lights.nav", "Nav Lights", K("NAV_LIGHTS_TOGGLE"), MsfsControlGroup.Lights),
        new("lights.beacon", "Beacon", K("TOGGLE_BEACON_LIGHTS"), MsfsControlGroup.Lights),
        new("lights.strobe", "Strobe", K("STROBES_TOGGLE"), MsfsControlGroup.Lights),
        new("lights.landing", "Landing", K("LANDING_LIGHTS_TOGGLE"), MsfsControlGroup.Lights),
        new("lights.taxi", "Taxi", K("TOGGLE_TAXI_LIGHTS"), MsfsControlGroup.Lights),
        new("lights.panel", "Panel Lights", K("PANEL_LIGHTS_TOGGLE"), MsfsControlGroup.Lights),

        new("engine.starter1", "Starter 1", K("TOGGLE_STARTER1"), MsfsControlGroup.Engine),
        new("engine.magnetos1", "Magnetos 1", K("MAGNETO1_BOTH"), MsfsControlGroup.Engine),
        new("engine.fuel_pump", "Fuel Pump", K("TOGGLE_ELECT_FUEL_PUMP"), MsfsControlGroup.Engine),

        new("radios.com1.swap", "COM1 Swap", K("COM_STBY_RADIO_SWAP"), MsfsControlGroup.Radios),
        new("radios.com2.swap", "COM2 Swap", K("COM2_RADIO_SWAP"), MsfsControlGroup.Radios),
        new("radios.nav1.swap", "NAV1 Swap", K("NAV1_RADIO_SWAP"), MsfsControlGroup.Radios),
        new("radios.nav2.swap", "NAV2 Swap", K("NAV2_RADIO_SWAP"), MsfsControlGroup.Radios),
        new("xpndr.ident", "XPDR Ident", K("XPNDR_IDENT_ON"), MsfsControlGroup.Radios),

        new("view.atc", "ATC Panel", K("PANEL_ATC_TOGGLE"), MsfsControlGroup.Instruments),
        new("view.vfrmap", "VFR Map", K("VFR_MAP_TOGGLE"), MsfsControlGroup.Instruments),
    ];

    public static IReadOnlyList<MsfsAdjustmentDefinition> Adjustments { get; } =
    [
        new("trim.elevator", "Elev Trim", K("ELEV_TRIM_UP"), K("ELEV_TRIM_DN"), MsfsControlGroup.FlightControls),
        new("trim.aileron", "Ail Trim", K("AILERON_TRIM_RIGHT"), K("AILERON_TRIM_LEFT"), MsfsControlGroup.FlightControls),
        new("trim.rudder", "Rud Trim", K("RUDDER_TRIM_RIGHT"), K("RUDDER_TRIM_LEFT"), MsfsControlGroup.FlightControls),
        new("flaps.step", "Flaps", K("FLAPS_INCR"), K("FLAPS_DECR"), MsfsControlGroup.FlightControls),
        new("spoilers.axis", "Spoilers", K("SPOILERS_ON"), K("SPOILERS_OFF"), MsfsControlGroup.FlightControls),

        new("ap.hdg_bug", "HDG Bug", K("HEADING_BUG_INC"), K("HEADING_BUG_DEC"), MsfsControlGroup.Autopilot, "bug.hdg"),
        new("ap.alt_100", "Alt Sel", K("AP_ALT_VAR_INC"), K("AP_ALT_VAR_DEC"), MsfsControlGroup.Autopilot, "bug.alt"),
        new("ap.vs", "VS Sel", K("AP_VS_VAR_INC"), K("AP_VS_VAR_DEC"), MsfsControlGroup.Autopilot, "bug.vs"),
        new("ap.ias", "IAS Sel", K("AP_SPD_VAR_INC"), K("AP_SPD_VAR_DEC"), MsfsControlGroup.Autopilot, "bug.ias"),

        new("engine.throttle1", "Throttle 1", K("THROTTLE1_INCR_SMALL"), K("THROTTLE1_DECR_SMALL"), MsfsControlGroup.Engine),
        new("engine.mixture1", "Mixture 1", K("MIXTURE1_RICH"), K("MIXTURE1_LEAN"), MsfsControlGroup.Engine),
        new("engine.prop1", "Prop 1", K("PROP_PITCH1_INCR_SMALL"), K("PROP_PITCH1_DECR_SMALL"), MsfsControlGroup.Engine),

        new("radios.com1.whole", "COM1 MHz", K("COM_RADIO_WHOLE_INC"), K("COM_RADIO_WHOLE_DEC"), MsfsControlGroup.Radios, "com1.stby"),
        new("radios.com1.fract", "COM1 kHz", K("COM_RADIO_FRACT_INC"), K("COM_RADIO_FRACT_DEC"), MsfsControlGroup.Radios, "com1.stby"),
        new("radios.com2.whole", "COM2 MHz", K("COM2_RADIO_WHOLE_INC"), K("COM2_RADIO_WHOLE_DEC"), MsfsControlGroup.Radios, "com2.stby"),
        new("radios.com2.fract", "COM2 kHz", K("COM2_RADIO_FRACT_INC"), K("COM2_RADIO_FRACT_DEC"), MsfsControlGroup.Radios, "com2.stby"),
        new("radios.nav1.whole", "NAV1 MHz", K("NAV1_RADIO_WHOLE_INC"), K("NAV1_RADIO_WHOLE_DEC"), MsfsControlGroup.Radios, "nav1.stby"),
        new("radios.nav1.fract", "NAV1 kHz", K("NAV1_RADIO_FRACT_INC"), K("NAV1_RADIO_FRACT_DEC"), MsfsControlGroup.Radios, "nav1.stby"),
        new("radios.nav2.whole", "NAV2 MHz", K("NAV2_RADIO_WHOLE_INC"), K("NAV2_RADIO_WHOLE_DEC"), MsfsControlGroup.Radios, "nav2.stby"),
        new("radios.nav2.fract", "NAV2 kHz", K("NAV2_RADIO_FRACT_INC"), K("NAV2_RADIO_FRACT_DEC"), MsfsControlGroup.Radios, "nav2.stby"),
        new("xpndr.code", "XPDR Code", K("XPNDR_INC_TENS"), K("XPNDR_DEC_TENS"), MsfsControlGroup.Radios),

        new("baro", "Baro", K("KOHLSMAN_INC"), K("KOHLSMAN_DEC"), MsfsControlGroup.Instruments),
    ];

    public static MsfsCommandDefinition? FindCommand(String id) =>
        Commands.FirstOrDefault(command => command.Id == id);

    public static MsfsAdjustmentDefinition? FindAdjustment(String id) =>
        Adjustments.FirstOrDefault(adjustment => adjustment.Id == id);

    public static BitmapColor GroupColor(MsfsControlGroup group) => group switch
    {
        MsfsControlGroup.Autopilot => new BitmapColor(255, 179, 0),
        MsfsControlGroup.FlightControls => new BitmapColor(64, 137, 255),
        MsfsControlGroup.Lights => new BitmapColor(255, 218, 77),
        MsfsControlGroup.Engine => new BitmapColor(255, 92, 64),
        MsfsControlGroup.Radios => new BitmapColor(0, 204, 255),
        MsfsControlGroup.Navigation => new BitmapColor(0, 204, 68),
        _ => new BitmapColor(180, 190, 205),
    };

    private static String K(String name) => $"(>K:{name})";
}
