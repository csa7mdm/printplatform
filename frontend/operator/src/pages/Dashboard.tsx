import { useQuery } from '@tanstack/react-query';
import { FileText, Briefcase, CheckCircle, DollarSign, TrendingUp } from 'lucide-react';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, PieChart, Pie, Cell } from 'recharts';
import KpiCard from '../components/KpiCard';

const mockKpis = {
  pendingQuotes: 12,
  activeJobs: 45,
  awaitingQc: 8,
  pendingPayouts: 24000,
  todayRevenue: 4500
};

const revenueData = [
  { date: '01/10', revenue: 4000 },
  { date: '05/10', revenue: 3000 },
  { date: '10/10', revenue: 5000 },
  { date: '15/10', revenue: 4500 },
  { date: '20/10', revenue: 6000 },
  { date: '25/10', revenue: 5500 },
  { date: '30/10', revenue: 7000 },
];

const statusData = [
  { name: 'Pending Quote', value: 12, color: '#f59e0b' },
  { name: 'Awaiting Dispatch', value: 15, color: '#6366f1' },
  { name: 'Printing', value: 45, color: '#3b82f6' },
  { name: 'Awaiting QC', value: 8, color: '#8b5cf6' },
  { name: 'Completed', value: 120, color: '#10b981' },
];

export default function Dashboard() {
  return (
    <div className="space-y-6">
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-4">
        <KpiCard title="Pending Quotes" value={mockKpis.pendingQuotes} icon={<FileText className="w-5 h-5" />} trend={{ value: 12, isPositive: true }} />
        <KpiCard title="Active Jobs" value={mockKpis.activeJobs} icon={<Briefcase className="w-5 h-5" />} />
        <KpiCard title="Awaiting QC" value={mockKpis.awaitingQc} icon={<CheckCircle className="w-5 h-5" />} trend={{ value: 5, isPositive: false }} />
        <KpiCard title="Pending Payouts" value={`EGP ${mockKpis.pendingPayouts}`} icon={<DollarSign className="w-5 h-5" />} />
        <KpiCard title="Today's Revenue" value={`EGP ${mockKpis.todayRevenue}`} icon={<TrendingUp className="w-5 h-5" />} trend={{ value: 8, isPositive: true }} />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-white p-6 rounded-lg border border-gray-200 shadow-sm">
          <h3 className="text-lg font-medium text-gray-900 mb-4">Revenue (30 Days)</h3>
          <div className="h-72">
            <ResponsiveContainer width="100%" height="100%">
              <LineChart data={revenueData}>
                <CartesianGrid strokeDasharray="3 3" vertical={false} />
                <XAxis dataKey="date" stroke="#9ca3af" fontSize={12} tickLine={false} axisLine={false} />
                <YAxis stroke="#9ca3af" fontSize={12} tickLine={false} axisLine={false} tickFormatter={(value) => `£${value}`} />
                <Tooltip />
                <Line type="monotone" dataKey="revenue" stroke="#0ea5e9" strokeWidth={3} dot={{ r: 4 }} activeDot={{ r: 6 }} />
              </LineChart>
            </ResponsiveContainer>
          </div>
        </div>

        <div className="bg-white p-6 rounded-lg border border-gray-200 shadow-sm">
          <h3 className="text-lg font-medium text-gray-900 mb-4">Orders by Status</h3>
          <div className="h-72 flex flex-col justify-center">
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie
                  data={statusData}
                  cx="50%"
                  cy="50%"
                  innerRadius={60}
                  outerRadius={100}
                  paddingAngle={5}
                  dataKey="value"
                >
                  {statusData.map((entry, index) => (
                    <Cell key={`cell-${index}`} fill={entry.color} />
                  ))}
                </Pie>
                <Tooltip />
              </PieChart>
            </ResponsiveContainer>
            <div className="flex flex-wrap justify-center gap-4 mt-4">
              {statusData.map((entry, index) => (
                <div key={index} className="flex items-center text-sm">
                  <div className="w-3 h-3 rounded-full mr-2" style={{ backgroundColor: entry.color }}></div>
                  <span className="text-gray-600">{entry.name} ({entry.value})</span>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}