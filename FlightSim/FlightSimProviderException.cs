using System.Collections.ObjectModel;

namespace FlightSim
{
    public sealed class FlightSimProviderException : Exception
    {
        public string Provider { get; }
        public string? Code { get; }
        public IReadOnlyDictionary<string, object?> DataBag { get; }

        public FlightSimProviderException(
            string provider,
            string message,
            string? code = null,
            Exception? innerException = null,
            IReadOnlyDictionary<string, object?>? dataBag = null)
            : base($"[{provider}]{(code != null ? $"[{code}]" : "")} {message}", innerException)
        {
            Provider = provider;
            Code = code;
            DataBag = dataBag ?? new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>());
        }
    }
}
