import { useState } from 'react';
import StatusChip from '../components/StatusChip';

const mockPayouts = [
  { id: 'PAY-101', owner: 'Mahmoud Ali', period: 'Oct 2023', gross: 2500, fee: 250, net: 2250, status: 'pending' },
  { id: 'PAY-102', owner: 'Cairo 3D Hub', period: 'Oct 2023', gross: 8000, fee: 800, net: 7200, status: 'completed' },
];

export default function Payouts() {
  const [selectedPayout, setSelectedPayout] = useState<string | null>(null);
  const [refNumber, setRefNumber] = useState('');

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h2 className="text-2xl font-bold text-gray-900">Printer Owner Payouts</h2>
      </div>

      <div className="bg-white shadow-sm rounded-lg border border-gray-200 overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Owner</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Period</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Gross</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Fee (10%)</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Net Payout</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
              <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Action</th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {mockPayouts.map((payout) => (
              <tr key={payout.id} className="hover:bg-gray-50">
                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{payout.owner}</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{payout.period}</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{payout.gross} EGP</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-red-500">-{payout.fee} EGP</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm font-bold text-green-600">{payout.net} EGP</td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <StatusChip status={payout.status} />
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                  {payout.status === 'pending' ? (
                    <button 
                      onClick={() => setSelectedPayout(payout.id)}
                      className="text-primary-600 hover:text-primary-900"
                    >
                      Mark as Sent
                    </button>
                  ) : (
                    <span className="text-gray-400">Done</span>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {selectedPayout && (
        <div className="fixed inset-0 bg-gray-500 bg-opacity-75 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-md w-full">
            <h3 className="text-lg font-medium text-gray-900 mb-4">Confirm InstaPay Transfer</h3>
            <p className="text-sm text-gray-500 mb-4">Enter the transaction reference number from InstaPay to mark this payout as completed.</p>
            <input
              type="text"
              placeholder="InstaPay Ref #"
              className="w-full border border-gray-300 rounded-md p-2 mb-4"
              value={refNumber}
              onChange={e => setRefNumber(e.target.value)}
            />
            <div className="flex justify-end gap-3">
              <button 
                onClick={() => { setSelectedPayout(null); setRefNumber(''); }}
                className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50"
              >
                Cancel
              </button>
              <button 
                onClick={() => { setSelectedPayout(null); setRefNumber(''); }}
                className="px-4 py-2 text-sm font-medium text-white bg-primary-600 border border-transparent rounded-md hover:bg-primary-700"
              >
                Confirm Transfer
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}