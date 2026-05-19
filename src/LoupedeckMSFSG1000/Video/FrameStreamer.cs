namespace LoupedeckMSFSG1000.Video;

public class FrameStreamer
{
    private const int TargetFps = 20;

    public Task StartAsync(string targetId, Func<object> renderFrame, CancellationToken cancellationToken = default)
    {
        _ = targetId;
        _ = renderFrame;
        _ = cancellationToken;
        _ = TargetFps;
        throw new NotImplementedException();
    }
}
