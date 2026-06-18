import { Check } from 'lucide-react';

interface Step {
  id: string;
  label: string;
  date?: string;
  isCompleted: boolean;
  isCurrent: boolean;
}

interface Props {
  steps: Step[];
}

export default function JobTimeline({ steps }: Props) {
  return (
    <div className="py-4">
      <div className="flex items-center justify-between">
        {steps.map((step, index) => (
          <div key={step.id} className="flex-1 relative">
            {index < steps.length - 1 && (
              <div className={`absolute top-4 left-1/2 w-full h-0.5 ${step.isCompleted ? 'bg-primary-600' : 'bg-gray-200'}`} />
            )}
            
            <div className="relative flex flex-col items-center group">
              <div className={`w-8 h-8 rounded-full flex items-center justify-center z-10 ${
                step.isCompleted ? 'bg-primary-600 text-white' :
                step.isCurrent ? 'bg-white border-2 border-primary-600 text-primary-600' :
                'bg-white border-2 border-gray-200 text-gray-400'
              }`}>
                {step.isCompleted ? <Check className="w-4 h-4" /> : <span className="text-xs font-medium">{index + 1}</span>}
              </div>
              
              <div className="mt-3 text-center">
                <div className={`text-sm font-medium ${step.isCurrent ? 'text-primary-600' : step.isCompleted ? 'text-gray-900' : 'text-gray-500'}`}>
                  {step.label}
                </div>
                {step.date && (
                  <div className="text-xs text-gray-500 mt-1">{new Date(step.date).toLocaleDateString()}</div>
                )}
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}