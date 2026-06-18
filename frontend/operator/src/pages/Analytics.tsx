import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend } from 'recharts';

const monthlyData = [
  { month: 'Jun', revenue: 12000, profit: 1200 },
  { month: 'Jul', revenue: 19000, profit: 1900 },
  { month: 'Aug', revenue: 25000, profit: 2500 },
  { month: 'Sep', revenue: 32000, profit: 3200 },
  { month: 'Oct', revenue: 48000, profit: 4800 },
];

export default function Analytics() {
  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold text-gray-900">Platform Analytics</h2>

      <div className="grid grid-cols-1 gap-6">
        <div className="bg-white p-6 rounded-lg border border-gray-200 shadow-sm">
          <h3 className="text-lg font-medium text-gray-900 mb-4">Revenue & Platform Profit (6 Months)</h3>
          <div className="h-80">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={monthlyData} margin={{ top: 20, right: 30, left: 20, bottom: 5 }}>
                <CartesianGrid strokeDasharray="3 3" vertical={false} />
                <XAxis dataKey="month" stroke="#9ca3af" axisLine={false} tickLine={false} />
                <YAxis stroke="#9ca3af" axisLine={false} tickLine={false} />
                <Tooltip cursor={{ fill: '#f3f4f6' }} />
                <Legend />
                <Bar dataKey="revenue" name="Total Revenue (EGP)" fill="#bae6fd" radius={[4, 4, 0, 0]} />
                <Bar dataKey="profit" name="Platform Profit (10%)" fill="#0ea5e9" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div className="bg-white p-6 rounded-lg border border-gray-200 shadow-sm">
            <h3 className="text-lg font-medium text-gray-900 mb-4">Top Performing Owners</h3>
            <div className="space-y-4">
              {[
                { name: 'Cairo 3D Hub', volume: '142 orders', rating: '5.0' },
                { name: 'Mahmoud Ali', volume: '89 orders', rating: '4.8' },
                { name: 'Alex Makers', volume: '65 orders', rating: '4.9' },
              ].map((owner, i) => (
                <div key={i} className="flex justify-between items-center p-3 bg-gray-50 rounded-lg">
                  <div className="flex items-center gap-3">
                    <div className="w-8 h-8 rounded-full bg-primary-100 flex items-center justify-center text-primary-700 font-bold text-sm">
                      #{i + 1}
                    </div>
                    <span className="font-medium text-gray-900">{owner.name}</span>
                  </div>
                  <div className="text-right">
                    <div className="text-sm text-gray-500">{owner.volume}</div>
                    <div className="text-xs text-yellow-500 font-medium">★ {owner.rating}</div>
                  </div>
                </div>
              ))}
            </div>
          </div>

          <div className="bg-white p-6 rounded-lg border border-gray-200 shadow-sm">
            <h3 className="text-lg font-medium text-gray-900 mb-4">System Metrics</h3>
            <ul className="space-y-4">
              <li className="flex justify-between items-center border-b border-gray-100 pb-3">
                <span className="text-gray-600">QC Pass Rate</span>
                <span className="font-bold text-green-600">94.2%</span>
              </li>
              <li className="flex justify-between items-center border-b border-gray-100 pb-3">
                <span className="text-gray-600">Avg. Order to Delivery</span>
                <span className="font-bold text-gray-900">4.2 Days</span>
              </li>
              <li className="flex justify-between items-center border-b border-gray-100 pb-3">
                <span className="text-gray-600">Quote Conversion Rate</span>
                <span className="font-bold text-gray-900">68%</span>
              </li>
              <li className="flex justify-between items-center">
                <span className="text-gray-600">Active Printers</span>
                <span className="font-bold text-gray-900">45</span>
              </li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  );
}