namespace PortalProject.Models
{
    public class GridModelList
    {
        public List<GridModel> GridData { get; set; }

        public GridCalculations Calculations { get; set; }
    }

    public class GridModel
    {
        public string Price { get; set; }
        public string Date { get; set; }
    }

    public class GridCalculations
    {
        public string Min { get; set; }
        public string Max { get; set; }
        public string Avg { get; set; }
        public string MostExpWindow { get; set; }
    }
}