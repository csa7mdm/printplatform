import React from 'react';

type StatusType = 'pending' | 'active' | 'completed' | 'failed' | 'qc_pending' | 'ready';

interface Props {
  status: StatusType | string;
}

export default function StatusChip({ status }: Props) {
  const getStyles = (s: string) => {
    switch (s.toLowerCase()) {
      case 'pending': return 'bg-yellow-100 text-yellow-800';
      case 'active': return 'bg-blue-100 text-blue-800';
      case 'qc_pending': return 'bg-purple-100 text-purple-800';
      case 'completed': return 'bg-green-100 text-green-800';
      case 'ready': return 'bg-green-100 text-green-800';
      case 'failed': return 'bg-red-100 text-red-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  };

  const label = status.replace('_', ' ').replace(/\b\w/g, l => l.toUpperCase());

  return (
    <span className={`px-2.5 py-0.5 rounded-full text-xs font-medium ${getStyles(status)}`}>
      {label}
    </span>
  );
}