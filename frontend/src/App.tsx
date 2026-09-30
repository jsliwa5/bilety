import React from 'react';
import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import Zones from './pages/Zones';
import ViolationTypes from './pages/ViolationTypes';
import Inspectors from './pages/Inspectors';
import Inspections from './pages/Inspections';
import ResidentCards from './pages/ResidentCards';
import { Menu } from 'lucide-react';

function App() {
  return (
    <Router>
      <div className="flex h-screen bg-gray-100">
        <nav className="w-64 bg-white shadow-lg">
          <div className="p-4 border-b">
            <h1 className="text-xl font-bold text-gray-800 flex items-center gap-2">
              <Menu size={24} />
              PTickets
            </h1>
          </div>
          <ul className="py-4">
            <li><Link className="block px-6 py-2 hover:bg-gray-100" to="/">Inspections</Link></li>
            <li><Link className="block px-6 py-2 hover:bg-gray-100" to="/zones">Zones & Streets</Link></li>
            <li><Link className="block px-6 py-2 hover:bg-gray-100" to="/violations">Violation Types</Link></li>
            <li><Link className="block px-6 py-2 hover:bg-gray-100" to="/inspectors">Inspectors</Link></li>
            <li><Link className="block px-6 py-2 hover:bg-gray-100" to="/cards">Resident Cards</Link></li>
          </ul>
        </nav>
        <main className="flex-1 overflow-auto p-8">
          <Routes>
            <Route path="/" element={<Inspections />} />
            <Route path="/zones" element={<Zones />} />
            <Route path="/violations" element={<ViolationTypes />} />
            <Route path="/inspectors" element={<Inspectors />} />
            <Route path="/cards" element={<ResidentCards />} />
          </Routes>
        </main>
      </div>
    </Router>
  );
}

export default App;

