import { Link } from 'react-router-dom';
import StatusChip from '../components/StatusChip';
import { usePendingDispatch } from '../api/hooks';
import { JobAssignmentStatus } from '../api/types';

export default function Dispatch() {
  const { data: jobs, isLoading, error } = usePendingDispatch();

  if (isLoading) {
    return (
      <div className="py-12 text-center text-gray-500">
        Loading pending dispatch items...
      </div>
    );
  }

  if (error) {
    return (
      <div className="py-12 text-center text-red-500">
        Failed to load pending dispatch items.
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h2 className="text-2xl font-bold text-gray-900">Awaiting Dispatch</h2>
        <p className="text-sm text-gray-500">Orders confirmed by customer, awaiting printer assignment</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {jobs && jobs.map(job => (
          <div key={job.id} className="bg-white rounded-lg border border-gray-200 shadow-sm p-6 flex flex-col">
            <div className="flex justify-between items-start mb-4">
              <div>
                <h3 className="text-lg font-bold text-gray-900">Job Assignment #{job.id.slice(0, 8)}</h3>
                <p className="text-sm text-gray-500">Order Item #{job.orderItemId.slice(0, 8)}</p>
              </div>
              <StatusChip status={job.status === JobAssignmentStatus.Offered ? 'pending' : 'ready'} />
            </div>
            
            <div className="grid grid-cols-2 gap-4 mb-6">
              <div>
                <span className="text-xs text-gray-500 block">Payout to Owner</span>
                <span className="text-sm font-medium text-gray-900">{job.payoutAmount} EGP</span>
              </div>
              <div>
                <span className="text-xs text-gray-500 block">Offered At</span>
                <span className="text-sm font-medium text-gray-900">{new Date(job.offeredAt).toLocaleDateString()}</span>
              </div>
            </div>

            <div className="mt-auto pt-4 border-t border-gray-100">
              <Link
                to={`/dispatch/${job.orderItemId}/assign`}
                className="w-full block text-center bg-primary-50 text-primary-700 hover:bg-primary-100 py-2 px-4 rounded-md text-sm font-medium transition-colors"
              >
                Find Printer & Assign
              </Link>
            </div>
          </div>
        ))}
        {(!jobs || jobs.length === 0) && (
          <div className="col-span-full py-12 text-center text-gray-500 bg-white rounded-lg border border-gray-200 border-dashed">
            No orders awaiting dispatch.
          </div>
        )}
      </div>
    </div>
  );
}