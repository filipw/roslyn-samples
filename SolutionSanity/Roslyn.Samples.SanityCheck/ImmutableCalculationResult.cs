namespace Roslyn.Samples.SanityCheck
{
    public class ImmutableCalculationResult : ICalculationResult
    {
        public int Total { get; private set; }

        public ImmutableCalculationResult(int total)
        {
            Total = total;
        }
    }
}