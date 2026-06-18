import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import ModelPreview3D from '../components/ModelPreview3D';
import RankedPrinterList, { PrinterOption } from '../components/RankedPrinterList';

const mockPrinters: PrinterOption[] = [
  { id: 'PRN-001', ownerName: 'Mahmoud Ali', printerModel: 'Creality Ender 3 V2', certStars: 4.8, distanceKm: 2.5, activeJobs: 1, matchScore: 95 },
  { id: 'PRN-002', ownerName: 'Cairo 3D Hub', printerModel: 'Prusa i3 MK3S+', certStars: 5.0, distanceKm: 8.2, activeJobs: 3, matchScore: 88 },
  { id: 'PRN-003', ownerName: 'Youssef Prints', printerModel: 'Anycubic Vyper', certStars: 4.2, distanceKm: 1.1, activeJobs: 0, matchScore: 75 },
];

export default function DispatchAssign() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [selectedPrinter, setSelectedPrinter] = useState<string | null>(null);

  const handleAssign = () => {
    if (!selectedPrinter) return;
    navigate('/dispatch');
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h2 className="text-2xl font-bold text-gray-900">Assign Order: {id}</h2>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-white p-6 rounded-lg border border-gray-200 shadow-sm flex flex-col">
          <h3 className="text-lg font-medium text-gray-900 mb-4">Order Details</h3>
          <div className="h-64 bg-gray-50 rounded-lg mb-4">
            <ModelPreview3D url="#" />
          </div>
          <div className="grid grid-cols-2 gap-y-4 gap-x-8 text-sm">
            <div>
              <span className="text-gray-500 block">Customer</span>
              <p className="font-medium text-gray-900">Khaled M.</p>
            </div>
            <div>
              <span className="text-gray-500 block">Material</span>
              <p className="font-medium text-gray-900">PLA (White)</p>
            </div>
            <div>
              <span className="text-gray-500 block">Weight</span>
              <p className="font-medium text-gray-900">145g</p>
            </div>
            <div>
              <span className="text-gray-500 block">Est. Time</span>
              <p className="font-medium text-gray-900">4.5h</p>
            </div>
            <div>
              <span className="text-gray-500 block">Delivery Deadline</span>
              <p className="font-medium text-gray-900">2023-11-25</p>
            </div>
            <div>
              <span className="text-gray-500 block">Price to Owner</span>
              <p className="font-medium text-gray-900">380 EGP</p>
            </div>
          </div>
        </div>

        <div className="bg-white p-6 rounded-lg border border-gray-200 shadow-sm flex flex-col">
          <div className="flex justify-between items-center mb-4">
            <h3 className="text-lg font-medium text-gray-900">Available Printers</h3>
            <span className="text-sm text-gray-500">Sorted by match score</span>
          </div>

          <div className="flex-1 overflow-y-auto pr-2">
            <RankedPrinterList
              printers={mockPrinters}
              selectedId={selectedPrinter}
              onSelect={setSelectedPrinter}
            />
          </div>

          <div className="mt-6 pt-4 border-t border-gray-200">
            <button
              onClick={handleAssign}
              disabled={!selectedPrinter}
              className={`w-full flex justify-center py-2.5 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white ${
                selectedPrinter
                  ? 'bg-primary-600 hover:bg-primary-700'
                  : 'bg-gray-300 cursor-not-allowed'
              }`}
            >
              Confirm Assignment
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}