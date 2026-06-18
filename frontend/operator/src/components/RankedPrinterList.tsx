import { Check, Star, Navigation } from 'lucide-react';

export interface PrinterOption {
  id: string;
  ownerName: string;
  printerModel: string;
  certStars: number;
  distanceKm: number;
  activeJobs: number;
  matchScore: number;
}

interface Props {
  printers: PrinterOption[];
  selectedId: string | null;
  onSelect: (id: string) => void;
}

export default function RankedPrinterList({ printers, selectedId, onSelect }: Props) {
  return (
    <div className="space-y-4">
      {printers.map((printer) => (
        <div
          key={printer.id}
          onClick={() => onSelect(printer.id)}
          className={`p-4 rounded-lg border-2 cursor-pointer transition-colors ${
            selectedId === printer.id
              ? 'border-primary-500 bg-primary-50'
              : 'border-gray-200 bg-white hover:border-primary-300'
          }`}
        >
          <div className="flex justify-between items-start mb-2">
            <div>
              <h4 className="font-medium text-gray-900">{printer.ownerName}</h4>
              <p className="text-sm text-gray-500">{printer.printerModel}</p>
            </div>
            {selectedId === printer.id && <Check className="h-5 w-5 text-primary-600" />}
          </div>
          
          <div className="grid grid-cols-3 gap-2 mt-4 text-sm">
            <div className="flex items-center text-gray-600">
              <Star className="h-4 w-4 mr-1 text-yellow-400 fill-current" />
              {printer.certStars.toFixed(1)}
            </div>
            <div className="flex items-center text-gray-600">
              <Navigation className="h-4 w-4 mr-1" />
              {printer.distanceKm} km
            </div>
            <div className="text-gray-600 text-right">
              {printer.activeJobs} active
            </div>
          </div>

          <div className="mt-3">
            <div className="flex justify-between text-xs mb-1">
              <span className="text-gray-500">Match Score</span>
              <span className="font-medium text-gray-900">{printer.matchScore}%</span>
            </div>
            <div className="w-full bg-gray-200 rounded-full h-1.5">
              <div
                className={`h-1.5 rounded-full ${
                  printer.matchScore >= 80 ? 'bg-green-500' :
                  printer.matchScore >= 50 ? 'bg-yellow-500' : 'bg-red-500'
                }`}
                style={{ width: `${printer.matchScore}%` }}
              ></div>
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}