import React from 'react';
import { Outlet, NavLink } from 'react-router-dom';
import { Map, AlertTriangle, UserCheck, Settings } from 'lucide-react';

export default function Layout() {
  return (
    <div className="flex min-h-screen">
      {/* Sidebar */}
      <aside className="w-64 glass-panel m-4 flex flex-col overflow-hidden fixed h-[calc(100vh-2rem)]">
        <div className="p-6 border-b border-[rgba(255,255,255,0.05)]">
          <h1 className="text-2xl font-bold bg-clip-text text-transparent bg-gradient-to-r from-indigo-400 to-purple-400">
            PTickets MVP
          </h1>
          <p className="text-sm text-slate-400 mt-1">System Kontroli Biletów</p>
        </div>
        
        <nav className="flex-1 p-4 flex flex-col space-y-2">
          <NavLink 
            to="/" 
            className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}
            end
          >
            <UserCheck size={20} />
            <span>Panel Kontrolera</span>
          </NavLink>
          
          <NavLink 
            to="/zones" 
            className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}
          >
            <Map size={20} />
            <span>Strefy i Ulice</span>
          </NavLink>
          
          <NavLink 
            to="/violations" 
            className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}
          >
            <AlertTriangle size={20} />
            <span>Wykroczenia</span>
          </NavLink>
        </nav>
      </aside>

      {/* Main Content */}
      <main className="flex-1 ml-72 p-4">
        <div className="max-w-5xl mx-auto">
          <Outlet />
        </div>
      </main>
    </div>
  );
}
