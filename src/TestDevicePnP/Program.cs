var xx = new Models.Solar_Panel_v1_1_7k6
{
    PowerAmountKW = 33,
    Temperature = 50,
    EnergyAmountkWh = 33,
    NominalVoltage = 22,
    Efficiency = 0,
    PanelStatus = -1
};

xx.Do();

Console.ReadLine();

namespace Models
{
    public partial class Solar_Panel_v1_1_7k6
    {
        public void Do()
        {
            this.EnergyAmountkWh++;
        }
    }
}
