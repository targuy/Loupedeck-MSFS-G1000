using LoupedeckMSFSG1000.Spikes;

var selectedSpike = args.FirstOrDefault()?.ToLowerInvariant();

if (selectedSpike is "s1s3")
{
    await Spike_S1S3_WaSimBidi.RunAsync();
}
else if (selectedSpike is "s2")
{
    await Spike_S2_DynamicFolder.RunAsync();
}
else
{
    Console.WriteLine("Usage: dotnet run --project src/LoupedeckMSFSG1000.Spikes/LoupedeckMSFSG1000.Spikes.csproj -- [s1s3|s2]");
}
