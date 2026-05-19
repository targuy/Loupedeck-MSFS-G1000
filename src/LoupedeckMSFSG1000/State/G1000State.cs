namespace LoupedeckMSFSG1000.State;

public class G1000State
{
    public bool AvionicsMasterOn { get; set; }
    public bool AutopilotMaster { get; set; }
    public double Com1ActiveMhz { get; set; }
    public double Com1StandbyMhz { get; set; }
    public double Com2ActiveMhz { get; set; }
    public double Com2StandbyMhz { get; set; }
    public double Nav1ActiveMhz { get; set; }
    public double Nav1StandbyMhz { get; set; }
    public double Nav2ActiveMhz { get; set; }
    public double Nav2StandbyMhz { get; set; }
}
