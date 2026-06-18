import { useState, useRef } from 'react';
import { Camera, Image as ImageIcon, X, UploadCloud } from 'lucide-react';

interface PhotoUploaderProps {
  minPhotos?: number;
  onUpload: (files: File[]) => void;
}

export default function PhotoUploader({ minPhotos = 3, onUpload }: PhotoUploaderProps) {
  const [photos, setPhotos] = useState<{ url: string; file: File }[]>([]);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const cameraInputRef = useRef<HTMLInputElement>(null);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      const newFiles = Array.from(e.target.files);
      const newPhotos = newFiles.map(file => ({
        url: URL.createObjectURL(file),
        file
      }));
      setPhotos(prev => [...prev, ...newPhotos]);
    }
  };

  const removePhoto = (index: number) => {
    setPhotos(prev => prev.filter((_, i) => i !== index));
  };

  const handleUploadClick = () => {
    if (photos.length >= minPhotos) {
      onUpload(photos.map(p => p.file));
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex gap-2">
        <button
          onClick={() => cameraInputRef.current?.click()}
          className="flex-1 bg-slate-100 border border-slate-200 text-slate-700 py-3 rounded-xl flex items-center justify-center font-medium active:bg-slate-200 transition-colors"
        >
          <Camera size={20} className="mr-2" /> Take Photo
        </button>
        <button
          onClick={() => fileInputRef.current?.click()}
          className="flex-1 bg-slate-100 border border-slate-200 text-slate-700 py-3 rounded-xl flex items-center justify-center font-medium active:bg-slate-200 transition-colors"
        >
          <ImageIcon size={20} className="mr-2" /> Gallery
        </button>
      </div>

      <input
        type="file"
        accept="image/*"
        capture="environment"
        ref={cameraInputRef}
        onChange={handleFileChange}
        className="hidden"
      />
      <input
        type="file"
        accept="image/*"
        multiple
        ref={fileInputRef}
        onChange={handleFileChange}
        className="hidden"
      />

      {photos.length > 0 && (
        <div className="grid grid-cols-3 gap-2">
          {photos.map((photo, i) => (
            <div key={i} className="relative aspect-square rounded-lg overflow-hidden border border-slate-200">
              <img src={photo.url} alt={`Upload ${i}`} className="w-full h-full object-cover" />
              <button
                onClick={() => removePhoto(i)}
                className="absolute top-1 right-1 bg-black/50 text-white rounded-full p-1"
              >
                <X size={14} />
              </button>
            </div>
          ))}
        </div>
      )}

      <div className="pt-2">
        <button
          onClick={handleUploadClick}
          disabled={photos.length < minPhotos}
          className={`w-full py-3 rounded-xl font-medium flex items-center justify-center transition-colors ${
            photos.length >= minPhotos 
              ? 'bg-primary text-white shadow-sm hover:bg-primary/90' 
              : 'bg-slate-200 text-slate-400 cursor-not-allowed'
          }`}
        >
          <UploadCloud size={20} className="mr-2" />
          Submit for QC Review ({photos.length}/{minPhotos})
        </button>
        {photos.length < minPhotos && (
          <p className="text-center text-sm text-slate-500 mt-2">
            Please upload at least {minPhotos} photos showing different angles.
          </p>
        )}
      </div>
    </div>
  );
}
