import React from 'react';
import { BrowserRouter as Router, Routes, Route, NavLink } from 'react-router-dom';
import Zones from './pages/Zones';
import ViolationTypes from './pages/ViolationTypes';
import Inspectors from './pages/Inspectors';
import Inspections from './pages/Inspections';
import ResidentCards from './pages/ResidentCards';
import { Menu } from 'lucide-react';

function App() {
  const navLinkClass = ({ isActive }: { isActive: boolean }) =>
    `block px-6 py-2.5 font-medium transition-colors ${
      isActive
        ? 'bg-blue-50 text-blue-700 border-r-4 border-blue-600 font-semibold'
        : 'text-gray-700 hover:bg-gray-100 hover:text-gray-900'
    }`;

  return (
    <Router>
      <div className="flex h-screen bg-gray-100">
        <nav className="w-64 bg-white shadow-lg flex flex-col">
          <div className="p-4 border-b">
            <h1 className="text-xl font-bold text-gray-800 flex items-center gap-2">
              <Menu size={24} />
              PTickets
            </h1>
          </div>
          <ul className="py-4 space-y-1 flex-1">
            <li><NavLink className={navLinkClass} to="/">Inspections</NavLink></li>
            <li><NavLink className={navLinkClass} to="/zones">Zones & Streets</NavLink></li>
            <li><NavLink className={navLinkClass} to="/violations">Violation Types</NavLink></li>
            <li><NavLink className={navLinkClass} to="/inspectors">Inspectors</NavLink></li>
            <li><NavLink className={navLinkClass} to="/cards">Resident Cards</NavLink></li>
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

