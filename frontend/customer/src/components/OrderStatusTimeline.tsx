import { CheckCircle, Circle, Clock } from 'lucide-react';
import React from 'react';

export type OrderStatus = 'Pending' | 'Slicing' | 'Printing' | 'QC' | 'Shipped' | 'Delivered';

const steps: OrderStatus[] = ['Pending', 'Slicing', 'Printing', 'QC', 'Shipped', 'Delivered'];

export const OrderStatusTimeline: React.FC<{ currentStatus: OrderStatus }> = ({ currentStatus }) => {
  const currentIndex = steps.indexOf(currentStatus);

  return (
    <div className="flex flex-col space-y-4">
      {steps.map((step, index) => {
        const isCompleted = index < currentIndex;
        const isCurrent = index === currentIndex;
        const isPending = index > currentIndex;

        return (
          <div key={step} className="flex items-start gap-4">
            <div className="flex flex-col items-center">
              <div className={`w-8 h-8 rounded-full flex items-center justify-center shrink-0 ${isCompleted ? 'bg-green-100 text-green-600' : isCurrent ? 'bg-brand text-white' : 'bg-gray-100 text-gray-400'}`}>
                {isCompleted ? <CheckCircle className="w-5 h-5" /> : isCurrent ? <Clock className="w-5 h-5" /> : <Circle className="w-5 h-5" />}
              </div>
              {index < steps.length - 1 && (
                <div className={`w-0.5 h-12 mt-2 ${isCompleted ? 'bg-green-500' : 'bg-gray-200'}`} />
              )}
            </div>
            <div className="pt-1">
              <p className={`font-semibold ${isCompleted || isCurrent ? 'text-gray-900' : 'text-gray-400'}`}>{step}</p>
              {isCurrent && <p className="text-sm text-gray-500">We are currently working on this step.</p>}
            </div>
          </div>
        );
      })}
    </div>
  );
};
