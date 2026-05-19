namespace LoupedeckMSFSG1000.Actions;

using LoupedeckMSFSG1000.G1000;

public sealed class G1000DirectToCommand : FixedCalculatorCommandBase
{
    public G1000DirectToCommand() : base("G1000 D->", "G1000 direct-to.", "G1000 - Fixed", "(>H:AS1000_PFD_DIRECTTO)", G1000ControlPage.Fixed) { }
}

public sealed class G1000MenuCommand : FixedCalculatorCommandBase
{
    public G1000MenuCommand() : base("G1000 MENU", "G1000 menu.", "G1000 - Fixed", "(>H:AS1000_PFD_MENU_Push)", G1000ControlPage.Fixed) { }
}

public sealed class G1000FplCommand : FixedCalculatorCommandBase
{
    public G1000FplCommand() : base("G1000 FPL", "G1000 flight plan.", "G1000 - Fixed", "(>H:AS1000_PFD_FPL_Push)", G1000ControlPage.Fixed) { }
}

public sealed class G1000ProcCommand : FixedCalculatorCommandBase
{
    public G1000ProcCommand() : base("G1000 PROC", "G1000 procedures.", "G1000 - Fixed", "(>H:AS1000_PFD_PROC_Push)", G1000ControlPage.Fixed) { }
}

public sealed class G1000ClrCommand : FixedCalculatorCommandBase
{
    public G1000ClrCommand() : base("G1000 CLR", "G1000 clear.", "G1000 - Fixed", "(>H:AS1000_PFD_CLR)", G1000ControlPage.Fixed) { }
}

public sealed class G1000EntCommand : FixedCalculatorCommandBase
{
    public G1000EntCommand() : base("G1000 ENT", "G1000 enter.", "G1000 - Fixed", "(>H:AS1000_PFD_ENT_Push)", G1000ControlPage.Fixed) { }
}

public sealed class G1000Com1SwapCommand : FixedCalculatorCommandBase
{
    public G1000Com1SwapCommand() : base("COM1 Swap", "Swap COM1 active/standby.", "G1000 - COM/NAV", "(>H:AS1000_PFD_COM_Radio_1_PUSH)", G1000ControlPage.ComNav) { }
}

public sealed class G1000Com2SwapCommand : FixedCalculatorCommandBase
{
    public G1000Com2SwapCommand() : base("COM2 Swap", "Swap COM2 active/standby.", "G1000 - COM/NAV", "(>H:AS1000_PFD_COM_Radio_2_PUSH)", G1000ControlPage.ComNav) { }
}

public sealed class G1000Nav1SwapCommand : FixedCalculatorCommandBase
{
    public G1000Nav1SwapCommand() : base("NAV1 Swap", "Swap NAV1 active/standby.", "G1000 - COM/NAV", "(>H:AS1000_PFD_NAV_Radio_1_PUSH)", G1000ControlPage.ComNav) { }
}

public sealed class G1000Nav2SwapCommand : FixedCalculatorCommandBase
{
    public G1000Nav2SwapCommand() : base("NAV2 Swap", "Swap NAV2 active/standby.", "G1000 - COM/NAV", "(>H:AS1000_PFD_NAV_Radio_2_PUSH)", G1000ControlPage.ComNav) { }
}

public sealed class G1000Softkey1Command : FixedCalculatorCommandBase
{
    public G1000Softkey1Command() : base("PFD Softkey 1", "G1000 PFD softkey 1.", "G1000 - PFD Softkeys", "(>H:AS1000_PFD_SOFTKEYS_1)", G1000ControlPage.Pfd, displayStyle: ActionDisplayStyle.Softkey) { }
}

public sealed class G1000Softkey2Command : FixedCalculatorCommandBase
{
    public G1000Softkey2Command() : base("PFD Softkey 2", "G1000 PFD softkey 2.", "G1000 - PFD Softkeys", "(>H:AS1000_PFD_SOFTKEYS_2)", G1000ControlPage.Pfd, displayStyle: ActionDisplayStyle.Softkey) { }
}

