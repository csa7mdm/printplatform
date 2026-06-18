import { useState } from 'react';
import { Calendar, ChevronRight, Edit3, ArrowRightLeft } from 'lucide-react';
import EarningsCard from '../components/EarningsCard';

export default function Earnings() {
  const [instapay, setInstapay] = useState('01012345678');
  const [isEditing, setIsEditing] = useState(false);

  const history = [
    { id: 'TRX-102', date: '2026-06-14', amount: 850, ref: 'IPAY-99231' },
    { id: 'TRX-098', date: '2026-06-07', amount: 620, ref: 'IPAY-44120' },
    { id: 'TRX-090', date: '2026-05-31', amount: 1100, ref: 'IPAY-88221' },
  ];

  return (
    <div className="p-4 max-w-lg mx-auto pb-24 space-y-6">
      <h1 className="text-2xl font-bold text-slate-900 pt-2 mb-2">Earnings</h1>

      <div className="grid grid-cols-2 gap-4">
        <div className="col-span-2">
          <EarningsCard 
            title="Available for Next Payout" 
            amount={1450} 
            subtitle="Scheduled for Thursday, Jun 20"
            isHighlight
          />
        </div>
        <EarningsCard title="This Week" amount={600} />
        <EarningsCard title="This Month" amount={3200} />
      </div>

      {/* Payout Method */}
      <div className="bg-white rounded-xl p-5 border border-slate-200">
        <div className="flex justify-between items-center mb-4">
          <h3 className="font-bold text-slate-900">Payout Method</h3>
          <button onClick={() => setIsEditing(!isEditing)} className="text-primary text-sm font-medium flex items-center">
            <Edit3 size={14} className="mr-1" /> Edit
          </button>
        </div>
        
        <div className="flex items-center bg-slate-50 p-3 rounded-lg border border-slate-100">
          <div className="w-10 h-10 bg-purple-100 text-purple-700 rounded-full flex items-center justify-center font-bold text-xs mr-3">
            IP
          </div>
          <div className="flex-1">
            <div className="text-sm font-bold text-slate-900">InstaPay Number</div>
            {isEditing ? (
              <input 
                autoFocus
                value={instapay}
                onChange={e => setInstapay(e.target.value)}
                onBlur={() => setIsEditing(false)}
                className="w-full bg-white border border-primary p-1 rounded text-sm outline-none mt-1"
              />
            ) : (
              <div className="text-slate-500 text-sm">•••• ••• {instapay.slice(-4)}</div>
            )}
          </div>
        </div>
      </div>

      {/* Payout History */}
      <div>
        <h3 className="font-bold text-slate-900 mb-4">Payout History</h3>
        <div className="space-y-3">
          {history.map(item => (
            <div key={item.id} className="bg-white p-4 rounded-xl shadow-sm border border-slate-100 flex items-center justify-between">
              <div className="flex items-center">
                <div className="w-10 h-10 rounded-full bg-slate-50 border border-slate-100 flex items-center justify-center mr-3 text-slate-400">
                  <ArrowRightLeft size={16} />
                </div>
                <div>
                  <div className="font-bold text-slate-900">{new Date(item.date).toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' })}</div>
                  <div className="text-xs text-slate-500">Ref: {item.ref}</div>
                </div>
              </div>
              <div className="text-right">
                <div className="font-bold text-success">+{item.amount} EGP</div>
                <div className="text-xs text-slate-400">Transferred</div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
