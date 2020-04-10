using System.Linq;
using LiveCharts;
using LiveCharts.Configurations;
using MARGO.BL;
using System;
using System.Collections.Generic;
using System.Text;

namespace MARGO.ViewModels
{
    public class StatisticsVM : ObservableBase
    {
        static StatisticsVM()
        {
            Charting.For<KeyValuePair<int, int>>(Mappers.Xy<KeyValuePair<int, int>>().X(pair => pair.Key).Y(pair => pair.Value));
            Charting.For<KeyValuePair<byte, int>>(Mappers.Xy<KeyValuePair<byte, int>>().X(pair => pair.Key).Y(pair => pair.Value));
        }

        private Project Project => Project.Instance;

        public string CurrentSeriesName { get; set; }

        public IChartValues CurrentSeries
            => CurrentSeriesName switch
            {
                "RAW" => new ChartValues<KeyValuePair<int, int>>(Project.RAW is null ? Enumerable.Empty<KeyValuePair<int, int>>() : Project.RAW.Stats),
                "BYTES" => new ChartValues<KeyValuePair<byte, int>>(Project.BYTES is null ? Enumerable.Empty<KeyValuePair<byte, int>>() : Project.BYTES.Stats),
                "LOGGED" => new ChartValues<KeyValuePair<byte, int>>(Project.LOGGED is null ? Enumerable.Empty<KeyValuePair<byte, int>>() : Project.LOGGED.Stats),
                _ => null
            };

        // TODO private void InitZoom() { }
    }
}