public sealed class G1000Softkey3Command : FixedCalculatorCommandBase
{
    public G1000Softkey3Command() : base("PFD Softkey 3", "G1000 PFD softkey 3.", "G1000 - PFD Softkeys", "(>H:AS1000_PFD_SOFTKEYS_3)", G1000ControlPage.Pfd, displayStyle: ActionDisplayStyle.Softkey) { }
}

public sealed class G1000Softkey4Command : FixedCalculatorCommandBase
{
    public G1000Softkey4Command() : base("PFD Softkey 4", "G1000 PFD softkey 4.", "G1000 - PFD Softkeys", "(>H:AS1000_PFD_SOFTKEYS_4)", G1000ControlPage.Pfd, displayStyle: ActionDisplayStyle.Softkey) { }
}

public sealed class G1000Softkey5Command : FixedCalculatorCommandBase
{
    public G1000Softkey5Command() : base("PFD Softkey 5", "G1000 PFD softkey 5.", "G1000 - PFD Softkeys", "(>H:AS1000_PFD_SOFTKEYS_5)", G1000ControlPage.Pfd, displayStyle: ActionDisplayStyle.Softkey) { }
}

public sealed class G1000Softkey6Command : FixedCalculatorCommandBase
{
    public G1000Softkey6Command() : base("PFD Softkey 6", "G1000 PFD softkey 6.", "G1000 - PFD Softkeys", "(>H:AS1000_PFD_SOFTKEYS_6)", G1000ControlPage.Pfd, displayStyle: ActionDisplayStyle.Softkey) { }
}

public sealed class G1000Softkey7Command : FixedCalculatorCommandBase
{
    public G1000Softkey7Command() : base("PFD Softkey 7", "G1000 PFD softkey 7.", "G1000 - PFD Softkeys", "(>H:AS1000_PFD_SOFTKEYS_7)", G1000ControlPage.Pfd, displayStyle: ActionDisplayStyle.Softkey) { }
}

public sealed class G1000Softkey8Command : FixedCalculatorCommandBase
{
    public G1000Softkey8Command() : base("PFD Softkey 8", "G1000 PFD softkey 8.", "G1000 - PFD Softkeys", "(>H:AS1000_PFD_SOFTKEYS_8)", G1000ControlPage.Pfd, displayStyle: ActionDisplayStyle.Softkey) { }
}

public sealed class G1000Softkey9Command : FixedCalculatorCommandBase
{
    public G1000Softkey9Command() : base("PFD Softkey 9", "G1000 PFD softkey 9.", "G1000 - PFD Softkeys", "(>H:AS1000_PFD_SOFTKEYS_9)", G1000ControlPage.Pfd, displayStyle: ActionDisplayStyle.Softkey) { }
}

public sealed class G1000Softkey10Command : FixedCalculatorCommandBase
{
    public G1000Softkey10Command() : base("PFD Softkey 10", "G1000 PFD softkey 10.", "G1000 - PFD Softkeys", "(>H:AS1000_PFD_SOFTKEYS_10)", G1000ControlPage.Pfd, displayStyle: ActionDisplayStyle.Softkey) { }
}

public sealed class G1000Softkey11Command : FixedCalculatorCommandBase
{
    public G1000Softkey11Command() : base("PFD Softkey 11", "G1000 PFD softkey 11.", "G1000 - PFD Softkeys", "(>H:AS1000_PFD_SOFTKEYS_11)", G1000ControlPage.Pfd, displayStyle: ActionDisplayStyle.Softkey) { }
}

public sealed class G1000Softkey12Command : FixedCalculatorCommandBase
{
    public G1000Softkey12Command() : base("PFD Softkey 12", "G1000 PFD softkey 12.", "G1000 - PFD Softkeys", "(>H:AS1000_PFD_SOFTKEYS_12)", G1000ControlPage.Pfd, displayStyle: ActionDisplayStyle.Softkey) { }
}
