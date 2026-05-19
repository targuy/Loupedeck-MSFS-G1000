namespace LoupedeckMSFSG1000.Actions;

using LoupedeckMSFSG1000.G1000;

public sealed class MsfsHeadingBugAdjustment : FixedCalculatorAdjustmentBase
{
    public MsfsHeadingBugAdjustment() : base("HDG Bug", "Adjust heading bug.", "MSFS - Autopilot", "(>K:HEADING_BUG_INC)", "(>K:HEADING_BUG_DEC)", G1000ControlPage.Autopilot, "bug.hdg", "ap.hdg") { }
}

public sealed class MsfsAltitudeSelectAdjustment : FixedCalculatorAdjustmentBase
{
    public MsfsAltitudeSelectAdjustment() : base("ALT Select", "Adjust selected altitude.", "MSFS - Autopilot", "(>K:AP_ALT_VAR_INC)", "(>K:AP_ALT_VAR_DEC)", G1000ControlPage.Autopilot, "bug.alt", "ap.alt") { }
}

public sealed class MsfsVerticalSpeedAdjustment : FixedCalculatorAdjustmentBase
{
    public MsfsVerticalSpeedAdjustment() : base("VS Select", "Adjust selected vertical speed.", "MSFS - Autopilot", "(>K:AP_VS_VAR_INC)", "(>K:AP_VS_VAR_DEC)", G1000ControlPage.Autopilot, "bug.vs", "ap.vs") { }
}

public sealed class MsfsElevatorTrimAdjustment : FixedCalculatorAdjustmentBase
{
    public MsfsElevatorTrimAdjustment() : base("Elev Trim", "Adjust elevator trim.", "MSFS - Flight Controls", "(>K:ELEV_TRIM_UP)", "(>K:ELEV_TRIM_DN)", G1000ControlPage.Fixed) { }
}

public sealed class MsfsFlapsAdjustment : FixedCalculatorAdjustmentBase
{
    public MsfsFlapsAdjustment() : base("FLAP", "Adjust flaps one step.", "MSFS - Flight Controls", "(>K:FLAPS_INCR)", "(>K:FLAPS_DECR)", G1000ControlPage.Fixed, "flaps.pct") { }
}

public sealed class G1000BaroAdjustment : FixedCalculatorAdjustmentBase
{
    public G1000BaroAdjustment() : base("G1000 BARO", "Adjust G1000 barometer.", "G1000 - PFD", "(>H:AS1000_PFD_BARO_INC)", "(>H:AS1000_PFD_BARO_DEC)", G1000ControlPage.Pfd) { }
}

public sealed class G1000Com1MhzAdjustment : FixedCalculatorAdjustmentBase
{
    public G1000Com1MhzAdjustment() : base("COM1 MHz", "Adjust COM1 standby MHz.", "G1000 - COM/NAV", "(>H:AS1000_PFD_COM_Radio_1_Whole_INC)", "(>H:AS1000_PFD_COM_Radio_1_Whole_DEC)", G1000ControlPage.ComNav, "com1.stby") { }
}

public sealed class G1000Com1KhzAdjustment : FixedCalculatorAdjustmentBase
{
    public G1000Com1KhzAdjustment() : base("COM1 kHz", "Adjust COM1 standby kHz.", "G1000 - COM/NAV", "(>H:AS1000_PFD_COM_Radio_1_Fract_INC)", "(>H:AS1000_PFD_COM_Radio_1_Fract_DEC)", G1000ControlPage.ComNav, "com1.stby") { }
}

public sealed class G1000Nav1MhzAdjustment : FixedCalculatorAdjustmentBase
{
    public G1000Nav1MhzAdjustment() : base("NAV1 MHz", "Adjust NAV1 standby MHz.", "G1000 - COM/NAV", "(>H:AS1000_PFD_NAV_Radio_1_Whole_INC)", "(>H:AS1000_PFD_NAV_Radio_1_Whole_DEC)", G1000ControlPage.ComNav, "nav1.stby") { }
}

public sealed class G1000Nav1KhzAdjustment : FixedCalculatorAdjustmentBase
{
    public G1000Nav1KhzAdjustment() : base("NAV1 kHz", "Adjust NAV1 standby kHz.", "G1000 - COM/NAV", "(>H:AS1000_PFD_NAV_Radio_1_Fract_INC)", "(>H:AS1000_PFD_NAV_Radio_1_Fract_DEC)", G1000ControlPage.ComNav, "nav1.stby") { }
}
