import React, { useState, useEffect } from 'react';
import { UserCheck, MapPin, Camera, FileText, CheckCircle, Search, AlertCircle, Plus } from 'lucide-react';

const API_BASE = 'http://localhost:5090/api';

export default function InspectorDashboard() {
  const [step, setStep] = useState(0);
  const [inspectorId, setInspectorId] = useState('');
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  
  const [sessionId, setSessionId] = useState('');
  const [inspectionId, setInspectionId] = useState('');
  const [registration, setRegistration] = useState('');
  
  // Dane referencyjne
  const [zones, setZones] = useState([]);
  const [violations, setViolations] = useState([]);

  // Wybory
  const [selectedZone, setSelectedZone] = useState('');
  const [selectedStreet, setSelectedStreet] = useState('');
  const [selectedViolation, setSelectedViolation] = useState('');
  
  const [ticketStatus, setTicketStatus] = useState(null);

  useEffect(() => {
    fetch(`${API_BASE}/zones`).then(r => r.json()).then(setZones).catch(console.error);
    fetch(`${API_BASE}/violation-types`).then(r => r.json()).then(setViolations).catch(console.error);
  }, []);

  const createInspector = async (e) => {
    e.preventDefault();
    try {
      const res = await fetch(`${API_BASE}/inspectors`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ firstName, lastName })
      });
      if (res.ok) {
        const data = await res.json();
        // Sprawdź czy odpowiedź ma pole inspectorId lub id
        const id = data.inspectorId || data.id || data; 
        setInspectorId(id);
        setStep(1);
      } else {
        const err = await res.text();
        alert(`Błąd tworzenia inspektora: ${err}`);
      }
    } catch(e) { console.error(e); }
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
        setStep(2);
      }
    } catch(e) { console.error(e); }
  };

  const selectStreet = async () => {
    if(!selectedZone) return; 
    
    let streetIdToSelect = selectedStreet;
    const zoneObj = zones.find(z => (typeof z.id === 'object' ? z.id.value : z.id) === selectedZone);
    if (zoneObj && zoneObj.type === 1 && zoneObj.streets?.length > 0) {
      streetIdToSelect = typeof zoneObj.streets[0].id === 'object' ? zoneObj.streets[0].id.value : zoneObj.streets[0].id;
    }

    if (!streetIdToSelect) {
      alert("Proszę wybrać ulicę.");
      return;
    }

    try {
      const res = await fetch(`${API_BASE}/sessions/${sessionId}/street`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ zoneId: selectedZone, streetId: streetIdToSelect })
      });
      if (res.ok) {
        setStep(3);
      }
    } catch(e) { console.error(e); }
  };

  const startInspection = async (e) => {
    e.preventDefault();
    try {
      const res = await fetch(`${API_BASE}/inspections`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ 
          sessionId, 
          registrationNumber: registration, 
          latitude: 52.2297, 
          longitude: 21.0122 
        })
      });
      if (res.ok) {
        const id = await res.json();
        setInspectionId(id);
        setStep(4);
      }
    } catch(e) { console.error(e); }
  };

  const checkTicket = async () => {
    try {
      const res = await fetch(`${API_BASE}/inspections/${inspectionId}/check-ticket`, { method: 'POST' });
      if (res.ok) {
        const data = await res.json();
        setTicketStatus(data);
      }
    } catch(e) { console.error(e); }
  };

  const addViolation = async () => {
    if(!selectedViolation) return;
    try {
      const res = await fetch(`${API_BASE}/inspections/${inspectionId}/violations`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ violationTypeId: selectedViolation })
      });
      if(res.ok) {
        setStep(5);
      }
    } catch(e) { console.error(e); }
  };

  const finalizeInspection = async () => {
    try {
      await fetch(`${API_BASE}/inspections/${inspectionId}/photos`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ 
          photoUrls: ["http://example.com/photo1.jpg"],
          metadata: "Zrobiono zdjęcie"
        })
      });
      
      const res = await fetch(`${API_BASE}/inspections/${inspectionId}/notice`, { method: 'POST' });
      if (res.ok) {
        setStep(6);
      }
    } catch(e) { console.error(e); }
  };

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <header className="mb-8 text-center">
        <h1 className="text-3xl font-bold text-white mb-2">Panel Kontrolera</h1>
        <p className="text-slate-400">Przeprowadź kontrolę krok po kroku.</p>
      </header>

      {/* STEP 0 - Create Inspector */}
      {step === 0 && (
        <div className="glass-panel p-8 space-y-6 animate-in fade-in zoom-in duration-500 max-w-md mx-auto">
          <h2 className="text-2xl font-semibold flex items-center justify-center">
            <UserCheck className="mr-2 text-indigo-400" /> Rejestracja Inspektora
          </h2>
          <p className="text-slate-400 text-center">Wymagane jest utworzenie konta w systemie.</p>
          <form onSubmit={createInspector} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-slate-300 mb-1">Imię</label>
              <input type="text" className="glass-input w-full" value={firstName} onChange={e => setFirstName(e.target.value)} required />
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-300 mb-1">Nazwisko</label>
              <input type="text" className="glass-input w-full" value={lastName} onChange={e => setLastName(e.target.value)} required />
            </div>
            <button type="submit" className="btn-primary w-full flex justify-center items-center">
              <Plus size={18} className="mr-2" /> Zarejestruj
            </button>
          </form>
        </div>
      )}

      {/* STEP 1 */}
      {step === 1 && (
        <div className="glass-panel p-8 text-center space-y-6 animate-in fade-in zoom-in duration-500">
          <div className="mx-auto bg-indigo-500/20 w-24 h-24 rounded-full flex items-center justify-center">
            <UserCheck size={48} className="text-indigo-400" />
          </div>
          <h2 className="text-2xl font-semibold">Witaj, {firstName} {lastName}</h2>
          <p className="text-slate-400">Kliknij poniżej, aby rozpocząć nową sesję patrolową.</p>
          <button onClick={startSession} className="btn-primary w-full py-4 text-lg">
            Rozpocznij Sesję
          </button>
        </div>
      )}

      {/* STEP 2 */}
      {step === 2 && (() => {
        const zoneObj = zones.find(z => (typeof z.id === 'object' ? z.id.value : z.id) === selectedZone);
        const isMulti = zoneObj && zoneObj.type === 0;
        const canSubmit = selectedZone && (!isMulti || selectedStreet);
        
        return (
          <div className="glass-panel p-8 space-y-6 animate-in fade-in slide-in-from-right duration-500">
            <h2 className="text-2xl font-semibold flex items-center"><MapPin className="mr-2 text-indigo-400"/> Wybierz Rejon</h2>
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-slate-300 mb-1">Strefa Płatnego Parkowania</label>
                <select className="glass-input w-full" value={selectedZone} onChange={e => { setSelectedZone(e.target.value); setSelectedStreet(''); }}>
                  <option value="">-- Wybierz strefę --</option>
                  {zones.map(z => {
                    const val = typeof z.id === 'object' ? z.id.value : z.id;
                    return <option key={val} value={val}>{z.name}</option>;
                  })}
                </select>
              </div>

              {isMulti && (
                <div className="animate-in fade-in slide-in-from-top-2">
                  <label className="block text-sm font-medium text-slate-300 mb-1">Ulica</label>
                  <select className="glass-input w-full" value={selectedStreet} onChange={e => setSelectedStreet(e.target.value)}>
                    <option value="">-- Wybierz ulicę --</option>
                    {zoneObj.streets?.map(s => {
                      const sVal = typeof s.id === 'object' ? s.id.value : s.id;
                      return <option key={sVal} value={sVal}>{s.name}</option>;
                    })}
                  </select>
                </div>
              )}
            </div>
            
            <button onClick={selectStreet} className="btn-primary w-full" disabled={!canSubmit}>
              Zatwierdź Rejon
            </button>
          </div>
        );
      })()}

      {/* STEP 3 */}
      {step === 3 && (
        <div className="glass-panel p-8 space-y-6 animate-in fade-in slide-in-from-right duration-500">
          <h2 className="text-2xl font-semibold flex items-center"><Search className="mr-2 text-indigo-400"/> Nowa Kontrola</h2>
          <form onSubmit={startInspection} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-slate-300 mb-1">Numer Rejestracyjny Pojazdu</label>
              <input 
                type="text" 
                className="glass-input w-full text-2xl tracking-widest font-mono uppercase" 
                placeholder="KR 12345"
                value={registration}
                onChange={e => setRegistration(e.target.value.toUpperCase())}
                required
              />
            </div>
            <button type="submit" className="btn-primary w-full">
              Wprowadź Pojazd
            </button>
          </form>
        </div>
      )}

      {/* STEP 4 */}
      {step === 4 && (
        <div className="glass-panel p-8 space-y-6 animate-in fade-in slide-in-from-right duration-500">
          <h2 className="text-2xl font-semibold">Sprawdzenie Uprawnień: {registration}</h2>
          
          <div className="bg-slate-800/50 p-6 rounded-lg text-center">
            {ticketStatus === null ? (
              <button onClick={checkTicket} className="btn-secondary w-full">Odpytaj System Biletowy</button>
            ) : (
              <div className="space-y-4">
                {ticketStatus.hasValidTicket ? (
                   <div className="text-emerald-400 flex flex-col items-center">
                     <CheckCircle size={48} className="mb-2" />
                     <span className="text-xl">Pojazd posiada ważny bilet!</span>
                   </div>
                ) : (
                   <div className="text-rose-400 flex flex-col items-center">
                     <AlertCircle size={48} className="mb-2" />
                     <span className="text-xl">Brak ważnego biletu.</span>
                   </div>
                )}
              </div>
            )}
          </div>

          {ticketStatus && !ticketStatus.hasValidTicket && (
            <div className="space-y-4 border-t border-slate-700 pt-6">
              <label className="block text-sm font-medium text-slate-300">Wybierz Sposób Wykroczenia</label>
              <select className="glass-input w-full" value={selectedViolation} onChange={e => setSelectedViolation(e.target.value)}>
                <option value="">-- Wybierz --</option>
                {violations.map(v => {
                  const val = typeof v.id === 'object' ? v.id.value : v.id;
                  return <option key={val} value={val}>{v.name}</option>;
                })}
              </select>
              <button onClick={addViolation} className="btn-danger w-full" disabled={!selectedViolation}>
                Zarejestruj Wykroczenie
              </button>
            </div>
          )}

          {ticketStatus && ticketStatus.hasValidTicket && (
            <button onClick={() => { setStep(3); setRegistration(''); setTicketStatus(null); }} className="btn-primary w-full">
              Zakończ (Kolejny Pojazd)
            </button>
          )}
        </div>
      )}

      {/* STEP 5 */}
      {step === 5 && (
        <div className="glass-panel p-8 space-y-6 text-center animate-in fade-in slide-in-from-right duration-500">
          <h2 className="text-2xl font-semibold">Dokumentacja Wykroczenia</h2>
          <div className="mx-auto bg-slate-800 w-32 h-32 rounded-xl flex items-center justify-center border border-dashed border-slate-500">
            <Camera size={48} className="text-slate-400" />
          </div>
          <p className="text-slate-400">Wykonano zdjęcia (Symulacja)</p>
          <button onClick={finalizeInspection} className="btn-primary w-full py-3">
            Wystaw Zawiadomienie (Notice)
          </button>
        </div>
      )}

      {/* STEP 6 */}
      {step === 6 && (
        <div className="glass-panel p-8 space-y-6 text-center animate-in fade-in zoom-in duration-500 border border-emerald-500/50">
          <div className="mx-auto bg-emerald-500/20 w-24 h-24 rounded-full flex items-center justify-center">
            <FileText size={48} className="text-emerald-400" />
          </div>
          <h2 className="text-3xl font-semibold text-emerald-400">Zawiadomienie Wystawione!</h2>
          <p className="text-slate-300">Zawiadomienie zostało poprawnie zapisane w systemie.</p>
          <button onClick={() => { setStep(3); setRegistration(''); setTicketStatus(null); }} className="btn-secondary w-full">
            Kolejna Kontrola
          </button>
        </div>
      )}
    </div>
  );
}
