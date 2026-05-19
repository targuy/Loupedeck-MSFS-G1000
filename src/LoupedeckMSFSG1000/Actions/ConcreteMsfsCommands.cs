namespace LoupedeckMSFSG1000.Actions;

using LoupedeckMSFSG1000.G1000;

public sealed class MsfsGearToggleCommand : FixedCalculatorCommandBase
{
    public MsfsGearToggleCommand() : base("Gear Toggle", "Toggle landing gear.", "MSFS - Flight Controls", "(>K:GEAR_TOGGLE)", G1000ControlPage.Fixed, "gear.handle", displayStyle: ActionDisplayStyle.BooleanButton) { }
}

public sealed class MsfsParkingBrakeCommand : FixedCalculatorCommandBase
{
    public MsfsParkingBrakeCommand() : base("Parking Brake", "Toggle parking brake.", "MSFS - Flight Controls", "(>K:PARKING_BRAKES)", G1000ControlPage.Fixed, "parking_brake", displayStyle: ActionDisplayStyle.BooleanButton) { }
}

public sealed class MsfsFlapsUpCommand : FixedCalculatorCommandBase
{
    public MsfsFlapsUpCommand() : base("Flaps Up", "Retract flaps one step.", "MSFS - Flight Controls", "(>K:FLAPS_DECR)", G1000ControlPage.Fixed, "flaps.pct", activeStateId: null, displayStyle: ActionDisplayStyle.ValueButton) { }
}

public sealed class MsfsFlapsDownCommand : FixedCalculatorCommandBase
{
    public MsfsFlapsDownCommand() : base("Flaps Down", "Extend flaps one step.", "MSFS - Flight Controls", "(>K:FLAPS_INCR)", G1000ControlPage.Fixed, "flaps.pct", activeStateId: null, displayStyle: ActionDisplayStyle.ValueButton) { }
}

public sealed class MsfsPauseCommand : FixedCalculatorCommandBase
{
    public MsfsPauseCommand() : base("Pause", "Toggle simulator pause.", "MSFS - Simulator", "(>K:PAUSE_TOGGLE)", G1000ControlPage.Fixed) { }
}

public sealed class MsfsAtcPanelCommand : FixedCalculatorCommandBase
{
    public MsfsAtcPanelCommand() : base("ATC Panel", "Toggle ATC panel.", "MSFS - Simulator", "(>K:PANEL_ATC_TOGGLE)", G1000ControlPage.Fixed) { }
}

public sealed class MsfsVfrMapCommand : FixedCalculatorCommandBase
{
    public MsfsVfrMapCommand() : base("VFR Map", "Toggle VFR map.", "MSFS - Simulator", "(>K:VFR_MAP_TOGGLE)", G1000ControlPage.Fixed) { }
}

public sealed class MsfsBatteryMasterCommand : FixedCalculatorCommandBase
{
    public MsfsBatteryMasterCommand() : base("Battery", "Toggle master battery.", "MSFS - Power", "(>K:TOGGLE_MASTER_BATTERY)", G1000ControlPage.Fixed, displayStyle: ActionDisplayStyle.BooleanButton) { }
}

public sealed class MsfsAvionicsMasterCommand : FixedCalculatorCommandBase
{
    public MsfsAvionicsMasterCommand() : base("Avionics", "Toggle avionics master.", "MSFS - Power", "(>K:TOGGLE_AVIONICS_MASTER)", G1000ControlPage.Fixed, displayStyle: ActionDisplayStyle.BooleanButton) { }
}

public sealed class MsfsFuelPumpCommand : FixedCalculatorCommandBase
{
    public MsfsFuelPumpCommand() : base("Fuel Pump", "Toggle electric fuel pump.", "MSFS - Power", "(>K:TOGGLE_ELECT_FUEL_PUMP)", G1000ControlPage.Fixed, displayStyle: ActionDisplayStyle.BooleanButton) { }
}

public sealed class MsfsMagnetosBothCommand : FixedCalculatorCommandBase
{
    public MsfsMagnetosBothCommand() : base("Magnetos", "Set magnetos to both.", "MSFS - Power", "(>K:MAGNETO1_BOTH)", G1000ControlPage.Fixed) { }
}

public sealed class MsfsStarter1Command : FixedCalculatorCommandBase
{
    public MsfsStarter1Command() : base("Starter 1", "Toggle starter 1.", "MSFS - Power", "(>K:TOGGLE_STARTER1)", G1000ControlPage.Fixed) { }
}

