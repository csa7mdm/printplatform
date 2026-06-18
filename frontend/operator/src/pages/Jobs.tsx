import { Link } from 'react-router-dom';
import StatusChip from '../components/StatusChip';
import { Filter } from 'lucide-react';

const mockJobs = [
  { id: 'JOB-9001', owner: 'Mahmoud Ali', orderId: 'ORD-5001', status: 'active', started: '2023-11-21', eta: '2023-11-22' },
  { id: 'JOB-9002', owner: 'Cairo 3D Hub', orderId: 'ORD-5002', status: 'qc_pending', started: '2023-11-19', eta: '2023-11-21' },
  { id: 'JOB-9003', owner: 'Youssef Prints', orderId: 'ORD-4099', status: 'failed', started: '2023-11-20', eta: '-' },
];

export default function Jobs() {
  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h2 className="text-2xl font-bold text-gray-900">Active Jobs</h2>
        <button className="flex items-center px-4 py-2 bg-white border border-gray-300 rounded-md text-sm font-medium text-gray-700 hover:bg-gray-50">
          <Filter className="w-4 h-4 mr-2" />
          Filter
        </button>
      </div>

      <div className="bg-white shadow-sm rounded-lg border border-gray-200 overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Job ID</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Owner</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Order Ref</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Started</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">ETA</th>
              <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Action</th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {mockJobs.map((job) => (
              <tr key={job.id} className="hover:bg-gray-50">
                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{job.id}</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{job.owner}</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{job.orderId}</td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <StatusChip status={job.status} />
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{job.started}</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{job.eta}</td>
                <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                  <Link to={`/jobs/${job.id}`} className="text-primary-600 hover:text-primary-900">
                    View
                  </Link>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}