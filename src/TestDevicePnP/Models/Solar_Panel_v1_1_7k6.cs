using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models;

public partial class Solar_Panel_v1_1_7k6
{
    int i = 0;
    int delta = 1;

    public void Update()
    {
        PowerAmountKW = i;
        Temperature = i;
        EnergyAmountkWh = i;
        NominalVoltage = i;
        Efficiency = i;
        PanelStatus = i;

        i += delta;
        if (i == 0 || i == 20) delta = -delta;
    }
}