public sealed class MsfsNavLightsCommand : FixedCalculatorCommandBase
{
    public MsfsNavLightsCommand() : base("Nav Lights", "Toggle navigation lights.", "MSFS - Lights", "(>K:NAV_LIGHTS_TOGGLE)", G1000ControlPage.ComNav, "lights.nav", displayStyle: ActionDisplayStyle.BooleanButton) { }
}

public sealed class MsfsBeaconCommand : FixedCalculatorCommandBase
{
    public MsfsBeaconCommand() : base("Beacon", "Toggle beacon lights.", "MSFS - Lights", "(>K:TOGGLE_BEACON_LIGHTS)", G1000ControlPage.ComNav, "lights.beacon", displayStyle: ActionDisplayStyle.BooleanButton) { }
}

public sealed class MsfsStrobeCommand : FixedCalculatorCommandBase
{
    public MsfsStrobeCommand() : base("Strobe", "Toggle strobe lights.", "MSFS - Lights", "(>K:STROBES_TOGGLE)", G1000ControlPage.ComNav, "lights.strobe", displayStyle: ActionDisplayStyle.BooleanButton) { }
}

public sealed class MsfsLandingLightsCommand : FixedCalculatorCommandBase
{
    public MsfsLandingLightsCommand() : base("Landing Lights", "Toggle landing lights.", "MSFS - Lights", "(>K:LANDING_LIGHTS_TOGGLE)", G1000ControlPage.ComNav, "lights.landing", displayStyle: ActionDisplayStyle.BooleanButton) { }
}

public sealed class MsfsTaxiLightsCommand : FixedCalculatorCommandBase
{
    public MsfsTaxiLightsCommand() : base("Taxi Lights", "Toggle taxi lights.", "MSFS - Lights", "(>K:TOGGLE_TAXI_LIGHTS)", G1000ControlPage.ComNav, "lights.taxi", displayStyle: ActionDisplayStyle.BooleanButton) { }
}

public sealed class MsfsApHdgCommand : FixedCalculatorCommandBase
{
    public MsfsApHdgCommand() : base("AP HDG", "Toggle autopilot heading mode.", "MSFS - Autopilot", "(>K:AP_HDG_HOLD)", G1000ControlPage.Autopilot, "current.hdg", "ap.hdg", ActionDisplayStyle.ApButton) { }
}

public sealed class MsfsApNavCommand : FixedCalculatorCommandBase
{
    public MsfsApNavCommand() : base("AP NAV", "Toggle autopilot NAV mode.", "MSFS - Autopilot", "(>K:AP_NAV1_HOLD)", G1000ControlPage.Autopilot, "ap.nav", "ap.nav", ActionDisplayStyle.ApButton) { }
}

public sealed class MsfsApAltCommand : FixedCalculatorCommandBase
{
    public MsfsApAltCommand() : base("AP ALT", "Toggle autopilot altitude hold.", "MSFS - Autopilot", "(>K:AP_ALT_HOLD)", G1000ControlPage.Autopilot, "current.alt", "ap.alt", ActionDisplayStyle.ApButton) { }
}

public sealed class MsfsApVsCommand : FixedCalculatorCommandBase
{
    public MsfsApVsCommand() : base("AP VS", "Toggle autopilot vertical speed mode.", "MSFS - Autopilot", "(>K:AP_VS_HOLD)", G1000ControlPage.Autopilot, "current.vs", "ap.vs", ActionDisplayStyle.ApButton) { }
}

public sealed class MsfsApAprCommand : FixedCalculatorCommandBase
{
    public MsfsApAprCommand() : base("AP APR", "Toggle autopilot approach mode.", "MSFS - Autopilot", "(>K:AP_APR_HOLD)", G1000ControlPage.Autopilot, "ap.apr", "ap.apr", ActionDisplayStyle.ApButton) { }
}

public sealed class MsfsApFlcCommand : FixedCalculatorCommandBase
{
    public MsfsApFlcCommand() : base("AP FLC", "Toggle flight level change.", "MSFS - Autopilot", "(>K:FLIGHT_LEVEL_CHANGE)", G1000ControlPage.Autopilot, "current.ias", "ap.flc", ActionDisplayStyle.ApButton) { }
}
