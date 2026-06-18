import { Link } from 'react-router-dom';
import StatusChip from '../components/StatusChip';

const mockQuotes = [
  { id: 'Q-1001', customer: 'Ahmed Hassan', material: 'PLA (White)', quantity: 10, date: '2023-11-20T10:00:00Z', status: 'pending' },
  { id: 'Q-1002', customer: 'Sara Ali', material: 'PETG (Black)', quantity: 2, date: '2023-11-21T14:30:00Z', status: 'pending' },
];

export default function Quotes() {
  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h2 className="text-2xl font-bold text-gray-900">Quotes Queue</h2>
      </div>

      <div className="bg-white shadow-sm rounded-lg border border-gray-200 overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Quote ID</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Customer</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Material</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Quantity</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Date</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
              <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Action</th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {mockQuotes.map((quote) => (
              <tr key={quote.id} className="hover:bg-gray-50">
                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{quote.id}</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{quote.customer}</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{quote.material}</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{quote.quantity}</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{new Date(quote.date).toLocaleDateString()}</td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <StatusChip status={quote.status} />
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                  <Link to={`/quotes/${quote.id}/review`} className="text-primary-600 hover:text-primary-900">
                    Open & Confirm
                  </Link>
                </td>
              </tr>
            ))}
            {mockQuotes.length === 0 && (
              <tr>
                <td colSpan={7} className="px-6 py-8 text-center text-gray-500">
                  No pending quotes to review.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}