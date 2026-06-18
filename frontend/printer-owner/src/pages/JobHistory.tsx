import { useState } from 'react';
import { Filter, ChevronRight, CheckCircle2, XCircle } from 'lucide-react';
import { Link } from 'react-router-dom';

export default function JobHistory() {
  const [filter, setFilter] = useState<'all' | '7d' | '30d'>('30d');

  const history = [
    { id: 'JO-8830', date: '2026-06-15', material: 'PLA White', payout: 150, status: 'pass' },
    { id: 'JO-8825', date: '2026-06-12', material: 'PETG Grey', payout: 220, status: 'pass' },
    { id: 'JO-8810', date: '2026-06-05', material: 'ABS Black', payout: 180, status: 'fail' },
    { id: 'JO-8790', date: '2026-05-28', material: 'PLA Blue', payout: 90, status: 'pass' },
  ];

  return (
    <div className="p-4 max-w-lg mx-auto pb-24">
      <div className="flex justify-between items-center mb-6 pt-2">
        <h1 className="text-2xl font-bold text-slate-900">Job History</h1>
        
        <div className="bg-white border border-slate-200 rounded-lg p-1 flex">
          {(['7d', '30d', 'all'] as const).map(f => (
            <button
              key={f}
              onClick={() => setFilter(f)}
              className={`px-3 py-1 text-xs font-medium rounded-md transition-colors ${
                filter === f ? 'bg-primary text-white' : 'text-slate-500 hover:text-slate-900'
              }`}
            >
              {f === 'all' ? 'All' : f === '7d' ? '7 Days' : '30 Days'}
            </button>
          ))}
        </div>
      </div>

      <div className="space-y-3">
        {history.map(job => (
          <Link 
            key={job.id} 
            to={`#`} // normally links to a detail page
            className="block bg-white p-4 rounded-xl shadow-sm border border-slate-100 hover:border-primary/30 transition-colors"
          >
            <div className="flex justify-between items-start mb-2">
              <div>
                <h3 className="font-bold text-slate-900">{job.id}</h3>
                <p className="text-xs text-slate-500">{new Date(job.date).toLocaleDateString()}</p>
              </div>
              <div className="text-right">
                <div className="font-bold text-slate-900">{job.payout} EGP</div>
                <div className={`flex items-center justify-end text-xs font-medium mt-1 ${job.status === 'pass' ? 'text-success' : 'text-destructive'}`}>
                  {job.status === 'pass' ? <CheckCircle2 size={12} className="mr-1"/> : <XCircle size={12} className="mr-1"/>}
                  {job.status === 'pass' ? 'QC Passed' : 'QC Failed'}
                </div>
              </div>
            </div>
            
            <div className="flex justify-between items-center mt-3 pt-3 border-t border-slate-50">
              <span className="text-sm text-slate-600 bg-slate-100 px-2 py-1 rounded-md">{job.material}</span>
              <ChevronRight size={16} className="text-slate-400" />
            </div>
          </Link>
        ))}
      </div>
    </div>
  );
}
