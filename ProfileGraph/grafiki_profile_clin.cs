
namespace ProfileGraph
{
    using System.Collections.Generic;
    using OxyPlot;

    public class Grafiki_profile_clin
    {
        public Grafiki_profile_clin()
        {
            this.AxisX_min = -1100;
            this.AxisX_max = 1100;
            this.AxisY_min = 2.3f;
            this.AxisY_max = 3.1f;
            this.Title_sred = "Средний";
            this.Title_actual = "Текущий";
            this.Points_sred = new List<DataPoint> { new DataPoint(0, 0) };
            this.Points_actual = new List<DataPoint> { new DataPoint(0, 0) };
            this.Points_plus10perc = new List<DataPoint> { new DataPoint(0, 0) };
            this.Points_minus10perc = new List<DataPoint> { new DataPoint(0, 0) };
            this.Points_set = new List<DataPoint> { new DataPoint(AxisX_min, 0), new DataPoint(AxisX_max, 0) };

        }
        public int AxisX_min { get; set; }
        public int AxisX_max { get; set; }
        public float AxisY_min { get; set; }
        public float AxisY_max { get; set; }
        public string Title_sred { get; private set; }
        public string Title_actual { get; private set; }
        public IList<DataPoint> Points_sred { get; private set; }
        public IList<DataPoint> Points_actual { get; private set; }
        public IList<DataPoint> Points_plus10perc { get; private set; }
        public IList<DataPoint> Points_minus10perc { get; private set; }
        public IList<DataPoint> Points_set { get; private set; }
    }
}
