import React, { useEffect, useState } from 'react';
import { api } from '../api';

export default function Inspectors() {
  const [inspectors, setInspectors] = useState<any[]>([]);
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');

  const fetchInspectors = async () => {
    try {
      const res = await api.get('/inspectors');
      setInspectors(res.data);
    } catch (e) {
      console.error(e);
    }
  };

  useEffect(() => {
    fetchInspectors();
  }, []);

  const createInspector = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await api.post('/inspectors', { firstName, lastName });
      setFirstName('');
      setLastName('');
      fetchInspectors();
    } catch (e) {
      console.error(e);
    }
  };

  return (
    <div>
      <h2 className="text-2xl font-bold mb-6">Inspectors</h2>
      
      <div className="bg-white p-6 rounded-lg shadow mb-8">
        <h3 className="text-lg font-bold mb-4">Add Inspector</h3>
        <form onSubmit={createInspector} className="flex gap-4 items-end">
          <div className="flex-1">
            <label className="block text-sm font-medium mb-1">First Name</label>
            <input 
              type="text" 
              className="border p-2 rounded w-full"
              value={firstName}
              onChange={e => setFirstName(e.target.value)}
              required
            />
          </div>
          <div className="flex-1">
            <label className="block text-sm font-medium mb-1">Last Name</label>
            <input 
              type="text" 
              className="border p-2 rounded w-full"
              value={lastName}
              onChange={e => setLastName(e.target.value)}
              required
            />
          </div>
          <button type="submit" className="bg-blue-600 text-white px-6 py-2 rounded h-[42px]">Add</button>
        </form>
      </div>

      <div className="bg-white p-6 rounded-lg shadow">
        <h3 className="text-lg font-bold mb-4">All Inspectors</h3>
        {inspectors.length === 0 ? <p className="text-gray-500">No inspectors found.</p> : (
          <ul className="divide-y border rounded">
            {inspectors.map(ins => (
              <li key={ins.id} className="p-4 flex justify-between items-center">
                <div>
                  <span className="font-bold">{ins.firstName} {ins.lastName}</span>
                  <span className="text-sm text-gray-500 ml-2">({ins.id})</span>
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
}

