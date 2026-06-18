import { Link } from 'react-router-dom';
import StatusChip from '../components/StatusChip';

const mockOrders = [
  { id: 'ORD-5001', customer: 'Khaled M.', material: 'PLA', price: 450, date: '2023-11-20', status: 'ready' },
  { id: 'ORD-5002', customer: 'Nour E.', material: 'Resin', price: 1200, date: '2023-11-20', status: 'ready' },
];

export default function Dispatch() {
  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h2 className="text-2xl font-bold text-gray-900">Awaiting Dispatch</h2>
        <p className="text-sm text-gray-500">Orders confirmed by customer, awaiting printer assignment</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {mockOrders.map(order => (
          <div key={order.id} className="bg-white rounded-lg border border-gray-200 shadow-sm p-6 flex flex-col">
            <div className="flex justify-between items-start mb-4">
              <div>
                <h3 className="text-lg font-bold text-gray-900">{order.id}</h3>
                <p className="text-sm text-gray-500">{order.customer}</p>
              </div>
              <StatusChip status={order.status} />
            </div>
            
            <div className="grid grid-cols-2 gap-4 mb-6">
              <div>
                <span className="text-xs text-gray-500 block">Material</span>
                <span className="text-sm font-medium text-gray-900">{order.material}</span>
              </div>
              <div>
                <span className="text-xs text-gray-500 block">Price</span>
                <span className="text-sm font-medium text-gray-900">{order.price} EGP</span>
              </div>
              <div>
                <span className="text-xs text-gray-500 block">Confirmed</span>
                <span className="text-sm font-medium text-gray-900">{new Date(order.date).toLocaleDateString()}</span>
              </div>
            </div>

            <div className="mt-auto pt-4 border-t border-gray-100">
              <Link
                to={`/dispatch/${order.id}/assign`}
                className="w-full block text-center bg-primary-50 text-primary-700 hover:bg-primary-100 py-2 px-4 rounded-md text-sm font-medium transition-colors"
              >
                Find Printer & Assign
              </Link>
            </div>
          </div>
        ))}
        {mockOrders.length === 0 && (
          <div className="col-span-full py-12 text-center text-gray-500 bg-white rounded-lg border border-gray-200 border-dashed">
            No orders awaiting dispatch.
          </div>
        )}
      </div>
    </div>
  );
}