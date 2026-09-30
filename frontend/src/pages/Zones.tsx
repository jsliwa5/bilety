import React, { useEffect, useState } from 'react';
import { api } from '../api';

export default function Zones() {
  const [zones, setZones] = useState<any[]>([]);

  // Create Zone State
  const [newZoneName, setNewZoneName] = useState('');
  const [zoneType, setZoneType] = useState<number>(0);
  const [zoneStartTime, setZoneStartTime] = useState('08:00');
  const [zoneEndTime, setZoneEndTime] = useState('18:00');
  const [zonePaidDays, setZonePaidDays] = useState<number[]>([1, 2, 3, 4, 5]); // Mon-Fri

  // Add Street State
  const [selectedZoneId, setSelectedZoneId] = useState('');
  const [newStreetName, setNewStreetName] = useState('');
  const [overrideSchedule, setOverrideSchedule] = useState(false);
  const [streetStartTime, setStreetStartTime] = useState('08:00');
  const [streetEndTime, setStreetEndTime] = useState('18:00');
  const [streetPaidDays, setStreetPaidDays] = useState<number[]>([1, 2, 3, 4, 5]);

  const daysOfWeek = [
    { value: 1, label: 'Monday' },
    { value: 2, label: 'Tuesday' },
    { value: 3, label: 'Wednesday' },
    { value: 4, label: 'Thursday' },
    { value: 5, label: 'Friday' },
    { value: 6, label: 'Saturday' },
    { value: 0, label: 'Sunday' },
  ];

  const fetchZones = async () => {
    try {
      const res = await api.get('/zones');
      setZones(res.data);
    } catch (e) {
      console.error(e);
    }
  };

  useEffect(() => {
    fetchZones();
  }, []);

  const createZone = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await api.post('/zones', { 
        name: newZoneName,
        type: zoneType,
        startTime: zoneStartTime + ":00",
        endTime: zoneEndTime + ":00",
        paidDays: zonePaidDays
      });
      setNewZoneName('');
      fetchZones();
      alert('Zone created!');
    } catch (e) {
      console.error(e);
      alert('Error creating zone');
    }
  };

  const addStreet = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedZoneId) return;
    try {
      const payload: any = {
        name: newStreetName,
        representsWholeZone: false
      };

      if (overrideSchedule) {
        payload.startTime = streetStartTime + ":00";
        payload.endTime = streetEndTime + ":00";
        payload.paidDays = streetPaidDays;
      }

      await api.post(`/zones/${selectedZoneId}/streets`, payload);
      setNewStreetName('');
      fetchZones();
      alert('Street added!');
    } catch (e) {
      console.error(e);
      alert('Error adding street');
    }
  };

  const toggleDay = (day: number, currentDays: number[], setter: (days: number[]) => void) => {
    if (currentDays.includes(day)) {
      setter(currentDays.filter(d => d !== day));
    } else {
      setter([...currentDays, day]);
    }
  };

  // Only MultiStreet zones can have streets added manually
  const addableZones = zones.filter(z => z.type === 0);

  return (
    <div>
      <h2 className="text-2xl font-bold mb-6">Zones & Streets</h2>
      
      <div className="grid grid-cols-2 gap-8 mb-8">
        {/* Create Zone */}
        <div className="bg-white p-6 rounded-lg shadow">
          <h3 className="text-lg font-bold mb-4">Create Zone</h3>
          <form onSubmit={createZone} className="flex flex-col gap-4">
            <div>
              <label className="block text-sm font-medium mb-1">Zone Name</label>
              <input 
                type="text" 
                className="border p-2 rounded w-full"
                value={newZoneName}
                onChange={e => setNewZoneName(e.target.value)}
                required
              />
            </div>
            
            <div>
              <label className="block text-sm font-medium mb-1">Zone Type</label>
              <select 
                className="border p-2 rounded w-full"
                value={zoneType}
                onChange={e => setZoneType(Number(e.target.value))}
              >
                <option value={0}>Multi-Street (Standard)</option>
                <option value={1}>Single-Street (Standalone)</option>
              </select>
            </div>

            <div className="flex gap-4">
              <div className="flex-1">
                <label className="block text-sm font-medium mb-1">Start Time</label>
                <input type="time" className="border p-2 rounded w-full" value={zoneStartTime} onChange={e => setZoneStartTime(e.target.value)} required />
              </div>
              <div className="flex-1">
                <label className="block text-sm font-medium mb-1">End Time</label>
                <input type="time" className="border p-2 rounded w-full" value={zoneEndTime} onChange={e => setZoneEndTime(e.target.value)} required />
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium mb-1">Paid Days</label>
              <div className="flex flex-wrap gap-2">
                {daysOfWeek.map(day => (
                  <label key={day.value} className="flex items-center gap-1">
                    <input 
                      type="checkbox" 
                      checked={zonePaidDays.includes(day.value)}
                      onChange={() => toggleDay(day.value, zonePaidDays, setZonePaidDays)}
                    />
                    <span className="text-sm">{day.label.slice(0, 3)}</span>
                  </label>
                ))}
              </div>
            </div>

            <button type="submit" className="bg-blue-600 text-white px-4 py-2 rounded self-start mt-2">Create Zone</button>
          </form>
        </div>

        {/* Add Street */}
        <div className="bg-white p-6 rounded-lg shadow">
          <h3 className="text-lg font-bold mb-4">Add Street to Zone</h3>
          <form onSubmit={addStreet} className="flex flex-col gap-4">
            <div>
              <label className="block text-sm font-medium mb-1">Select Zone (Multi-Street only)</label>
              <select 
                className="border p-2 rounded w-full" 
                value={selectedZoneId}
                onChange={e => setSelectedZoneId(e.target.value)}
                required
              >
                <option value="">Select Zone</option>
                {addableZones.map(z => <option key={z.id} value={z.id}>{z.name}</option>)}
              </select>
            </div>
            
            <div>
              <label className="block text-sm font-medium mb-1">Street Name</label>
              <input 
                type="text" 
                className="border p-2 rounded w-full"
                value={newStreetName}
                onChange={e => setNewStreetName(e.target.value)}
                required
              />
            </div>

            <div>
              <label className="flex items-center gap-2 text-sm font-medium mb-2 mt-2">
                <input type="checkbox" checked={overrideSchedule} onChange={e => setOverrideSchedule(e.target.checked)} />
                Override Zone Schedule for this Street
              </label>
            </div>

            {overrideSchedule && (
              <div className="border p-4 rounded bg-gray-50 flex flex-col gap-4">
                <div className="flex gap-4">
                  <div className="flex-1">
                    <label className="block text-sm font-medium mb-1">Start Time</label>
                    <input type="time" className="border p-2 rounded w-full bg-white" value={streetStartTime} onChange={e => setStreetStartTime(e.target.value)} required />
                  </div>
                  <div className="flex-1">
                    <label className="block text-sm font-medium mb-1">End Time</label>
                    <input type="time" className="border p-2 rounded w-full bg-white" value={streetEndTime} onChange={e => setStreetEndTime(e.target.value)} required />
                  </div>
                </div>
                <div>
                  <label className="block text-sm font-medium mb-1">Paid Days</label>
                  <div className="flex flex-wrap gap-2">
                    {daysOfWeek.map(day => (
                      <label key={day.value} className="flex items-center gap-1">
                        <input 
                          type="checkbox" 
                          checked={streetPaidDays.includes(day.value)}
                          onChange={() => toggleDay(day.value, streetPaidDays, setStreetPaidDays)}
                        />
                        <span className="text-sm">{day.label.slice(0, 3)}</span>
                      </label>
                    ))}
                  </div>
                </div>
              </div>
            )}

            <button type="submit" className="bg-green-600 text-white px-4 py-2 rounded self-start mt-2">Add Street</button>
          </form>
        </div>
      </div>

      {/* All Zones */}
      <div className="bg-white p-6 rounded-lg shadow">
        <h3 className="text-lg font-bold mb-4">All Zones</h3>
        {zones.length === 0 ? <p className="text-gray-500">No zones found.</p> : (
          <ul className="divide-y border rounded">
            {zones.map(z => (
              <li key={z.id} className="p-4">
                <div className="flex items-center justify-between mb-2">
                  <span className="font-bold text-lg">{z.name}</span>
                  <span className="text-xs bg-gray-200 px-2 py-1 rounded font-mono">
                    {z.type === 1 ? 'Single (Standalone)' : 'Multi-Street'}
                  </span>
                </div>
                {z.schedule && (
                  <div className="text-sm text-gray-700 mb-2">
                    Schedule: {z.schedule.startTime} - {z.schedule.endTime} (Days: {z.schedule.paidDays?.join(', ')})
                  </div>
                )}
                
                <div className="text-sm">
                  <strong className="text-gray-600 block mb-1">Streets:</strong>
                  {z.streets?.length > 0 ? (
                    <ul className="list-disc pl-5">
                      {z.streets.map((s: any) => (
                        <li key={s.id} className="mb-1 text-gray-800">
                          {s.name}
                          {s.schedule && (
                            <span className="text-xs text-gray-500 ml-2">
                              (Override: {s.schedule.startTime}-{s.schedule.endTime}, Days: {s.schedule.paidDays?.join(',')})
                            </span>
                          )}
                        </li>
                      ))}
                    </ul>
                  ) : (
                    <span className="text-gray-500">None</span>
                  )}
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
}

