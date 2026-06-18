import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Power, Settings, ChevronRight, Activity, Zap } from 'lucide-react';
import JobOfferCard from '../components/JobOfferCard';

export default function Home() {
  const [isOnline, setIsOnline] = useState(true);
  
  // Mock Data
  const currentOffers = [
    {
      id: 'JO-8832',
      material: 'PLA+ Black',
      weight: 120,
      payoutAmount: 180,
      expiresAt: new Date(Date.now() + 2 * 60 * 60 * 1000).toISOString() // 2 hours from now
    }
  ];

  const activeJob = null; // Mocking no active job right now to show offers/idle state

  const handleAccept = (id: string) => alert(`Accepted ${id}`);
  const handleDecline = (id: string) => alert(`Declined ${id}`);

  return (
    <div className="p-4 pb-24 max-w-lg mx-auto">
      {/* Header Status Bar */}
      <div className="flex items-center justify-between bg-white p-4 rounded-xl shadow-sm border border-slate-100 mb-6">
        <div>
          <h2 className="font-bold text-slate-900">Ender 3 V2</h2>
          <div className="flex items-center text-sm mt-0.5">
            <span className={`w-2 h-2 rounded-full mr-2 ${isOnline ? 'bg-success' : 'bg-slate-300'}`}></span>
            <span className={isOnline ? 'text-success font-medium' : 'text-slate-500'}>
              {isOnline ? 'Online & Available' : 'Paused'}
            </span>
          </div>
        </div>
        <button 
          onClick={() => setIsOnline(!isOnline)}
          className={`p-3 rounded-full transition-colors ${isOnline ? 'bg-success/10 text-success' : 'bg-slate-100 text-slate-400'}`}
        >
          <Power size={24} />
        </button>
      </div>

      {/* Main Content Area */}
      <div className="space-y-6">
        
        {/* State 1: Active Job (Highest Priority) */}
        {activeJob ? (
          <div>
            <h3 className="text-sm font-bold text-slate-500 uppercase tracking-wider mb-3">Current Print</h3>
            {/* Active Job Card Placeholder */}
            <Link to="/jobs/active/1" className="block bg-white rounded-xl shadow-sm border border-slate-100 p-4">
              <div className="flex justify-between items-center mb-2">
                <span className="font-bold text-lg text-slate-900">Job #1</span>
                <span className="px-2.5 py-1 bg-warning/10 text-warning text-xs font-bold rounded-full">Printing</span>
              </div>
              <p className="text-slate-600 text-sm mb-4">PLA White • Estimated time left: 4h 30m</p>
              <div className="w-full bg-slate-100 rounded-full h-2">
                <div className="bg-warning h-2 rounded-full w-1/2"></div>
              </div>
            </Link>
          </div>
        ) : 
        
        /* State 2: Job Offers */
        currentOffers.length > 0 && isOnline ? (
          <div>
            <h3 className="text-sm font-bold text-slate-500 uppercase tracking-wider mb-3 flex items-center">
              <Zap size={16} className="mr-1 text-primary" /> Action Required
            </h3>
            <div className="space-y-4">
              {currentOffers.map(offer => (
                <JobOfferCard 
                  key={offer.id} 
                  {...offer} 
                  onAccept={handleAccept} 
                  onDecline={handleDecline} 
                />
              ))}
            </div>
          </div>
        ) : 
        
        /* State 3: Idle / Waiting */
        (
          <div className="bg-slate-100 border border-slate-200 border-dashed rounded-xl p-8 flex flex-col items-center justify-center text-center mt-8">
            <div className={`w-16 h-16 rounded-full flex items-center justify-center mb-4 ${isOnline ? 'bg-primary/10 text-primary' : 'bg-slate-200 text-slate-400'}`}>
              <Activity size={32} className={isOnline ? 'animate-pulse' : ''} />
            </div>
            <h3 className="text-lg font-bold text-slate-900 mb-1">
              {isOnline ? 'Waiting for jobs...' : 'You are offline'}
            </h3>
            <p className="text-slate-500 text-sm">
              {isOnline 
                ? "Keep your app open. We'll notify you as soon as a local order matches your printer capabilities." 
                : "Toggle your status to online above to receive new job offers."}
            </p>
          </div>
        )}

      </div>
    </div>
  );
}
