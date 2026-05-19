namespace LoupedeckMSFSG1000.Sim;

public static class SimVariables
{
    public const String AutopilotMasterState = "(A:AUTOPILOT MASTER, bool)";
    public const String AutopilotMasterToggle = "(>K:AP_MASTER)";
    public const String AvionicsBusVoltage = "(A:ELECTRICAL AVIONICS BUS VOLTAGE, Volts)";

    public static IReadOnlyList<SimSubscription> Subscriptions { get; } =
    [
        Bool("ap.master", AutopilotMasterState),
        Bool("ap.fd", "(A:AUTOPILOT FLIGHT DIRECTOR ACTIVE, bool)"),
        Bool("ap.hdg", "(A:AUTOPILOT HEADING LOCK, bool)"),
        Bool("ap.nav", "(A:AUTOPILOT NAV1 LOCK, bool)"),
        Bool("ap.alt", "(A:AUTOPILOT ALTITUDE LOCK, bool)"),
        Bool("ap.vs", "(A:AUTOPILOT VERTICAL HOLD, bool)"),
        Bool("ap.apr", "(A:AUTOPILOT APPROACH HOLD, bool)"),
        Bool("ap.flc", "(A:AUTOPILOT FLIGHT LEVEL CHANGE, bool)"),
        Bool("ap.bc", "(A:AUTOPILOT BACKCOURSE HOLD, bool)"),
        Number("bug.hdg", "(A:AUTOPILOT HEADING LOCK DIR, degrees)"),
        Number("bug.alt", "(A:AUTOPILOT ALTITUDE LOCK VAR, feet)"),
        Number("bug.vs", "(A:AUTOPILOT VERTICAL HOLD VAR, feet per minute)"),
        Number("bug.ias", "(A:AUTOPILOT AIRSPEED HOLD VAR, knots)"),
        Number("current.hdg", "(A:PLANE HEADING DEGREES MAGNETIC, degrees)"),
        Number("current.alt", "(A:INDICATED ALTITUDE, feet)"),
        Number("current.vs", "(A:VERTICAL SPEED, feet per minute)"),
        Number("current.ias", "(A:AIRSPEED INDICATED, knots)"),

        Bool("gear.handle", "(A:GEAR HANDLE POSITION, bool)"),
        Number("flaps.pct", "(A:FLAPS HANDLE PERCENT, percent)"),
        Bool("parking_brake", "(A:BRAKE PARKING INDICATOR, bool)"),

        Bool("lights.nav", "(A:LIGHT NAV, bool)"),
        Bool("lights.beacon", "(A:LIGHT BEACON, bool)"),
        Bool("lights.strobe", "(A:LIGHT STROBE, bool)"),
        Bool("lights.landing", "(A:LIGHT LANDING, bool)"),
        Bool("lights.taxi", "(A:LIGHT TAXI, bool)"),

        Number("com1.active", "(A:COM ACTIVE FREQUENCY:1, MHz)"),
        Number("com1.stby", "(A:COM STANDBY FREQUENCY:1, MHz)"),
        Number("com2.active", "(A:COM ACTIVE FREQUENCY:2, MHz)"),
        Number("com2.stby", "(A:COM STANDBY FREQUENCY:2, MHz)"),
        Number("nav1.active", "(A:NAV ACTIVE FREQUENCY:1, MHz)"),
        Number("nav1.stby", "(A:NAV STANDBY FREQUENCY:1, MHz)"),
        Number("nav2.active", "(A:NAV ACTIVE FREQUENCY:2, MHz)"),
        Number("nav2.stby", "(A:NAV STANDBY FREQUENCY:2, MHz)"),
    ];

    private static SimSubscription Bool(String id, String code) =>
        new(id, code, TimeSpan.FromMilliseconds(200), SimValueKind.Boolean);

    private static SimSubscription Number(String id, String code) =>
        new(id, code, TimeSpan.FromMilliseconds(250), SimValueKind.Double);
}
