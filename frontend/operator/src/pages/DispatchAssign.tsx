import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import ModelPreview3D from '../components/ModelPreview3D';
import RankedPrinterList, { PrinterOption } from '../components/RankedPrinterList';
import { useAvailablePrintersForJob, useAssignJob } from '../api/hooks';

export default function DispatchAssign() {
  const { id } = useParams<{ id: string }>(); // orderItemId
  const navigate = useNavigate();
  const [selectedPrinter, setSelectedPrinter] = useState<string | null>(null);

  const { data: availablePrinters, isLoading, error } = useAvailablePrintersForJob(id!, {
    enabled: !!id,
  });
  const assignMutation = useAssignJob();

  const handleAssign = () => {
    if (!selectedPrinter || !id || !availablePrinters) return;

    const matched = availablePrinters.find(p => p.printer.id === selectedPrinter);
    if (!matched) return;

    assignMutation.mutate({
      orderItemId: id,
      printerId: selectedPrinter,
      printerOwnerUserId: matched.printer.printerOwnerProfileId,
      payoutAmount: 380, // fallback or price to owner
      operatorNotes: 'Assigned via operator portal',
    }, {
      onSuccess: () => {
        alert('Job assigned successfully!');
        navigate('/dispatch');
      },
      onError: (err: any) => {
        alert(err.response?.data?.detail || 'Failed to assign job.');
      }
    });
  };

  const mappedPrinters: PrinterOption[] = (availablePrinters || []).map(p => ({
    id: p.printer.id,
    ownerName: `Owner #${p.printer.printerOwnerProfileId.slice(0, 8)}`,
    printerModel: `${p.printer.brand} ${p.printer.model}`,
    certStars: p.certificationScore * 5 || 5.0,
    distanceKm: parseFloat(p.distanceKm.toFixed(1)),
    activeJobs: 1, // approximate load
    matchScore: Math.round(p.score * 100) || 85,
  }));

  if (isLoading) {
    return (
      <div className="py-12 text-center text-gray-500">
        Finding eligible printers...
      </div>
    );
  }

  if (error) {
    return (
      <div className="py-12 text-center text-red-500">
        Failed to load available printers.
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h2 className="text-2xl font-bold text-gray-900">Assign Order: {id?.slice(0, 8)}</h2>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-white p-6 rounded-lg border border-gray-200 shadow-sm flex flex-col">
          <h3 className="text-lg font-medium text-gray-900 mb-4">Order Details</h3>
          <div className="h-64 bg-gray-50 rounded-lg mb-4">
            <ModelPreview3D url="#" />
          </div>
          <div className="grid grid-cols-2 gap-y-4 gap-x-8 text-sm">
            <div>
              <span className="text-gray-500 block">Order Item ID</span>
              <p className="font-medium text-gray-900">{id}</p>
            </div>
            <div>
              <span className="text-gray-500 block">Country</span>
              <p className="font-medium text-gray-900">Egypt</p>
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
              printers={mappedPrinters}
              selectedId={selectedPrinter}
              onSelect={setSelectedPrinter}
            />
          </div>

          <div className="mt-6 pt-4 border-t border-gray-200">
            <button
              onClick={handleAssign}
              disabled={!selectedPrinter || assignMutation.isPending}
              className={`w-full flex justify-center py-2.5 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white ${
                selectedPrinter && !assignMutation.isPending
                  ? 'bg-primary-600 hover:bg-primary-700'
                  : 'bg-gray-300 cursor-not-allowed'
              }`}
            >
              {assignMutation.isPending ? 'Assigning...' : 'Confirm Assignment'}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}