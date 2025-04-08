using System.Runtime.InteropServices;

namespace RadioStation.GrpcProto.ConnectionChecker;

/// <summary>
/// Healthcheck failure dump helper
/// </summary>
public static class HealthCheckTerminationLog
{
    //this dump makes fail reason visible in kubernetes "kubectl describe" to help troubleshooting
    //https://kubernetes.io/docs/tasks/debug-application-cluster/determine-reason-pod-failure/

    /// <summary>
    /// Path to error-file
    /// </summary>
    private const string TERMINATION_LOG_PATH = "/dev/termination-log";

    /// <summary>
    /// Dump message to /dev/termination-log
    /// </summary>
    public static void Dump(string? errorMessage)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(errorMessage) || !RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return;

            File.WriteAllText(TERMINATION_LOG_PATH, errorMessage);
        }
        catch (Exception) { } //this helper dump should never throw any ex
    }
}