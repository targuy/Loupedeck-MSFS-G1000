namespace LoupedeckMSFSG1000.Actions;

using Loupedeck;
using LoupedeckMSFSG1000.G1000;
using LoupedeckMSFSG1000.Runtime;
using LoupedeckMSFSG1000.State;

public abstract class FixedCalculatorAdjustmentBase : PluginDynamicAdjustment
{
    private readonly String _label;
    private readonly String _incrementCode;
    private readonly String _decrementCode;
    private readonly G1000ControlPage _page;
    private readonly String? _stateId;
    private readonly String? _activeStateId;
    private readonly ActionDisplayStyle _displayStyle;

    protected FixedCalculatorAdjustmentBase(
        String label,
        String description,
        String groupName,
        String incrementCode,
        String decrementCode,
        G1000ControlPage page,
        String? stateId = null,
        String? activeStateId = null,
        ActionDisplayStyle displayStyle = ActionDisplayStyle.Encoder)
        : base(label, description, groupName, hasReset: false)
    {
        _label = label;
        _incrementCode = incrementCode;
        _decrementCode = decrementCode;
        _page = page;
        _stateId = stateId;
        _activeStateId = activeStateId;
        _displayStyle = displayStyle;
        PluginRuntime.StateChanged += this.OnStateChanged;
    }

    protected override void ApplyAdjustment(String actionParameter, Int32 diff)
    {
        if (diff == 0)
        {
            return;
        }

        _ = this.ExecuteAsync(diff > 0 ? _incrementCode : _decrementCode, Math.Abs(diff));
    }

    protected override String GetAdjustmentDisplayName(String actionParameter, PluginImageSize imageSize)
        => DisplayText.Hidden;

    protected override String GetAdjustmentValue(String actionParameter) =>
        DisplayText.Hidden;

    protected override BitmapImage GetAdjustmentImage(String actionParameter, PluginImageSize imageSize) =>
        G1000ActionRenderer.RenderButton(
            _label,
            _page,
            imageSize,
            StateValueFormatter.Format(_stateId, PluginRuntime.State.GetValue(_stateId)),
            StateValueFormatter.ToBoolean(_activeStateId, PluginRuntime.State.GetValue(_activeStateId)),
            _displayStyle);

    private async Task ExecuteAsync(String calculatorCode, Int32 repeatCount)
    {
        try
        {
            PluginRuntime.StartStateInBackground();
            for (var i = 0; i < repeatCount; i++)
            {
                await PluginRuntime.SimLayer.ExecuteCalculatorCodeAsync(calculatorCode);
            }

            PluginLog.Info($"Fixed adjustment sent: {_label} x{repeatCount} -> {calculatorCode}");
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, $"Fixed adjustment failed: {_label}");
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
            this.AdjustmentValueChanged();
        }
    }
}
