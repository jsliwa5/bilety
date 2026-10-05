import React, { useState, useEffect } from 'react';
import { api } from '../api';

type InspectionPhase = 'first-check' | 'second-check';

export default function Inspections() {
  const [inspectors, setInspectors] = useState<any[]>([]);
  const [zones, setZones] = useState<any[]>([]);
  const [violationTypes, setViolationTypes] = useState<any[]>([]);

  // Session state
  const [sessionId, setSessionId] = useState<string | null>(null);
  const [selectedInspectorId, setSelectedInspectorId] = useState('');

  // Inspection state
  const [inspectionId, setInspectionId] = useState<string | null>(null);
  const [inspectionStatus, setInspectionStatus] = useState<string | null>(null);
  const [inspectionPhase, setInspectionPhase] = useState<InspectionPhase>('first-check');
  const [photoCount, setPhotoCount] = useState<number>(0);
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
  const [secondCheckResult, setSecondCheckResult] = useState<any>(null);
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

  const fetchInspectionStatus = async (id: string) => {
    try {
      const res = await api.get(`/inspections/${id}`);
      setInspectionStatus(res.data.status);
      setPhotoCount(res.data.photoCount ?? 0);
      return res.data;
    } catch (e) {
      console.error('Error fetching inspection status', e);
      return null;
    }
  };

  const resetInspection = () => {
    setInspectionId(null);
    setInspectionStatus(null);
    setInspectionPhase('first-check');
    setPhotoCount(0);
    setTicketResult(null);
    setSecondCheckResult(null);
    setNotice(null);
    setExistingNotices([]);
  };

  const startSession = async () => {
    if (!selectedInspectorId) return alert('Select inspector');
    try {
      const res = await api.post('/sessions', { inspectorId: selectedInspectorId });
      setSessionId(res.data?.id || res.data || 'dummy-session-id-if-empty');
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
      resetInspection();
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
      resetInspection();
      const payload = {
        sessionId,
        registrationNumber: regNumber,
        latitude,
        longitude,
        zoneId: selectedZoneId,
        streetId: actualStreetId
      };
      const res = await api.post('/inspections', payload);
      const newInspectionId = res.data?.id || res.data;
      setInspectionId(newInspectionId);

      // Check if this is a second-check inspection
      const data = await fetchInspectionStatus(newInspectionId);
      if (data?.status === 'AwaitingSecondCheck') {
        setInspectionPhase('second-check');
      } else {
        setInspectionPhase('first-check');
      }
      
      // Fetch existing notices for today
      try {
        const todayStr = new Date().toISOString().split('T')[0];
        const noticesRes = await api.get(`/notices/car/${regNumber}/date/${todayStr}`);
        setExistingNotices(noticesRes.data);
      } catch (err: any) {
        if (err.response?.status !== 404) {
          console.error("Failed to check existing notices:", err);
        }
      }

    } catch (e: any) {
      console.error(e);
      const msg = e.response?.data || 'Error starting inspection';
      alert(typeof msg === 'string' ? msg : JSON.stringify(msg));
    }
  };

  const checkTicket = async () => {
    if (!inspectionId) return;
    try {
      const res = await api.post(`/inspections/${inspectionId}/check-ticket`);
      const result = res.data;

      if (inspectionPhase === 'first-check') {
        setTicketResult(result);

        if (!result.isValid) {
          // First check failed — fetch updated status to confirm AwaitingSecondCheck
          const data = await fetchInspectionStatus(inspectionId);
          if (data?.status === 'AwaitingSecondCheck') {
            setInspectionPhase('second-check');
          }
        } else {
          await fetchInspectionStatus(inspectionId);
        }
      } else {
        // Second check
        setSecondCheckResult(result);
        await fetchInspectionStatus(inspectionId);
      }
    } catch (e: any) {
      console.error(e);
      const msg = e.response?.data || 'Error checking ticket';
      alert(typeof msg === 'string' ? msg : JSON.stringify(msg));
    }
  };

  const addViolation = async () => {
    if (!inspectionId || !selectedViolationTypeId) return;
    try {
      await api.post(`/inspections/${inspectionId}/violations`, {
        inspectionId,
        violationTypeId: selectedViolationTypeId
      });
      await fetchInspectionStatus(inspectionId);
      alert('Violation added!');
    } catch (e: any) {
      console.error(e);
      const msg = e.response?.data || 'Error adding violation';
      alert(typeof msg === 'string' ? msg : JSON.stringify(msg));
    }
  };

  const addPhotos = async () => {
    if (!inspectionId || !photoId) return;
    try {
      await api.post(`/inspections/${inspectionId}/photos`, {
        inspectionId,
        fileIds: [photoId]
      });
      await fetchInspectionStatus(inspectionId);
      alert('Photo attached successfully!');
      setPhotoId('');
    } catch (e: any) {
      console.error(e);
      const msg = e.response?.data || 'Error attaching photos';
      alert(typeof msg === 'string' ? msg : JSON.stringify(msg));
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
        await fetchInspectionStatus(inspectionId);
      } else {
        alert('Notice issued, but could not retrieve ID.');
      }
    } catch (e: any) {
      console.error(e);
      const msg = e.response?.data || 'Error issuing notice';
      alert(typeof msg === 'string' ? msg : JSON.stringify(msg));
    }
  };

  const approveInspection = async () => {
    if (!inspectionId) return;
    try {
      await api.post(`/inspections/${inspectionId}/approve`);
      await fetchInspectionStatus(inspectionId);
      alert('Inspection approved!');
    } catch (e: any) {
      console.error(e);
      const msg = e.response?.data || 'Error approving inspection';
      alert(typeof msg === 'string' ? msg : JSON.stringify(msg));
    }
  };

  const selectedZone = zones.find(z => z.id === selectedZoneId);

  const isTerminalStatus = inspectionStatus === 'Approved' || inspectionStatus === 'NoticeIssued';

  // Determine which actions are available based on status
  const isAwaitingSecondCheck = inspectionStatus === 'AwaitingSecondCheck';
  const hasFirstCheckPhotos = photoCount > 0;
  
  // Can only conduct 2nd ticket check if first check photos have been attached
  const canCheckTicket = 
    inspectionStatus === 'AwaitingDecision' || 
    (isAwaitingSecondCheck && hasFirstCheckPhotos);

  const canAddViolation = inspectionStatus === 'AwaitingDecision' || isAwaitingSecondCheck;
  const canAddPhotos = isAwaitingSecondCheck || inspectionStatus === 'ViolationFound';
  const canIssueNotice = inspectionStatus === 'PhotosAttached';
  const canApprove = inspectionStatus === 'AwaitingDecision' || inspectionStatus === 'Approved';

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
          <div className="flex justify-between items-start mb-4">
            <div>
              <h3 className="text-lg font-bold">Active Inspection</h3>
              <p className="text-sm text-gray-600">ID: {inspectionId}</p>
            </div>
            <div className="flex items-center gap-2">
              <span className="text-xs bg-gray-100 text-gray-700 px-2 py-1 rounded">
                📷 Zdjęcia: {photoCount}
              </span>
              {inspectionStatus && (
                <span className={`px-3 py-1 rounded-full text-sm font-medium ${
                  inspectionStatus === 'Approved' ? 'bg-green-100 text-green-800' :
                  inspectionStatus === 'AwaitingDecision' ? 'bg-yellow-100 text-yellow-800' :
                  inspectionStatus === 'AwaitingSecondCheck' ? 'bg-orange-100 text-orange-800' :
                  inspectionStatus === 'ViolationFound' ? 'bg-red-100 text-red-800' :
                  inspectionStatus === 'PhotosAttached' ? 'bg-purple-100 text-purple-800' :
                  inspectionStatus === 'NoticeIssued' ? 'bg-gray-100 text-gray-800' :
                  'bg-gray-100 text-gray-800'
                }`}>
                  {inspectionStatus === 'AwaitingSecondCheck' ? '⏳ Awaiting 2nd Check' : inspectionStatus}
                </span>
              )}
            </div>
          </div>

          {/* Second check banners */}
          {isAwaitingSecondCheck && !hasFirstCheckPhotos && (
            <div className="bg-yellow-50 border-l-4 border-yellow-400 p-4 mb-6">
              <div className="flex items-center">
                <span className="text-xl mr-3">📷</span>
                <div>
                  <h4 className="text-sm font-bold text-yellow-800">Wymagane zdjęcie z 1. sprawdzenia</h4>
                  <p className="text-sm text-yellow-700">
                    Brak ważnego biletu. Dołącz zdjęcie pojazdu poniżej, aby udokumentować 1. sprawdzenie przed wykonaniem 2. weryfikacji.
                  </p>
                </div>
              </div>
            </div>
          )}

          {isAwaitingSecondCheck && hasFirstCheckPhotos && (
            <div className="bg-orange-50 border-l-4 border-orange-400 p-4 mb-6">
              <div className="flex items-center">
                <svg className="h-5 w-5 text-orange-400 mr-3" viewBox="0 0 20 20" fill="currentColor">
                  <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                </svg>
                <div>
                  <h4 className="text-sm font-bold text-orange-800">Gotowe do 2. sprawdzenia biletu</h4>
                  <p className="text-sm text-orange-700">
                    Zdjęcie z 1. kontroli jest załączone. Możesz teraz zweryfikować bilet po raz drugi.
                  </p>
                </div>
              </div>
            </div>
          )}

          {inspectionStatus === 'ViolationFound' && (
            <div className="bg-red-50 border-l-4 border-red-400 p-4 mb-6">
              <div className="flex items-center">
                <span className="text-xl mr-3">📷</span>
                <div>
                  <h4 className="text-sm font-bold text-red-800">Wymagane zdjęcie z 2. sprawdzenia</h4>
                  <p className="text-sm text-red-700">
                    Wykroczenie potwierdzone! Dołącz drugie zdjęcie pojazdu poniżej, aby odblokować wystawienie wezwania (Issue Notice).
                  </p>
                </div>
              </div>
            </div>
          )}

          {inspectionStatus === 'PhotosAttached' && (
            <div className="bg-purple-50 border-l-4 border-purple-400 p-4 mb-6">
              <div className="flex items-center">
                <span className="text-xl mr-3">✅</span>
                <div>
                  <h4 className="text-sm font-bold text-purple-800">Komplet dowodów załączony</h4>
                  <p className="text-sm text-purple-700">
                    Zdjęcia z obu kontroli zostały załączone. Możesz wystawić wezwanie do zapłaty (Issue Notice).
                  </p>
                </div>
              </div>
            </div>
          )}

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
            {/* Ticket Verification */}
            <div className="flex flex-col gap-2">
              <button 
                onClick={checkTicket} 
                disabled={!canCheckTicket}
                className={`px-4 py-2 rounded ${
                  canCheckTicket 
                    ? 'bg-gray-800 text-white hover:bg-gray-700' 
                    : 'bg-gray-300 text-gray-500 cursor-not-allowed'
                }`}
              >
                {isAwaitingSecondCheck
                  ? '🔁 Verify Ticket (2nd Check)'
                  : 'Verify Ticket'}
              </button>
              {isAwaitingSecondCheck && !hasFirstCheckPhotos && (
                <span className="text-xs text-red-600 font-medium">
                  ⚠️ Aby wykonać 2. sprawdzenie, najpierw załącz zdjęcie z 1. kontroli poniżej.
                </span>
              )}
            </div>

            {/* First check result */}
            {ticketResult && (
              <div className={`p-4 rounded border ${ticketResult.isValid ? 'bg-green-100 border-green-300 text-green-900' : 'bg-red-100 border-red-300 text-red-900'}`}>
                <strong>1st Check Result: </strong>
                {ticketResult.isValid ? '✅ VALID (Brak potrzeby zdjęć)' : '❌ INVALID (Wymagane zdjęcie 1. kontroli)'}
                <br />
                <span className="text-sm opacity-80">Message: {ticketResult.message || ticketResult.providerMessage || 'N/A'}</span>
              </div>
            )}

            {/* Second check result */}
            {secondCheckResult && (
              <div className={`p-4 rounded border ${secondCheckResult.isValid ? 'bg-green-100 border-green-300 text-green-900' : 'bg-red-100 border-red-300 text-red-900'}`}>
                <strong>2nd Check Result: </strong>
                {secondCheckResult.isValid ? '✅ VALID — Bilet kupiony w okresie karencji!' : '❌ INVALID — Wykroczenie potwierdzone (Wymagane drugie zdjęcie)'}
                <br />
                <span className="text-sm opacity-80">Message: {secondCheckResult.message || secondCheckResult.providerMessage || 'N/A'}</span>
              </div>
            )}
            
            <hr />

            {/* Photos (Available in AwaitingSecondCheck or ViolationFound) */}
            <div className="p-4 bg-gray-50 rounded-lg border border-gray-200">
              <h4 className="text-sm font-semibold mb-2">📸 Załączanie zdjęć dowodowych</h4>
              <p className="text-xs text-gray-600 mb-3">
                Zdjęcia są wymagane wyłącznie przy stwierdzeniu braku biletu lub wykroczenia.
              </p>
              <div className="flex gap-4 items-end">
                <div className="flex-1">
                  <label className="block text-sm font-medium mb-1">Photo UUID (Mock)</label>
                  <input 
                    type="text" 
                    className="border p-2 rounded w-full bg-white" 
                    placeholder="Wpisz identyfikator zdjęcia np. photo-1" 
                    value={photoId} 
                    onChange={e => setPhotoId(e.target.value)} 
                    disabled={!canAddPhotos}
                  />
                </div>
                <button 
                  onClick={addPhotos} 
                  disabled={!canAddPhotos || !photoId}
                  className={`px-4 py-2 rounded ${
                    canAddPhotos && photoId
                      ? 'bg-indigo-600 text-white hover:bg-indigo-700' 
                      : 'bg-gray-300 text-gray-500 cursor-not-allowed'
                  }`}
                >
                  {isAwaitingSecondCheck ? 'Załącz zdjęcie (1. kontrola)' : 'Załącz zdjęcie (2. kontrola)'}
                </button>
              </div>
            </div>

            <hr />

            {/* Add Visual Violation */}
            <div className="flex gap-4 items-end">
              <div className="flex-1">
                <label className="block text-sm font-medium mb-1">Violation Type (Wykroczenie wizualne)</label>
                <select 
                  className="border p-2 rounded w-full" 
                  value={selectedViolationTypeId} 
                  onChange={e => setSelectedViolationTypeId(e.target.value)}
                  disabled={!canAddViolation}
                >
                  <option value="">Select Violation Type</option>
                  {violationTypes.map(v => <option key={v.id} value={v.id}>{v.name}</option>)}
                </select>
              </div>
              <button 
                onClick={addViolation} 
                disabled={!canAddViolation || !selectedViolationTypeId}
                className={`px-4 py-2 rounded ${
                  canAddViolation && selectedViolationTypeId
                    ? 'bg-orange-500 text-white hover:bg-orange-600' 
                    : 'bg-gray-300 text-gray-500 cursor-not-allowed'
                }`}
              >
                Add Violation
              </button>
            </div>

            <hr />

            {/* Final actions */}
            <div className="flex gap-4 items-center">
              <button 
                onClick={issueNotice} 
                disabled={!canIssueNotice}
                className={`px-4 py-2 rounded flex-1 ${
                  canIssueNotice 
                    ? 'bg-red-600 text-white hover:bg-red-700' 
                    : 'bg-gray-300 text-gray-500 cursor-not-allowed'
                }`}
              >
                Issue Notice
              </button>
              <button 
                onClick={approveInspection} 
                disabled={!canApprove}
                className={`px-4 py-2 rounded ${
                  canApprove 
                    ? 'bg-green-600 text-white hover:bg-green-700' 
                    : 'bg-gray-300 text-gray-500 cursor-not-allowed'
                }`}
              >
                Approve
              </button>
              <button 
                onClick={resetInspection} 
                className="bg-gray-200 text-gray-800 px-4 py-2 rounded hover:bg-gray-300"
              >
                Finish / Next Vehicle
              </button>
            </div>
            
            {/* Notice result */}
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

            {/* Terminal status info */}
            {isTerminalStatus && (
              <div className={`p-4 rounded border ${
                inspectionStatus === 'Approved' ? 'bg-green-50 border-green-300 text-green-800' : 'bg-gray-50 border-gray-300 text-gray-800'
              }`}>
                <strong>Inspection completed</strong> — Status: {inspectionStatus}. Click "Finish / Next Vehicle" to continue.
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

