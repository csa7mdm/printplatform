import { useState } from 'react';
import { useParams } from 'react-router-dom';
import StatusChip from '../components/StatusChip';
import JobTimeline from '../components/JobTimeline';
import QCPhotoGrid from '../components/QCPhotoGrid';
import { CheckCircle, XCircle } from 'lucide-react';

const mockTimeline = [
  { id: '1', label: 'Assigned', date: '2023-11-19', isCompleted: true, isCurrent: false },
  { id: '2', label: 'Printing', date: '2023-11-19', isCompleted: true, isCurrent: false },
  { id: '3', label: 'QC Review', date: '2023-11-21', isCompleted: false, isCurrent: true },
  { id: '4', label: 'Delivered', isCompleted: false, isCurrent: false },
];

export default function JobDetail() {
  const { id } = useParams();
  const [showRejectModal, setShowRejectModal] = useState(false);
  const [rejectReason, setRejectReason] = useState('');

  const isQcPending = true;

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">Job: {id}</h2>
          <p className="text-sm text-gray-500 mt-1">Order Ref: ORD-5002 • Owner: Cairo 3D Hub</p>
        </div>
        <StatusChip status="qc_pending" />
      </div>

      <div className="bg-white p-6 rounded-lg border border-gray-200 shadow-sm">
        <h3 className="text-lg font-medium text-gray-900 mb-6">Progress</h3>
        <JobTimeline steps={mockTimeline} />
      </div>

      {isQcPending && (
        <div className="bg-white p-6 rounded-lg border border-purple-200 shadow-sm">
          <div className="flex justify-between items-center mb-6">
            <h3 className="text-lg font-medium text-purple-900">Quality Control Review</h3>
            <span className="text-sm text-purple-600 bg-purple-50 px-3 py-1 rounded-full border border-purple-100">
              Action Required
            </span>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
            <div>
              <h4 className="text-sm font-medium text-gray-700 mb-3">Owner Submitted Photos</h4>
              <QCPhotoGrid photos={[
                'https://images.unsplash.com/photo-1620641788421-7a1c342ea42e?auto=format&fit=crop&w=400&q=80',
                'https://images.unsplash.com/photo-1631541909061-71e349d1f203?auto=format&fit=crop&w=400&q=80'
              ]} />
            </div>

            <div>
              <h4 className="text-sm font-medium text-gray-700 mb-3">QC Checklist</h4>
              <ul className="space-y-3 mb-6">
                <li className="flex items-start">
                  <input type="checkbox" className="mt-1 mr-3 h-4 w-4 text-primary-600 rounded border-gray-300" />
                  <span className="text-sm text-gray-700">Dimensions match specifications</span>
                </li>
                <li className="flex items-start">
                  <input type="checkbox" className="mt-1 mr-3 h-4 w-4 text-primary-600 rounded border-gray-300" />
                  <span className="text-sm text-gray-700">No visible stringing or layer shifts</span>
                </li>
                <li className="flex items-start">
                  <input type="checkbox" className="mt-1 mr-3 h-4 w-4 text-primary-600 rounded border-gray-300" />
                  <span className="text-sm text-gray-700">Correct material and color used</span>
                </li>
              </ul>

              <div className="flex gap-4">
                <button className="flex-1 flex items-center justify-center bg-green-600 text-white py-2 px-4 rounded-md shadow-sm text-sm font-medium hover:bg-green-700">
                  <CheckCircle className="w-5 h-5 mr-2" />
                  Approve QC
                </button>
                <button 
                  onClick={() => setShowRejectModal(true)}
                  className="flex-1 flex items-center justify-center bg-white text-red-600 border border-red-300 py-2 px-4 rounded-md shadow-sm text-sm font-medium hover:bg-red-50"
                >
                  <XCircle className="w-5 h-5 mr-2" />
                  Reject
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {showRejectModal && (
        <div className="fixed inset-0 bg-gray-500 bg-opacity-75 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-md w-full">
            <h3 className="text-lg font-medium text-gray-900 mb-4">Reject Quality Control</h3>
            <p className="text-sm text-gray-500 mb-4">Please provide a reason for rejection. This will be sent to the printer owner.</p>
            <textarea
              className="w-full border border-gray-300 rounded-md p-2 mb-4"
              rows={4}
              placeholder="e.g., Visible layer shifting on the Z axis..."
              value={rejectReason}
              onChange={e => setRejectReason(e.target.value)}
            ></textarea>
            <div className="flex justify-end gap-3">
              <button 
                onClick={() => setShowRejectModal(false)}
                className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50"
              >
                Cancel
              </button>
              <button className="px-4 py-2 text-sm font-medium text-white bg-red-600 border border-transparent rounded-md hover:bg-red-700">
                Confirm Rejection
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}