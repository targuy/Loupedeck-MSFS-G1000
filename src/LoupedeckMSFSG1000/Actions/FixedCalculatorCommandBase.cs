namespace LoupedeckMSFSG1000.Actions;

using Loupedeck;
using LoupedeckMSFSG1000.G1000;
using LoupedeckMSFSG1000.Runtime;
using LoupedeckMSFSG1000.State;

public abstract class FixedCalculatorCommandBase : PluginDynamicCommand
{
    private readonly String _label;
    private readonly String _calculatorCode;
    private readonly G1000ControlPage _page;
    private readonly String? _stateId;
    private readonly String? _activeStateId;
    private readonly ActionDisplayStyle _displayStyle;

    protected FixedCalculatorCommandBase(
        String label,
        String description,
        String groupName,
        String calculatorCode,
        G1000ControlPage page,
        String? stateId = null,
        String? activeStateId = null,
        ActionDisplayStyle displayStyle = ActionDisplayStyle.Standard)
        : base(label, description, groupName)
    {
        _label = label;
        _calculatorCode = calculatorCode;
        _page = page;
        _stateId = stateId;
        _displayStyle = displayStyle;
        _activeStateId = activeStateId ?? (displayStyle == ActionDisplayStyle.BooleanButton ? stateId : null);
        PluginRuntime.StateChanged += this.OnStateChanged;
    }

    protected override void RunCommand(String actionParameter)
    {
        _ = this.ExecuteAsync();
    }

    protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        => DisplayText.Hidden;

    protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize) =>
        G1000ActionRenderer.RenderButton(
            _label,
            _page,
            imageSize,
            StateValueFormatter.Format(_stateId, PluginRuntime.State.GetValue(_stateId)),
            StateValueFormatter.ToBoolean(_activeStateId, PluginRuntime.State.GetValue(_activeStateId)),
            _displayStyle);

    private async Task ExecuteAsync()
    {
        try
        {
            PluginRuntime.StartStateInBackground();
            await PluginRuntime.SimLayer.ExecuteCalculatorCodeAsync(_calculatorCode);
            this.ToggleLocalActiveState();
            PluginLog.Info($"Fixed command sent: {_label} -> {_calculatorCode}");
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, $"Fixed command failed: {_label}");
        }
    }

    private void OnStateChanged(Object? sender, G1000StateChangedEventArgs e)
    {
        if (_stateId is null && _activeStateId is null)
        {
            return;
        }

        var previous = e.Previous.GetValue(_stateId);
        var current = e.Current.GetValue(_stateId);
        var previousActive = e.Previous.GetValue(_activeStateId);
        var currentActive = e.Current.GetValue(_activeStateId);
        if (previous != current || previousActive != currentActive)
        {
            this.ActionImageChanged();
        }
    }

    private void ToggleLocalActiveState()
    {
        if (_activeStateId is null)
        {
            return;
        }

        var current = PluginRuntime.State.GetValue(_activeStateId);
        var isOn = StateValueFormatter.ToBoolean(_activeStateId, current);
        PluginRuntime.SetLocalStateValue(_activeStateId, isOn == true ? 0.0 : 1.0);
        this.ActionImageChanged();
    }
}
