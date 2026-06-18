import { useState } from 'react';
import { X } from 'lucide-react';

interface Props {
  photos: string[];
}

export default function QCPhotoGrid({ photos }: Props) {
  const [selectedPhoto, setSelectedPhoto] = useState<string | null>(null);

  return (
    <>
      <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
        {photos.map((photo, i) => (
          <div 
            key={i} 
            className="aspect-square bg-gray-100 rounded-lg overflow-hidden cursor-pointer hover:opacity-90"
            onClick={() => setSelectedPhoto(photo)}
          >
            <img src={photo} alt={`QC ${i+1}`} className="w-full h-full object-cover" />
          </div>
        ))}
      </div>

      {selectedPhoto && (
        <div className="fixed inset-0 z-50 bg-black/90 flex items-center justify-center p-4">
          <button 
            className="absolute top-4 right-4 text-white hover:text-gray-300"
            onClick={() => setSelectedPhoto(null)}
          >
            <X className="h-8 w-8" />
          </button>
          <img src={selectedPhoto} alt="QC Full" className="max-w-full max-h-full object-contain" />
        </div>
      )}
    </>
  );
}