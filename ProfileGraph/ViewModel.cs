
namespace ProfileGraph
{
    using System.Collections.Generic;
    using OxyPlot;

    public class ViewModel
    {
        public ViewModel()
        {
            this.Grafik_u = new Grafik_uhod();
            this.Grafik_p_c = new Grafiki_profile_clin();
        }
        public Grafik_uhod Grafik_u { get; set; }
        public Grafiki_profile_clin Grafik_p_c { get; set; }  
    }

}
