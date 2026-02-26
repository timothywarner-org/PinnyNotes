using System.Net.Sockets;

namespace TimmyTools.WpfUi.Services;

public record NtpResult(DateTime NetworkTime, TimeSpan Offset, bool Success);

public class NtpService
{
    private static readonly string[] NtpServers =
    [
        "time.nist.gov",
        "pool.ntp.org",
        "time.google.com",
        "time.windows.com"
    ];

    private const int NtpPort = 123;
    private const int NtpPacketSize = 48;
    private const int TimeoutMilliseconds = 3000;

    // NTP epoch is January 1, 1900
    private static readonly DateTime NtpEpoch = new(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public async Task<NtpResult> GetNetworkTimeAsync()
    {
        foreach (string server in NtpServers)
        {
            try
            {
                NtpResult result = await QueryServerAsync(server);
                if (result.Success)
                    return result;
            }
            catch
            {
                // Try next server
            }
        }

        return new NtpResult(DateTime.UtcNow, TimeSpan.Zero, false);
    }

    private static async Task<NtpResult> QueryServerAsync(string server)
    {
        byte[] ntpData = new byte[NtpPacketSize];

        // Set version number to 4, mode to 3 (client)
        ntpData[0] = 0x23; // LI=0, VN=4, Mode=3

        using UdpClient client = new();
        client.Client.ReceiveTimeout = TimeoutMilliseconds;
        client.Client.SendTimeout = TimeoutMilliseconds;

        await client.SendAsync(ntpData, ntpData.Length, server, NtpPort);

        using CancellationTokenSource cts = new(TimeoutMilliseconds);

        UdpReceiveResult response = await client.ReceiveAsync(cts.Token);
        byte[] responseData = response.Buffer;

        if (responseData.Length < NtpPacketSize)
            return new NtpResult(DateTime.UtcNow, TimeSpan.Zero, false);

        // Validate Leap Indicator (bits 6-7 of byte 0): value 3 means unsynchronized
        int leapIndicator = (responseData[0] >> 6) & 0x03;
        if (leapIndicator == 3)
            return new NtpResult(DateTime.UtcNow, TimeSpan.Zero, false);

        // Validate stratum (byte 1): 0 is kiss-of-death, > 15 is invalid
        int stratum = responseData[1];
        if (stratum == 0 || stratum > 15)
            return new NtpResult(DateTime.UtcNow, TimeSpan.Zero, false);

        // Extract transmit timestamp from bytes 40-47
        ulong intPart = (ulong)responseData[40] << 24
                       | (ulong)responseData[41] << 16
                       | (ulong)responseData[42] << 8
                       | responseData[43];

        ulong fractPart = (ulong)responseData[44] << 24
                         | (ulong)responseData[45] << 16
                         | (ulong)responseData[46] << 8
                         | responseData[47];

        double milliseconds = (intPart * 1000.0) + ((fractPart * 1000.0) / 0x100000000L);
        DateTime networkTime = NtpEpoch.AddMilliseconds(milliseconds);

        TimeSpan offset = networkTime - DateTime.UtcNow;

        // Reject if offset exceeds 24 hours in either direction
        if (Math.Abs(offset.TotalHours) > 24)
            return new NtpResult(DateTime.UtcNow, TimeSpan.Zero, false);

        return new NtpResult(networkTime, offset, true);
    }
}
