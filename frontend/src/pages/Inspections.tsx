import React, { useState, useEffect } from 'react';
import { api } from '../api';

export default function Inspections() {
  const [inspectors, setInspectors] = useState<any[]>([]);
  const [zones, setZones] = useState<any[]>([]);
  const [violationTypes, setViolationTypes] = useState<any[]>([]);

  // Session state
  const [sessionId, setSessionId] = useState<string | null>(null);
  const [selectedInspectorId, setSelectedInspectorId] = useState('');

  // Inspection state
  const [inspectionId, setInspectionId] = useState<string | null>(null);
  const [regNumber, setRegNumber] = useState('');
  const [selectedZoneId, setSelectedZoneId] = useState('');
  const [selectedStreetId, setSelectedStreetId] = useState('');
  const [latitude, setLatitude] = useState(52.2297);
  const [longitude, setLongitude] = useState(21.0122);

  // Actions state
  const [selectedViolationTypeId, setSelectedViolationTypeId] = useState('');
  const [photoId, setPhotoId] = useState('');
  
  // Results state
  const [ticketResult, setTicketResult] = useState<any>(null);
  const [notice, setNotice] = useState<any>(null);
  const [existingNotices, setExistingNotices] = useState<any[]>([]);

  useEffect(() => {
    const loadData = async () => {
      try {
        const [insp, z, v] = await Promise.all([
          api.get('/inspectors'),
          api.get('/zones'),
          api.get('/violation-types')
        ]);
        setInspectors(insp.data);
        setZones(z.data);
        setViolationTypes(v.data);
      } catch (e) {
        console.error('Error loading data', e);
      }
    };
    loadData();
  }, []);

  const startSession = async () => {
    if (!selectedInspectorId) return alert('Select inspector');
    try {
      // In this setup, maybe API returns the session ID as raw string or in an object
      const res = await api.post('/sessions', { inspectorId: selectedInspectorId });
      setSessionId(res.data?.id || res.data || 'dummy-session-id-if-empty'); // Adjust based on actual API
      alert('Session started!');
    } catch (e) {
      console.error(e);
      alert('Error starting session');
    }
  };

  const closeSession = async () => {
    if (!sessionId) return;
    try {
      await api.post(`/sessions/${sessionId}/close`);
      setSessionId(null);
      setInspectionId(null);
      setTicketResult(null);
      setNotice(null);
      setExistingNotices([]);
      alert('Session closed!');
    } catch (e) {
      console.error(e);
      alert('Error closing session');
    }
  };

  const startInspection = async () => {
    if (!sessionId) return alert('Start session first');
    if (!regNumber || !selectedZoneId) return alert('Fill required fields');
    
    const zone = zones.find(z => z.id === selectedZoneId);
    const actualStreetId = zone?.type === 1 ? zone?.streets?.[0]?.id : selectedStreetId;

    if (!actualStreetId) return alert('Please select a street');

    try {
      setTicketResult(null);
      setNotice(null);
      setExistingNotices([]);
      const payload = {
        sessionId,
        registrationNumber: regNumber,
        latitude,
        longitude,
        zoneId: selectedZoneId,
        streetId: actualStreetId
      };
      const res = await api.post('/inspections', payload);
      setInspectionId(res.data?.id || res.data); // Adjust based on API
      alert('Inspection started!');
      
      // Fetch existing notices for today
      try {
        const todayStr = new Date().toISOString().split('T')[0]; // format yyyy-mm-dd
        const noticesRes = await api.get(`/notices/car/${regNumber}/date/${todayStr}`);
        setExistingNotices(noticesRes.data);
      } catch (err: any) {
        if (err.response?.status !== 404) {
          console.error("Failed to check existing notices:", err);
        }
      }

    } catch (e) {
      console.error(e);
      alert('Error starting inspection');
    }
  };

  const checkTicket = async () => {
    if (!inspectionId) return;
    try {
      setTicketResult(null);
      const res = await api.post(`/inspections/${inspectionId}/check-ticket`);
      setTicketResult(res.data);
    } catch (e) {
      console.error(e);
      alert('Error checking ticket');
    }
  };

  const addViolation = async () => {
    if (!inspectionId || !selectedViolationTypeId) return;
    try {
      await api.post(`/inspections/${inspectionId}/violations`, {
        inspectionId,
        violationTypeId: selectedViolationTypeId
      });
      alert('Violation added!');
    } catch (e) {
      console.error(e);
      alert('Error adding violation');
    }
  };

  const addPhotos = async () => {
    if (!inspectionId || !photoId) return;
    try {
      await api.post(`/inspections/${inspectionId}/photos`, {
        inspectionId,
        fileIds: [photoId]
      });
      alert('Photos attached!');
      setPhotoId('');
    } catch (e) {
      console.error(e);
      alert('Error attaching photos');
    }
  };

  const issueNotice = async () => {
    if (!inspectionId) return;
    try {
      setNotice(null);
      const res = await api.post(`/inspections/${inspectionId}/notice`);
      const noticeId = res.data?.id || res.data;
      if (noticeId) {
        const noticeRes = await api.get(`/notices/${noticeId}`);
        setNotice(noticeRes.data);
      } else {
        alert('Notice issued, but could not retrieve ID.');
      }
    } catch (e) {
      console.error(e);
      alert('Error issuing notice');
    }
  };

  const selectedZone = zones.find(z => z.id === selectedZoneId);

  return (
    <div className="max-w-4xl mx-auto">
      <h2 className="text-2xl font-bold mb-6">Inspections Dashboard</h2>
      
      {!sessionId ? (
        <div className="bg-white p-6 rounded-lg shadow mb-8">
          <h3 className="text-lg font-bold mb-4">Start Session</h3>
          <div className="flex gap-4 items-end">
            <div className="flex-1">
              <label className="block text-sm font-medium mb-1">Inspector</label>
              <select 
                className="border p-2 rounded w-full"
                value={selectedInspectorId}
                onChange={e => setSelectedInspectorId(e.target.value)}
              >
                <option value="">Select Inspector</option>
                {inspectors.map(i => <option key={i.id} value={i.id}>{i.firstName} {i.lastName}</option>)}
              </select>
            </div>
            <button onClick={startSession} className="bg-blue-600 text-white px-6 py-2 rounded">Start Session</button>
          </div>
        </div>
      ) : (
        <div className="bg-green-50 border border-green-200 p-6 rounded-lg shadow mb-8 flex justify-between items-center">
          <div>
            <h3 className="text-lg font-bold text-green-800">Session Active</h3>
            <p className="text-sm text-green-700">ID: {sessionId}</p>
          </div>
          <button onClick={closeSession} className="bg-red-600 text-white px-4 py-2 rounded">Close Session</button>
        </div>
      )}

      {sessionId && !inspectionId && (
        <div className="bg-white p-6 rounded-lg shadow mb-8">
          <h3 className="text-lg font-bold mb-4">New Inspection</h3>
          <div className="grid grid-cols-2 gap-4 mb-4">
            <div>
              <label className="block text-sm font-medium mb-1">Registration Number</label>
              <input type="text" className="border p-2 rounded w-full" value={regNumber} onChange={e => setRegNumber(e.target.value)} />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Zone</label>
              <select className="border p-2 rounded w-full" value={selectedZoneId} onChange={e => { setSelectedZoneId(e.target.value); setSelectedStreetId(''); }}>
                <option value="">Select Zone</option>
                {zones.map(z => <option key={z.id} value={z.id}>{z.name}</option>)}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Street</label>
              <select 
                className="border p-2 rounded w-full" 
                value={selectedStreetId} 
                onChange={e => setSelectedStreetId(e.target.value)} 
                disabled={!selectedZone || selectedZone.type === 1}
              >
                {selectedZone?.type === 1 ? (
                  <option value={selectedZone?.streets?.[0]?.id || ''}>
                    {selectedZone?.streets?.[0]?.name || 'Standalone Zone (Auto-selected)'}
                  </option>
                ) : (
                  <>
                    <option value="">Select Street</option>
                    {selectedZone?.streets?.map((s: any) => <option key={s.id} value={s.id}>{s.name}</option>)}
                  </>
                )}
              </select>
            </div>
          </div>
          <button onClick={startInspection} className="bg-blue-600 text-white px-6 py-2 rounded w-full">Start Inspection</button>
        </div>
      )}

      {inspectionId && (
        <div className="bg-white p-6 rounded-lg shadow mb-8 border-l-4 border-blue-500">
          <h3 className="text-lg font-bold mb-4">Active Inspection Actions</h3>
          <p className="text-sm text-gray-600 mb-6">Inspection ID: {inspectionId}</p>

          {existingNotices.length > 0 && (
            <div className="bg-red-50 border-l-4 border-red-500 p-4 mb-6">
              <div className="flex">
                <div className="flex-shrink-0">
                  <svg className="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
                    <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                  </svg>
                </div>
                <div className="ml-3">
                  <h3 className="text-sm font-medium text-red-800">
                    Warning: This vehicle already has {existingNotices.length} notice(s) issued today!
                  </h3>
                  <div className="mt-2 text-sm text-red-700">
                    <ul className="list-disc pl-5 space-y-1">
                      {existingNotices.map((n, idx) => (
                        <li key={idx}>Notice ID: {n.id} | Amount: {n.totalAmount} PLN | Time: {new Date(n.issuedAt).toLocaleTimeString()}</li>
                      ))}
                    </ul>
                  </div>
                </div>
              </div>
            </div>
          )}
          
          <div className="space-y-6">
            <div className="flex gap-4 items-center">
              <button onClick={checkTicket} className="bg-gray-800 text-white px-4 py-2 rounded flex-1">Verify Ticket</button>
            </div>
            
            {ticketResult && (
              <div className={`p-4 rounded border ${ticketResult.isValid ? 'bg-green-100 border-green-300 text-green-900' : 'bg-red-100 border-red-300 text-red-900'}`}>
                <strong>Ticket Verification Result: </strong>
                {ticketResult.isValid ? 'VALID' : 'INVALID'}
                <br />
                <span className="text-sm opacity-80">Message: {ticketResult.providerMessage || 'N/A'}</span>
              </div>
            )}
            
            <hr />

            <div className="flex gap-4 items-end">
              <div className="flex-1">
                <label className="block text-sm font-medium mb-1">Violation Type (Optional Manual)</label>
                <select className="border p-2 rounded w-full" value={selectedViolationTypeId} onChange={e => setSelectedViolationTypeId(e.target.value)}>
                  <option value="">Select Violation Type</option>
                  {violationTypes.map(v => <option key={v.id} value={v.id}>{v.name}</option>)}
                </select>
              </div>
              <button onClick={addViolation} className="bg-orange-500 text-white px-4 py-2 rounded">Add Violation</button>
            </div>

            <hr />

            <div className="flex gap-4 items-end">
              <div className="flex-1">
                <label className="block text-sm font-medium mb-1">Photo UUID (Mock)</label>
                <input type="text" className="border p-2 rounded w-full" placeholder="uuid" value={photoId} onChange={e => setPhotoId(e.target.value)} />
              </div>
              <button onClick={addPhotos} className="bg-indigo-500 text-white px-4 py-2 rounded">Add Photo</button>
            </div>

            <hr />

            <div className="flex gap-4 items-center">
              <button onClick={issueNotice} className="bg-red-600 text-white px-4 py-2 rounded flex-1">Issue Notice</button>
              <button onClick={() => { setInspectionId(null); setTicketResult(null); setNotice(null); setExistingNotices([]); }} className="bg-gray-200 text-gray-800 px-4 py-2 rounded">Finish / Next Vehicle</button>
            </div>
            
            {notice && (
              <div className="mt-4 p-4 rounded bg-yellow-100 border border-yellow-300 text-yellow-900 shadow">
                <h4 className="font-bold text-lg mb-2">Notice Issued Successfully!</h4>
                <div className="text-sm grid grid-cols-2 gap-2">
                  <div className="font-medium">Notice ID:</div><div>{notice.id}</div>
                  <div className="font-medium">Registration:</div><div>{notice.registrationNumber}</div>
                  <div className="font-medium">Total Amount:</div><div className="font-bold text-red-600">{notice.totalAmount} PLN</div>
                  <div className="font-medium">Date:</div><div>{new Date(notice.issuedAt).toLocaleString()}</div>
                </div>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

