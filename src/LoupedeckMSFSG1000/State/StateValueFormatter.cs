namespace LoupedeckMSFSG1000.State;

public static class StateValueFormatter
{
    public static String Format(String? stateId, Double? value)
    {
        if (stateId is null || value is null)
        {
            return String.Empty;
        }

        return stateId switch
        {
            "gear.handle" => value > 0.5 ? "DOWN" : "UP",
            "flaps.pct" => $"{Math.Round(value.Value):0}%",
            "bug.hdg" => $"{PositiveDegrees(value.Value):000}",
            "bug.alt" => $"{Math.Round(value.Value / 100.0) * 100:0}",
            "bug.vs" => $"{Math.Round(value.Value / 100.0) * 100:+0;-0;0}",
            "bug.ias" => $"{Math.Round(value.Value):0}KT",
            "current.hdg" => $"{PositiveDegrees(value.Value):000}",
            "current.alt" => $"{Math.Round(value.Value / 100.0) * 100:0}",
            "current.vs" => $"{Math.Round(value.Value / 100.0) * 100:+0;-0;0}",
            "current.ias" => $"{Math.Round(value.Value):0}KT",
            "com1.active" or "com1.stby" or "com2.active" or "com2.stby" => $"{value.Value:000.000}",
            "nav1.active" or "nav1.stby" or "nav2.active" or "nav2.stby" => $"{value.Value:000.00}",
            _ when IsBooleanState(stateId) => value > 0.5 ? "ON" : "OFF",
            _ => $"{value.Value:0}",
        };
    }

    public static Boolean? ToBoolean(String? stateId, Double? value)
    {
        if (stateId is null || value is null)
        {
            return null;
        }

        return stateId switch
        {
            "flaps.pct" => value > 1.0,
            "gear.handle" => value > 0.5,
            _ when IsBooleanState(stateId) => value > 0.5,
            _ => null,
        };
    }

    private static Boolean IsBooleanState(String stateId) =>
        stateId.StartsWith("ap.", StringComparison.Ordinal) ||
        stateId.StartsWith("lights.", StringComparison.Ordinal) ||
        stateId == "parking_brake";

    private static Double PositiveDegrees(Double value)
    {
        var degrees = value % 360.0;
        return degrees < 0 ? degrees + 360.0 : degrees;
    }
}
