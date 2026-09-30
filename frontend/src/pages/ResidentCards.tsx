import React, { useState, useEffect } from 'react';
import { api } from '../api';

export default function ResidentCards() {
  const [zones, setZones] = useState<any[]>([]);
  const [zoneId, setZoneId] = useState('');
  const [streetId, setStreetId] = useState('');
  const [registrationNumber, setRegistrationNumber] = useState('');
  const [validFrom, setValidFrom] = useState('');
  const [validTo, setValidTo] = useState('');

  useEffect(() => {
    const fetchZones = async () => {
      try {
        const res = await api.get('/zones');
        setZones(res.data);
      } catch (e) {
        console.error(e);
      }
    };
    fetchZones();
  }, []);

  const selectedZone = zones.find(z => z.id === zoneId);

  // If a zone is standalone (Type === 1), it only has 1 street.
  // We automatically infer it.
  const actualStreetId = selectedZone?.type === 1 
    ? selectedZone?.streets?.[0]?.id 
    : streetId;

  const issueCard = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!actualStreetId) return alert('Please select a street');
    
    try {
      await api.post('/resident-cards', {
        streetId: actualStreetId,
        registrationNumber,
        validFrom: new Date(validFrom).toISOString(),
        validTo: new Date(validTo).toISOString()
      });
      alert('Resident card issued successfully!');
      setRegistrationNumber('');
      setValidFrom('');
      setValidTo('');
    } catch (e) {
      console.error(e);
      alert('Error issuing card.');
    }
  };

  return (
    <div>
      <h2 className="text-2xl font-bold mb-6">Resident Cards</h2>
      
      <div className="bg-white p-6 rounded-lg shadow mb-8">
        <h3 className="text-lg font-bold mb-4">Issue Resident Card</h3>
        <form onSubmit={issueCard} className="flex flex-col gap-4">
          <div className="flex gap-4">
            <div className="flex-1">
              <label className="block text-sm font-medium mb-1">Registration Number (Plates)</label>
              <input 
                type="text" 
                className="border p-2 rounded w-full"
                value={registrationNumber}
                onChange={e => setRegistrationNumber(e.target.value)}
                required
              />
            </div>
          </div>
          <div className="flex gap-4">
            <div className="flex-1">
              <label className="block text-sm font-medium mb-1">Valid From</label>
              <input 
                type="date" 
                className="border p-2 rounded w-full"
                value={validFrom}
                onChange={e => setValidFrom(e.target.value)}
                required
              />
            </div>
            <div className="flex-1">
              <label className="block text-sm font-medium mb-1">Valid To</label>
              <input 
                type="date" 
                className="border p-2 rounded w-full"
                value={validTo}
                onChange={e => setValidTo(e.target.value)}
                required
              />
            </div>
          </div>
          <div className="flex gap-4">
            <div className="flex-1">
              <label className="block text-sm font-medium mb-1">Zone</label>
              <select 
                className="border p-2 rounded w-full" 
                value={zoneId}
                onChange={e => { setZoneId(e.target.value); setStreetId(''); }}
                required
              >
                <option value="">Select Zone</option>
                {zones.map(z => <option key={z.id} value={z.id}>{z.name}</option>)}
              </select>
            </div>
            <div className="flex-1">
              <label className="block text-sm font-medium mb-1">Street</label>
              <select 
                className="border p-2 rounded w-full" 
                value={streetId}
                onChange={e => setStreetId(e.target.value)}
                disabled={!selectedZone || selectedZone.type === 1}
                required={selectedZone?.type === 0}
              >
                {selectedZone?.type === 1 ? (
                  <option value={selectedZone?.streets?.[0]?.id || ''}>
                    {selectedZone?.streets?.[0]?.name || 'Standalone Zone (Auto-selected)'}
                  </option>
                ) : (
                  <>
                    <option value="">Select Street</option>
                    {selectedZone?.streets?.map((s: any) => (
                      <option key={s.id} value={s.id}>{s.name}</option>
                    ))}
                  </>
                )}
              </select>
            </div>
          </div>
          <button type="submit" className="bg-blue-600 text-white px-6 py-2 rounded mt-2 self-start">Issue Card</button>
        </form>
      </div>
    </div>
  );
}

