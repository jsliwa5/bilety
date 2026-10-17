namespace ExternalNoticesSystem.Api.Common;

public class ChaosState
{
    private int _failuresRemaining;

    public void SetFailures(int count) => Interlocked.Exchange(ref _failuresRemaining, count);

    public bool TryConsumeFailure(out int remaining)
    {
        while (true)
        {
            var current = Volatile.Read(ref _failuresRemaining);
            if (current <= 0)
            {
                remaining = 0;
                return false;
            }

            if (Interlocked.CompareExchange(ref _failuresRemaining, current - 1, current) == current)
            {
                remaining = current - 1;
                return true;
            }
        }
    }
}

