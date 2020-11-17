
namespace ProfileGraph
{
    using System.Collections.Generic;
    using OxyPlot;
    public class Grafik_uhod
    {
        public Grafik_uhod()
        {
            this.AxisX_max = 800;
            this.leftMin_1 = -100;
            this.leftMax_1 = 100;
            this.leftMin_2 = -100;
            this.leftMax_2 = 100;
            this.leftMin_3 = -100;
            this.leftMax_3 = 100;
            this.leftMin45 = -100;
            this.leftMax45 = 100;
            this.Title_1 = "Клеть 1";
            this.Title_2 = "Клеть 2";
            this.Title_3 = "Клеть 3";
            this.Title_45 = "Клеть 4, 5";
            this.Points_1 = new List<DataPoint> { new DataPoint(0, 0) };
            this.Points_2 = new List<DataPoint> { new DataPoint(0, 0) };
            this.Points_3 = new List<DataPoint> { new DataPoint(0, 0) };
            this.Points_4 = new List<DataPoint> { new DataPoint(0, 0) };
            this.Points_5 = new List<DataPoint> { new DataPoint(0, 0) };
            this.Points_0 = new List<DataPoint> { new DataPoint(0, 0), new DataPoint(3500, 0) };
            this.Points_100 = new List<DataPoint> { new DataPoint(0, 100), new DataPoint(3500, 100) };
            this.Points_m100 = new List<DataPoint> { new DataPoint(0, -100), new DataPoint(3500, -100) };
            this.Points_150 = new List<DataPoint> { new DataPoint(0, 150), new DataPoint(3500, 150) };
            this.Points_m150 = new List<DataPoint> { new DataPoint(0, -150), new DataPoint(3500, -150) };
            this.Points_200 = new List<DataPoint> { new DataPoint(0, 200), new DataPoint(3500, 200) };
            this.Points_m200 = new List<DataPoint> { new DataPoint(0, -200), new DataPoint(3500, -200) };
        }

        public int AxisX_max { get; set; }
        public int leftMin_1 { get; set; }
        public int leftMax_1 { get; set; }
        public int leftMin_2 { get; set; }
        public int leftMax_2 { get; set; }
        public int leftMin_3 { get; set; }
        public int leftMax_3 { get; set; }
        public int leftMin45 { get; set; }
        public int leftMax45 { get; set; }
        public string Title_1 { get; private set; }
        public string Title_2 { get; private set; }
        public string Title_3 { get; private set; }
        public string Title_45 { get; private set; }
        public IList<DataPoint> Points_1 { get; private set; }
        public IList<DataPoint> Points_2 { get; private set; }
        public IList<DataPoint> Points_3 { get; private set; }
        public IList<DataPoint> Points_4 { get; private set; }
        public IList<DataPoint> Points_5 { get; private set; }
        public IList<DataPoint> Points_0 { get; private set; }
        public IList<DataPoint> Points_100 { get; private set; }
        public IList<DataPoint> Points_m100 { get; private set; }
        public IList<DataPoint> Points_150 { get; private set; }
        public IList<DataPoint> Points_m150 { get; private set; }
        public IList<DataPoint> Points_200 { get; private set; }
        public IList<DataPoint> Points_m200 { get; private set; }
    }
}
