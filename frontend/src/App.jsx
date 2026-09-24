import { useState, useEffect } from 'react';

const API_BASE = 'http://localhost:5090/api';

const generateGuid = () => crypto.randomUUID();

function App() {
  const [zones, setZones] = useState([]);
  const [violationTypes, setViolationTypes] = useState([]);
  const [log, setLog] = useState([]);

  // Zone creation state
  const [newZoneName, setNewZoneName] = useState('');
  const [newZoneType, setNewZoneType] = useState(0);
  const [zoneStart, setZoneStart] = useState('');
  const [zoneEnd, setZoneEnd] = useState('');
  const [zoneDays, setZoneDays] = useState([]);

  // Street creation state
  const [selectedZoneId, setSelectedZoneId] = useState('');
  const [newStreetName, setNewStreetName] = useState('');
  const [streetStart, setStreetStart] = useState('');
  const [streetEnd, setStreetEnd] = useState('');
  const [streetDays, setStreetDays] = useState([]);

  // Resident card state
  const [rcRegNumber, setRcRegNumber] = useState('');
  const [rcStreetId, setRcStreetId] = useState('');
  const [rcValidFrom, setRcValidFrom] = useState(new Date().toISOString().slice(0, 10));
  const [rcValidTo, setRcValidTo] = useState(new Date(new Date().setFullYear(new Date().getFullYear() + 1)).toISOString().slice(0, 10));

  // Inspection state
  const [inspectorId, setInspectorId] = useState('');
  const [sessionId, setSessionId] = useState(null);
  const [inspectionZoneId, setInspectionZoneId] = useState('');
  const [inspectionStreetId, setInspectionStreetId] = useState('');
  const [inspectionRegNumber, setInspectionRegNumber] = useState('');
  const [currentInspectionId, setCurrentInspectionId] = useState(null);
  const [selectedViolationTypeId, setSelectedViolationTypeId] = useState('');

  const daysOfWeek = [
    { value: 1, label: 'Pn' }, { value: 2, label: 'Wt' }, { value: 3, label: 'Śr' },
    { value: 4, label: 'Cz' }, { value: 5, label: 'Pt' }, { value: 6, label: 'Sb' }, { value: 0, label: 'Nd' }
  ];

  const addLog = (msg) => {
    setLog(prev => [`[${new Date().toLocaleTimeString()}] ${msg}`, ...prev]);
  };

  const fetchZones = async () => {
    try {
      const res = await fetch(`${API_BASE}/zones`);
      const data = await res.json();
      setZones(data);
    } catch (e) { addLog(`Błąd pobierania stref: ${e.message}`); }
  };

  const fetchViolationTypes = async () => {
    try {
      const res = await fetch(`${API_BASE}/violation-types`);
      const data = await res.json();
      setViolationTypes(data);
    } catch (e) { addLog(`Błąd pobierania naruszeń: ${e.message}`); }
  };

  useEffect(() => {
    fetchZones();
    fetchViolationTypes();
  }, []);

  const handleCreateZone = async (e) => {
    e.preventDefault();
    const payload = { 
      name: newZoneName, 
      type: parseInt(newZoneType),
      startTime: zoneStart ? `${zoneStart}:00` : null,
      endTime: zoneEnd ? `${zoneEnd}:00` : null,
      paidDays: zoneDays.length > 0 ? zoneDays : null
    };
    try {
      const res = await fetch(`${API_BASE}/zones`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });
      if (res.ok) {
        const data = await res.json();
        addLog(`[OK] Utworzono strefę: ${data.id}`);
        setNewZoneName('');
        fetchZones();
      } else { addLog(`[BŁĄD] Tworzenie strefy: ${res.statusText}`); }
    } catch (e) { addLog(`[BŁĄD] Tworzenie strefy: ${e.message}`); }
  };

  const handleCreateStreet = async (e) => {
    e.preventDefault();
    const payload = { 
      name: newStreetName,
      startTime: streetStart ? `${streetStart}:00` : null,
      endTime: streetEnd ? `${streetEnd}:00` : null,
      paidDays: streetDays.length > 0 ? streetDays : null
    };
    try {
      const res = await fetch(`${API_BASE}/zones/${selectedZoneId}/streets`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });
      if (res.ok) {
        const data = await res.json();
        addLog(`[OK] Dodano ulicę: ${data.id}`);
        setNewStreetName('');
        fetchZones();
      } else {
        const err = await res.json();
        addLog(`[BŁĄD] Dodawanie ulicy: ${err.Error || res.statusText}`);
      }
    } catch (e) { addLog(`[BŁĄD] Dodawanie ulicy: ${e.message}`); }
  };

  const handleIssueCard = async (e) => {
    e.preventDefault();
    try {
      const res = await fetch(`${API_BASE}/resident-cards`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          registrationNumber: rcRegNumber,
          streetId: rcStreetId,
          validFrom: new Date(rcValidFrom).toISOString(),
          validTo: new Date(rcValidTo).toISOString()
        })
      });
      if (res.ok) { addLog(`[OK] Wydano kartę mieszkańca dla ${rcRegNumber}`); }
      else { addLog(`[BŁĄD] Wydawanie karty: ${res.statusText}`); }
    } catch (e) { addLog(`[BŁĄD] Wydawanie karty: ${e.message}`); }
  };

  // --- INSPECTION FLOW ---
  const registerInspector = async () => {
    try {
      const res = await fetch(`${API_BASE}/inspectors`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ firstName: "Jan", lastName: "Kowalski" })
      });
      if (res.ok) {
        const data = await res.json();
        setInspectorId(data.id);
        addLog(`[INSPEKTOR] Zarejestrowano w systemie: ${data.id}`);
      } else {
        addLog(`[BŁĄD] Rejestracja inspektora`);
      }
    } catch (e) { addLog(`[BŁĄD] ${e.message}`); }
  };
  const startSession = async () => {
    try {
      const res = await fetch(`${API_BASE}/sessions`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ inspectorId })
      });
      if (res.ok) {
        const id = await res.json();
        setSessionId(id);
        addLog(`[SESJA] Rozpoczęto sesję: ${id}`);
      }
    } catch (e) { addLog(`[BŁĄD] ${e.message}`); }
  };

  const setLocation = async () => {
    try {
      const res = await fetch(`${API_BASE}/sessions/${sessionId}/street`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ zoneId: inspectionZoneId, streetId: inspectionStreetId })
      });
      if (res.ok) addLog(`[SESJA] Ustawiono lokalizację kontroli`);
    } catch (e) { addLog(`[BŁĄD] ${e.message}`); }
  };

  const startInspection = async () => {
    try {
      const res = await fetch(`${API_BASE}/inspections`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ sessionId, registrationNumber: inspectionRegNumber, latitude: 50.06143, longitude: 19.93658 })
      });
      if (res.ok) {
        const id = await res.json();
        setCurrentInspectionId(id);
        addLog(`[KONTROLA] Rozpoczęto sprawdzanie auta ${inspectionRegNumber}`);
      }
    } catch (e) { addLog(`[BŁĄD] ${e.message}`); }
  };

  const checkTicket = async () => {
    try {
      const res = await fetch(`${API_BASE}/inspections/${currentInspectionId}/check-ticket`, { method: 'POST' });
      if (res.ok) {
        const result = await res.json();
        addLog(`[KONTROLA] Bilet: ${result.isValid ? 'WAŻNY (' + result.providerMessage + ')' : 'NIEWAŻNY'}`);
      }
    } catch (e) { addLog(`[BŁĄD] ${e.message}`); }
  };

  const addViolation = async () => {
    try {
      const res = await fetch(`${API_BASE}/inspections/${currentInspectionId}/violations`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ violationTypeId: selectedViolationTypeId, evidencePhotoIds: [] })
      });
      if (res.ok) addLog(`[KONTROLA] Dodano naruszenie.`);
    } catch (e) { addLog(`[BŁĄD] ${e.message}`); }
  };

  const issueNotice = async () => {
    try {
      const res = await fetch(`${API_BASE}/inspections/${currentInspectionId}/notice`, { method: 'POST' });
      if (res.ok) {
        const id = await res.json();
        addLog(`[WEZWANIE] Wystawiono mandat! ID: ${id}`);
        setCurrentInspectionId(null);
        setInspectionRegNumber('');
      }
    } catch (e) { addLog(`[BŁĄD] ${e.message}`); }
  };

  const toggleDay = (day, state, setState) => {
    if (state.includes(day)) setState(state.filter(d => d !== day));
    else setState([...state, day]);
  };

  const DayPicker = ({ state, setState }) => (
    <div className="flex gap-2 mt-1">
      {daysOfWeek.map(d => (
        <label key={d.value} className="flex items-center space-x-1 text-xs">
          <input type="checkbox" checked={state.includes(d.value)} onChange={() => toggleDay(d.value, state, setState)} />
          <span>{d.label}</span>
        </label>
      ))}
    </div>
  );

  return (
    <div className="min-h-screen bg-gray-50 text-gray-800 p-8 font-sans">
      <div className="max-w-7xl mx-auto grid grid-cols-1 lg:grid-cols-4 gap-8">

        <div className="lg:col-span-3 space-y-8">
          <h1 className="text-3xl font-bold text-blue-600">PTickets Admin Panel & Terminal</h1>

          {/* Zones List */}
          <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-100">
            <h2 className="text-xl font-semibold mb-4 border-b pb-2">Istniejące Strefy</h2>
            {zones.length === 0 ? <p className="text-gray-500">Brak stref.</p> : (
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {zones.map(z => (
                  <div key={z.id} className="p-4 bg-gray-50 rounded border border-gray-200">
                    <div className="flex justify-between items-center mb-2">
                      <strong className="text-lg">{z.name}</strong>
                      <span className="text-xs bg-blue-100 text-blue-800 px-2 py-1 rounded">
                        {z.type === 1 ? 'Single' : 'Multi'}
                      </span>
                    </div>
                    {z.schedule && (
                      <div className="text-xs text-blue-600 mb-2 font-mono">
                        Harmonogram strefy: {z.schedule.startTime} - {z.schedule.endTime} (Dni: {z.schedule.paidDays.join(',')})
                      </div>
                    )}
                    {z.streets && z.streets.length > 0 && (
                      <ul className="list-disc pl-5 space-y-1 mt-2">
                        {z.streets.map(s => (
                          <li key={s.id} className="text-sm">
                            {s.name}
                            {s.schedule && <span className="ml-2 text-xs text-orange-500">(Własne: {s.schedule.startTime}-{s.schedule.endTime})</span>}
                          </li>
                        ))}
                      </ul>
                    )}
                  </div>
                ))}
              </div>
            )}
            <button onClick={fetchZones} className="mt-4 px-4 py-2 bg-gray-100 hover:bg-gray-200 rounded text-sm">Odśwież</button>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            {/* Create Zone */}
            <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-100">
              <h2 className="text-xl font-semibold mb-4 border-b pb-2">1. Utwórz Strefę</h2>
              <form onSubmit={handleCreateZone} className="space-y-4">
                <input type="text" placeholder="Nazwa" value={newZoneName} onChange={e => setNewZoneName(e.target.value)} required className="w-full p-2 border rounded" />
                <select value={newZoneType} onChange={e => setNewZoneType(e.target.value)} className="w-full p-2 border rounded">
                  <option value={0}>MultiStreet (wiele ulic)</option>
                  <option value={1}>Single (jedna ulica auto-generowana)</option>
                </select>
                <div className="p-3 bg-gray-50 rounded border">
                  <span className="text-xs font-semibold">Opcjonalny Harmonogram (Domyślny)</span>
                  <div className="flex gap-2 mt-2">
                    <input type="time" value={zoneStart} onChange={e => setZoneStart(e.target.value)} className="p-1 border rounded text-sm w-full" />
                    <input type="time" value={zoneEnd} onChange={e => setZoneEnd(e.target.value)} className="p-1 border rounded text-sm w-full" />
                  </div>
                  <DayPicker state={zoneDays} setState={setZoneDays} />
                </div>
                <button type="submit" className="w-full bg-blue-600 text-white p-2 rounded">Utwórz Strefę</button>
              </form>
            </div>

            {/* Create Street */}
            <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-100">
              <h2 className="text-xl font-semibold mb-4 border-b pb-2">2. Dodaj Ulicę (MultiStreet)</h2>
              <form onSubmit={handleCreateStreet} className="space-y-4">
                <select value={selectedZoneId} onChange={e => setSelectedZoneId(e.target.value)} required className="w-full p-2 border rounded">
                  <option value="" disabled>-- Wybierz strefę --</option>
                  {zones.filter(z => z.type === 0).map(z => <option key={z.id} value={z.id}>{z.name}</option>)}
                </select>
                <input type="text" placeholder="Nazwa Ulicy" value={newStreetName} onChange={e => setNewStreetName(e.target.value)} required className="w-full p-2 border rounded" />
                <div className="p-3 bg-gray-50 rounded border">
                  <span className="text-xs font-semibold">Nadpisz Harmonogram (Zostaw puste by odziedziczyć)</span>
                  <div className="flex gap-2 mt-2">
                    <input type="time" value={streetStart} onChange={e => setStreetStart(e.target.value)} className="p-1 border rounded text-sm w-full" />
                    <input type="time" value={streetEnd} onChange={e => setStreetEnd(e.target.value)} className="p-1 border rounded text-sm w-full" />
                  </div>
                  <DayPicker state={streetDays} setState={setStreetDays} />
                </div>
                <button type="submit" className="w-full bg-blue-600 text-white p-2 rounded">Dodaj Ulicę</button>
              </form>
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            {/* Issue Resident Card */}
            <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-100">
              <h2 className="text-xl font-semibold mb-4 border-b pb-2">3. Wydaj Kartę Mieszkańca</h2>
              <form onSubmit={handleIssueCard} className="space-y-4">
                <input type="text" placeholder="Rejestracja (np. KR123)" value={rcRegNumber} onChange={e => setRcRegNumber(e.target.value)} required className="w-full p-2 border rounded" />
                <select value={rcStreetId} onChange={e => setRcStreetId(e.target.value)} required className="w-full p-2 border rounded">
                  <option value="" disabled>-- Wybierz ulicę --</option>
                  {zones.flatMap(z => z.streets.map(s => <option key={s.id} value={s.id}>{z.name} - {s.name}</option>))}
                </select>
                <div className="flex gap-2">
                  <input type="date" value={rcValidFrom} onChange={e => setRcValidFrom(e.target.value)} required className="w-full p-2 border rounded" />
                  <input type="date" value={rcValidTo} onChange={e => setRcValidTo(e.target.value)} required className="w-full p-2 border rounded" />
                </div>
                <button type="submit" className="w-full bg-green-600 text-white p-2 rounded font-bold">Wydaj Kartę</button>
              </form>
            </div>

            {/* INSPECTOR TERMINAL */}
            <div className="bg-gray-800 text-white p-6 rounded-lg shadow-sm border border-gray-700">
              <h2 className="text-xl font-semibold mb-4 border-b border-gray-600 pb-2 text-yellow-400">Terminal Kontrolera</h2>
              
              {!inspectorId ? (
                <div className="space-y-4">
                  <p className="text-sm text-gray-300">Wymagane zalogowanie Inspektora</p>
                  <button onClick={registerInspector} className="w-full bg-blue-600 text-white font-bold p-3 rounded hover:bg-blue-700">
                    Stwórz i zaloguj Inspektora (Kowalski)
                  </button>
                </div>
              ) : !sessionId ? (
                <div className="space-y-4">
                  <div className="bg-gray-700 p-3 rounded text-sm text-green-400">Zalogowano: {inspectorId.substring(0,8)}</div>
                  <button onClick={startSession} className="w-full bg-yellow-500 text-black font-bold p-3 rounded hover:bg-yellow-600">
                    Rozpocznij Szychę (Sesję)
                  </button>
                </div>
              ) : (
                <div className="space-y-4">
                  <div className="bg-gray-700 p-3 rounded text-sm">Sesja: {sessionId.substring(0,8)}...</div>
                  
                  <div className="flex gap-2">
                    <select value={inspectionZoneId} onChange={e => setInspectionZoneId(e.target.value)} className="w-full p-2 bg-gray-900 border border-gray-600 rounded text-sm">
                      <option value="">-- Strefa --</option>
                      {zones.map(z => <option key={z.id} value={z.id}>{z.name}</option>)}
                    </select>
                    <select value={inspectionStreetId} onChange={e => setInspectionStreetId(e.target.value)} className="w-full p-2 bg-gray-900 border border-gray-600 rounded text-sm">
                      <option value="">-- Ulica --</option>
                      {inspectionZoneId && zones.find(z => z.id === inspectionZoneId)?.streets.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
                    </select>
                  </div>
                  <button onClick={setLocation} className="w-full bg-gray-600 p-2 rounded text-sm">Ustaw Lokalizację GPS</button>

                  <hr className="border-gray-600" />
                  
                  {!currentInspectionId ? (
                    <div className="flex gap-2">
                      <input type="text" placeholder="Rejestracja" value={inspectionRegNumber} onChange={e => setInspectionRegNumber(e.target.value.toUpperCase())} className="w-full p-2 bg-gray-900 border border-gray-600 rounded" />
                      <button onClick={startInspection} className="bg-blue-600 px-4 rounded font-bold">Sprawdź</button>
                    </div>
                  ) : (
                    <div className="space-y-3 bg-gray-900 p-4 rounded border border-gray-600">
                      <div className="text-center text-xl font-mono text-yellow-400 tracking-widest">{inspectionRegNumber}</div>
                      <button onClick={checkTicket} className="w-full bg-blue-600 p-2 rounded">Weryfikuj Opłatę / Bilet</button>
                      
                      <div className="flex gap-2 mt-4">
                        <select value={selectedViolationTypeId} onChange={e => setSelectedViolationTypeId(e.target.value)} className="w-full p-2 bg-gray-800 border border-gray-600 rounded text-sm">
                          <option value="">-- Naruszenie --</option>
                          {violationTypes.map(v => <option key={v.id} value={v.id}>{v.name}</option>)}
                        </select>
                        <button onClick={addViolation} className="bg-orange-600 px-3 rounded text-sm">Dodaj</button>
                      </div>
                      
                      <button onClick={issueNotice} className="w-full bg-red-600 text-white font-bold p-3 rounded mt-2">Wystaw Mandat (Zawiadomienie)</button>
                    </div>
                  )}
                </div>
              )}
            </div>
          </div>

        </div>

        {/* Logs sidebar */}
        <div className="bg-gray-900 text-gray-100 p-6 rounded-lg shadow-sm h-[80vh] overflow-y-auto sticky top-8">
          <h2 className="text-xl font-semibold mb-4 border-b border-gray-700 pb-2 flex justify-between items-center">
            Konsola Logów
            <button onClick={() => setLog([])} className="text-xs bg-gray-700 hover:bg-gray-600 px-2 py-1 rounded">Wyczyść</button>
          </h2>
          <div className="space-y-2 font-mono text-xs leading-relaxed">
            {log.length === 0 ? <p className="text-gray-500">Brak logów API.</p> : (
              log.map((msg, i) => (
                <div key={i} className={`pb-2 border-b border-gray-800 
                  ${msg.includes('[BŁĄD]') ? 'text-red-400' :
                    msg.includes('[KONTROLA]') ? 'text-blue-300' :
                      msg.includes('[WEZWANIE]') ? 'text-yellow-400' : 'text-green-400'}`}>
                  {msg}
                </div>
              ))
            )}
          </div>
        </div>

      </div>
    </div>
  );
}

export default App;

