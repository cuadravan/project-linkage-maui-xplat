using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLinkage.Models
{
    public static class CebuLocationCoordinates
    {
        public static readonly Dictionary<CebuLocation, (double Latitude, double Longitude)> Map = new()
    {
        // Highly Urbanized Cities
        { CebuLocation.CebuCity, (10.3157, 123.8854) },
        { CebuLocation.MandaueCity, (10.3400, 123.9500) },
        { CebuLocation.LapuLapuCity, (10.3103, 123.9495) },
        { CebuLocation.TalisayCity, (10.2447, 123.8494) },

        // Component Cities
        { CebuLocation.CarcarCity, (10.1060, 123.6400) },
        { CebuLocation.DanaoCity, (10.5200, 124.0200) },
        { CebuLocation.NagaCity, (10.2096, 123.7579) },
        { CebuLocation.ToledoCity, (10.3773, 123.6380) },
        { CebuLocation.BogoCity, (11.0500, 124.0000) },

        // Municipalities
        { CebuLocation.Alcantara, (9.9792, 123.3983) },
        { CebuLocation.Alcoy, (9.7197, 123.5153) },
        { CebuLocation.Alegria, (9.7297, 123.3422) },
        { CebuLocation.Aloguinsan, (10.2225, 123.5481) },
        { CebuLocation.Argao, (9.8797, 123.6044) },
        { CebuLocation.Asturias, (10.5650, 123.7167) },
        { CebuLocation.Badian, (9.8700, 123.3900) },
        { CebuLocation.Balamban, (10.4833, 123.7167) },
        { CebuLocation.Bantayan, (11.1500, 123.7500) },
        { CebuLocation.Barili, (10.1175, 123.5117) },
        { CebuLocation.Boljoon, (9.6283, 123.4725) },
        { CebuLocation.Borbon, (10.7833, 123.9833) },
        { CebuLocation.Carmen, (10.6333, 124.0000) },
        { CebuLocation.Catmon, (10.7167, 124.0000) },
        { CebuLocation.Compostela, (10.4500, 124.0000) },
        { CebuLocation.Consolacion, (10.4000, 123.9500) },
        { CebuLocation.Cordova, (10.2500, 123.9500) },
        { CebuLocation.Daanbantayan, (11.2500, 124.0000) },
        { CebuLocation.Dalaguete, (9.7667, 123.5333) },
        { CebuLocation.Dumanjug, (10.0500, 123.5000) },
        { CebuLocation.Ginatilan, (9.5833, 123.3333) },
        { CebuLocation.Liloan, (10.4000, 124.0000) },
        { CebuLocation.Madridejos, (11.2500, 123.7333) },
        { CebuLocation.Malabuyoc, (9.6167, 123.4000) },
        { CebuLocation.Medellin, (11.0000, 124.0000) },
        { CebuLocation.Minglanilla, (10.2333, 123.8000) },
        { CebuLocation.Moalboal, (9.9500, 123.4000) },
        { CebuLocation.Oslob, (9.5206, 123.4318) },
        { CebuLocation.Pilar, (10.6333, 124.4000) },
        { CebuLocation.Pinamungajan, (10.2500, 123.5833) },
        { CebuLocation.Poro, (10.6333, 124.4000) },
        { CebuLocation.Ronda, (9.9667, 123.4333) },
        { CebuLocation.Samboan, (9.5167, 123.3000) },
        { CebuLocation.SanFernando, (10.1667, 123.7000) },
        { CebuLocation.SanFrancisco, (10.6333, 124.4000) },
        { CebuLocation.SanRemigio, (11.0500, 123.9500) },
        { CebuLocation.SantaFe, (11.1333, 123.8000) },
        { CebuLocation.Santander, (9.3333, 123.3333) },
        { CebuLocation.Sibonga, (10.0167, 123.6333) },
        { CebuLocation.Sogod, (10.7500, 124.0000) },
        { CebuLocation.Tabogon, (10.9500, 124.0000) },
        { CebuLocation.Tabuelan, (10.7333, 123.8500) },
        { CebuLocation.Tuburan, (10.7333, 123.8333) },
        { CebuLocation.Tudela, (10.6333, 124.4000) }
    };
    }


    public enum CebuLocation
    {
        // Highly Urbanized Cities
        CebuCity,
        MandaueCity,
        LapuLapuCity,
        TalisayCity,

        // Component Cities
        CarcarCity,
        DanaoCity,
        NagaCity,
        ToledoCity,
        BogoCity,

        // Municipalities
        Alcantara,
        Alcoy,
        Alegria,
        Aloguinsan,
        Argao,
        Asturias,
        Badian,
        Balamban,
        Bantayan,
        Barili,
        Boljoon,
        Borbon,
        Carmen,
        Catmon,
        Compostela,
        Consolacion,
        Cordova,
        Daanbantayan,
        Dalaguete,
        Dumanjug,
        Ginatilan,
        Liloan,
        Madridejos,
        Malabuyoc,
        Medellin,
        Minglanilla,
        Moalboal,
        Oslob,
        Pilar,
        Pinamungajan,
        Poro,
        Ronda,
        Samboan,
        SanFernando,
        SanFrancisco,
        SanRemigio,
        SantaFe,
        Santander,
        Sibonga,
        Sogod,
        Tabogon,
        Tabuelan,
        Tuburan,
        Tudela
    }
}
