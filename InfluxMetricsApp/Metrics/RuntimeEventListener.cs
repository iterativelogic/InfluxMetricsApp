using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using InfluxDB.Client;
using System.Diagnostics.Tracing;

namespace InfluxMetricsApp.Metrics
{
    public class RuntimeEventListener : EventListener
    {
        private const string CPU_USAGE = "cpu-usage";
        private const string WORKING_SET = "working-set";
        private readonly InfluxDBClient _influxClient;

        public RuntimeEventListener()
        {
            string token = "8CLJNig-9qYdL2494chuAh8krpaviNLohTSP8BMqbNb5hBaPhoR7hyuztlZfieIK4JEUzGgMRx29HbZGBglu3w==";
            _influxClient = new InfluxDBClient("http://localhost:8086", token);
        }


        protected override void OnEventSourceCreated(EventSource source)
        {
            Console.WriteLine(source.Name);
            if (source.Name != "System.Runtime")
                return;

            EnableEvents(source, EventLevel.Verbose, EventKeywords.All, new Dictionary<string, string>()
            {
                ["EventCounterIntervalSec"] = "1"
            });
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (!eventData.EventName.Equals("EventCounters")) 
                return;

            for (int i = 0; i < eventData.Payload.Count; ++i)
            {
                if (eventData.Payload[i] is IDictionary<string, object> eventPayload)
                {
                    bool hasEventName = eventPayload.TryGetValue("Name", out object eventNameItem);
                    
                    if (!hasEventName) 
                        return;

                    string eventName = eventNameItem.ToString();

                    if (eventName == CPU_USAGE)
                    {
                        var mean = eventPayload["Mean"];
                        Console.WriteLine($"{CPU_USAGE}: {mean}");
                        MeasureInfluxMetric(CPU_USAGE, (double)mean);
                    }

                    if (eventName == WORKING_SET)
                    {
                        var mean = eventPayload["Mean"];
                        Console.WriteLine($"{WORKING_SET}: {mean}");
                        MeasureInfluxMetric(WORKING_SET, (double)mean);
                    }
                }
            }
        }

        public void MeasureInfluxMetric(string metric, double value)
        {
            using var writeApi = _influxClient.GetWriteApi();

            var point = PointData.Measurement("resource_usage")
                    .Tag("resource", metric)
                    .Field("mean", value)
                    .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

            writeApi.WritePoint(point, "TestBucket", "Citi");
        }


        private static (string counterName, string counterValue) GetRelevantMetric(IDictionary<string, object> eventPayload)
        {
            var counterName = "";
            var counterValue = "";

            if (eventPayload.TryGetValue("DisplayName", out object displayValue))
            {
                counterName = displayValue.ToString();
            }
            if (eventPayload.TryGetValue("Mean", out object value) ||
                eventPayload.TryGetValue("Increment", out value))
            {
                counterValue = value.ToString();
            }

            return (counterName, counterValue);
        }

        public override void Dispose()
        {
            base.Dispose();
            _influxClient.Dispose();
        }
    }
}
