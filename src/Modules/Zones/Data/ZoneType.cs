namespace PTickets.Modules.Zones.Data;

public enum ZoneType
{
    /// <summary>
    /// Strefa z wieloma ulicami (Standardowa). Można do niej dodawać wiele ulic.
    /// </summary>
    MultiStreet = 0,
    
    /// <summary>
    /// Pojedyncza strefa. Automatycznie tworzy jedną ulicę (reprezentującą całą strefę) i blokuje dodawanie kolejnych.
    /// </summary>
    Single = 1
}

