import React, { useEffect, useState } from 'react';
import { api } from '../api';

export default function ViolationTypes() {
  const [types, setTypes] = useState<any[]>([]);
  const [name, setName] = useState('');
  const [amount, setAmount] = useState<number | ''>('');

  const fetchTypes = async () => {
    try {
      const res = await api.get('/violation-types');
      setTypes(res.data);
    } catch (e) {
      console.error(e);
    }
  };

  useEffect(() => {
    fetchTypes();
  }, []);

  const createType = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      // Create type
      const res = await api.post('/violation-types', { name });
      const newTypeId = res.data?.id || res.data; // Depending on API response
      
      // Set penalty amount if provided
      if (amount && newTypeId) {
        await api.post(`/violation-types/${newTypeId}/penalty-amount`, { 
          amount: Number(amount),
          effectiveFrom: new Date().toISOString()
        });
      }

      setName('');
      setAmount('');
      fetchTypes();
    } catch (e) {
      console.error(e);
    }
  };

  return (
    <div>
      <h2 className="text-2xl font-bold mb-6">Violation Types</h2>
      
      <div className="bg-white p-6 rounded-lg shadow mb-8">
        <h3 className="text-lg font-bold mb-4">Add Violation Type</h3>
        <form onSubmit={createType} className="flex gap-4 items-end">
          <div className="flex-1">
            <label className="block text-sm font-medium mb-1">Name / Description</label>
            <input 
              type="text" 
              className="border p-2 rounded w-full"
              value={name}
              onChange={e => setName(e.target.value)}
              required
            />
          </div>
          <div className="w-48">
            <label className="block text-sm font-medium mb-1">Penalty Amount</label>
            <input 
              type="number" 
              className="border p-2 rounded w-full"
              value={amount}
              onChange={e => setAmount(Number(e.target.value))}
            />
          </div>
          <button type="submit" className="bg-blue-600 text-white px-6 py-2 rounded h-[42px]">Add</button>
        </form>
      </div>

      <div className="bg-white p-6 rounded-lg shadow">
        <h3 className="text-lg font-bold mb-4">All Violation Types</h3>
        {types.length === 0 ? <p className="text-gray-500">No violation types found.</p> : (
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="border-b">
                <th className="py-2">Name</th>
                <th className="py-2">Penalty Amount</th>
              </tr>
            </thead>
            <tbody>
              {types.map(t => {
                const id = typeof t.id === 'object' && t.id !== null ? t.id.value : t.id;
                const penalty = t.currentPenaltyAmount ?? t.penaltyAmount;
                return (
                  <tr key={id} className="border-b">
                    <td className="py-2">{t.name}</td>
                    <td className="py-2">{penalty ? `${penalty} PLN` : 'Not set'}</td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}

