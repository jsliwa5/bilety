import React, { useState, useEffect } from 'react';
import { AlertTriangle, Plus, Loader, DollarSign } from 'lucide-react';

const API_BASE = 'http://localhost:5090/api';

export default function ViolationsManager() {
  const [violations, setViolations] = useState([]);
  const [surcharges, setSurcharges] = useState([]);
  const [loading, setLoading] = useState(true);
  
  // Forms state
  const [newTypeName, setNewTypeName] = useState('');
  const [newTypeDesc, setNewTypeDesc] = useState('');
  const [selectedType, setSelectedType] = useState('');
  const [penaltyAmount, setPenaltyAmount] = useState('');
  
  const [minMinutes, setMinMinutes] = useState('');
  const [maxMinutes, setMaxMinutes] = useState('');
  const [surchargeAmount, setSurchargeAmount] = useState('');

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    setLoading(true);
    try {
      const [resV, resS] = await Promise.all([
        fetch(`${API_BASE}/violation-types`),
        fetch(`${API_BASE}/surcharge-tiers`)
      ]);
      
      if (resV.ok) {
        const data = await resV.json();
        setViolations(data);
      }
      if (resS.ok) {
        const data = await resS.json();
        setSurcharges(data);
      }
    } catch (e) {
      console.error(e);
    }
    setLoading(false);
  };

  const createViolationType = async (e) => {
    e.preventDefault();
    try {
      const res = await fetch(`${API_BASE}/violation-types`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name: newTypeName, description: newTypeDesc })
      });
      if (res.ok) {
        setNewTypeName('');
        setNewTypeDesc('');
        fetchData();
      }
    } catch (e) {
      console.error(e);
    }
  };

  const setPenalty = async (e) => {
    e.preventDefault();
    if (!selectedType) return;
    try {
      const res = await fetch(`${API_BASE}/violation-types/${selectedType}/penalty-amount`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ 
          amount: parseFloat(penaltyAmount), 
          effectiveFrom: new Date().toISOString() 
        })
      });
      if (res.ok) {
        setPenaltyAmount('');
        setSelectedType('');
        alert('Ustalono wysokość kary!');
      }
    } catch (e) {
      console.error(e);
    }
  };

  const addSurchargeTier = async (e) => {
    e.preventDefault();
    try {
      const res = await fetch(`${API_BASE}/surcharge-tiers`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ 
          minMinutes: parseInt(minMinutes), 
          maxMinutes: parseInt(maxMinutes), 
          amount: parseFloat(surchargeAmount) 
        })
      });
      if (res.ok) {
        setMinMinutes('');
        setMaxMinutes('');
        setSurchargeAmount('');
        fetchData();
      }
    } catch (e) {
      console.error(e);
    }
  };

  return (
    <div className="space-y-6">
      <header className="mb-8">
        <h1 className="text-3xl font-bold text-white mb-2">Wykroczenia i Kary</h1>
        <p className="text-slate-400">Zarządzaj sposobami wykroczeń oraz taryfikatorem.</p>
      </header>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {/* Create Violation Type */}
        <div className="glass-panel p-6">
          <h2 className="text-xl font-semibold mb-4 flex items-center">
            <AlertTriangle className="mr-2 text-rose-400" /> Dodaj Wykroczenie
          </h2>
          <form onSubmit={createViolationType} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-slate-300 mb-1">Nazwa</label>
              <input type="text" className="glass-input w-full" placeholder="np. Brak biletu"
                value={newTypeName} onChange={e => setNewTypeName(e.target.value)} required />
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-300 mb-1">Opis</label>
              <input type="text" className="glass-input w-full" placeholder="Krótki opis"
                value={newTypeDesc} onChange={e => setNewTypeDesc(e.target.value)} required />
            </div>
            <button type="submit" className="btn-danger w-full flex justify-center items-center">
              <Plus size={18} className="mr-2" /> Dodaj Sposób Wykroczenia
            </button>
          </form>
        </div>

        {/* Set Penalty */}
        <div className="glass-panel p-6">
          <h2 className="text-xl font-semibold mb-4 flex items-center">
            <DollarSign className="mr-2 text-emerald-400" /> Ustal Karę
          </h2>
          <form onSubmit={setPenalty} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-slate-300 mb-1">Wykroczenie</label>
              <select className="glass-input w-full" value={selectedType} onChange={e => setSelectedType(e.target.value)} required>
                <option value="" disabled>Wybierz...</option>
                {violations.map(v => {
                  const val = typeof v.id === 'object' ? v.id.value : v.id;
                  return <option key={val} value={val}>{v.name}</option>;
                })}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-300 mb-1">Kwota (PLN)</label>
              <input type="number" step="0.01" className="glass-input w-full" placeholder="250.00"
                value={penaltyAmount} onChange={e => setPenaltyAmount(e.target.value)} required />
            </div>
            <button type="submit" className="btn-primary bg-emerald-600 hover:bg-emerald-500 shadow-emerald-500/30 w-full flex justify-center items-center">
              <Plus size={18} className="mr-2" /> Zapisz Karę
            </button>
          </form>
        </div>
      </div>
      
      <div className="glass-panel p-6 mt-6">
        <h2 className="text-xl font-semibold mb-4">Dodaj Próg Dopłaty (Surcharge)</h2>
        <form onSubmit={addSurchargeTier} className="grid grid-cols-1 md:grid-cols-4 gap-4 items-end">
           <div>
              <label className="block text-sm font-medium text-slate-300 mb-1">Min. Minuty</label>
              <input type="number" className="glass-input w-full" value={minMinutes} onChange={e => setMinMinutes(e.target.value)} required />
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-300 mb-1">Max. Minuty</label>
              <input type="number" className="glass-input w-full" value={maxMinutes} onChange={e => setMaxMinutes(e.target.value)} required />
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-300 mb-1">Kwota Dopłaty</label>
              <input type="number" step="0.01" className="glass-input w-full" value={surchargeAmount} onChange={e => setSurchargeAmount(e.target.value)} required />
            </div>
            <button type="submit" className="btn-secondary h-[42px]">
              Dodaj Próg
            </button>
        </form>
      </div>
    </div>
  );
}
