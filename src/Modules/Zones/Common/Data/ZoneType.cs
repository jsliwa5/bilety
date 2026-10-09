namespace PTickets.Modules.Zones.Common.Data;

public enum ZoneType
{
    /// <summary>
    /// Strefa z wieloma ulicami (Standardowa). MoĹĽna do niej dodawaÄ‡ wiele ulic.
    /// </summary>
    MultiStreet = 0,
    
    /// <summary>
    /// Pojedyncza strefa. Automatycznie tworzy jednÄ… ulicÄ™ (reprezentujÄ…cÄ… caĹ‚Ä… strefÄ™) i blokuje dodawanie kolejnych.
    /// </summary>
    Single = 1
}


