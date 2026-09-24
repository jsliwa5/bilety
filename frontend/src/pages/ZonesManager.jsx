import React, { useState, useEffect } from 'react';
import { MapPin, Plus, Loader, CalendarClock } from 'lucide-react';

const API_BASE = 'http://localhost:5090/api';

const DAYS_OF_WEEK = [
  { value: 1, label: 'Pon' },
  { value: 2, label: 'Wt' },
  { value: 3, label: 'Śr' },
  { value: 4, label: 'Czw' },
  { value: 5, label: 'Pt' },
  { value: 6, label: 'Sob' },
  { value: 0, label: 'Niedz' }
];

export default function ZonesManager() {
  const [zones, setZones] = useState([]);
  const [loading, setLoading] = useState(true);
  
  // Forms state - Zone
  const [zoneName, setZoneName] = useState('');
  const [zoneType, setZoneType] = useState('0'); // 0=MultiStreet, 1=Single
  const [zoneStartTime, setZoneStartTime] = useState('08:00');
  const [zoneEndTime, setZoneEndTime] = useState('20:00');
  const [zonePaidDays, setZonePaidDays] = useState([1,2,3,4,5]);

  // Forms state - Street
  const [selectedZone, setSelectedZone] = useState('');
  const [streetName, setStreetName] = useState('');
  const [overrideSchedule, setOverrideSchedule] = useState(false);
  const [streetStartTime, setStreetStartTime] = useState('08:00');
  const [streetEndTime, setStreetEndTime] = useState('20:00');
  const [streetPaidDays, setStreetPaidDays] = useState([1,2,3,4,5]);

  useEffect(() => {
    fetchZones();
  }, []);

  const fetchZones = async () => {
    setLoading(true);
    try {
      const res = await fetch(`${API_BASE}/zones`);
      if (res.ok) {
        const data = await res.json();
        setZones(data);
      }
    } catch (e) {
      console.error(e);
    }
    setLoading(false);
  };

  const handleDayToggle = (dayValue, currentDays, setDays) => {
    if (currentDays.includes(dayValue)) {
      setDays(currentDays.filter(d => d !== dayValue));
    } else {
      setDays([...currentDays, dayValue]);
    }
  };

  const createZone = async (e) => {
    e.preventDefault();
    try {
      const res = await fetch(`${API_BASE}/zones`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          name: zoneName,
          type: parseInt(zoneType),
          startTime: zoneStartTime + ":00",
          endTime: zoneEndTime + ":00",
          paidDays: zonePaidDays
        })
      });
      if (res.ok) {
        setZoneName('');
        setZoneType('0');
        fetchZones();
      } else {
        const err = await res.text();
        alert(`Błąd: ${err}`);
      }
    } catch (e) {
      console.error(e);
    }
  };

  const createStreet = async (e) => {
    e.preventDefault();
    if (!selectedZone) return;
    try {
      const payload = {
        name: streetName,
        representsWholeZone: false
      };
      
      if (overrideSchedule) {
        payload.startTime = streetStartTime + ":00";
        payload.endTime = streetEndTime + ":00";
        payload.paidDays = streetPaidDays;
      }

      const res = await fetch(`${API_BASE}/zones/${selectedZone}/streets`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });
      
      if (res.ok) {
        setStreetName('');
        setSelectedZone('');
        setOverrideSchedule(false);
        alert('Dodano ulicę!');
        fetchZones();
      } else {
        const err = await res.text();
        alert(`Błąd: ${err}`);
      }
    } catch (e) {
      console.error(e);
    }
  };

  const renderDaysSelector = (currentDays, setDays) => (
    <div className="flex flex-wrap gap-2 mt-1">
      {DAYS_OF_WEEK.map(d => {
        const isSelected = currentDays.includes(d.value);
        return (
          <button
            key={d.value}
            type="button"
            onClick={() => handleDayToggle(d.value, currentDays, setDays)}
            className={`px-3 py-1 rounded-full text-sm font-medium transition-colors ${
              isSelected ? 'bg-indigo-500 text-white' : 'bg-slate-700/50 text-slate-400 hover:bg-slate-600'
            }`}
          >
            {d.label}
          </button>
        );
      })}
    </div>
  );

  const selectedZoneObj = zones.find(z => {
     const val = typeof z.id === 'object' ? z.id.value : z.id;
     return val === selectedZone;
  });

  // Zakładamy, że 1 to Single, 0 to MultiStreet.
  // Jeśli backend nie zwraca 'type', to spróbujemy użyć ew. właściwości, która o tym mówi.
  const isSingleZone = selectedZoneObj && selectedZoneObj.type === 1;

  return (
    <div className="space-y-6">
      <header className="mb-8">
        <h1 className="text-3xl font-bold text-white mb-2">Zarządzanie Strefami</h1>
        <p className="text-slate-400">Dodawaj nowe strefy (z harmonogramami) i przypisuj do nich ulice.</p>
      </header>

      <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
        {/* Add Zone */}
        <div className="glass-panel p-6">
          <h2 className="text-xl font-semibold mb-4 flex items-center">
            <MapPin className="mr-2 text-indigo-400" /> Dodaj Strefę
          </h2>
          <form onSubmit={createZone} className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-slate-300 mb-1">Nazwa Strefy</label>
                <input 
                  type="text" 
                  className="glass-input w-full" 
                  placeholder="np. Strefa A"
                  value={zoneName}
                  onChange={e => setZoneName(e.target.value)}
                  required
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-300 mb-1">Typ Strefy</label>
                <select className="glass-input w-full" value={zoneType} onChange={e => setZoneType(e.target.value)}>
                  <option value="0">Wieloulicowa (Standard)</option>
                  <option value="1">Pojedyncza (Ulica to Strefa)</option>
                </select>
              </div>
            </div>

            <div className="bg-slate-800/30 p-4 rounded-lg border border-slate-700 space-y-4">
              <h3 className="text-sm font-semibold text-slate-200 flex items-center">
                <CalendarClock size={16} className="mr-2 text-indigo-400" /> Harmonogram Płatności
              </h3>
              
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-xs font-medium text-slate-400 mb-1">Od (godzina)</label>
                  <input type="time" className="glass-input w-full" value={zoneStartTime} onChange={e => setZoneStartTime(e.target.value)} required />
                </div>
                <div>
                  <label className="block text-xs font-medium text-slate-400 mb-1">Do (godzina)</label>
                  <input type="time" className="glass-input w-full" value={zoneEndTime} onChange={e => setZoneEndTime(e.target.value)} required />
                </div>
              </div>

              <div>
                <label className="block text-xs font-medium text-slate-400 mb-1">Dni płatne</label>
                {renderDaysSelector(zonePaidDays, setZonePaidDays)}
              </div>
            </div>

            <button type="submit" className="btn-primary w-full flex justify-center items-center">
              <Plus size={18} className="mr-2" /> Utwórz Strefę
            </button>
          </form>
        </div>

        {/* Add Street */}
        <div className="glass-panel p-6 flex flex-col">
          <h2 className="text-xl font-semibold mb-4 flex items-center">
            <MapPin className="mr-2 text-purple-400" /> Dodaj Ulicę do Strefy
          </h2>
          
          <div className="space-y-4 flex-1">
            <div>
              <label className="block text-sm font-medium text-slate-300 mb-1">Strefa Docelowa</label>
              <select 
                className="glass-input w-full"
                value={selectedZone}
                onChange={e => setSelectedZone(e.target.value)}
                required
              >
                <option value="" disabled>Wybierz strefę...</option>
                {zones.map(z => {
                  const val = typeof z.id === 'object' ? z.id.value : z.id;
                  const typeLabel = z.type === 1 ? ' [Pojedyncza]' : '';
                  return <option key={val} value={val}>{z.name || val}{typeLabel}</option>;
                })}
              </select>
            </div>

            {isSingleZone ? (
               <div className="p-4 bg-rose-500/10 border border-rose-500/30 rounded-lg text-rose-300 text-sm">
                 Wybrana strefa jest typu <strong>Pojedynczego</strong>. Backend automatycznie utworzył dla niej ulicę i blokuje możliwość dodawania kolejnych.
               </div>
            ) : (
              <form onSubmit={createStreet} className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-slate-300 mb-1">Nazwa Ulicy</label>
                  <input 
                    type="text" 
                    className="glass-input w-full" 
                    placeholder="np. Floriańska"
                    value={streetName}
                    onChange={e => setStreetName(e.target.value)}
                    required
                  />
                </div>

                <div className="flex items-center mt-4">
                  <input 
                    type="checkbox" 
                    id="override" 
                    className="mr-2 h-4 w-4 text-purple-600 focus:ring-purple-500 border-slate-600 rounded bg-slate-700"
                    checked={overrideSchedule}
                    onChange={e => setOverrideSchedule(e.target.checked)}
                  />
                  <label htmlFor="override" className="text-sm font-medium text-slate-300">
                    Ulica posiada inny harmonogram płatności niż strefa
                  </label>
                </div>

                {overrideSchedule && (
                  <div className="bg-slate-800/30 p-4 rounded-lg border border-slate-700 space-y-4 mt-2 animate-in fade-in slide-in-from-top-2">
                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <label className="block text-xs font-medium text-slate-400 mb-1">Od (godzina)</label>
                        <input type="time" className="glass-input w-full" value={streetStartTime} onChange={e => setStreetStartTime(e.target.value)} required />
                      </div>
                      <div>
                        <label className="block text-xs font-medium text-slate-400 mb-1">Do (godzina)</label>
                        <input type="time" className="glass-input w-full" value={streetEndTime} onChange={e => setStreetEndTime(e.target.value)} required />
                      </div>
                    </div>
                    <div>
                      <label className="block text-xs font-medium text-slate-400 mb-1">Dni płatne</label>
                      {renderDaysSelector(streetPaidDays, setStreetPaidDays)}
                    </div>
                  </div>
                )}

                <button type="submit" className="btn-primary w-full flex justify-center items-center bg-purple-600 hover:bg-purple-500 shadow-purple-500/30 mt-4">
                  <Plus size={18} className="mr-2" /> Dodaj Ulicę
                </button>
              </form>
            )}
          </div>
        </div>
      </div>

      <div className="glass-panel p-6 mt-6">
        <h2 className="text-xl font-semibold mb-4">Lista Stref (Podgląd)</h2>
        {loading ? (
          <div className="flex justify-center p-8"><Loader className="animate-spin text-indigo-500" /></div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {zones.length === 0 ? (
              <p className="text-slate-400 col-span-full">Brak zdefiniowanych stref.</p>
            ) : (
              zones.map(z => {
                const val = typeof z.id === 'object' ? z.id.value : z.id;
                const typeName = z.type === 1 ? 'Pojedyncza (Ulica)' : 'Wieloulicowa';
                return (
                  <div key={val} className="p-4 bg-slate-800/50 rounded-lg border border-slate-700 flex flex-col justify-between">
                    <div>
                      <div className="flex justify-between items-start mb-2">
                        <h3 className="font-medium text-lg text-indigo-300">{z.name || `Strefa`}</h3>
                        <span className="text-xs px-2 py-1 bg-slate-700 rounded-full text-slate-300">{typeName}</span>
                      </div>
                      <p className="text-xs text-slate-500 font-mono mb-2 break-all">{val}</p>
                    </div>
                  </div>
                );
              })
            )}
          </div>
        )}
      </div>
    </div>
  );
}
