import { useState } from 'react';
import { User, Bell, Printer, Shield, ChevronRight, LogOut, Plus } from 'lucide-react';
import { useNavigate } from 'react-router-dom';

export default function Profile() {
  const navigate = useNavigate();
  const [isAvailable, setIsAvailable] = useState(true);

  const handleLogout = () => {
    localStorage.removeItem('partner_token');
    navigate('/login');
  };

  return (
    <div className="p-4 max-w-lg mx-auto pb-24 space-y-6">
      <div className="flex items-center justify-between pt-2">
        <h1 className="text-2xl font-bold text-slate-900">Profile</h1>
      </div>

      {/* User Info */}
      <div className="bg-white rounded-2xl p-6 shadow-sm border border-slate-100 flex items-center">
        <div className="w-16 h-16 bg-primary/10 text-primary rounded-full flex items-center justify-center mr-4">
          <User size={32} />
        </div>
        <div className="flex-1">
          <h2 className="text-xl font-bold text-slate-900">Ahmed Hassan</h2>
          <p className="text-slate-500 text-sm">Maadi, Cairo</p>
        </div>
      </div>

      {/* Global Availability */}
      <div className="bg-white rounded-2xl p-4 shadow-sm border border-slate-100 flex items-center justify-between">
        <div>
          <h3 className="font-bold text-slate-900">Global Availability</h3>
          <p className="text-xs text-slate-500 mt-0.5">Toggle to stop receiving all job offers</p>
        </div>
        <label className="relative inline-flex items-center cursor-pointer">
          <input type="checkbox" className="sr-only peer" checked={isAvailable} onChange={() => setIsAvailable(!isAvailable)} />
          <div className="w-11 h-6 bg-slate-200 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-slate-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-success"></div>
        </label>
      </div>

      {/* Printers */}
      <div>
        <div className="flex items-center justify-between mb-3">
          <h3 className="font-bold text-slate-900 text-sm uppercase tracking-wider">My Printers</h3>
          <button className="text-primary text-sm font-medium flex items-center bg-primary/10 px-2 py-1 rounded-md">
            <Plus size={14} className="mr-1" /> Add Printer
          </button>
        </div>
        
        <div className="bg-white rounded-xl shadow-sm border border-slate-100 overflow-hidden">
          <div className="p-4 border-b border-slate-100 flex items-center justify-between">
            <div className="flex items-center">
              <Printer size={20} className="text-slate-400 mr-3" />
              <div>
                <div className="font-bold text-slate-900">Ender 3 V2</div>
                <div className="text-xs text-slate-500">FDM • PLA, PETG</div>
              </div>
            </div>
            <div className="text-xs font-medium bg-success/10 text-success px-2 py-1 rounded-full">Active</div>
          </div>
          
          <div className="p-4 flex items-center justify-between opacity-50">
            <div className="flex items-center">
              <Printer size={20} className="text-slate-400 mr-3" />
              <div>
                <div className="font-bold text-slate-900">Elegoo Mars 3</div>
                <div className="text-xs text-slate-500">SLA • Resin</div>
              </div>
            </div>
            <div className="text-xs font-medium bg-slate-100 text-slate-500 px-2 py-1 rounded-full">Maintenance</div>
          </div>
        </div>
      </div>

      {/* Settings List */}
      <div>
        <h3 className="font-bold text-slate-900 text-sm uppercase tracking-wider mb-3">Settings</h3>
        <div className="bg-white rounded-xl shadow-sm border border-slate-100 overflow-hidden">
          {[
            { icon: Bell, label: 'Notifications' },
            { icon: Shield, label: 'Security & Privacy' },
          ].map((item, i) => (
            <button key={i} className="w-full p-4 flex items-center justify-between hover:bg-slate-50 border-b border-slate-50 last:border-0 transition-colors">
              <div className="flex items-center text-slate-700">
                <item.icon size={20} className="mr-3 text-slate-400" />
                <span className="font-medium">{item.label}</span>
              </div>
              <ChevronRight size={18} className="text-slate-400" />
            </button>
          ))}
        </div>
      </div>

      {/* Logout */}
      <button 
        onClick={handleLogout}
        className="w-full bg-white border border-destructive/20 text-destructive font-medium p-4 rounded-xl flex items-center justify-center shadow-sm active:bg-destructive/5 transition-colors"
      >
        <LogOut size={20} className="mr-2" />
        Log Out
      </button>

    </div>
  );
}
