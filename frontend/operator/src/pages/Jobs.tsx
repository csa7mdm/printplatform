import { Link } from 'react-router-dom';
import StatusChip from '../components/StatusChip';
import { Filter } from 'lucide-react';
import { useDispatchJobs } from '../api/hooks';
import { JobAssignmentStatus } from '../api/types';

const mapStatusToChip = (status: JobAssignmentStatus): string => {
  switch (status) {
    case JobAssignmentStatus.Offered:
      return 'pending';
    case JobAssignmentStatus.Accepted:
    case JobAssignmentStatus.Printing:
      return 'active';
    case JobAssignmentStatus.QCPending:
      return 'qc_pending';
    case JobAssignmentStatus.QCApproved:
      return 'qc_approved';
    case JobAssignmentStatus.QCRejected:
      return 'failed';
    default:
      return 'active';
  }
};

export default function Jobs() {
  const { data: jobs, isLoading } = useDispatchJobs();

  const activeJobs = jobs || [
    { id: 'JOB-9001', printerOwnerUserId: 'Owner-Ali', orderItemId: 'ORD-5001', status: JobAssignmentStatus.Printing, offeredAt: '2023-11-21', completedAt: '2023-11-22' },
    { id: 'JOB-9002', printerOwnerUserId: 'Owner-Cairo3D', orderItemId: 'ORD-5002', status: JobAssignmentStatus.QCPending, offeredAt: '2023-11-19', completedAt: '2023-11-21' },
  ];

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
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Owner Ref</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Order Item Ref</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Started</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Completed</th>
              <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Action</th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {activeJobs.map((job) => (
              <tr key={job.id} className="hover:bg-gray-50">
                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{job.id.slice(0, 8)}</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{job.printerOwnerUserId.slice(0, 8)}</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{job.orderItemId.slice(0, 8)}</td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <StatusChip status={mapStatusToChip(job.status)} />
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {new Date(job.offeredAt).toLocaleDateString()}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {job.completedAt ? new Date(job.completedAt).toLocaleDateString() : '-'}
                </td>
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